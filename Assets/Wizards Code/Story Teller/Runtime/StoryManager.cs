using Ink.Runtime;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace WizardsCode.StoryTeller
{
    [RequireComponent(typeof(AudioSource))]
    public class StoryManager : MonoBehaviour
    {
        internal const string SCENE_PROGRESS_KEY_PREFIX = "SceneLoadCount_";

        // Ink
        [SerializeField, Tooltip("The Ink file to work with."), BoxGroup("Ink Configuration"), Required]
        TextAsset m_InkJSON;
        
        // Scenes
        [SerializeField, Tooltip("The scene to play if no profiles exist. This is the start of the story."), Scene, BoxGroup("Scenes")]
        private string introScene;
        [SerializeField, Tooltip("The loading scene that will be used to transition between scenes. When this scene is loaded some of the story content will be displayed."), Scene, BoxGroup("Scenes")]
        private string loadingScreen;

        // Actors
        [SerializeField, Tooltip("The actors that are available in this scene."), BoxGroup("Actors")]
        IActorController[] m_Actors;
        [SerializeField, Tooltip("The cue to send to an actor when they start talking."), BoxGroup("Actors")]
        ActorCue m_startTalkingCue;
        [SerializeField, Tooltip("The cue to send to an actor when they hav finished talking."), BoxGroup("Actors")]
        ActorCue m_stopTalkingCue;

        // UI
        [SerializeField, Tooltip("The canvas on which to display the story UI."), BoxGroup("UI")]
        Canvas m_StoryCanvas;
        [SerializeField, Tooltip("The panel on which to display the choice buttons in the story."), BoxGroup("UI")]
        RectTransform choicesPanel;
        [SerializeField, Tooltip("Story choice button"), BoxGroup("UI")]
        Button m_ChoiceButtonPrefab;
        [SerializeField, Tooltip("X offset for the buttons position on screen."), BoxGroup("UI")]
        float m_ButtonXOffset = 150;
        [SerializeField, Tooltip("Y offset for each consecutive button. The Y position of the button will be the number multiplied by this amount, meaning the buttons will be stacked on top of one another."), BoxGroup("UI")]
        float m_ButtonYBottomMargin = 10;
        [SerializeField, Tooltip("Y offset for each consecutive button. The Y position of the button will be the number multiplied by this amount, meaning the buttons will be stacked on top of one another."), BoxGroup("UI")]
        float m_ButtonYOffset = 60;
        [SerializeField, Tooltip("The time it takes for a button to move from its start position to its target position when spawned in."), BoxGroup("UI")]
        float m_ButtonAnimationTime = 0.6f;
        [SerializeField, Tooltip("Dialogue and narration text controller that will display the currently active text. If this is null then a TextController with the name 'Current Text' will be used."), BoxGroup("UI")]
        StoryTextController m_CurrentText;
        [SerializeField, Tooltip("When the Ink story calls for an actor to tall how longer, per character in the text, should they be kept in an active state. The actor will not carry out any other actions until this time has elepased. Set to 0 to not have the speaker wait."), BoxGroup("UI")]
        float m_ActiveTimePerCharacter = 0.01f;
        [SerializeField, Tooltip("If there is only one option available in the story should it automatically be chosen? If set to false the story will wait for the player to select the choice."), BoxGroup("UI")]
        bool m_autoAdvanceSingleChoice = false;
        [SerializeField, Tooltip("If there is no text to display should the UI be hidden? Setting this to false may result in unexpected behaviour if the UI is not designed to manage knots with empty text."), BoxGroup("UI")]
        bool m_AutoHideUIOnEmptyText = true;

        // Debug
        [SerializeField, Tooltip("Show the debug tooling for the story system."), BoxGroup("Debug")]
        private bool showDebugOptions = false;
        [SerializeField, Tooltip("Use verbose logging."), BoxGroup("Debug"), ShowIf("showDebugOptions")]
        private bool verboseLogging = false;
        [SerializeField, Tooltip("Should the story be reset when the game starts?"), BoxGroup("Debug"), ShowIf("showDebugOptions"), FormerlySerializedAs("resetTutorial")]
        private bool resetStory = false;

        Story m_Story;
        private IActorController m_activeSpeaker;

        private Dictionary<string, Type> directions = new Dictionary<string, Type>();
        private Dictionary<string, Transform> m_CachedObjects = new Dictionary<string, Transform>();
        private Dictionary<string, SceneToKnotMapping> m_SceneToKnotMapping = new Dictionary<string, SceneToKnotMapping>();

        private static StoryManager _instance;
        List<AbstractWaitForDirection> waitForStates = new List<AbstractWaitForDirection>();

        AudioSource audioSource;
        
        private bool m_IsDisplayingUI = false;
        bool isUIDirty = false;
        StringBuilder m_NewTextToDisplay = new StringBuilder();
        bool wasWaiting = false; // Set to true when we were waiting for something to happen, e.g. an actor to reach a target position and it has now happened. This is used to re-trigger the story progression, e.g. in Update().
        bool resumeStory = false; // Set to true if the should be resumed from the current StoryPath. This is used, for example, when a new scene has been loaded and we need to resume from a specific knot.

        internal bool IsDisplayingUI
        {
            get { return m_IsDisplayingUI; }
            set
            {
                m_IsDisplayingUI = value;
                isUIDirty = value;
                m_StoryCanvas.gameObject.SetActive(value);
            }
        }

        private bool isWaiting
        {
            get
            {
                for (int i = waitForStates.Count - 1; i >= 0; i--)
                {
                    if (waitForStates[i].IsWaiting)
                    {
                        return true;
                    } 
                    else
                    {
                        wasWaiting = true;
                        waitForStates.RemoveAt(i);
                        if (waitForStates.Count == 0) return false;
                    }
                }
                return false;
            }
        }

        public static StoryManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    if (_instance == null)
                    {
                        _instance = FindAnyObjectByType<StoryManager>();
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            m_Story = new Story(m_InkJSON.text);
            IsDisplayingUI = true;

            DontDestroyOnLoad(gameObject);
            audioSource = gameObject.GetComponent<AudioSource>();
        }

        private void Start()
        {
            // From the original InkManager: https://github.com/TheWizardsCode/Character-Dev/blob/140f9f3a144c630f3b71cb974e9eb530e47238b0/Assets/Wizards%20Code/Character%20AI/Ink/Scripts/Runtime/InkManager.cs#L150
            //if (cinemachine == null)
            //{
            //    Debug.LogWarning("Cinemachine brain is not set in the inspector. Auto discovering. You should set this in the inspectr.");
            //    cinemachine = GameObject.FindObjectOfType<CinemachineBrain>();
            //}

            //// TODO: These lookups are here so that we can have the UI and the world in different scenes. However, looking up by name is very brittle. We should find a better way to do this.
            //if (choicesPanel == null)
            //{
            //    choicesPanel = GameObject.Find("Choices Panel").GetComponent<RectTransform>();
            //}
            //if (m_CurrentText == null)
            //{
            //    m_CurrentText = GameObject.Find("Current Text").GetComponent<TextController>();
            //}

            //if (!string.IsNullOrEmpty(m_StartingPath))
            //{
            //    m_Story.ChoosePathString(m_StartingPath);
            //}

            // Load all the directions that may be used by the story
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            IEnumerable<Type> implementingTypes = assemblies.SelectMany(a => a.GetTypes())
                                                             .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(AbstractDirection)));
            foreach (Type implementingType in implementingTypes)
            {
                directions.Add(implementingType.Name.Substring(0, implementingType.Name.IndexOf("Direction")), implementingType);
            }

            BindExternalFunctions();
        }

        public void ShowUI()
        {
            IsDisplayingUI = true;
        }

        public void HideUI()
        {
            IsDisplayingUI = false;
        }

        private void UpdateTextGUI()
        {
            string text = m_NewTextToDisplay.ToString();

            if (!string.IsNullOrEmpty(text))
            {
                if (!text.EndsWith("\n"))
                {
                    text += "\n";
                }
                m_CurrentText.AddText(m_activeSpeaker, text);

                m_NewTextToDisplay.Clear();
            }
        }

        private void UpdateChoicesGUI()
        {
            if (!m_CurrentText.isFinished) return;

            if (m_Story.currentChoices.Count >= 1)
            {
                for (int i = m_Story.currentChoices.Count - 1; i >= 0; i--)
                {
                    choicesPanel.gameObject.SetActive(true);
                    Choice choice = m_Story.currentChoices[i];
                    Button choiceButton = Instantiate(m_ChoiceButtonPrefab) as Button;
                    choiceButton.gameObject.transform.position = new Vector3(175, 900, 0);
                    choiceButton.gameObject.SetActive(true);
                    TextMeshProUGUI choiceText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
                    choiceText.text = choice.text;
                    choiceButton.transform.SetParent(choicesPanel.transform, false);

                    choiceButton.onClick.AddListener(delegate
                    {
                        ContinueStory(choice);
                    });

                    Vector3 pos = new Vector3(m_ButtonXOffset, m_ButtonYOffset * (m_Story.currentChoices.Count - i + 1) + m_ButtonYBottomMargin, 0);
                    StartCoroutine(AnimateButtonPlacement(choiceButton.GetComponent<RectTransform>(), pos));
                }

                isUIDirty = false;
            }
        }

        /// <summary>
        /// Remove the choices buttons from the panel. This indicates that a choice has been made.
        /// </summary>
        private void EraseChoices()
        {
            for (int i = 0; i < choicesPanel.transform.childCount; i++)
            {
                Destroy(choicesPanel.transform.GetChild(i).gameObject);
            }
        }

        IEnumerator AnimateButtonPlacement(RectTransform rect, Vector3 targetPos)
        {
            yield return new WaitForEndOfFrame();

            float time = 0;
            Vector3 startPos = rect.anchoredPosition;

            while (time < m_ButtonAnimationTime)
            {
                time += Time.unscaledDeltaTime;
                rect.anchoredPosition = Vector3.Lerp(startPos, targetPos, time / m_ButtonAnimationTime);
                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// Called whenever the story needs to progress.
        /// </summary>
        /// <param name="choice">The choice made to progress the story.</param>
        void ContinueStory(Choice choice)
        {
            EraseChoices();
            m_Story.ChooseChoiceIndex(choice.index);
            m_NewTextToDisplay.Clear();
            m_CurrentText.ClearText();
            isUIDirty = true;
        }

        /// <summary>
        /// Grab the current story chunk and parse it for processing.
        /// </summary>
        void ProcessStoryChunk()
        {
            if (!m_Story.canContinue && !isWaiting)
            {
                if (m_Story.currentChoices.Count == 1)
                {
                    if (m_autoAdvanceSingleChoice)
                    {
                        Log("Only one choice available and auto advance is on. Automatically choosing the one option.");
                        m_Story.ChooseChoiceIndex(0);
                    }
                }
            }

            string line;
            while (m_NewTextToDisplay.Length == 0 && m_CurrentText.isFinished && m_Story.canContinue && !isWaiting)
            {
                line = m_Story.Continue();
                Log("Processing line: " + line);

                // Process Directions;
                int cmdIdx = line.IndexOf(">>>");
                if (cmdIdx >= 0)
                {
                    m_NewTextToDisplay.Clear();

                    int startIdx = line.IndexOf(' ', cmdIdx);
                    int endIdx = line.IndexOf(':') - startIdx;
                    if (endIdx < 0)
                    {
                        Debug.LogError("Syntax error in the direction: " + line + " Are you missing a ':' after the command name?");
                        break;
                    }
                    string name = line.Substring(startIdx, endIdx).Trim();

                    AbstractDirection cmd = null;
                    foreach (var direction in directions.Values)
                    {
                        if (direction.Name.ToLower() == name.ToLower() + "direction")
                        {
                            Log($"Found direction: {direction.Name}.");
                            cmd = (AbstractDirection)Activator.CreateInstance(direction);
                            break;
                        }
                    }

                    if (cmd == null)
                    {
                        Debug.LogError("Unknown Direction: " + line);
                        continue;
                    }

                    string[] args = line.Substring(endIdx + startIdx + 1).Split(',');
                    args = Array.ConvertAll(args, s => s.Trim());

                    Log($"Executing direction: {cmd.DirectionName} with argumens {string.Join(", ", args)}.");
                    cmd.Execute(args);
                }

                // is it dialogue?
                else if (Regex.IsMatch(line, "^(\\w*>)|^(\\w*\\s\\w*>)", RegexOptions.IgnoreCase)) // we have an actors name
                {
                    if (string.IsNullOrEmpty(line) && m_AutoHideUIOnEmptyText)
                    {
                        IsDisplayingUI = false;
                    }

                    int indexOfSpeakerChar = line.IndexOf(">");
                    string speaker = line.Substring(0, indexOfSpeakerChar).Trim();
                    string speech = line.Substring(indexOfSpeakerChar + 1).Trim();

                    m_activeSpeaker = FindActor(speaker);

                    if (m_activeSpeaker != null)
                    {
                        TalkFor(m_activeSpeaker, speech.Length * m_ActiveTimePerCharacter);
                    }

                    m_NewTextToDisplay.Append(speech);
                    if (m_ActiveTimePerCharacter > 0)
                    {
                        WaitForDurationDirection waitFor = new WaitForDurationDirection();
                        waitFor.Execute(m_ActiveTimePerCharacter * speech.Length);
                    }

                    isUIDirty = true;
                }
                // No named actor, so interpret it as narration/descriptive text
                else
                {
                    if (string.IsNullOrEmpty(line) && m_AutoHideUIOnEmptyText)
                    {
                        IsDisplayingUI = false;
                    }

                    m_activeSpeaker = null;
                    m_NewTextToDisplay.Append(line);
                    if (m_ActiveTimePerCharacter > 0)
                    {
                        WaitForDurationDirection waitFor = new WaitForDurationDirection();
                        waitFor.Execute(m_ActiveTimePerCharacter * line.Length);
                    }

                    isUIDirty = true;
                }
            }
        }

        /// <summary>
        /// Set a actor to talk for a number of seconds.
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="seconds"></param>
        void TalkFor(IActorController actor, float seconds)
        {
            m_activeSpeaker.Prompt(m_startTalkingCue);
            Invoke("StopTalking", seconds);
        }

        void StopTalking()
        {
            if (m_activeSpeaker == null) return;
            m_activeSpeaker.Prompt(m_stopTalkingCue);
        }

        /// <summary>
        /// Look through the known actors to see if we have one with the given name.
        /// </summary>
        /// <param name="actorName">The name of the actor we want.</param>
        /// <param name="logError">If true (the default) an error will be logged to the console if the actor is not found.</param>
        /// <returns>The actor with the given name or null if they cannot be found.</returns>
        /// 
        internal IActorController FindActor(string actorName, bool logError = true)
        {
            IActorController actor = null;
            for (int i = 0; i < m_Actors.Length; i++)
            {
                if (m_Actors[i].DisplayName == actorName.Trim())
                {
                    actor = m_Actors[i];
                    break;
                }
            }

            if (logError && actor == null)
            {
                Debug.LogError($"Ink script contains a direction for actor called '{actorName}`. However, the actor cannot be found.");
            }

            return actor;
        }

        void BindExternalFunctions()
        {
            m_Story.BindExternalFunction("ConvertToSpaced", (string value) =>
            {
                return value.Replace('_', ' ');
            });
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
            if (isWaiting || !m_CurrentText.isFinished) return;

            if (wasWaiting || resumeStory)
            {
                Log("Finished waiting for resumption of the story.");
                wasWaiting = false;
                resumeStory = false;
                IsDisplayingUI = true;
            }

            ProcessStoryChunk();

            if (IsDisplayingUI)
            {
                if (isUIDirty)
                {
                    Log("Updating the UI.");
                    UpdateTextGUI();
                    UpdateChoicesGUI();
                }
            }
        }

        void Log(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"<color=green>[StoryManager]</color> {message}");
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            foreach (KeyValuePair<string, SceneToKnotMapping> mapping in m_SceneToKnotMapping)
            {
                if (scene.name == mapping.Key)
                {
                    ResumeFromKnot(mapping.Value.path);
                    if (mapping.Value.oneShot)
                    {
                        m_SceneToKnotMapping.Remove(mapping.Key);
                    }
                    return;
                }
            }
        }

        internal void ResumeFromKnot(string knotName)
        {
            resumeStory = true;
            m_Story.ChoosePathString(knotName);
        }

        internal void AddSceneLoadListener(SceneToKnotMapping mapping)
        {
            if (m_SceneToKnotMapping.TryGetValue(mapping.sceneName, out SceneToKnotMapping existingMapping))
            {
                Debug.LogWarning($"The scene name '{mapping.sceneName}' already exists in the scene to knot mapping. The knot of '{existingMapping.path}' will be replaced with '{mapping.path}'. To avoid this warning explicitly remove the existing listener before adding a new one.");
                m_SceneToKnotMapping[mapping.sceneName] = mapping;
            }
            else
            {
                m_SceneToKnotMapping.Add(mapping.sceneName, mapping);
            }
        }

        private IEnumerator HideStoryManagedUIElements()
        {
            StoryManagedUIElement[] managedUIElements = null;
            Canvas canvas = FindObjectOfType<Canvas>();

            if (canvas != null)
            {
                managedUIElements = canvas.GetComponentsInChildren<StoryManagedUIElement>();
                if (managedUIElements != null)
                {
                    SetUIState(false, managedUIElements);
                }
            }

            yield return new WaitForSeconds(0.75f);

            if (managedUIElements != null)
            {
                SetUIState(true, managedUIElements);
            }
        }

        /// <summary>
        /// Add a value to a list variable.
        /// </summary>
        /// <param name="listVariableName">Name of the List variable as it appears in the Ink content.</param>
        /// <param name="item">The value of the item to add.</param>
        public static void AddToInkListVariable(string listVariableName, string item)
        {
            InkList list = Instance.m_Story.variablesState[listVariableName] as InkList;
            list.AddItem(item);
        }

        /// <summary>
        /// Set a variable in the Ink story.
        /// </summary>
        /// <param name="variableName">The name of the Ink variable</param>
        /// <param name="value">The value to set it to</param>
        public static void SetInkVariable(string variableName, int value)
        {
            Instance.m_Story.variablesState[variableName] = value;
        }


        /// <summary>
        /// Enables and disables UI elements when a story step is started or stopped.
        /// This allows us to minimize the clutter on screen during a story step.
        /// </summary>
        /// <param name="isActive"></param>
        /// <param name="managedUIElements"></param>
        private void SetUIState(bool isActive, StoryManagedUIElement[] managedUIElements)
        {
            foreach (StoryManagedUIElement uiElement in managedUIElements)
            {
                if (uiElement.disableDuringTutorial)
                {
                    uiElement.gameObject.SetActive(isActive);
                }
            }
        }

        internal Transform FindTarget(string objectName)
        {
            string trimmedName = objectName.Trim();
            if (m_CachedObjects.TryGetValue(trimmedName, out Transform cachedTransform))
            {
                return cachedTransform;
            }

            // TODO: consider the actor caching found in the original https://github.com/TheWizardsCode/Character-Dev/blob/140f9f3a144c630f3b71cb974e9eb530e47238b0/Assets/Wizards%20Code/Character%20AI/Ink/Scripts/Runtime/InkManager.cs#L896

            //OPTIMIZATION Don't use Find at runtime. When initiating the InkManager we should consider pre-emptively parse all directions and cache the results in m_CachedObjects - or perhaps (since the story may be larger or dynamic) we should do it in a Coroutine just ahead of execution of the story chunk
            GameObject go = GameObject.Find(trimmedName);
            if (go)
            {
                m_CachedObjects[trimmedName] = go.transform;
                return go.transform;
            }
            else
            {
                Debug.LogError($"There is a direction that needs to operate on {objectName}, but the object cannot be found.");
                return null;
            }
        }

        internal void AddWaitForState(AbstractWaitForDirection direction)
        {
            waitForStates.Add(direction);
        }

        internal static string GetCurrentKnotName()
        {
            return Instance.m_Story.state.currentPathString;
        }
    }

    class SceneToKnotMapping
    {
        public string sceneName;
        public string path;
        public bool oneShot = true;

        /// <summary>
        /// Create a new mappwing.
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="knotName"></param>
        /// <Param name="oneShot"></param>
        public SceneToKnotMapping(string sceneName, string path, bool oneShot = true)
        {
            this.sceneName = sceneName;
            this.path = path;
            this.oneShot = oneShot;
        }
    }
}
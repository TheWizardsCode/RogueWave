using ElevenLabs;
using ElevenLabs.Voices;
using RogueWave;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEngine;
using UnityEngine.Audio;
using WizardsCode.Audio;

namespace WizardsCode.Speech
{

    public class SpeechEditorWindow : EditorWindow
    {
        private AudioMixerGroup audioMixerGroup;
        private string textToConvert = string.Empty;
        private string filename;
        private string category;
        private string path = "_Dev/Audio/Nanobots";
        private RecorderController recordingController;
        private AudioSource audioSource;
        private AbstractRecipe[] allRecipes;
        private int voicelineVariations = 4;
        private int processingVariations = 3;

        private Vector2 recipesScrollPosition;

        [MenuItem("Tools/Wizards Code/Speech")]
        public static void ShowWindow()
        {
            GetWindow<SpeechEditorWindow>("Text to Speech");
        }

        private void OnEnable()
        {
            audioSource = FindAnyObjectByType<AudioSource>();

            // Get all Recipes from the Resources folder
            allRecipes = Resources.LoadAll<AbstractRecipe>("Recipes");

            voicelineVariations = EditorPrefs.GetInt("SpeechEditorWindow.voicelineVariations");
            processingVariations = EditorPrefs.GetInt("SpeechEditorWindow.processingVariations");
            int mixerGroupID = EditorPrefs.GetInt("SpeechEditorWindow.mixerGroup");
            if (mixerGroupID != -1)
            {
                audioMixerGroup = (AudioMixerGroup)EditorUtility.InstanceIDToObject(mixerGroupID);
            }
        }

        private void OnDisable()
        {
            EditorPrefs.SetInt("SpeechEditorWindow.voicelineVariations", voicelineVariations);
            EditorPrefs.SetInt("SpeechEditorWindow.processingVariations", processingVariations);
            EditorPrefs.SetInt("SpeechEditorWindow.mixerGroup", audioMixerGroup == null ? -1 : audioMixerGroup.GetInstanceID());
        }

        private void OnGUI()
        {
            audioMixerGroup = (AudioMixerGroup)EditorGUILayout.ObjectField("Audio Mixer Group", audioMixerGroup, typeof(AudioMixerGroup), false);

            filename = EditorGUILayout.TextField("Audio clip name", filename);

            category = EditorGUILayout.TextField("Category", category);

            GUILayout.Label("Text to Convert to Speech:", EditorStyles.boldLabel);
            textToConvert = EditorGUILayout.TextArea(textToConvert, GUILayout.Height(80));

            voicelineVariations = EditorGUILayout.IntField("Voiceline variations", voicelineVariations);

            processingVariations = EditorGUILayout.IntField("Processing variations", processingVariations);


            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Please enter play mode to process audio.", MessageType.Warning);
                if (GUILayout.Button("Enter Play Mode", GUILayout.Height(80)))
                {
                    EditorApplication.isPlaying = true;
                }
            }
            else if (string.IsNullOrEmpty(textToConvert) || string.IsNullOrEmpty(filename))
            {
                EditorGUILayout.HelpBox("Please ensure you have both a clip name and some text to convert to speech. If any options are available below pressing the generate button will fill in these boxes for you.", MessageType.Warning);
            }
            else if (EditorApplication.isPlaying)
            {
                if (GUILayout.Button("Generate Nanobot Voicelines"))
                {
                    EditorApplication.delayCall += async () =>
                    {
                        List<AudioClip> clips = await GenerateNanobotVoicelines();
                        // do nothing with the clips
                    };
                }
            }

            OnRecipeListGUI();
        }

        private async void OnRecipeListGUI()
        {
            recipesScrollPosition = EditorGUILayout.BeginScrollView(recipesScrollPosition);

            List<AbstractRecipe> voiced = new List<AbstractRecipe>();

            GUILayout.Space(10);
            GUILayout.Label("Unvoiced Recipes", EditorStyles.boldLabel);
            foreach (AbstractRecipe recipe in allRecipes)
            {
                GUILayout.BeginHorizontal();
                if (recipe.nameClips.Length > 0)
                {
                    voiced.Add(recipe);
                } else
                {
                    if (GUILayout.Button(recipe.DisplayName, EditorStyles.label))
                    {
                        EditorGUIUtility.PingObject(recipe);
                        Selection.activeObject = recipe;
                    }
                    if (Application.isPlaying)
                    {
                        if (GUILayout.Button($"Generate {voicelineVariations * processingVariations} Name Voicelines", GUILayout.Width(200)))
                        {
                            Selection.activeObject = recipe;
                            textToConvert = recipe.DisplayName;
                            filename = recipe.DisplayName;
                            category = recipe.Category;
                            
                            EditorApplication.delayCall += async () =>
                            {
                                List<AudioClip> clips = await GenerateNanobotVoicelines();
                                recipe.nameClips = clips.ToArray();
                                EditorUtility.SetDirty(recipe);
                                AssetDatabase.SaveAssets();
                            };
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
            GUILayout.Label("Voiced Recipes", EditorStyles.boldLabel);
            foreach (AbstractRecipe recipe in voiced)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(recipe.DisplayName, EditorStyles.label))
                {
                    EditorGUIUtility.PingObject(recipe);
                    Selection.activeObject = recipe;
                }
                if (GUILayout.Button($"Play {recipe.nameClips.Length} Name Clips", GUILayout.Width(200)))
                {
                    audioSource.outputAudioMixerGroup = null;
                    foreach (AudioClip clip in recipe.nameClips)
                    {
                        audioSource.PlayOneShot(clip);
                        await Task.Delay((int)(clip.length + 0.35f) * 1000);
                    }
                }
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private async Task<List<AudioClip>> GenerateNanobotVoicelines()
        {
            Debug.Log("Generating Nanobot voice lines.");

            audioSource.outputAudioMixerGroup = audioMixerGroup;

            ElevenLabsClient api = new ElevenLabsClient();
            IReadOnlyList<Voice> allVoices = await api.VoicesEndpoint.GetAllVoicesAsync();

            for (int i = 0; i < voicelineVariations; i++)
            {
                Debug.Log($"Generating voice line {i + 1} of {voicelineVariations}");

                VoiceSettings defaultVoiceSettings = await api.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
                Voice voice = allVoices[Random.Range(0, allVoices.Count)];
                VoiceClip voiceClip = await api.TextToSpeechEndpoint.TextToSpeechAsync(textToConvert, voice, defaultVoiceSettings);

                string relativePath = GetRelativeFilepathWithoutExtensionForUnprocessedFile(i);
                SavWav.Save($"{Application.dataPath}/{relativePath}.wav", voiceClip.AudioClip);

                await Task.Delay(300);
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            List<AudioClip> clips = await StartRecording();

            Repaint();
            Debug.Log("Completed recording.");

            return clips;
        }

        private async Task<List<AudioClip>> StartRecording()
        {
            RecorderControllerSettings controllerSettings = CreateInstance<RecorderControllerSettings>();
            recordingController = new RecorderController(controllerSettings);

            AudioRecorderSettings audioRecorder = CreateInstance<AudioRecorderSettings>();
            audioRecorder.name = "Audio Processing Recorder";
            audioRecorder.Enabled = true;

            int frameRate = 30;
            controllerSettings.AddRecorderSettings(audioRecorder);
            controllerSettings.FrameRate = frameRate;
            controllerSettings.ExitPlayMode = false;

            RecorderOptions.VerboseMode = false;

            for (int i = 0; i < voicelineVariations; i++)
            {
                float pitchStep = 0.2f / processingVariations;
                for (int y = 0; y < processingVariations; y++)
                {
                    Debug.Log($"Processing variation {y + 1} of {processingVariations} for {i + 1} of {voicelineVariations} voicelines.");

                    audioSource.pitch = 0.9f + (y * pitchStep);
                    audioSource.clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/{GetRelativeFilepathWithoutExtensionForUnprocessedFile(i)}.wav");
                    audioSource.clip.LoadAudioData();
                    while (audioSource.clip.loadState == AudioDataLoadState.Loading)
                    {
                        await Task.Delay(100); // Wait for a short time before checking again
                    }

                    audioRecorder.OutputFile = GetProcessedFilepath(i, y);
                    controllerSettings.SetRecordModeToFrameInterval(2, Mathf.RoundToInt(audioSource.clip.length + 0.3f) * frameRate);

                    recordingController.PrepareRecording();
                    recordingController.StartRecording();

                    audioSource.Play();

                    while (recordingController.IsRecording())
                    {
                        await Task.Delay(100);
                    }

                    AssetDatabase.ImportAsset($"Assets/{GetRelativeFilepathWithoutExtensionForProcessedFile(i, y)}.wav", ImportAssetOptions.ForceSynchronousImport);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            List<AudioClip> processedClips = new List<AudioClip>();
            for (int i = 0; i < voicelineVariations; i++)
            {
                for (int y = 0; y < processingVariations; y++)
                {
                    processedClips.Add(AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/{GetRelativeFilepathWithoutExtensionForProcessedFile(i, y)}.wav"));
                }
            }

            return processedClips;
        }

        private string GetRelativeFilepathWithoutExtensionForUnprocessedFile(int variation)
        {
            return $"{path}/Unprocessed/{category}/{filename}_{variation.ToString("000")}";
        }

        private string GetProcessedFilepath(int voiceVariation, int processingVariation)
        {
            return $"{Application.dataPath}/{GetRelativeFilepathWithoutExtensionForProcessedFile(voiceVariation, processingVariation)}";
        }

        private string GetRelativeFilepathWithoutExtensionForProcessedFile(int voiceVariation, int processingVariation)
        {
            return $"{path}/{category}/{filename}_{voiceVariation.ToString("000")}_{processingVariation.ToString("000")}";
        }
    }

}
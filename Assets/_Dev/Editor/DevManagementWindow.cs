using PlasticPipe.PlasticProtocol.Messages;
using RogueWave;
using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public class DevManagementWindow : EditorWindow
{
    private const string k_MarketingStageScene = "Assets/_Marketing/Scenes/Stage.unity";
    private const string k_PlaytestScene = "Assets/_Dev/Scenes Dev/Playtest Dev.unity";
    private const string k_LevelDevScene = "Assets/_Dev/Scenes Dev/Level Dev.unity";
    private const string k_MainGameScene = "Assets/_Rogue Wave/Scenes/RogueWave_MainMenu.unity";
    private const string k_EnemyShowcaseScene = "Assets/_Marketing/Scenes/Meet the Enemies.unity";

    [SerializeField]
    private List<Object> lastSelectedItems = new List<Object>();

    [SerializeField]
    private List<Object> accessedFolders = new List<Object>();
    private Vector2 scrollPosition;

    private void OnEnable()
    {
        Selection.selectionChanged += OnSelectionChanged;
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= OnSelectionChanged;
        EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;
    }
    private void OnProjectWindowItemGUI(string guid, Rect selectionRect)
    {
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && selectionRect.Contains(Event.current.mousePosition))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.IsValidFolder(path))
            {
                Object selectedObject = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (!accessedFolders.Contains(selectedObject))
                {
                    accessedFolders.Add(selectedObject);
                    if (accessedFolders.Count > 10)
                    {
                        accessedFolders.RemoveAt(0);
                    }
                }

                EditorUtility.SetDirty(this);
                Repaint();
            }
        }
    }

    private void OnSelectionChanged()
    {
        if (Selection.objects.Length == 0)
        {
            return;
        }

        Object selectedObject = Selection.objects[0];
        string assetPath = AssetDatabase.GetAssetPath(selectedObject);

        if (AssetDatabase.IsValidFolder(assetPath))
        {
            return;
        }

        if (lastSelectedItems.Contains(selectedObject))
        {
            lastSelectedItems.Remove(selectedObject);
        }

        lastSelectedItems.Insert(0, selectedObject);
        
        if (lastSelectedItems.Count > 10)
        {
            lastSelectedItems.RemoveAt(10);
        }

        EditorUtility.SetDirty(this);
        Repaint();
    }

    [MenuItem("Tools/Rogue Wave/Dev Management Window")]
    public static void ShowWindow()
    {
        GetWindow<DevManagementWindow>("Dev Management");
    }

    void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.ExpandHeight(true));
        OnDeveloperGUI();
        Separator();

        OnMarketingGUI();

        OnValidationGUI();
        GUILayout.EndScrollView();
    }

    private static void Separator()
    {
        Color initialColor = GUI.color;
        GUI.color = Color.white;
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(5));
        GUI.color = initialColor;
    }

    private void OnSelectedItemsGUI()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        for (int i = accessedFolders.Count - 1; i >= 0; i--)
        {
            Object folder = accessedFolders[i];
            if (folder != null)
            {
                EditorGUILayout.ObjectField(folder, typeof(Object), true);
            }
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        for (int i = 0; i < lastSelectedItems.Count; i++)
        {
            Object item = lastSelectedItems[i];
            if (item != null)
            {
                EditorGUILayout.ObjectField(item, typeof(Object), true);
            }
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void OnDeveloperGUI()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        if (GUILayout.Button("Launch Playtest"))
        {
            LoadScene(new string[] { k_PlaytestScene });
            StartApplication();
        }

        if (GUILayout.Button("Launch Main Scene"))
        {
            LoadScene(new string[] { k_MainGameScene });
            StartApplication();
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        if (GUILayout.Button("Open Playtest Scene"))
        {
            LoadScene(new string[] { k_PlaytestScene });
        }

        if (GUILayout.Button("Open Dev Scenes Folder"))
        {
            var scenesFolder = AssetDatabase.LoadAssetAtPath<Object>(k_LevelDevScene);
            EditorGUIUtility.PingObject(scenesFolder);
            Selection.activeObject = scenesFolder;
        }

        if (GUILayout.Button("Open Prod Scenes Folder"))
        {
            var scenesFolder = AssetDatabase.LoadAssetAtPath<Object>(k_MainGameScene);
            EditorGUIUtility.PingObject(scenesFolder);
            Selection.activeObject = scenesFolder;
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        OnSelectedItemsGUI();
    }


    private void OnMarketingGUI()
    {
        GUILayout.Label("Marketing", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        if (GUILayout.Button("Launch Enemy Showcase"))
        {
            LoadScene(new string[] { k_MarketingStageScene, k_EnemyShowcaseScene });
            StartApplication();
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        if (GUILayout.Button("Open Marketing Scenes"))
        {
            LoadScene(new string[] { k_MarketingStageScene });
            var marketingScenesFolder = AssetDatabase.LoadAssetAtPath<Object>(k_MarketingStageScene);
            EditorGUIUtility.PingObject(marketingScenesFolder);
            Selection.activeObject = marketingScenesFolder;
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

    }

    private void OnValidationGUI()
    {
        GUILayout.Label("Validation", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        if (GUILayout.Button("Validate Everything"))
        {
            // Clear the console
            Type logEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            MethodInfo clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            clearMethod.Invoke(null, null);

            bool isValid = ValidateComponents<BasicEnemyController>();
            if (isValid)
            {
                Debug.Log("All Enemy tests passed.");
            }

            isValid = ValidateComponents<RWPooledExplosion>();
            if (isValid)
            {
                Debug.Log("All Pooled Explosion tests passed.");
            }
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private static bool ValidateComponents<T>() where T : Component
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Dev", "Assets/_Marketing", "Assets/_Rogue Wave" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            T testComponent = prefab.GetComponent<T>();
            if (testComponent != null)
            {
                Type type = testComponent.GetType();
                MethodInfo methodInfo = type.GetMethod("IsValid", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (methodInfo != null)
                {
                    object[] parameters = new object[] { null, null };
                    bool result = (bool)methodInfo.Invoke(testComponent, parameters);

                    string message = (string)parameters[0];
                    Component component = (Component)parameters[1];

                    if (!result)
                    {
                        Debug.LogError($"`{component.name}` is invalid because: {message}", component);
                        return false;
                    }
                }
                else
                {
                    Debug.LogError("IsValid method not found.");
                    return false;
                }
            }
        }

        return true;
    }


    void LoadScene(string[] scenePaths)
    {
        if (EditorSceneManager.GetActiveScene().isDirty)
        {
            if (EditorUtility.DisplayDialog("Unsaved Changes", "The current scene has unsaved changes. Do you want to save them?", "Save", "Don't Save"))
            {
                EditorSceneManager.SaveOpenScenes();
            }
        }

        bool isFirstScene = true;
        foreach (string scenePath in scenePaths)
        {
            OpenSceneMode mode = isFirstScene ? OpenSceneMode.Single : OpenSceneMode.Additive;
            EditorSceneManager.OpenScene(scenePath, mode);
        }
    }

    void StartApplication()
    {
        AssetDatabase.Refresh();
        EditorUtility.RequestScriptReload();

        EditorApplication.isPlaying = true;
    }



    /// <summary>
    /// Detects missing scripts in the project folders, logging the results to the console..
    /// </summary>
    [MenuItem("Tools/Wizards Code/Detect Missing Scripts")]
    public static void DetectMissingScripts()
    {
        string[] folders = new string[] { "Assets/_Dev", "Assets/_Marketing", "Assets/_Rogue Wave" };
        List<GameObject> invalidObjects = DetectMissingScripts(folders);
    }

    /// <summary>
    /// Detects missing scripts in the specified folders and logs warnings for each GameObject with missing scripts.
    /// </summary>
    /// <param name="folders">An array of folder paths to search for prefabs.</param>
    /// <returns>A list of GameObjects that have missing scripts.</returns>
    public static List<GameObject> DetectMissingScripts(string[] folders)
    {
        int step = 0;;

        EditorUtility.DisplayProgressBar("Detecting Missing Scripts", "Searching for prefabs to validate", 0.0f);

        step++;
        string[] guids = AssetDatabase.FindAssets("t:Prefab", folders);

        int numOfSteps = guids.Length;
        EditorUtility.DisplayProgressBar("Detecting Missing Scripts", $"Found {guids.Length} Game Objects to validate", step / numOfSteps);
        
        List<GameObject> invalidObjects = new List<GameObject>();
        List<GameObject> validOgbjects = new List<GameObject>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            Component[] components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    invalidObjects.Add(go);
                    Debug.LogError($"Missing script found in GameObject: {go.name}", go);
                }
                else
                {
                    validOgbjects.Add(go);
                }
            }

            step++;
            EditorUtility.DisplayProgressBar("Detecting Missing Scripts", $"{invalidObjects.Count} of {guids.Length} are invalid", step / numOfSteps);
        }


        if (invalidObjects.Count == 0)
        {
            EditorUtility.DisplayDialog("Missing Scripts Detection", $"No missing scripts found in {validOgbjects.Count} Game Objects.", "OK");
            Debug.Log("No missing scripts found.");
        } else
        {
            EditorUtility.DisplayDialog("Missing Scripts Detection", $"Found {invalidObjects.Count} of {validOgbjects.Count} GameObjects have missing scripts. See details in the console", "OK");
        }
        EditorUtility.ClearProgressBar();
        


        return invalidObjects;
    }
}

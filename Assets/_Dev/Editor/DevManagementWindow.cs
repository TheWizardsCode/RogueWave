using RogueWave;
using System;
using System.Collections.Generic;
using System.Reflection;
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
            var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke(null, null);

            bool isValid = isValid = ValidateComponents<BasicEnemyController>();
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
}

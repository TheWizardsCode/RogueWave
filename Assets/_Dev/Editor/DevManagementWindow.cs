using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

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
        OnDeveloperGUI();
        Separator();

        OnMarketingGUI();
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

        GUILayout.BeginVertical(GUILayout.Width(position.width * 0.5f));
        for (int i = accessedFolders.Count - 1; i >= 0; i--)
        {
            Object folder = accessedFolders[i];
            if (folder != null)
            {
                EditorGUILayout.ObjectField(folder, typeof(Object), true);
            }
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUILayout.Width(position.width * 0.5f));
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

        GUILayout.BeginVertical(GUILayout.Width(position.width * 0.5f));
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

        GUILayout.BeginVertical(GUILayout.Width(position.width * 0.5f));
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

        GUILayout.BeginVertical(GUILayout.Width(position.width * 0.5f));
        if (GUILayout.Button("Launch Enemy Showcase"))
        {
            LoadScene(new string[] { k_MarketingStageScene, k_EnemyShowcaseScene });
            StartApplication();
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUILayout.Width(position.width * 0.5f));
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

    void LoadScene(string[] scenePaths)
    {
        EditorSceneManager.SaveOpenScenes();
        bool isFirstScene = true;
        foreach (string scenePath in scenePaths)
        {
            OpenSceneMode mode = isFirstScene ? OpenSceneMode.Single : OpenSceneMode.Additive;
            EditorSceneManager.OpenScene(scenePath, mode);
        }
    }

    void StartApplication()
    {
        EditorApplication.isPlaying = true;
    }
}

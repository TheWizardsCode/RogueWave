using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class DevManagementWindow : EditorWindow
{
    private const string k_MarketingStageScene = "Assets/_Marketing/Scenes/Stage.unity";
    private const string k_PlaytestScene = "Assets/_Dev/Scenes Dev/Playtest Dev.unity";
    private const string k_MainGameScene = "Assets/_Rogue Wave/Scenes/RogueWave_MainMenu.unity";
    private const string k_EnemyShowcaseScene = "Assets/_Marketing/Scenes/Meet the Enemies.unity";

    [MenuItem("Tools/Rogue Wave/Dev Management Window")]
    public static void ShowWindow()
    {
        GetWindow<DevManagementWindow>("Dev Management");
    }

    void OnGUI()
    {
        OnDeveloperGUI();
        OnMarketingGUI();
    }

    private void OnDeveloperGUI()
    {
        GUILayout.Label("Developer", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Launch Playtest"))
        {
            LoadScene(new string[] { k_PlaytestScene });
            StartApplication();
        }

        if (GUILayout.Button("Launch Main"))
        {
            LoadScene(new string[] { k_MainGameScene });
            StartApplication();
        }
        GUILayout.EndHorizontal();
    }

    private void OnMarketingGUI()
    {
        GUILayout.Label("Marketing", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Launch Enemy Showcase"))
        {
            LoadScene(new string[] { k_MarketingStageScene, k_EnemyShowcaseScene });
            StartApplication();
        }

        if (GUILayout.Button("Open Marketing Scenes"))
        {
            LoadScene(new string[] { k_MarketingStageScene });
            var marketingScenesFolder = AssetDatabase.LoadAssetAtPath<Object>(k_MarketingStageScene);
            EditorGUIUtility.PingObject(marketingScenesFolder);
            Selection.activeObject = marketingScenesFolder;
        }
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

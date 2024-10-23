using UnityEngine;
using UnityEditor;
using RogueWave;
using System;
using UnityEditorInternal;
using System.Linq;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Diagnostics.Eventing.Reader;

namespace WizardsCode.RogueWave_Dev.Editor
{
    public class LevelWaveGeneratorWindow : EditorWindow
    {
        private static readonly AnimationCurve DefaultFlow = new AnimationCurve(
            new Keyframe(0, 0),
            new Keyframe(0.1f, 0.2f),
            new Keyframe(0.2f, 0.1f),
            new Keyframe(0.3f, 0.4f),
            new Keyframe(0.4f, 0.2f),
            new Keyframe(0.5f, 0.6f),
            new Keyframe(0.6f, 0.3f),
            new Keyframe(0.7f, 0.7f),
            new Keyframe(0.8f, 0.3f),
            new Keyframe(0.9f, 0.9f),
            new Keyframe(0.95f, 0.3f),
            new Keyframe(1, 1)
        );

        public TileDefinition[] PlayerSpawnTiles;
        public TileDefinition[] EnemySpawnTiles;
        public TileDefinition[] OtherTiles;

        private AnimationCurve flow = DefaultFlow;
        private int levelNumber = 9;
        private Vector2Int mapSize = new Vector2Int(10, 10);
        private int numberOfWaves = 5;
        private int startingChallengeRating = 500;
        private int peakChallengeRating = 1000;
        private TileDefinition[] prePlacedTiles = new TileDefinition[0];
        private BasicEnemyController[] allEnemies;
        private Dictionary<BasicEnemyController, float> earliestWavePercentage = new Dictionary<BasicEnemyController, float>();

        bool m_LevelExists = false;
        private bool LevelExists => m_LevelExists;

        [MenuItem("Tools/Rogue Wave/Level and Wave Generator")]
        public static void ShowWindow()
        {
            GetWindow<LevelWaveGeneratorWindow>("Level and Wave Generator");
        }

        private void OnEnable()
        {
            UpdateLevelExists();
            RefreshTileDefinitions();
            RefreshAllEnemies();
        }

        private Vector2 scrollPosition;
        private int overwriteChoice;

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height - 100));
            {
                DrawFlowField();
                DrawLevelSettings();
                DrawPrePlacedTiles();
                DrawEnemyAppearances();
            }
            EditorGUILayout.EndScrollView();

            DrawButtons();
        }

        private void DrawFlowField()
        {
            EditorGUILayout.BeginVertical("box");
            flow = EditorGUILayout.CurveField("Flow", flow);
            if (GUILayout.Button("Default Curve"))
            {
                flow = DefaultFlow;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawLevelSettings()
        {
            EditorGUILayout.BeginVertical("box");
            
            int originalLevelNumber = levelNumber;
            levelNumber = EditorGUILayout.IntField("Level Number", levelNumber);
            if (levelNumber != originalLevelNumber)
            {
                UpdateLevelExists();
            }
            if (LevelExists && GUILayout.Button("Load config from existing level"))
            {
                LoadGenerationSettingsForLevel();
            }

            mapSize = EditorGUILayout.Vector2IntField("Map Size", mapSize);
            numberOfWaves = EditorGUILayout.IntField("Number of Waves", numberOfWaves);
            startingChallengeRating = EditorGUILayout.IntField("Base Challenge Rating", startingChallengeRating);
            peakChallengeRating = EditorGUILayout.IntField("Peak Challenge Rating", peakChallengeRating);
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        bool UpdateLevelExists()
        {
            WfcDefinition wfcDefinition = AssetDatabase.LoadAssetAtPath<WfcDefinition>(GetWfcDefinitionPath());
            m_LevelExists = wfcDefinition != null;
            return m_LevelExists;
        }

        WfcDefinition LoadGenerationSettingsForLevel()
        {
            WfcDefinition wfcDefinition = AssetDatabase.LoadAssetAtPath<WfcDefinition>(GetWfcDefinitionPath());
            LevelWaveGenerationConfiguration config = wfcDefinition.levelWaveGenerationConfiguration;
            flow = config.flow;
            mapSize = config.MapSize;
            numberOfWaves = config.numberOfWaves;
            startingChallengeRating = config.startingChallengeRating;
            peakChallengeRating = config.peakChallengeRating;
            prePlacedTiles = config.prePlacedTiles;
            earliestWavePercentage = config.enemies.ToDictionary(enemy => enemy, enemy => config.earliestWavePercentage[Array.IndexOf(config.enemies, enemy)]);

            return wfcDefinition;
        }

        private void DrawPrePlacedTiles()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Pre-Placed Tiles");
            int newSize = EditorGUILayout.IntField("Size", prePlacedTiles != null ? prePlacedTiles.Length : 0);
            if (newSize != (prePlacedTiles != null ? prePlacedTiles.Length : 0))
            {
                Array.Resize(ref prePlacedTiles, newSize);
            }

            if (prePlacedTiles != null && prePlacedTiles.Length > 0)
            {
                for (int i = 0; i < prePlacedTiles.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    string name = "Select Option";
                    string tooltip = string.Empty;
                    if (prePlacedTiles[i] != null)
                    {
                        tooltip = $"{prePlacedTiles[i].Description} Placement between {prePlacedTiles[i].constraints.bottomLeftBoundary} and {prePlacedTiles[i].constraints.topRightBoundary}.";
                        name = prePlacedTiles[i].DisplayName;
                    }

                    prePlacedTiles[i] = (TileDefinition)EditorGUILayout.ObjectField(new GUIContent(name, tooltip), prePlacedTiles[i], typeof(TileDefinition), false);
                    if (i == 0 && GUILayout.Button("Options"))
                    {
                        ShowTileSelectionMenu(PlayerSpawnTiles, i);
                    }
                    else if (i == 1 && GUILayout.Button("Options"))
                    {
                        ShowTileSelectionMenu(EnemySpawnTiles, i);
                    }
                    else if (i >= 2 && GUILayout.Button("Options"))
                    {
                        ShowTileSelectionMenu(OtherTiles, i);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawEnemyAppearances()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Enemy Appearances");
            if (allEnemies != null)
            {
                foreach (BasicEnemyController enemy in allEnemies)
                {
                    BasicEnemyController currentEnemy = enemy;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent($"{enemy.name} (CR {enemy.challengeRating})", "The name of the enemy and a Challenge Rating that gives an indication of how tough the enemy is."));

                    int earliestWave = numberOfWaves - Mathf.RoundToInt(numberOfWaves * (1 - earliestWavePercentage[currentEnemy]));
                    earliestWavePercentage[currentEnemy] = EditorGUILayout.Slider(new GUIContent($"Earliest Wave {earliestWave}", "A normalized % of progress through the levels that the player will make before they are likely to encounter this enemy."), earliestWavePercentage[currentEnemy], 0f, 1f);
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawButtons()
        {
            if (GUILayout.Button("Generate", GUILayout.Height(40)))
            {
                Generate();
            }

            if (GUILayout.Button("Refresh Tile Definitions"))
            {
                RefreshTileDefinitions();
            }

            if (GUILayout.Button("Refresh Enemies"))
            {
                RefreshAllEnemies();
            }
        }

        private void ShowTileSelectionMenu(TileDefinition[] tiles, int index)
        {
            GenericMenu menu = new GenericMenu();
            foreach (TileDefinition tile in tiles)
            {
                menu.AddItem(new GUIContent(tile.name), false, () => { prePlacedTiles[index] = tile; });
            }
            menu.ShowAsContext();
        }

        private void Generate()
        {
            if (!ValidateInputs())
            {
                return;
            }

            overwriteChoice = 0;

            // Create the base WFC Definition
            if (LevelExists)
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "File Exists",
                    $"WfcDefinition for Level {levelNumber} already exists. Do you want to overwrite it?",
                    "Yes",
                    "No"
                );
                if (!overwrite)
                {
                    return;
                }
            }
            
            WfcDefinition wfcDefinition = ScriptableObject.CreateInstance<WfcDefinition>();
            
            wfcDefinition.name = $"Level {levelNumber}";
            wfcDefinition.prePlacedTiles = prePlacedTiles;
            wfcDefinition.GenerateNewWaves = true;
            wfcDefinition.MaxAlive = 100 + 10 * levelNumber;
            wfcDefinition.MapSize = mapSize;

            LevelWaveGenerationConfiguration config = new LevelWaveGenerationConfiguration();
            config.levelNumber = levelNumber;
            config.flow = flow;
            config.MapSize = mapSize;
            config.numberOfWaves = numberOfWaves;
            config.startingChallengeRating = startingChallengeRating;
            config.peakChallengeRating = peakChallengeRating;
            config.prePlacedTiles = prePlacedTiles;
            config.enemies = earliestWavePercentage.Keys.ToArray();
            config.earliestWavePercentage = earliestWavePercentage.Values.ToArray();
            wfcDefinition.levelWaveGenerationConfiguration = config;

            // Populate the WfcDefinition with the specified number of waves
            wfcDefinition.Waves = new WaveDefinition[numberOfWaves];
            for (int i = 0; i < numberOfWaves; i++)
            {
                WaveDefinition waveDefinition = AssetDatabase.LoadAssetAtPath<WaveDefinition>(GetWaveDefinitionPath(i));
                bool newWave = waveDefinition == null;

                // Check if the WaveDefinition asset already exists if it does get permission to overwrite it, if it doesn't create it
                if (newWave)
                {
                    waveDefinition = CreateInstance<WaveDefinition>();
                } 
                else
                {
                    if (overwriteChoice == 0)
                    {
                        overwriteChoice = EditorUtility.DisplayDialogComplex(
                            "File Exists",
                            $"WaveDefinition for Level {levelNumber} Wave {i + 1} already exists. What would you like to do?",
                            $"Yes to wave {i + 1}",
                            "Yes to All",
                            "No"
                        );
                    }

                    if (overwriteChoice == 3)
                    {
                        return;
                    }
                }

                waveDefinition.name = $"Level {levelNumber} Wave {i + 1} Definition";
                
                int targetChallengeRating = Mathf.RoundToInt(startingChallengeRating + (peakChallengeRating - startingChallengeRating) * flow.Evaluate((float)(i + 1) / numberOfWaves));
                List<BasicEnemyController> availableEnemies = new List<BasicEnemyController>();
                foreach (BasicEnemyController enemy in allEnemies)
                {
                    if (earliestWavePercentage[enemy] > 0 && numberOfWaves - Mathf.RoundToInt(numberOfWaves * (1 - earliestWavePercentage[enemy])) <= i)
                    {
                        availableEnemies.Add(enemy);
                    }
                }

                waveDefinition.GenerateWave(targetChallengeRating, true, availableEnemies.ToArray());

                wfcDefinition.Waves[i] = waveDefinition;

                if (newWave)
                {
                    AssetDatabase.CreateAsset(waveDefinition, GetWaveDefinitionPath(i));
                }
                else
                {
                    EditorUtility.SetDirty(waveDefinition);
                }
            }

            // Save the ScriptableObject as an asset
            AssetDatabase.CreateAsset(wfcDefinition, GetWfcDefinitionPath());
            AssetDatabase.SaveAssets();

            Selection.activeObject = wfcDefinition;
            EditorGUIUtility.PingObject(wfcDefinition);
        }

        private bool ValidateInputs()
        {
            // Validate input values
            if (levelNumber <= 0)
            {
                Debug.LogError("Level number must be greater than 0.");
                return false;
            }
            if (numberOfWaves <= 0)
            {
                Debug.LogError("Number of waves must be greater than 0.");
                return false;
            }
            if (startingChallengeRating < 0 || peakChallengeRating < 0)
            {
                Debug.LogError("Challenge ratings must be non-negative.");
                return false;
            }
            if (startingChallengeRating >= peakChallengeRating)
            {
                Debug.LogError("Base challenge rating must be less than or equal to peak challenge rating.");
                return false;
            }

            return true;
        }

        private string GetWaveDefinitionPath(int waveNumber)
        {
            return $"Assets/_Rogue Wave/Resources/Levels/Waves/Level {levelNumber} Wave {waveNumber + 1} Definition.asset";
        }

        private string GetWfcDefinitionPath()
        {
            return $"Assets/_Rogue Wave/Resources/Levels/Level Definitions/Level {levelNumber}.asset";
        }

        private void RefreshTileDefinitions()
        {
            TileDefinition[] allTiles = Resources.LoadAll<TileDefinition>("");

            allTiles = allTiles.Where(tile => AssetDatabase.GetAssetPath(tile).Contains("_Rogue Wave/Resources")).ToArray();

            PlayerSpawnTiles = allTiles.Where(tile => tile.name.Contains("Player Spawn"))
                                       .OrderBy(tile => tile.name)
                                       .ToArray();
            EnemySpawnTiles = allTiles.Where(tile => tile.name.Contains("Enemy Spawner"))
                                      .OrderBy(tile => tile.name)
                                      .ToArray();
            OtherTiles = allTiles.Where(tile => !tile.name.Contains("Player Spawn") && !tile.name.Contains("Enemy Spawner"))
                                 .OrderBy(tile => tile.name)
                                 .ToArray();
        }

        private void RefreshAllEnemies()
        {
            // Get all enemy prefabs
            allEnemies = Resources.LoadAll<BasicEnemyController>("");
            allEnemies = allEnemies
                .Where(enemy => AssetDatabase.GetAssetPath(enemy).Contains("_Rogue Wave/Resources"))
                .Where(enemy => enemy.isAvailableToWaveDefinitions)
                .OrderBy(enemy => enemy.challengeRating)
                .ToArray();

            // Calculate how early they should appear based on their challenge rating
            foreach (BasicEnemyController enemy in allEnemies)
            {
                BasicEnemyController currentEnemy = enemy;
                earliestWavePercentage[currentEnemy] = Mathf.Clamp01(((float)currentEnemy.challengeRating / peakChallengeRating) * Random.Range(0.8f, 1.2f));
            }

            // Normalize the earliest wave percentages so that they are between 0.01 and 1
            float min = earliestWavePercentage.Values.Min();
            float max = earliestWavePercentage.Values.Max();
            foreach (BasicEnemyController enemy in allEnemies)
            {
                earliestWavePercentage[enemy] = Mathf.Clamp01(((earliestWavePercentage[enemy] - min) / (max - min)) + 0.01f);
                // round to 2 decimal places
                earliestWavePercentage[enemy] = Mathf.Round(earliestWavePercentage[enemy] * 100) / 100;
            }
        }
    }
}

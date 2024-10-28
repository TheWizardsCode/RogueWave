using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace RogueWave.Editor
{
    public class EnemyDataWindow : EditorWindow
    {
        private string filter;
        Vector2 scrollPosition;
        private BasicEnemyController[] enemies;
        private bool enemyEdited;

        // Create a menu option to display a summary of all enemy data
        [MenuItem("Tools/Rogue Wave/Enemy Data", priority = 100)]
        static void Init()
        {
            EnemyDataWindow window = (EnemyDataWindow)GetWindow(typeof(EnemyDataWindow));
            window.Show();
        }

        private void OnEnable()
        {
            UpdateEnemyList();
        }

        private void UpdateEnemyList()
        {
            enemies = Resources.FindObjectsOfTypeAll<BasicEnemyController>();
            enemies = enemies.OrderBy(x => x.challengeRating).ToArray();
        }

        void OnGUI()
        {
            filter = EditorGUILayout.TextField("Filter", filter, GUILayout.Width(600));
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Meta Data", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(220));
            EditorGUILayout.LabelField("Senses", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(45));
            EditorGUILayout.LabelField("Movement", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("CR", GUILayout.Width(20));
            EditorGUILayout.LabelField("Display name", GUILayout.Width(200));
            // Senses
            EditorGUILayout.LabelField(new GUIContent("Req", "Whether the enemy must sense the player to take action."), GUILayout.Width(15));
            EditorGUILayout.LabelField(new GUIContent("Dist", "The distance that the enemy can see."), GUILayout.Width(30));
            // Movement
            EditorGUILayout.LabelField(new GUIContent("Min", "Min speed of the enemy."), GUILayout.Width(30));
            EditorGUILayout.LabelField(new GUIContent("Max", "Max speed of the enemy."), GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();
            
            foreach (BasicEnemyController enemy in enemies)
            {
                if (!string.IsNullOrEmpty(filter) && !enemy.name.Contains(filter))
                {
                    continue;
                }

                Color defaultColor = GUI.color;
                string tooltipMessage = enemy.description;
                if (!enemy.IsValid(out string errorMessage, out Component component))
                {
                    tooltipMessage = errorMessage;
                    GUI.color = Color.red;
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();

                // Meta Data
                EditorGUILayout.LabelField(enemy.challengeRating.ToString(), GUILayout.Width(20));
                if (GUILayout.Button(new GUIContent(enemy.name, tooltipMessage), GUILayout.Width(200)))
                {
                    EditorGUIUtility.PingObject(enemy);
                    Selection.activeObject = enemy;
                }

                // Senses
                enemy.requireLineOfSight = EditorGUILayout.Toggle(enemy.requireLineOfSight, GUILayout.Width(15));
                if (enemy.requireLineOfSight)
                {
                    enemy.viewDistance = EditorGUILayout.FloatField(enemy.viewDistance, GUILayout.Width(30));
                }
                else
                {
                    EditorGUILayout.LabelField("-", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(30));
                }

                // Movement
                BasicMovementController movementController = enemy.GetComponent<BasicMovementController>();
                if (movementController != null)
                {
                    movementController.minSpeed = EditorGUILayout.FloatField(movementController.minSpeed, GUILayout.Width(30));
                    movementController.maxSpeed = EditorGUILayout.FloatField(movementController.maxSpeed, GUILayout.Width(30));
                }
                else
                {
                    EditorGUILayout.LabelField("-", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(60));
                }

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(enemy);
                    enemyEdited = true;
                }
                EditorGUILayout.EndHorizontal();

                GUI.color = defaultColor;
            }
                       
            EditorGUILayout.EndScrollView();

            if (enemyEdited
                && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Tab))
            {
                AssetDatabase.SaveAssets();
                UpdateEnemyList();
                enemyEdited = false;
            }
        }
    }
}
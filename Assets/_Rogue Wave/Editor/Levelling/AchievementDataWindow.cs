using RogueWave.GameStats;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RogueWave.Editor
{
    public class AchievementDataWindow : EditorWindow
    {
        private string filter;
        Vector2 scrollPosition;
        private Achievement[] achievements;
        private bool achievementEdited;

        [MenuItem("Tools/Rogue Wave/Achievement Data", priority = 109)]
        static void Init()
        {
            AchievementDataWindow window = (AchievementDataWindow)GetWindow(typeof(AchievementDataWindow));
            window.Show();
        }

        private void OnEnable()
        {
            UpdateAchievementList();
        }

        private void OnDisable()
        {
            AssetDatabase.SaveAssets();
        }

        private void UpdateAchievementList()
        {
            achievements = Resources.LoadAll<Achievement>("");
            Array.Sort(achievements, (x, y) =>
            {
                int statComparison = x.stat.CompareTo(y.stat);
                if (statComparison == 0)
                {
                    return x.targetValue.CompareTo(y.targetValue);
                }
                if (statComparison == 0)
                {
                    return x.displayName.CompareTo(y.displayName);
                }
                return statComparison;
            });

        }

        enum Status { Any, Invalid, Valid }
        Status filterStatus = Status.Any;

        void OnGUI()
        {
            List<Achievement> filteredAchievements = FilterGUI();
            int demoLockedCount = filteredAchievements.Count(a => a.isDemoLocked);
            int invalidCount = filteredAchievements.Count(a => !a.Validate(out string statusMsg));

            EditorGUILayout.BeginHorizontal();
            // add a button to create a new achievement
            if (GUILayout.Button("Create New Achievement"))
            {
                Achievement newAchievement = ScriptableObject.CreateInstance<Achievement>();
                newAchievement.name = "New Achievement";
                AssetDatabase.CreateAsset(newAchievement, "Assets/_Rogue Wave/Resources/Achievements/New Achievement.asset");
                AssetDatabase.SaveAssets();
                UpdateAchievementList();

                EditorGUIUtility.PingObject(newAchievement);
                Selection.activeObject = newAchievement;
            }
            EditorGUILayout.LabelField($"Showing {filteredAchievements.Count} of {achievements.Length} (Demo Locked: {demoLockedCount}, of which {invalidCount} are invalid)");
            EditorGUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Category", GUILayout.Width(100));
            EditorGUILayout.LabelField("Display name", GUILayout.Width(200));
            EditorGUILayout.LabelField("Stat", GUILayout.Width(200));
            EditorGUILayout.LabelField("Value", GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();

            foreach (Achievement achievement in filteredAchievements)
            {
                if (achievement.Validate(out string statusMsg))
                {
                    if (achievement.isDemoLocked)
                    {
                        GUI.backgroundColor = Color.grey;
                        statusMsg += "\nValid, demo locked achievement. Click to select it.";
                    }
                    else
                    {
                        GUI.backgroundColor = Color.green;
                        statusMsg = "\nValid achievement, available in the demo. Click to select it.";
                    }
                }
                else
                {
                    if (achievement.isDemoLocked)
                    {
                        GUI.backgroundColor = Color.yellow;
                        statusMsg += "\nDemo locked achievement. Click to select it.";
                    }
                    else
                    {
                        GUI.backgroundColor = Color.red;
                        statusMsg += "\nClick to select the achievement.";
                    }
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();

                achievement.category = (Achievement.Category)EditorGUILayout.EnumPopup(achievement.category, GUILayout.Width(100));

                string displayName = $"{achievement.displayName}";
                GUIStyle richTextStyle = new GUIStyle(GUI.skin.button) { richText = true };
                if (GUILayout.Button(new GUIContent(displayName, $"{achievement.description}\n\n{statusMsg}"), richTextStyle, GUILayout.Width(200)))
                {
                    EditorGUIUtility.PingObject(achievement);
                    Selection.activeObject = achievement;
                }

                achievement.stat = (IntGameStat)EditorGUILayout.ObjectField(achievement.stat, typeof(IntGameStat), false, GUILayout.Width(200));
                achievement.targetValue = EditorGUILayout.FloatField(achievement.targetValue, GUILayout.Width(30));

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(achievement);
                    achievementEdited = true;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (achievementEdited
                && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Tab))
            {
                AssetDatabase.SaveAssets();
                UpdateAchievementList();
                achievementEdited = false;
            }
        }

        private List<Achievement> FilterGUI()
        {
            EditorGUILayout.BeginHorizontal();
            filter = EditorGUILayout.TextField("Filter", filter, GUILayout.Width(600));
            filterStatus = (Status)EditorGUILayout.EnumPopup(filterStatus);
            EditorGUILayout.EndHorizontal();

            List<Achievement> filteredAchievements = new List<Achievement>();
            foreach (Achievement achievement in achievements)
            {
                bool isValid = achievement.Validate(out string statusMsg);
                if ((filterStatus == Status.Invalid && isValid) || (filterStatus == Status.Valid && !isValid))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(filter) && !achievement.name.ToLower().Contains(filter.ToLower()))
                {
                    continue;
                }

                filteredAchievements.Add(achievement);
            }

            return filteredAchievements;
        }
    }
}
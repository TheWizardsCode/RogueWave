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
                int categoryComparison = x.category.CompareTo(y.category);
                if (categoryComparison != 0)
                {
                    return categoryComparison;
                }

                int statComparison = 0;
                if (x.stat != null && y.stat != null)
                {
                    statComparison = x.stat.CompareTo(y.stat);
                }
                else if (x.stat == null && y.stat != null)
                {
                    statComparison = -1;
                }
                else if (x.stat != null && y.stat == null)
                {
                    statComparison = 1;
                }

                if (statComparison != 0)
                {
                    return statComparison;
                }

                int targetValueComparison = x.targetValue.CompareTo(y.targetValue);
                if (targetValueComparison != 0)
                {
                    return targetValueComparison;
                }

                return x.displayName.CompareTo(y.displayName);
            });
        }



        enum Status { Any, Invalid, Valid }
        Status filterStatus = Status.Any;

        void OnGUI()
        {
            List<Achievement> AllFilteredAchievements = FilterGUI();
            List<Achievement> notDemoLockedAndInvalid = AllFilteredAchievements.Where(a => !a.isDemoLocked && !a.Validate(out string _)).ToList();
            List<Achievement> demoLockedAndInvalid = AllFilteredAchievements.Where(a => a.isDemoLocked && !a.Validate(out string _)).ToList();
            List<Achievement> notDemoLockedAndValid = AllFilteredAchievements.Where(a => !a.isDemoLocked && a.Validate(out string _)).ToList();
            List<Achievement> demoLockedAndValid = AllFilteredAchievements.Where(a => a.isDemoLocked && a.Validate(out string _)).ToList();

            HeadingGUI(AllFilteredAchievements);

            EditorGUILayout.Space();
            {
                EditorGUILayout.LabelField("Not Demo Locked and Invalid (Release Blockers)", EditorStyles.boldLabel);
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                AchievementList(notDemoLockedAndInvalid);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Demo Locked and Invalid Achievements (Important)", EditorStyles.boldLabel);
                AchievementList(demoLockedAndInvalid);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Valid Achievements in the Demo (All Good)", EditorStyles.boldLabel);
                AchievementList(notDemoLockedAndValid);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Valid Achievements not in the Demo (All Good)", EditorStyles.boldLabel);
                AchievementList(demoLockedAndValid);
            }
            EditorGUILayout.EndScrollView();

            if (achievementEdited
                && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Tab))
            {
                AssetDatabase.SaveAssets();
                UpdateAchievementList();
                achievementEdited = false;
            }

            // Add right-click context menu
            if (Event.current.type == EventType.ContextClick)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Refresh Data"), false, UpdateAchievementList);
                menu.ShowAsContext();
                Event.current.Use();
            }
        }

        private void AchievementList(List<Achievement> AllFilteredAchievements)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Category", GUILayout.Width(100));
            EditorGUILayout.LabelField("Display name", GUILayout.Width(200));
            EditorGUILayout.LabelField("Stat", GUILayout.Width(200));
            EditorGUILayout.LabelField("Value", GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            foreach (Achievement achievement in AllFilteredAchievements)
            {
                bool isValid = achievement.Validate(out string statusMsg);
                if (isValid)
                {
                    if (achievement.isDemoLocked)
                    {
                        GUI.backgroundColor = Color.grey;
                    }
                    else
                    {
                        GUI.backgroundColor = Color.green;
                    }
                }
                else
                {
                    if (achievement.isDemoLocked)
                    {
                        GUI.backgroundColor = Color.yellow;
                    }
                    else
                    {
                        GUI.backgroundColor = Color.red;
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
                achievement.targetValue = EditorGUILayout.FloatField(achievement.targetValue, GUILayout.Width(50));

                EditorGUILayout.LabelField(statusMsg);
                
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(achievement);
                    achievementEdited = true;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void HeadingGUI(List<Achievement> shownAchievements)
        {
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
            EditorGUILayout.LabelField($"Showing {shownAchievements.Count} of {this.achievements.Length}.");
            EditorGUILayout.EndHorizontal();
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
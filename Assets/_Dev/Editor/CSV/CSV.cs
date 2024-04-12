using RogueWave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace WizardsCode.csv
{
    public class CSV
    {
        private static string relativeDataDirectoryPath = "_Dev/Editor/CSV/Data";

        private static string GetFilePath(string type, string fileName)
        {
            return $"Assets/{relativeDataDirectoryPath}/{type}/{fileName}.csv";
        }

        [MenuItem("Tools/Rogue Wave/Data/Export All Recipes to CSV", priority = 100)]
        static void ExportRecipeCSV()
        {
            string dataType = "Recipes";

            List<AbstractRecipe> recipes = Resources.LoadAll<AbstractRecipe>(dataType).ToList();
            List<Type> types = recipes.Select(r => r.GetType()).Distinct().ToList();

            foreach (Type type in types)
            {
                // Get all recipes of this type
                ScriptableObject[] recipesOfType = recipes.Where(r => r.GetType() == type).ToArray();

                if (recipes.Count > 0)
                {
                    ExportToCSV(recipesOfType, dataType, type.Name);

                }
            }
        }

        [MenuItem("Tools/Rogue Wave/Data/Destructive/Import All Recipes from CSV", priority = 200)]
        static void ImportRecipeCSV()
        {
            string dataType = "Recipes";
            string[] csvFiles = Directory.GetFiles(Path.GetFullPath($"{Application.dataPath}/{relativeDataDirectoryPath}/{dataType}"), "*.csv");

            foreach (string file in csvFiles)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                Assembly assembly = Assembly.GetAssembly(typeof(IRecipe));
                Type type = assembly.GetType($"RogueWave.{fileNameWithoutExtension}");
                if (type == null)
                {
                    Debug.LogError($"Could not find type {fileNameWithoutExtension}");
                    continue;
                }
                MethodInfo method = typeof(CSV).GetMethod("ImportFromCSV", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo generic = method.MakeGenericMethod(type);
                generic.Invoke(null, new object[] { dataType, fileNameWithoutExtension });
            }
        }

        static void ImportFromCSV<T>(string type, string fileName) where T : ScriptableObject
        {
            int count = 0;
            string path = GetFilePath(type, fileName);

            Debug.Log($"Starting import of {fileName} from {path}");

            string[] lines = File.ReadAllLines(path);

            bool isHeader = true;
            foreach (string line in lines)
            {
                string[] values = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");


                if (isHeader)
                {
                    isHeader = false;
                }
                else
                {
                    count++;

                    bool isUpdating = true;
                    T recipe = AssetDatabase.LoadAssetAtPath<T>(values[2]);
                    if (recipe == null)
                    {
                        isUpdating = false;
                        recipe = ScriptableObject.CreateInstance<T>();
                    }

                    EditorUtility.SetDirty(recipe);

                    List<FieldInfo> fields = GetSerializeFields(recipe);
                    for (int i = 0; i < fields.Count; i++)
                    {
                        FieldInfo field = fields[i];
                        string value = values[i + 3];
                        if (field.FieldType == typeof(string))
                        {
                            field.SetValue(recipe, value.Trim('"'));
                        }
                        else
                        {
                            //Debug.Log("Attempting to set " + field.Name + " to " + value + " of type " + field.FieldType);
                            field.SetValue(recipe, Convert.ChangeType(value, field.FieldType));
                        }
                    }

                    if (isUpdating == false)
                    {
                        Debug.LogError($"Not yet saving newly created recipes: {recipe.name}");
                        count--;
                        // AssetDatabase.CreateAsset(recipe, $"Assets/_Dev/Resources/Recipes/{recipe.GetType().Name}.asset");
                    }
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Completed import of {count} {fileName} from {path}");
        }

        static void ExportToCSV(ScriptableObject[] dataObjects, string type, string fileName)
        {
            int count = 0;
            string path = GetFilePath(type, fileName);
            Debug.Log($"Starting export of {fileName} to {path}");

            StringBuilder csvContent = new StringBuilder();

            csvContent.Append("Class,InstanceID, Path,");

            List<FieldInfo> fields = GetSerializeFields(dataObjects[0]);

            foreach (FieldInfo field in fields)
            {
                csvContent.Append($"{field.Name} - {field.FieldType}");
                csvContent.Append(",");
            }
            csvContent.AppendLine();

            foreach (IRecipe recipe in dataObjects)
            {
                count++;
                csvContent.Append($"{recipe.GetType()},{((AbstractRecipe)recipe).GetInstanceID()},{AssetDatabase.GetAssetPath((AbstractRecipe)recipe)},");
                foreach (FieldInfo field in fields)
                {
                    if (field.FieldType == typeof(string))
                    {
                        csvContent.Append($"\"{field.GetValue(recipe)}\"");
                    }
                    else
                    {
                        csvContent.Append(field.GetValue(recipe));
                    }
                    csvContent.Append(",");
                }
                csvContent.AppendLine();
            }

            File.WriteAllText(path, csvContent.ToString());

            AssetDatabase.Refresh();

            Debug.Log($"Completed export of {count} {fileName} to {path}");
        }

        static List<FieldInfo> GetSerializeFields(ScriptableObject dataObject)
        {
            List<FieldInfo> serializedFields = new List<FieldInfo>();
            Type type = dataObject.GetType();

            while (type != null)
            {
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                foreach (FieldInfo field in fields)
                {
                    if (Attribute.IsDefined(field, typeof(SerializeField)) && (field.FieldType.IsPrimitive || field.FieldType == typeof(string)))
                    {
                        serializedFields.Add(field);
                    }
                }

                if (type == typeof(AbstractRecipe))
                {
                    break;
                }

                type = type.BaseType;
            }

            return serializedFields;
        }
    }
}
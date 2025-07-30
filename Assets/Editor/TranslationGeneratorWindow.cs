using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TranslationGeneratorWindow : EditorWindow
{
    private string csvPath = "Assets/Editor/translations.csv";
    private string outputFolder = "Assets/Resources/Translations/";

    [MenuItem("Window/Translation Generator")]
    private static void Init()
    {
        TranslationGeneratorWindow window = (TranslationGeneratorWindow)GetWindow(typeof(TranslationGeneratorWindow), false, "Translation Generator");
        window.Show();
    }

    private void OnGUI()
    {
		GUILayout.Label($"Path to CSV file");
        csvPath = GUILayout.TextField(csvPath);
		GUILayout.Label($"Output folder");
        outputFolder = GUILayout.TextField(outputFolder);
        GUILayout.Space(10f);
        if (GUILayout.Button("Generate Translations"))
        {
            GenerateTranslations();
        }
    }

    private void GenerateTranslations()
    {
        string[] lines = File.ReadAllLines(csvPath);
        List<string> languages = new List<string>();
        foreach (string key in lines[0].Split(',').Skip(1))
        {
            languages.Add(key);
        }
        Debug.Log($"Found languages in CSV: {string.Join(", ", languages)}");

        Dictionary<string, GameTranslation> translationMap = new Dictionary<string, GameTranslation>();
        foreach (GameTranslation translation in Resources.LoadAll<GameTranslation>("Translations").ToList())
        {
            translation.entries.Clear();
            translationMap[translation.languageCode] = translation;
        }
        if (translationMap.Keys.Count == 0)
        {
            Debug.LogError("No Translation ScriptableObjects found - you'll need to create these manually in Assets/Resources/Translations");
        }
        else
        {
            Debug.Log($"ScriptableObjects found for: {string.Join(", ", translationMap.Keys)}.\nUpdating...");
        }

        Dictionary<string, List<string>> allWords = new Dictionary<string, List<string>>();
        foreach (GameTranslation translation in translationMap.Values)
        {
            allWords[translation.languageCode] = new List<string>() { translation.languageName, "_" };
        }

        foreach (string line in lines.Skip(1))
        {
            string[] fields = line.Split(',');
            string key = fields[0];
            for (int i = 1; i < fields.Length; i++)
            {
                string language = languages[i - 1];
                string value = fields[i];
                if (translationMap.TryGetValue(language, out GameTranslation translation))
                {
                    if (key == "language_name")
                    {
                        translation.languageName = value;
                    }

                    translation.entries.Add(new GameTranslation.TranslationEntry { key = key, value = value });
                    allWords[language].Add(value);
                }
            }
        }

        foreach (string language in allWords.Keys)
        {
            string folder = outputFolder.EndsWith("/") || outputFolder.EndsWith("\\") ? outputFolder : outputFolder + "/";
            string filePath = folder + language + ".txt";
            string content = string.Join(" ", allWords[language]);
            File.WriteAllText(filePath, content);
        }

        foreach (GameTranslation translation in translationMap.Values)
        {
            EditorUtility.SetDirty(translation);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Finished generating translations");
    }
}

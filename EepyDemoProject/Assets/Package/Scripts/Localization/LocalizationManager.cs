using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

namespace Eepy
{
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        public static event Action<GameTranslation> OnLanguageChanged;

        // Determines whether menu text will be translated. If enabled, this overrides any text set in the Editor that has a Localization Key set
        // If it's set to `true` but no translations are available at game start, it will be automatically set to `false`
        [SerializeField]
        private bool isLocalizationEnabled = false;
        [SerializeField]
        private string defaultLanguageName = "English";

        // List of all supported languages
        // If left empty, the localization manager will load all languages from the Resources folder
        [SerializeField]
        private List<GameTranslation> allLanguages = new List<GameTranslation>();
        
        [HideInInspector]
        public GameTranslation currentTranslation { get; private set; }
        private Dictionary<string, GameTranslation> languageMap = new Dictionary<string, GameTranslation>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                if (Instance.isLocalizationEnabled)
                {
                    if (allLanguages == null || allLanguages.Count == 0)
                    {
                        allLanguages = new List<GameTranslation>(Resources.LoadAll<GameTranslation>("Translations"));
                    }

                    foreach (var language in allLanguages)
                    {
                        languageMap[language.languageCode] = language;
                    }
                    
                    if (allLanguages.Count == 0)
                    {
                        Instance.isLocalizationEnabled = false;
                    }
                }
            }
        }

        private void Start()
        {
            // Make sure all localized text is synced from the start
            OnLanguageChanged?.Invoke(currentTranslation);
        }

        public static string Get(string key)
        {
            if (Instance.currentTranslation == null)
            {
                return $"[{key}]";
            }

            return Instance.currentTranslation.Get(key);
        }

        public static void LoadLanguage(string code)
        {
            if (Instance.isLocalizationEnabled && (Instance.currentTranslation == null || Instance.currentTranslation.languageCode != code))
            {
                Util.Log($"Loading language: {code}");
                if (Instance.languageMap.TryGetValue(code, out var translation))
                {
                    Instance.currentTranslation = translation;
                    SettingsManager.SetLanguage(code);
                    OnLanguageChanged?.Invoke(translation);
                }
                else
                {
                    Util.LogWarning($"Language '{code}' not found, reverting to default");
                }
            }
        }

        public static bool IsLocalizationEnabled()
        {
            return Instance.isLocalizationEnabled;
        }

        public static TMP_FontAsset GetCurrentFont()
        {
            if (Instance.currentTranslation == null)
            {
                return null;
            }

            return Instance.currentTranslation.font;
        }

        public static string GetCurrentLanguageName()
        {
            if (Instance.currentTranslation == null)
            {
                return Instance.defaultLanguageName;
            }

            return Instance.currentTranslation.languageName;
        }

        public static List<GameTranslation> GetAllLanguages()
        {
            return Instance.allLanguages;
        }
    }
};
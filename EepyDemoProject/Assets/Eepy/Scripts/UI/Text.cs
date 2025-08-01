using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Eepy
{
    public class Text : MonoBehaviour
    {
        public TMP_Text text;

        [SerializeField]
        private string localizationKey;
        [SerializeField]
        private bool useGlobalFont = true;

        private void Awake()
        {
            LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        }

        private void OnDestroy()
        {
            LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
        }
        
        public void OnLanguageChanged(GameTranslation translation)
        {
            if (text != null && localizationKey != null && localizationKey != "" && LocalizationManager.IsLocalizationEnabled())
            {
                text.text = LocalizationManager.Get(localizationKey);
            }
            if (text != null && useGlobalFont)
            {
                text.font = translation.font;
            }
        }

        public void SetText(string text)
        {
            this.text.text = text;
        }
        public void SetText(string text, TMP_FontAsset font)
        {
            this.text.text = text;
            if (font != null)
            {
                this.text.font = font;
            }
        }

        public string GetText()
        {
            return text.text;
        }

        public void SetLocalizationKey(string key)
        {
            localizationKey = key;
            OnLanguageChanged(LocalizationManager.Instance.currentTranslation);
        }

        public void SetUnderline(bool underline)
        {
            Util.SetTextUnderline(text, underline);
        }
    }
};
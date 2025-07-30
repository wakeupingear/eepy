using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Eepy
{
    public class SettingsMenu : MenuScreen
    {
        [SerializeField]
        private Text resolutionText, languageText;

        // Sync text on Focus 
        protected override void OnFocused()
        {
            base.OnFocused();

            resolutionText.SetText(SettingsManager.resolution, LocalizationManager.GetCurrentFont());
            languageText.SetText(LocalizationManager.GetCurrentLanguageName(), LocalizationManager.GetCurrentFont());
        }

        public void OpenResolutionMenu()
        {
            GameplayUI.Instance.OpenMenu(GameplayUI.Instance.resolutionMenu);
        }

        public void OpenLanguageMenu()
        {
            GameplayUI.Instance.OpenMenu(GameplayUI.Instance.languageMenu);
        }
    }
};
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
            GameplayUI.OpenMenu(GameplayUI.resolutionMenu);
        }

        public void OpenLanguageMenu()
        {
            GameplayUI.OpenMenu(GameplayUI.languageMenu);
        }
    }
};
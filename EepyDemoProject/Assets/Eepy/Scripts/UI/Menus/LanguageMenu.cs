using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Eepy
{
    public class LanguageMenu : MenuScreen
    {
        [SerializeField]
        private GameObject languageButtonPrefab;
        
        private List<MenuButton> langButtons = new List<MenuButton>();

        protected override void Awake()
        {
            base.Awake();

            List<GameTranslation> languages = LocalizationManager.GetAllLanguages();
            for (int i = 0; i < languages.Count; i++)
            {
                GameTranslation language = languages[i];
                GameObject button = Instantiate(languageButtonPrefab, layoutGroup.transform);
                button.transform.SetParent(layoutGroup.transform, false);
                button.name = "LanguageButton_" + language.languageCode;

                MenuButton menuButton = button.GetComponent<MenuButton>();
                menuButton.menuScreen = this;
                langButtons.Add(menuButton);
                menuButton.GetPrimaryText().SetText(language.languageName, language.font);

                menuButton.OnClick.RemoveAllListeners();
                menuButton.OnClick.AddListener(() =>
                {
                    SettingsManager.SetLanguage(language.languageCode);
                    menuButton.GetPrimaryText().SetUnderline(true);
                    CloseMenu();
                });

                if (i > 0)
                {
                    menuButton.up = langButtons[i - 1];
                    langButtons[i - 1].down = menuButton;
                }
            }
            langButtons[^1].down = langButtons[0];
            langButtons[0].up = langButtons[^1];
        }

        public override void OnOpened()
        {
            startingButton = langButtons[0];
            for (int i = 0; i < langButtons.Count; i++)
            {
                GameTranslation language = LocalizationManager.GetAllLanguages()[i];
                MenuButton button = langButtons[i];
                bool selected = language.languageCode == SettingsManager.language;
                button.GetPrimaryText().SetUnderline(selected);
                if (selected)
                {
                    startingButton = button;
                }
            }

            base.OnOpened();
        }
    }
};
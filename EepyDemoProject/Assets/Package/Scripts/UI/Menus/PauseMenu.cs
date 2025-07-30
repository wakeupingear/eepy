using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Eepy
{
    public class PauseMenu : MenuScreen
    {
        public void OpenControlsMenu()
        {
            GameplayUI.Instance.OpenMenu(GameplayUI.Instance.controlsMenu);
        }
        
        public void OpenSettingsMenu()
        {
            GameplayUI.Instance.OpenMenu(GameplayUI.Instance.settingsMenu);
        }

        public void OpenQuitMenu()
        {
            GameplayUI.Instance.confirmMenu.SetTitleLocalizationKeyAndAction("quit_confirm", () =>
            {
                QuitGame();
            });
            GameplayUI.Instance.OpenMenu(GameplayUI.Instance.confirmMenu);
        }

        private void QuitGame()
        {
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
        }
    }
};
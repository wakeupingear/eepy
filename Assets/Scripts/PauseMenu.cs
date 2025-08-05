namespace Eepy
{
    public class PauseMenu : MenuScreen
    {
        public void OpenControlsMenu()
        {
            GameplayUI.OpenMenu(GameplayUI.controlsMenu);
        }
        
        public void OpenSettingsMenu()
        {
            GameplayUI.OpenMenu(GameplayUI.settingsMenu);
        }

        public void OpenQuitMenu()
        {
            GameplayUI.confirmMenu.SetTitleLocalizationKeyAndAction("quit_confirm", "Really Quit?", () =>
            {
                QuitGame();
            });
            GameplayUI.OpenMenu(GameplayUI.confirmMenu);
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
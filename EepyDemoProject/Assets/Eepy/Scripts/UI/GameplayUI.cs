using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Eepy
{
    public class GameplayUI : MonoBehaviour
    {
        public static GameplayUI Instance { get; private set; }

        public static event Action OnGameplayUIClosed;

        [SerializeField]
        private Image menuBackground;
        [SerializeField]
        private float menuBackgroundDuration = 0.1f, menuBackgroundAlpha = 0.5f;

        public static PauseMenu pauseMenu
        {
            get
            {
                return Instance._pauseMenu;
            }
        }
        [SerializeField]
        private PauseMenu _pauseMenu;

        public static ControlsMenu controlsMenu
        {
            get
            {
                return Instance._controlsMenu;
            }
        }
        [SerializeField]
        private ControlsMenu _controlsMenu;

        public static ChangeBindingMenu changeBindingMenu
        {
            get
            {
                return Instance._changeBindingMenu;
            }
        }
        [SerializeField]
        private ChangeBindingMenu _changeBindingMenu;

        public static SettingsMenu settingsMenu
        {
            get
            {
                return Instance._settingsMenu;
            }
        }
        [SerializeField]
        private SettingsMenu _settingsMenu;

        public static ConfirmMenu confirmMenu
        {
            get
            {
                return Instance._confirmMenu;
            }
        }
        [SerializeField]
        private ConfirmMenu _confirmMenu;

        public static ResolutionMenu resolutionMenu
        {
            get
            {
                return Instance._resolutionMenu;
            }
        }
        [SerializeField]
        private ResolutionMenu _resolutionMenu;

        public static LanguageMenu languageMenu
        {
            get
            {
                return Instance._languageMenu;
            }
        }
        [SerializeField]
        private LanguageMenu _languageMenu;

        public List<InputAction> upActions, leftActions, downActions, rightActions, interactActions, backActions;
        public List<InputAction> allFocusActions { get; private set; } = new List<InputAction>();

        private Stack<MenuScreen> activeMenuScreens = new Stack<MenuScreen>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                
                allFocusActions.AddRange(upActions);
                allFocusActions.AddRange(leftActions);
                allFocusActions.AddRange(downActions);
                allFocusActions.AddRange(rightActions);
                allFocusActions.AddRange(interactActions);
                allFocusActions.AddRange(backActions);

                Instance.menuBackground.color = new Color(Instance.menuBackground.color.r, Instance.menuBackground.color.g, Instance.menuBackground.color.b, 0f);
            }
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public int GetActiveMenuScreenCount()
        {
            return activeMenuScreens.Count;
        }

        public static void OpenMenu(MenuScreen screen)
        {
            if (screen != null)
            {
                if (Instance.activeMenuScreens.Count > 0)
                {
                    Instance.activeMenuScreens.Peek().OnCoveredUp();
                }

                screen.gameObject.SetActive(true);
                Instance.activeMenuScreens.Push(screen);
                screen.OnOpened();

                Instance.StopAllCoroutines();
                if (Instance.menuBackground.color.a < 1f)
                {
                    Instance.StartCoroutine(Util.FadeImage(Instance.menuBackground, 0f, Instance.menuBackgroundAlpha, Instance.menuBackgroundDuration));
                }
            }
        }

        public static void CloseMenu()
        {
            if (Instance.activeMenuScreens.Count > 0)
            {
                MenuScreen menuScreen = Instance.activeMenuScreens.Pop();
                menuScreen.OnClosed();

                if (Instance.activeMenuScreens.Count == 0)
                {
                    OnGameplayUIClosed?.Invoke();
                    
                    Instance.StopAllCoroutines();
                    Instance.StartCoroutine(Util.FadeImage(Instance.menuBackground, Instance.menuBackgroundAlpha, 0f, Instance.menuBackgroundDuration));
                }
                else
                {
                    Instance.activeMenuScreens.Peek().OnReopened();
                }
            }
        }

        public static void CloseGameplayUI()
        {
            while (Instance.activeMenuScreens.Count > 0)
            {
                CloseMenu();
            }
        }
    }
};
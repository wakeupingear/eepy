using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameplayUI : MonoBehaviour
{
    public static GameplayUI Instance { get; private set; }

    public static event Action OnGameplayUIClosed;

    [SerializeField]
    private Image menuBackground;
    [SerializeField]
    private float menuBackgroundDuration = 0.1f, menuBackgroundAlpha = 0.5f;

    public PauseMenu pauseMenu;
    public ControlsMenu controlsMenu;
    public ChangeBindingMenu changeBindingMenu;
    public SettingsMenu settingsMenu;
    public ConfirmMenu confirmMenu;
    public ResolutionMenu resolutionMenu;
    public LanguageMenu languageMenu;

    public List<InputAction> upActions, leftActions, downActions, rightActions, interactActions, backActions;
    public List<InputAction> allFocusActions { get; private set; } = new List<InputAction>();

    private List<MenuScreen> allMenuScreens = new List<MenuScreen>();
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

    public void OpenMenu(MenuScreen screen)
    {
        if (screen != null)
        {
            if (activeMenuScreens.Count > 0)
            {
                activeMenuScreens.Peek().OnCoveredUp();
            }

            screen.gameObject.SetActive(true);
            activeMenuScreens.Push(screen);
            screen.OnOpened();

            StopAllCoroutines();
            if (menuBackground.color.a < 1f)
            {
                StartCoroutine(Util.FadeImage(menuBackground, 0f, menuBackgroundAlpha, menuBackgroundDuration));
            }
        }
    }

    public void CloseMenu()
    {
        if (activeMenuScreens.Count > 0)
        {
            MenuScreen menuScreen = activeMenuScreens.Pop();
            menuScreen.OnClosed();

            if (activeMenuScreens.Count == 0)
            {
                OnGameplayUIClosed?.Invoke();
                CloseGameplayUI();
            }
            else
            {
                activeMenuScreens.Peek().OnReopened();
            }
        }
    }

    public void CloseGameplayUI()
    {
        while (activeMenuScreens.Count > 0)
        {
            CloseMenu();
        }

        StopAllCoroutines();
        StartCoroutine(Util.FadeImage(menuBackground, menuBackgroundAlpha, 0f, menuBackgroundDuration));

        GameManager.Instance.Unpause();
    }
}

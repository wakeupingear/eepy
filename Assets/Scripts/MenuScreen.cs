using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuScreen : MonoBehaviour
{
    [SerializeField]
    private float animationDuration = 0.1f;
    [SerializeField]
    private Vector2 slideOffset = new Vector2(36f, 0f);
    
    [SerializeField]
    protected MenuButton startingButton;

    private enum AutoLinkDirection
    {
        None = 0,
        Vertical = 1,
        Horizontal = 2
    }
    [SerializeField]
    private AutoLinkDirection autoLink = AutoLinkDirection.None;

    public bool areButtonsDisabled { get; protected set; } = false;
    
    protected RectTransform rectTransform;
    protected CanvasGroup canvasGroup;
    protected LayoutGroup layoutGroup;
    
    [HideInInspector]
    public MenuButton focusedButton { get; private set; }
    protected List<MenuButton> lastFocusedButtons = new List<MenuButton>();

    // All buttons in this screen
    private List<MenuButton> buttons;
    
    public enum MenuState
    {
        Closed = 0,
        Open = 1,
        CoveredUp = 2
    }
    public MenuState menuState { get; private set; } = MenuState.Closed;

    // Make sure to call base.Awake() in your Awake() function!
    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        layoutGroup = GetComponentInChildren<LayoutGroup>();
        
        canvasGroup.alpha = 0;
    }

    // Make sure to call base.Start() AT THE END of your Start() function!
    protected virtual void Start()
    {
        buttons = GetComponentsInChildren<MenuButton>().ToList();
        foreach (var button in buttons)
        {
            button.menuScreen = this;
        }

        if (autoLink != AutoLinkDirection.None)
        {
            List<MenuButton> autoLinkedButtons = Util.GetComponentsInChildrenConditional<MenuButton>(layoutGroup.transform);
            autoLinkedButtons = autoLinkedButtons.Where(button => button.inputType != MenuButton.InputType.Mouse).ToList();
            if (autoLinkedButtons.Count > 0)
            {
                if (autoLink == AutoLinkDirection.Vertical)
                {
                    for (int i = 1; i < autoLinkedButtons.Count; i++)
                    {
                        autoLinkedButtons[i - 1].down = autoLinkedButtons[i];
                        autoLinkedButtons[i].up = autoLinkedButtons[i - 1];
                    }

                    autoLinkedButtons[0].up = autoLinkedButtons[^1];
                    autoLinkedButtons[^1].down = autoLinkedButtons[0];
                }
                else if (autoLink == AutoLinkDirection.Horizontal)
                {
                    for (int i = 1; i < autoLinkedButtons.Count; i++)
                    {
                        autoLinkedButtons[i - 1].right = autoLinkedButtons[i];
                        autoLinkedButtons[i].left = autoLinkedButtons[i - 1];
                    }

                    autoLinkedButtons[0].left = autoLinkedButtons[^1];
                    autoLinkedButtons[^1].right = autoLinkedButtons[0];
                }
            }
        }

        // We want to disable this object so it's scripts don't run while the menu is closed,
        // but we want the GameObject to be active for a frame to do rect calculations.
        // This is why all menus aren't disabled in the GameplayUI prefab by default
        StartCoroutine(HideAfterOneFrame());
    }

    protected virtual void Update()
    {
        if (menuState == MenuState.Open)
        {
            // Remove disabled/null buttons from the history
            while (lastFocusedButtons.Count > 0 && (lastFocusedButtons[^1] == null || lastFocusedButtons[^1].isDisabled))
            {
                lastFocusedButtons.RemoveAt(lastFocusedButtons.Count - 1);
            }
            // Limit the history to 10 buttons - arbitrary but works since very few buttons get disabled/destroyed
            while (lastFocusedButtons.Count > 10)
            {
                lastFocusedButtons.RemoveAt(lastFocusedButtons.Count - 1);
            }
            if (lastFocusedButtons.Count == 0 && startingButton != null)
            {
                lastFocusedButtons.Add(startingButton);
            }

            if (InputManager.isInputManagerEnabled)
            {
                if (Util.AnyInputActionListDown(GameplayUI.Instance.backActions))
                {
                    CloseMenu();
                }
                else if (Util.AnyInputActionListDown(GameplayUI.Instance.allFocusActions))
                {
                    if (focusedButton != null)
                    {
                        focusedButton.ReceiveDirectionalInput();
                    }
                    else if (lastFocusedButtons.Count > 0)
                    {
                        lastFocusedButtons[^1].Focus();
                    }
                }
            }
        }
    }

    // Called when the menu is added to the top of the menu stack.
    public virtual void OnOpened()
    {
        OnFocused();
        StartStateChangeAnimation(MenuState.Open);

        // Since we haven't visited this menu yet, the starting button should be automatically highlighted.
        // This isn't required for OnReopened because menu/button state is preserved while covered up.
        lastFocusedButtons.Clear();
        if (startingButton != null)
        {
            startingButton.Focus(Direction.Down);
            lastFocusedButtons.Add(startingButton);
        }
    }

    // Called when the menu returns to the top of the menu stack after the topmost menu is closed.
    public virtual void OnReopened()
    {
        OnFocused();
        StartStateChangeAnimation(MenuState.Open);
    }

    // Called when the menu receives focus (OnOpened or OnReopened).
    protected virtual void OnFocused()
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }
    }

    // Called when the menu is popped off the menu stack.
    public virtual void OnClosed()
    {
        OnUnfocused();
        StartStateChangeAnimation(MenuState.Closed);
    }

    //Called when another menu is added to the top of the menu stack.
    public virtual void OnCoveredUp()
    {
        OnUnfocused();
        StartStateChangeAnimation(MenuState.CoveredUp);
    }

    // Called when the menu loses focus (OnClosed or OnCoveredUp)
    protected virtual void OnUnfocused()
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void SetFocusedButton(MenuButton inButton)
    {
        focusedButton = inButton;

        if (focusedButton != null && focusedButton.inputType != MenuButton.InputType.Mouse && (lastFocusedButtons.Count == 0 || lastFocusedButtons[^1] != focusedButton))
        {
            lastFocusedButtons.Add(focusedButton);
        }
    }

    public void CloseMenu()
    {
        GameplayUI.Instance.CloseMenu();
    }

    private IEnumerator HideAfterOneFrame()
    {
        yield return null;
        gameObject.SetActive(false);
    }

    private void StartStateChangeAnimation(MenuState targetState)
    {
        if (targetState != menuState)
        {
            StopAllCoroutines();
            StartCoroutine(StateChangeAnimationCoroutine(targetState));
        }
    }

    // Animate the state change. If you want to make custom animations, rewrite this function!
    private IEnumerator StateChangeAnimationCoroutine(MenuState targetState)
    {
        MenuState prevState = menuState;
        menuState = targetState;

        float elapsed = 0f;
        float fromAlpha = targetState == MenuState.Open ? 0f : 1f;
        float toAlpha = 1f - fromAlpha;
        canvasGroup.alpha = fromAlpha;

        int menuCount = GameplayUI.Instance.GetActiveMenuScreenCount();
        Vector2 fromPos = 
            prevState == MenuState.Open || 
                (menuCount == 1 && prevState == MenuState.Closed)
                ? Vector2.zero :
            prevState == MenuState.Closed ? slideOffset :
            -slideOffset;
        Vector2 toPos = 
            targetState == MenuState.Open ||
                (menuCount == 0 && prevState == MenuState.Open) 
                ? Vector2.zero :
            targetState == MenuState.Closed ? slideOffset :
            -slideOffset;
        rectTransform.anchoredPosition = fromPos;

        while (elapsed < animationDuration)
        {
            float t = elapsed / animationDuration;
            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            rectTransform.anchoredPosition = Vector2.Lerp(fromPos, toPos, t);
            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }

        canvasGroup.alpha = toAlpha;
        rectTransform.anchoredPosition = toPos;

        if (targetState == MenuState.Closed)
        {
            gameObject.SetActive(false);
        }
    }
}

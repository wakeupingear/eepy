using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MenuButton : MenuInput, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public enum InputType
    {
        All = 0,
        Mouse = 1,
        Directional = 2
    }
    public InputType inputType = InputType.All;

    // Adjacent buttons that you can move to with directional inputs
    public MenuButton up, down, left, right;

    // Bind functions to these listeners in code or in the Editor
    public UnityEvent OnClick, OnFocus, OnUnfocus;

    [SerializeField]
    private Color normalColor = Color.white, focusedColor = Color.yellow;
    [SerializeField]
    private float disabledOpacity = 0.5f;

    [SerializeField]
    private List<Text> text = new List<Text>();
    
    // Reference to the container menu
    [HideInInspector]
    public MenuScreen menuScreen;

    public bool isFocused { get; private set; } = false;
    public bool isDisabled { get; private set; } = false;

    private bool isPointerOver = false;
    private Vector3 lastMousePos;
    private SettingToggle settingToggle;

    private void Awake()
    {
        settingToggle = GetComponent<SettingToggle>();
    }

    private void Update()
    {
        if (isFocused)
        {
            lastMousePos = Input.mousePosition;
        }
        else if (isPointerOver && Input.mousePosition != lastMousePos)
        {
            Focus();
            lastMousePos = Input.mousePosition;
        }
    }

    public void Focus(Direction direction = Direction.MAX)
    {
        if (!isFocused)
        {
            if (isDisabled)
            {
                if ((direction == Direction.Down || direction == Direction.MAX) && down != null)
                {
                    down.Focus(direction);
                }
                else if ((direction == Direction.Up || direction == Direction.MAX) && up != null)
                {
                    up.Focus(direction);
                }
                else if ((direction == Direction.Left || direction == Direction.MAX) && left != null)
                {
                    left.Focus(direction);
                }
                else if ((direction == Direction.Right || direction == Direction.MAX) && right != null)
                {
                    right.Focus(direction);
                }
            }
            else
            {
                if (menuScreen.focusedButton != null && menuScreen.focusedButton != this)
                {
                    menuScreen.focusedButton.Unfocus();
                }

                menuScreen.SetFocusedButton(this);
                SetTextColor(focusedColor);

                isFocused = true;
                OnFocus?.Invoke();
            }
        }
    }

    public void Unfocus(bool changeColor = true)
    {
        if (isFocused)
        {
            if (menuScreen != null && menuScreen.focusedButton == this)
            {
                menuScreen.SetFocusedButton(null);
            }

            if (changeColor)
            {
                SetTextColor(normalColor);
            }

            isFocused = false;
            OnUnfocus?.Invoke();
        }
    }

    public void SetDisabled(bool disabled)
    {
        isDisabled = disabled;
        SetTextColor(isFocused && !disabled ? focusedColor : normalColor);

        if (disabled)
        {
            Unfocus(false);
        }
    }

    private void SetTextColor(Color color)
    {
        foreach (Text t in text)
        {
            t.text.color = isDisabled ? new Color(color.r, color.g, color.b, disabledOpacity) : color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDisabled && !menuScreen.areButtonsDisabled && InputManager.isInputManagerEnabled)
        {
            if (inputType == InputType.Mouse || inputType == InputType.All)
            {
                isPointerOver = true;
                Focus();
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isDisabled && !menuScreen.areButtonsDisabled && InputManager.isInputManagerEnabled)
        {
            isPointerOver = false;
            Unfocus();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDisabled && !menuScreen.areButtonsDisabled && InputManager.isInputManagerEnabled)
        {
            if (inputType != InputType.Directional)
            {
                Focus();
                Click();
            }
        }
    }

    private void Click()
    {
        if (menuScreen.menuState == MenuScreen.MenuState.Open)
        {
            OnClick?.Invoke();
        }
    }

    public override bool ReceiveDirectionalInput()
    {
        if (inputType != InputType.Mouse)
        {
            if (Util.AnyInputActionListDown(GameplayUI.Instance.interactActions))
            {
                Click();
                return true;
            }
            else if (Util.AnyInputActionListDown(GameplayUI.Instance.upActions) && up != null)
            {
                up.Focus(Direction.Up);
                return true;
            }
            else if (Util.AnyInputActionListDown(GameplayUI.Instance.leftActions) && left != null)
            {
                left.Focus(Direction.Left);
                return true;
            }
            else if (Util.AnyInputActionListDown(GameplayUI.Instance.downActions) && down != null)
            {
                down.Focus(Direction.Down);
                return true;
            }
            else if (Util.AnyInputActionListDown(GameplayUI.Instance.rightActions) && right != null)
            {
                right.Focus(Direction.Right);
                return true;
            }
        }

        if (settingToggle != null)
        {
            return settingToggle.ReceiveDirectionalInput();
        }

        return false;
    }

    public Text GetPrimaryText()
    {
        return text[0];
    }
}

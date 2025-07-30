using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

namespace Eepy
{
    public class ChangeBindingMenu : MenuScreen
    {
        [SerializeField]
        private Text title;
        [SerializeField]
        private GameObject bindingButtonPrefab;
        [SerializeField]
        private MenuButton addButton, backButton;
        [SerializeField]
        private string initialAddTextKey = "add_keybinding", inProgressAddTextKey = "adding_keybinding", removeTextKey = "remove_keybinding", invalidInputTextKey = "invalid_keybinding";
        [SerializeField]
        private float invalidInputMessageDuration = 1.0f;
        [SerializeField]
        private int siblingIndex = 4;
        [SerializeField]
        private int maxBindings = 8;
        [SerializeField]
        private List<KeyCode> exitAddModeKeys = new List<KeyCode>();
        [SerializeField]
        private List<InputAction> exitAddModeActions = new List<InputAction>() { InputAction.Pause };

        public InputAction inputAction { get; private set; }

        private KeyCode currentlyPressedKeyCode = 0;
        private bool isAddMode = false, justEnteredAddMode = false, isInvalidInput = false;
        private List<MenuButton> bindingButtons = new List<MenuButton>();
        private InputManager.ControllerAxisBinding? currentlyPressedAxis = null;

        protected override void Update()
        {
            if (isAddMode && !isInvalidInput)
            {
                if (justEnteredAddMode)
                {
                    justEnteredAddMode = false;
                }
                else if (Util.AnyKeyCodeListUp(exitAddModeKeys) || Util.AnyInputActionListUp(exitAddModeActions, true))
                {
                    ExitAddMode();
                }
                else if (!Util.AnyKeyCodeList(exitAddModeKeys) && !Util.AnyInputActionList(exitAddModeActions, true) && currentlyPressedKeyCode == 0 && !currentlyPressedAxis.HasValue)
                {
                    if (Input.anyKeyDown)
                    {
                        foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
                        {
                            if (Input.GetKeyDown(kcode))
                            {
                                int rawCode = InputManager.GetCodeFromRawKeyCode(kcode);
                                if ((KeyCode)rawCode != KeyCode.None && InputManager.CanAddKeyBinding(rawCode))
                                {
                                    currentlyPressedKeyCode = kcode;
                                    rawCode = InputManager.GetCodeFromRawKeyCode(kcode);
                                    InputManager.AddKeyBinding(inputAction, rawCode);
                                    ResetBindingButtons();
                                    StartCoroutine(ExitAfterKeyReleased());

                                    break;
                                }
                                else
                                {
                                    InvalidInput();
                                }
                            }
                        }
                    }
                    else
                    {
                        var axis = InputManager.GetActiveUnusedAxis();
                        if (axis.HasValue)
                        {
                            if (InputManager.CanAddKeyBinding((int)axis.Value.code))
                            {
                                currentlyPressedAxis = axis.Value;
                                InputManager.AddKeyBinding(inputAction, (int)axis.Value.code);
                                ResetBindingButtons();
                                StartCoroutine(ExitAfterKeyReleased());
                            }
                            else
                            {
                                InvalidInput();
                            }
                        }
                    }
                }
            }

            base.Update();
        }

        public override void OnOpened()
        {
            ResetBindingButtons();
            
            base.OnOpened();
        }

        public void SetTitle(string title, TMP_FontAsset font)
        {
            this.title.SetText(title, font);
        }

        public void SetInputAction(InputAction inputAction)
        {
            this.inputAction = inputAction;
        }

        private void ResetBindingButtons()
        {
            ResetAddButton();

            var bindings = InputManager.GetKeyBindings(inputAction);
            while (bindingButtons.Count < bindings.Count)
            {
                GameObject obj = Instantiate(bindingButtonPrefab, transform);
                var newButton = obj.GetComponent<MenuButton>();
                newButton.transform.SetParent(layoutGroup.transform, false);
                newButton.transform.SetSiblingIndex(siblingIndex + bindingButtons.Count);
                newButton.menuScreen = this;
                bindingButtons.Add(newButton);
            }

            for (int i = 0; i < bindingButtons.Count; i++)
            {
                if (i < bindings.Count)
                {
                    var bindingButton = bindingButtons[i];
                    bindingButton.gameObject.SetActive(true);
                    bindingButton.GetPrimaryText().SetLocalizationKey(removeTextKey);

                    int index = i;
                    bindingButton.OnClick.RemoveAllListeners();
                    bindingButton.OnClick.AddListener(() =>
                    {
                        if (InputManager.RemoveKeyBinding(inputAction, bindings[index]))
                        {
                            bindingButton.Unfocus();
                            ResetBindingButtons();
                            MenuButton nextButton = null;
                            if (InputManager.GetKeyBindings(inputAction).Count == 1)
                            {
                                nextButton = addButton;
                            }
                            else
                            {
                                nextButton = bindingButton.up;
                            }
                            while (nextButton.isDisabled)
                            {
                                nextButton = nextButton.up;
                            }
                            nextButton.Focus();
                        }
                    });

                    bindingButton.SetDisabled(!InputManager.CanRemoveKeyBinding(inputAction, bindings[i]));
                    bindingButton.gameObject.GetComponentInChildren<InputIcon>().SetCode(bindings[i]);

                    if (i == 0)
                    {
                        bindingButton.up = addButton;
                        addButton.down = bindingButton;
                    }
                    else
                    {
                        bindingButtons[i - 1].down = bindingButton;
                        bindingButton.up = bindingButtons[i - 1];
                    }
                }
                else
                {
                    bindingButtons[i].gameObject.SetActive(false);
                }
            }
            backButton.up = bindingButtons[bindings.Count - 1];
            bindingButtons[bindings.Count - 1].down = backButton;
        }

        private void ResetAddButton()
        {
            StopCoroutine(ResetColorAfterDelay());

            addButton.GetPrimaryText().SetLocalizationKey(initialAddTextKey);
            addButton.OnClick.RemoveAllListeners();
            addButton.OnClick.AddListener(() =>
            {
                isAddMode = true;
                justEnteredAddMode = true;
                addButton.GetPrimaryText().SetLocalizationKey(inProgressAddTextKey);
                InputManager.DisableInputManager(true);
                areButtonsDisabled = true;
            });

            int count = InputManager.GetKeyBindings(inputAction).Count;
            if (count >= maxBindings)
            {
                if (addButton.isFocused)
                {
                    addButton.Unfocus();
                    backButton.Focus();
                }
                addButton.SetDisabled(true);
                startingButton = backButton;
            }
            else
            {
                addButton.SetDisabled(false);
                startingButton = addButton;
            }
        }

        private void InvalidInput()
        {
            StartCoroutine(ResetColorAfterDelay());
        }

        private IEnumerator ResetColorAfterDelay()
        {
            Color originalColor = addButton.GetPrimaryText().text.color;
            addButton.GetPrimaryText().SetLocalizationKey(invalidInputTextKey);
            addButton.GetPrimaryText().text.color = Color.red;
            isInvalidInput = true;

            yield return new WaitForSecondsRealtime(invalidInputMessageDuration);

            addButton.GetPrimaryText().SetLocalizationKey(initialAddTextKey);
            addButton.GetPrimaryText().text.color = originalColor;
            isInvalidInput = false;
        }

        private IEnumerator ExitAfterKeyReleased()
        {
            while (Input.GetKey(currentlyPressedKeyCode) || (currentlyPressedAxis.HasValue && Util.IsAxisPressed(currentlyPressedAxis.Value)))
            {
                yield return null;
            }

            currentlyPressedKeyCode = 0;
            currentlyPressedAxis = null;
            ExitAddMode();
        }

        private void ExitAddMode()
        {
            isAddMode = false;
            InputManager.DisableInputManager(false);
            areButtonsDisabled = false;
            ResetAddButton();
        }
    }
};
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Eepy
{
    public class ControlsMenu : MenuScreen
    {
        [SerializeField]
        private MenuButton backButton, resetButton;
        [SerializeField]
        private string resetAllKey = "reset_all", resetAllConfirmKey = "reset_all_confirm";
        [SerializeField]
        private GameObject controlBindingOverviewButtonPrefab;
        [SerializeField]
        private int bindingSiblingIndex = 2;

        private List<ControlBindingOverviewButton> controlBindingOverviewButtons = new List<ControlBindingOverviewButton>();
        private bool resetConfirmation = false;

        protected override void Awake()
        {
            base.Awake();

            InputManager.OnInputActionsChanged += OnInputActionsChanged;
            resetButton.OnUnfocus.AddListener(ResetResetButton);

            // iterate through InputAction
            foreach (InputManager.InputState inputState in InputManager.GetInputStates())
            {
                if (inputState.allowRebinding)
                {    
                    GameObject button = Instantiate(controlBindingOverviewButtonPrefab, layoutGroup.transform);
                    button.name = inputState.action.ToString();
                    button.transform.SetSiblingIndex(bindingSiblingIndex + controlBindingOverviewButtons.Count);
                    ControlBindingOverviewButton controlBindingOverviewButton = button.GetComponent<ControlBindingOverviewButton>();
                    controlBindingOverviewButton.SetInputAction(inputState.action);
                    controlBindingOverviewButtons.Add(controlBindingOverviewButton);

                    MenuButton buttonComponent = controlBindingOverviewButton.GetMenuButton();
                    if (controlBindingOverviewButtons.Count == 1)
                    {
                        backButton.down = buttonComponent;
                        resetButton.down = buttonComponent;
                        buttonComponent.up = backButton;
                        buttonComponent.down = backButton;
                    }
                    else
                    {
                        buttonComponent.up = controlBindingOverviewButtons[^2].GetMenuButton();
                        controlBindingOverviewButtons[^2].GetMenuButton().down = buttonComponent;
                    }
                }
            }

            if (controlBindingOverviewButtons.Count > 0)
            {
                backButton.up = controlBindingOverviewButtons[^1].GetMenuButton();
                resetButton.up = controlBindingOverviewButtons[^1].GetMenuButton();
                controlBindingOverviewButtons[^1].GetMenuButton().down = backButton;
                startingButton = controlBindingOverviewButtons[0].GetMenuButton();
            }
        }

        private void OnDestroy()
        {
            InputManager.OnInputActionsChanged -= OnInputActionsChanged;
            if (resetButton != null)
            {
                resetButton.OnUnfocus.RemoveListener(ResetResetButton);
            }
        }
        
        protected override void OnFocused()
        {
            base.OnFocused();

            resetButton.GetPrimaryText().SetLocalizationKey(resetAllKey);
            CheckIfCanReset();
        }

        private void OnInputActionsChanged(List<InputAction> inputActions)
        {
            CheckIfCanReset();

        }

        private void CheckIfCanReset()
        {
            Dictionary<InputAction, List<int>> keyBindings = InputManager.GetAllKeyBindings(), defaultBindings = InputManager.GetAllDefaultKeyBindings();
            bool canReset = false;
            foreach (var binding in keyBindings)
            {
                if (!defaultBindings.ContainsKey(binding.Key) || !defaultBindings[binding.Key].SequenceEqual(binding.Value))
                {
                    canReset = true;
                    break;
                }
            }

            resetButton.SetDisabled(!canReset);
        }

        private void ResetResetButton()
        {
            resetButton.GetPrimaryText().SetLocalizationKey(resetAllKey);
            resetConfirmation = false;
            resetButton.SetDisabled(true);
            backButton.Focus();
        }

        public void ResetButtonPressed()
        {
            if (resetConfirmation)
            {
                InputManager.ResetAllBindings();
                ResetResetButton();
            }
            else
            {
                resetButton.GetPrimaryText().SetLocalizationKey(resetAllConfirmKey);
                resetConfirmation = true;
            }
        }
    }
};
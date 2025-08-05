using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Eepy
{
    public class ControlBindingOverviewButton : MonoBehaviour
    {
        [SerializeField]
        private MenuButton button;
        [SerializeField]
        private GameObject inputIconPrefab;
        [SerializeField]
        private HorizontalLayoutGroup layoutGroup;
        [SerializeField]
        private string titleFallbackText;
        [SerializeField]
        private string keybindingsKey = "keybindings";
        [SerializeField]
        private Text title;

        public InputAction inputAction { get; private set; }

        private struct BindingImage
        {
            public InputIcon icon;
            public int code;
        }
        private List<BindingImage> bindingImages = new List<BindingImage>();

        private void Awake()
        {
            InputManager.OnInputActionsChanged += OnInputActionsChanged;
        }

        private void OnDestroy()
        {
            InputManager.OnInputActionsChanged -= OnInputActionsChanged;
        }

        private void OnInputActionsChanged(List<InputAction> actions)
        {
            if (actions.Contains(inputAction))
            {
                UpdateBindingImages();
            }
        }

        public void SetInputAction(InputAction inputAction)
        {
            this.inputAction = inputAction;

            title.SetLocalizationKey(inputAction.ToString().ToLower(), inputAction.ToString());
            button.OnClick.AddListener(() =>
            {
                GameplayUI.changeBindingMenu.SetTitle(title.text.text + " - " + LocalizationManager.Get(keybindingsKey, titleFallbackText), LocalizationManager.GetCurrentFont());
                GameplayUI.changeBindingMenu.SetInputAction(inputAction);
                GameplayUI.OpenMenu(GameplayUI.changeBindingMenu);
            });

            UpdateBindingImages();
        }

        private void UpdateBindingImages()
        {
            List<int> bindingCodes = InputManager.GetKeyBindings(inputAction);
            while (bindingImages.Count < bindingCodes.Count)
            {
                var newImage = Instantiate(inputIconPrefab, layoutGroup.transform);
                InputIcon icon = newImage.GetComponent<InputIcon>();
                bindingImages.Add(new BindingImage { icon = icon, code = 0 });
            }

            for (int i = 0; i < bindingImages.Count; i++)
            {
                if (i < bindingCodes.Count)
                {
                    bindingImages[i].icon.gameObject.SetActive(true);
                    bindingImages[i].icon.gameObject.name = "Code " + Util.FormatInputCode(bindingCodes[i]);
                    bindingImages[i].icon.SetCode(bindingCodes[i]);
                    if (bindingImages[i].code != bindingCodes[i])
                    {
                        bindingImages[i] = new BindingImage { icon = bindingImages[i].icon, code = bindingCodes[i] };
                    }
                }
                else
                {
                    bindingImages[i].icon.gameObject.name = "Unused Binding " + i;
                    bindingImages[i].icon.gameObject.SetActive(false);
                }
            }
        }

        public MenuButton GetMenuButton()
        {
            return button;
        }
    }
};

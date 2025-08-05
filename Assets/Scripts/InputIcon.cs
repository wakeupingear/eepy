using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace Eepy
{
    public class InputIcon : MonoBehaviour
    {
        // InputIcon can either take an InputAction or a code (KeyCode or ControllerCode).
        // There are various cases where we want to use one over the other.
        // Ex: Show the first keycode for an input action, but a specific input code when listing all available inputs.
        public InputAction? inputAction { get; set; } = null;
        public int code { get; set; } = 0;

        [SerializeField]
        private Image glyphImage, backgroundImage;
        [SerializeField]
        private TextMeshProUGUI glyphText;
        [SerializeField]
        private Sprite blankSprite, roundBackgroundSprite, squareBackgroundSprite, smallRectBackgroundSprite, dpadBackgroundSprite, stickBackgroundSprite, stickDownBackgroundSprite, lTriggerBackgroundSprite, rTriggerBackgroundSprite;
        [SerializeField]
        private Color backgroundColor;

        private int prevCodeToUse = int.MinValue;

        // Heuristic to determine what black backgrounds to use
        private string[] smallRectBackgroundSpriteNames = new string[] { "_shift", "_alt", "_ctrl", "_space", "_enter", "_backspace", "_delete", "_tab", "_esc", "_capslock" };

        private void UpdateIcon()
        {
            if (prevCodeToUse == code)
            {
                return;
            }
            prevCodeToUse = code;

            int codeToUse = code;
            if (codeToUse == 0 && inputAction != null)
            {
                List<int> bindingCodes = InputManager.GetKeyBindings(inputAction.Value);
                if (bindingCodes.Count > 0)
                {
                    codeToUse = bindingCodes[0];
                }
            }

            // If no code to use, hide the icon
            if (codeToUse == 0)
            {
                glyphImage.sprite = null;
                backgroundImage.sprite = null;
                glyphText.text = "";

                return;
            }

            if (InputManager.TryGetKeySprite(codeToUse, out Sprite sprite))
            {
                glyphImage.sprite = sprite;
                backgroundImage.color = backgroundColor;
                glyphText.text = "";

                if (sprite.name.Contains("_button_"))
                {
                    backgroundImage.sprite = roundBackgroundSprite;
                }
                else if (smallRectBackgroundSpriteNames.Any(name => sprite.name.Contains(name)))
                {
                    backgroundImage.sprite = smallRectBackgroundSprite;
                }
                else if (sprite.name.Contains("keyboard_"))
                {
                    backgroundImage.sprite = squareBackgroundSprite;
                }
                else if (sprite.name.Contains("dpad_"))
                {
                    backgroundImage.sprite = dpadBackgroundSprite;
                }
                else if (sprite.name.Contains("stick_"))
                {
                    backgroundImage.sprite = stickBackgroundSprite;
                }
                else if (sprite.name.Contains("trigger_l"))
                {
                    backgroundImage.sprite = lTriggerBackgroundSprite;
                }
                else if (sprite.name.Contains("trigger_r"))
                {
                    backgroundImage.sprite = rTriggerBackgroundSprite;
                }
                else
                {
                    backgroundImage.color = new Color(1, 1, 1, 0);
                }

                return;
            }

            if (Util.InputCodeIsKeyboard(codeToUse))
            {
                glyphText.text = ((KeyCode)codeToUse).ToString().Replace("Alpha", "").Replace("Keypad", "");
                glyphImage.sprite = blankSprite;
                backgroundImage.sprite = null;
                backgroundImage.color = new Color(1, 1, 1, 0);

                return;
            }
            
            glyphImage.sprite = blankSprite;
            backgroundImage.sprite = null;
            backgroundImage.color = new Color(1, 1, 1, 0);
            glyphText.text = "";
        }

        public void SetInputAction(InputAction inputAction)
        {
            this.inputAction = inputAction;
            UpdateIcon();
        }

        public void SetCode(int code)
        {
            this.code = code;
            UpdateIcon();
        }
    }
};
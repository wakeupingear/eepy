using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Eepy
{
    public class SettingToggle : MenuInput
    {
        [SerializeField]
        private Text label;
        [SerializeField]
        private MenuButton groupButton, decButton, incButton;

        public enum SettingsType
        {
            Int,
            String
        }
        [SerializeField]
        private SettingsType dataType = SettingsType.Int;
        [SerializeField]
        private string saveKey;

        [Serializable]
        public struct Options
        {
            public string value;
            public string localizationKey;
            public string universalLabel;
        }
        [SerializeField]
        private List<Options> options;
        [SerializeField]
        private float labelAnimationTime = 0.25f;
        [SerializeField]
        private Vector3 labelAnimationOffset = new Vector3(20f, 0f, 0f);
        [SerializeField]
        private Vector3 arrowAnimationOffset = new Vector3(0f, 0f, 0f);
        [SerializeField]
        private AnimationCurve arrowAnimationCurve;
        [SerializeField]
        private float minLabelWidth = 60f, labelPadding = 10f;
        
        private Text labelDuplicate;
        private CanvasGroup labelCanvasGroup, labelDuplicateCanvasGroup;
        private RectTransform labelRT, labelDuplicateRT;

        private Vector3 labelPos, incPos, decPos;
        private bool duplicatesSetUp = false;
        private int currentOption = -1;
        private float targetLabelWidth = 0f;

        private void Awake()
        {
            SyncCurrentOption();

            labelCanvasGroup = label.GetComponent<CanvasGroup>();
            labelRT = label.GetComponent<RectTransform>();
        }

        private void Start()
        {
            SettingsManager.OnSettingsChanged += SyncMenuItem;
            LocalizationManager.OnLanguageChanged += ApplyTranslation;
            ApplyTranslation(LocalizationManager.Instance.currentTranslation);
        }

        private void OnDestroy()
        {
            SettingsManager.OnSettingsChanged -= SyncMenuItem;
            LocalizationManager.OnLanguageChanged -= ApplyTranslation;
        }

        public override bool ReceiveDirectionalInput()
        {
            if (Util.AnyInputActionListDown(GameplayUI.Instance.leftActions))
            {
                Decrement();
                return true;
            }
            else if (Util.AnyInputActionListDown(GameplayUI.Instance.rightActions))
            {
                Increment();
                return true;
            }

            return false;
        }

        private void SyncCurrentOption()
        {
            if (dataType == SettingsType.Int)
            {
                currentOption = options.FindIndex(o => o.value == SettingsManager.GetInt(saveKey).ToString());
            }
            else if (dataType == SettingsType.String)
            {
                currentOption = options.FindIndex(o => o.value == SettingsManager.GetString(saveKey));
            }
            currentOption = Mathf.Clamp(currentOption, 0, options.Count - 1);
        }

        private void ApplyTranslation(GameTranslation translation)
        {
            UpdateLabel(0);

            label.OnLanguageChanged(translation);
            if (labelDuplicate != null)
            {
                labelDuplicate.OnLanguageChanged(translation);
            }

            // loop through all options and calculate the width of the longest option
            float longestWidth = 0f;
            string originalText = label.GetText();
            foreach (var option in options)
            {
                label.SetText(option.universalLabel == "" ? LocalizationManager.Get(option.localizationKey) : option.universalLabel);
                label.text.ForceMeshUpdate();
                float width = label.text.GetPreferredValues().x;
                if (width > longestWidth)
                {
                    longestWidth = Mathf.Ceil(width);
                }
            }
            label.SetText(originalText);
            targetLabelWidth = Mathf.Max(longestWidth, minLabelWidth) + labelPadding;

            UpdateLabelWidth(labelRT);
            if (labelDuplicateRT != null)
            {
                UpdateLabelWidth(labelDuplicateRT);
            }
        }

        private void UpdateLabelWidth(RectTransform rt)
        {
            // set parent first
            rt.transform.parent.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetLabelWidth);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetLabelWidth);
        }   

        private bool CanIncrement()
        {
            return options.Count > 0 && currentOption < options.Count - 1;
        }

        public void Increment()
        {
            if (CanIncrement())
            {
                currentOption++;
                UpdateSetting(1);
            }
            else
            {
            }
        }

        public void IncrementWithWrap()
        {
            if (CanIncrement())
            {
                Increment();
            }
            else
            {
                currentOption = 0;
                UpdateSetting(1);
            }
        }

        private bool CanDecrement()
        {
            return options.Count > 0 && currentOption > 0;
        }

        public void Decrement()
        {
            if (CanDecrement())
            {
                currentOption--;
                UpdateSetting(-1);
            }
        }

        public void DecrementWithWrap()
        {
            if (CanDecrement())
            {
                Decrement();
            }
            else
            {
                currentOption = options.Count - 1;
                UpdateSetting(-1);
            }
        }

        private void UpdateSetting(int dir)
        {
            if (dataType == SettingsType.Int)
            {
                SettingsManager.SetInt(saveKey, int.Parse(options[currentOption].value));
            }
            else if (dataType == SettingsType.String)
            {
                SettingsManager.SetString(saveKey, options[currentOption].value);
            }

            UpdateLabel(dir);
        }

        private void UpdateLabel(int dir)
        {
            string newText = "";
            string newLocalizationKey = "";

            var currentOptionSafe = options[currentOption];
            if (currentOptionSafe.universalLabel != null && currentOptionSafe.universalLabel != "")
            {
                newText = currentOptionSafe.universalLabel;
            } 
            else 
            {
                newLocalizationKey = currentOptionSafe.localizationKey;
            }

            if (dir != 0)
            {
                if (!duplicatesSetUp)
                {
                    labelPos = label.transform.localPosition;
                    incPos = incButton.transform.localPosition;
                    decPos = decButton.transform.localPosition;

                    // Create duplicate text object
                    var labelDuplicateObj = Instantiate(label.gameObject, label.transform.parent);
                    labelDuplicateObj.name = label.gameObject.name + "_duplicate";
                    labelDuplicate = labelDuplicateObj.GetComponent<Text>();
                    labelDuplicateRT = labelDuplicate.GetComponent<RectTransform>();
                    labelDuplicateCanvasGroup = labelDuplicate.GetComponent<CanvasGroup>();
                    labelDuplicateCanvasGroup.alpha = 0f;

                    UpdateLabelWidth(labelDuplicateRT);

                    duplicatesSetUp = true;
                }

                StartCoroutine(AnimateChange(label.text.text, newText, newLocalizationKey, dir));
            }
            else
            {
                if (newLocalizationKey != "")
                {
                    label.SetLocalizationKey(newLocalizationKey);
                }
                else
                {
                    label.SetText(newText);
                }
            }

            if (groupButton != null)
            {
                if (!groupButton.isDisabled)
                {
                    incButton.SetDisabled(!CanIncrement());
                    decButton.SetDisabled(!CanDecrement());
                }
                else
                {
                    incButton.SetDisabled(true);
                    decButton.SetDisabled(true);
                }
            }
        }

        public void SyncMenuItem()
        {
            SyncCurrentOption();
            UpdateLabel(0);
        }

        public bool ReceiveInput(InputAction inAction)
        {
            if (inAction == InputAction.Left)
            {
                if (CanDecrement())
                {
                    Decrement();
                }

                return true;
            }
            else if (inAction == InputAction.Right)
            {
                if (CanIncrement())
                {
                    Increment();
                }

                return true;
            }

            return false;
        }

        private IEnumerator AnimateChange(string prev, string next, string nextLocalizationKey, int dir)
        {
            if (nextLocalizationKey != "" && nextLocalizationKey != null)
            {
                label.SetLocalizationKey(nextLocalizationKey);
            } else
            {
                label.SetText(next);
            }
            labelCanvasGroup.alpha = 0f;
            label.transform.localPosition = labelPos + dir * labelAnimationOffset;
            if(label.text) label.text.ForceMeshUpdate();

            labelDuplicate.SetText(prev);
            labelDuplicateCanvasGroup.alpha = 1f;
            labelDuplicate.transform.localPosition = labelPos;
            if(labelDuplicate.text) labelDuplicate.text.ForceMeshUpdate();
            if(label.text) labelDuplicate.text.color = label.text.color;

            float t = 0f;
            while (t < labelAnimationTime)
            {
                t += Time.unscaledDeltaTime;
                float prog = Mathf.Clamp01(t / labelAnimationTime);

                labelCanvasGroup.alpha = prog;
                label.transform.localPosition = labelPos + (1 - prog) * dir * labelAnimationOffset;

                labelDuplicateCanvasGroup.alpha = 1f - prog;
                labelDuplicate.transform.localPosition = labelPos + prog * dir * -labelAnimationOffset;
                if (label.text)
                {
                    labelDuplicate.text.color = label.text.color;
                }

                if (dir == 1)
                {
                    incButton.transform.localPosition = incPos + arrowAnimationCurve.Evaluate(prog) * arrowAnimationOffset;
                }
                else if (dir == -1)
                {
                    decButton.transform.localPosition = decPos + arrowAnimationCurve.Evaluate(prog) * arrowAnimationOffset;
                }

                yield return null;
            }

            labelDuplicateCanvasGroup.alpha = 0f;
            labelCanvasGroup.alpha = 1f;
            label.transform.localPosition = labelPos;

            incButton.transform.localPosition = incPos;
            decButton.transform.localPosition = decPos;
        }
    }
};
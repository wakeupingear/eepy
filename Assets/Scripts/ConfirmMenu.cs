using UnityEngine;
using UnityEngine.Events;

namespace Eepy
{
    public class ConfirmMenu : MenuScreen
    {
        [SerializeField]
        private Text title;
        [SerializeField]
        private MenuButton confirmButton;

        public void SetTitleLocalizationKeyAndAction(string key, string fallbackText, UnityAction onConfirm)
        {
            title.SetLocalizationKey(key, fallbackText);

            confirmButton.OnClick.RemoveAllListeners();
            confirmButton.OnClick.AddListener(onConfirm);
        }

        public void SetTitleTextAndAction(string text, UnityAction onConfirm)
        {
            title.SetText(text);

            confirmButton.OnClick.RemoveAllListeners();
            confirmButton.OnClick.AddListener(onConfirm);
        }
    }
};
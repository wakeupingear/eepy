using System.Collections.Generic;
using UnityEngine;

namespace Eepy
{
    [CreateAssetMenu(fileName = "ControllerConfig", menuName = "Scriptable Objects/Controller Config")]
    public class ControllerConfig : ScriptableObject
    {
        public string controllerName;
        public string controllerNameLocalizationKey;
        // Terms to search for in a controller's device name
        public List<string> searchTerms;

        public List<InputManager.ControllerKeyBinding> keyBindings;
        public List<InputManager.ControllerAxisBinding> axisBindings;

        public Sprite dPadSprite;
        public Sprite leftStickSprite;
        public Sprite rightStickSprite;
    }
};
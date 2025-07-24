using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

// Modify this enum to add/remove input actions
public enum InputAction
{
    Up = 1,
    Left = 2,
    Down = 3,
    Right = 4,
    Interact = 9,
    Pause = 10,
}

public class InputManager : MonoBehaviour
{
    #region Static Variables

    public static InputManager Instance { get; private set; }

    // Toggle to disable input manager (ex: for button remapping)
    public static bool isInputManagerEnabled = true;

    public static PrimaryInputType primaryInputType { get; private set; } = PrimaryInputType.Unknown;

    public static event Action<List<ControllerConfig>> OnGamepadConnected;
    public static event Action<List<ControllerConfig>> OnGamepadDisconnected;
    public static event Action<List<InputAction>> OnInputActionsChanged;

    public static ControllerConfig primaryControllerConfig
    {
        get
        {
            return lastPrimaryController ?? Instance.defaultControllerConfig;
        }
    }

    // Bools for whether each controller axis is fully available for movement
    // This is useful for displaying controller glyphs - you likely want to show a 'movement' glyph that represents the Up/Down/Left/Right actions
    // By default, this will likely be an axis - LStick, DPad, or RStick
    // But if a player rebinds these inputs, you can no longer just show a unified symbol
    public static bool lStickBoundToMovement { get; private set; } = false;
    public static bool rStickBoundToMovement { get; private set; } = false;
    public static bool dPadBoundToMovement { get; private set; } = false;

    // Represents how cursor visibility should be handled
    public enum CursorBehavior
    {
        Hide = 0,
        Show = 1,
        HideUntilMove = 2,
    }
    [SerializeField]
    private CursorBehavior cursorBehavior = CursorBehavior.HideUntilMove;

    public static void SetCursorBehavior(CursorBehavior behavior)
    {
        switch (behavior)
        {
            case CursorBehavior.Hide:
                Cursor.visible = false;
                break;
            case CursorBehavior.Show:
                Cursor.visible = true;
                break;
            default:
                break;
        }
        Instance.cursorBehavior = behavior;
    }

    // Returns true while the user holds down the key identified by the key InputAction enum parameter.
    public static bool GetKey(InputAction action, bool allowWhenInputManagerDisabled = false)
    {
        if ((isInputManagerEnabled || allowWhenInputManagerDisabled) && inputStatesMap.TryGetValue(action, out InputState state))
        {
            return state.held;
        }

        return false;
    }

    // Returns true during the frame the user starts pressing down the key identified by the key InputAction enum parameter.
    // This also fires for repeat presses, not just the initial press.
    public static bool GetKeyDown(InputAction action, bool allowWhenInputManagerDisabled = false, uint maxRepeats = uint.MaxValue)
    {
        if ((isInputManagerEnabled || allowWhenInputManagerDisabled) && inputStatesMap.TryGetValue(action, out InputState state))
        {
            return state.numRepeatedPresses <= maxRepeats && state.pressedThisFrame;
        }

        return false;
    }

    // Returns true during the frame the user releases the key identified by the key KeyCode enum parameter.
    public static bool GetKeyUp(InputAction action, bool allowWhenInputManagerDisabled = false)
    {
        if ((isInputManagerEnabled || allowWhenInputManagerDisabled) && inputStatesMap.TryGetValue(action, out InputState state))
        {
            return state.releasedThisFrame;
        }

        return false;
    }
    public static InputState GetKeyState(InputAction action, bool allowWhenInputManagerDisabled = false)
    {
        if ((isInputManagerEnabled || allowWhenInputManagerDisabled) && inputStatesMap.TryGetValue(action, out InputState state))
        {
            return state;
        }

        return null;
    }

    public static void QueueManualInput(InputAction action)
    {
        manualInputActions.Add(action);
    }

    // Different types of input that can have primary control
    public enum PrimaryInputType
    {
        Unknown = 0,
        Keyboard = 1,
        Controller = 2,
    }

    [Serializable]
    public struct ControllerKeyBinding
    {
        public ControllerCode code;
        public KeyCode keyCode;
        public Sprite keySprite;
    }

    public enum ControllerAxisDir
    {
        Positive,
        Negative
    }
    [Serializable]
    public struct ControllerAxisBinding
    {
        public ControllerCode code;
        public string axis;
        public ControllerAxisDir dir;
        public Sprite keySprite;
    }

    public static void Rumble(ushort intensity, float duration)
    {
#if !DISABLESTEAMWORKS
        if (SteamManager.Initialized)
        {
            try
            {
                InputHandle_t[] inputHandles = new InputHandle_t[Constants.STEAM_INPUT_MAX_COUNT];
                SteamInput.GetConnectedControllers(inputHandles);
                SteamInput.TriggerVibration(inputHandles[0], intensity, intensity);

                rumbleDuration = duration;
            }
            catch (Exception e)
            {
                Util.LogError("Error rumbling: " + e.Message);
            }
        }
#endif
    }

    public static void StopRumble()
    {
        rumbleDuration = 0f;
        Rumble(0, 0f);
    }

    public static void AddKeyBinding(InputAction action, int key)
    {
        if (keyBindings.ContainsKey(action))
        {
            if (!keyBindings[action].Contains(key))
            {
                keyBindings[action].Add(key);
                OnInputActionsChanged?.Invoke(new List<InputAction> { action });
                SaveKeyBindings();
            }
        }
        else
        {
            keyBindings[action] = new List<int> { key };
            OnInputActionsChanged?.Invoke(new List<InputAction> { action });
            SaveKeyBindings();
        }
    }

    public static Dictionary<InputAction, List<int>> GetAllKeyBindings()
    {
        return keyBindings;
    }

    public static Dictionary<InputAction, List<int>> GetAllDefaultKeyBindings()
    {
        return defaultBindings;
    }

    public static List<int> GetKeyBindings(InputAction action, bool mixedInput = true)
    {
        if (keyBindings.ContainsKey(action))
        {
            if (mixedInput)
            {
                return keyBindings[action];
            }

            if (primaryInputType == PrimaryInputType.Controller)
            {
                return keyBindings[action].Where(code => !Util.InputCodeIsKeyboard(code)).ToList();
            }
            else
            {
                return keyBindings[action].Where(code => Util.InputCodeIsKeyboard(code)).ToList();
            }
        }

        return new List<int>();
    }

    public static bool CanAddKeyBinding(int key)
    {
        int keyToCheck = (int)GetNormalizedKeyCode((KeyCode)key);
        if (connectedControllerKeyCodeToControllerCode.ContainsKey((KeyCode)keyToCheck))
        {
            keyToCheck = (int)connectedControllerKeyCodeToControllerCode[(KeyCode)keyToCheck];
        }

        bool found = false;
        foreach (var keyCode in keyBindings.Keys)
        {
            if (keyBindings[keyCode].Contains(keyToCheck))
            {
                found = true;
                break;
            }
        }


        return !found;
    }

    public static bool CanRemoveKeyBinding(InputAction action, int key)
    {
        if (keyBindings.ContainsKey(action))
        {
            return keyBindings[action].Count(code => Util.InputCodeIsKeyboard(code) == Util.InputCodeIsKeyboard(key)) > 1;
        }

        return false;
    }

    public static bool RemoveKeyBinding(InputAction action, int key)
    {
        if (!CanRemoveKeyBinding(action, key))
        {
            return false;
        }

        if (keyBindings.ContainsKey(action))
        {
            keyBindings[action].Remove(key);
            SaveKeyBindings();
            OnInputActionsChanged?.Invoke(new List<InputAction> { action });

            return true;
        }

        return false;
    }

    public static void ResetAllBindings()
    {
        keyBindings.Clear();
        FillUnassignedBindings(keyBindings);
        OnInputActionsChanged?.Invoke(keyBindings.Keys.ToList());

        SaveKeyBindings();
    }

    public static int GetCodeFromRawKeyCode(KeyCode keyCode)
    {
        if (keyCode < KeyCode.JoystickButton0)
        {
            if (Instance.keyCodeAliases.ContainsKey(keyCode))
            {
                keyCode = Instance.keyCodeAliases[keyCode];
            }
            if (Instance.disabledKeyCodes.Contains(keyCode))
            {
                return (int)KeyCode.None;
            }

            return (int)keyCode;
        }

        // map back to controller code
        KeyCode normalizedKeyCode = GetNormalizedKeyCode(keyCode);
        foreach (var controller in connectedControllers)
        {
            foreach (var mapping in controller.keyBindings)
            {
                if (mapping.keyCode == normalizedKeyCode)
                {
                    return (int)mapping.code;
                }
            }
        }
        return -1;
    }

    public static void DisableInputManager(bool disabled)
    {
        // Reset input states if the state is being changed
        if (isInputManagerEnabled == disabled)
        {
            foreach (InputState state in inputStatesMap.Values)
            {
                state.held = false;
                state.releasedThisFrame = false;
                state.pressedThisFrame = false;
                state.numRepeatedPresses = 0;
                state.nextInputTime = 0f;
            }
        }

        isInputManagerEnabled = !disabled;
    }

    public static ControllerAxisBinding? GetActiveUnusedAxis()
    {
        foreach (var controller in connectedControllers)
        {
            foreach (var axis in controller.axisBindings)
            {
                if (Util.IsAxisPressed(axis))
                {
                    return axis;
                }
            }
        }

        return null;
    }

    #endregion

    #region Inspector Variables

    // Tracks the state of a given input
    // We use the same struct for both user config and internal state, which simplifies the code a lot
    // We also use a class so we can pass by reference

    [Serializable]
    public class InputState
    {
        public InputAction action;
        public bool allowRepeatsWhenHeld = false;
        public AnimationCurve repeatDelay;
        public bool allowRebinding = true;

        [HideInInspector]
        public bool held = false;
        [HideInInspector]
        public bool releasedThisFrame = false;
        [HideInInspector]
        public bool pressedThisFrame = false;
        [HideInInspector]
        public int numRepeatedPresses = 0;
        [HideInInspector]
        public float nextInputTime = 0f;
    }

    [SerializeField, Tooltip("Configure all InputAction behavior here.")]
    private InputState[] inputConfigs;

    [Serializable]
    private struct SavedKeyBindings
    {
        public InputAction action;
        public int[] keyCodes;
    }

    [Serializable]
    private struct SavedKeyBindingsList
    {
        public List<SavedKeyBindings> bindings;
    }

    // Default keyboard bindings
    [SerializeField, Tooltip("Default keyboard bindings. Players can override these bindings in-game.")]
    private Dictionary<InputAction, List<KeyCode>> defaultKeyBindings = new Dictionary<InputAction, List<KeyCode>>
    {
        { InputAction.Up, new List<KeyCode> { KeyCode.W, KeyCode.UpArrow } },
        { InputAction.Left, new List<KeyCode> { KeyCode.A, KeyCode.LeftArrow } },
        { InputAction.Down, new List<KeyCode> { KeyCode.S, KeyCode.DownArrow } },
        { InputAction.Right, new List<KeyCode> { KeyCode.D, KeyCode.RightArrow } },
        { InputAction.Interact, new List<KeyCode> { KeyCode.E, KeyCode.Return } },
        { InputAction.Pause, new List<KeyCode> { KeyCode.Escape } }
    };

    // Default controller bindings
    [SerializeField, Tooltip("Default controller bindings. Players can override these bindings in-game.")]
    private Dictionary<InputAction, List<ControllerCode>> defaultControllerBindings = new Dictionary<InputAction, List<ControllerCode>>()
    {
        { InputAction.Up, new List<ControllerCode> { ControllerCode.DPadUp, ControllerCode.LeftStickUp } },
        { InputAction.Left, new List<ControllerCode> { ControllerCode.DPadLeft, ControllerCode.LeftStickLeft } },
        { InputAction.Down, new List<ControllerCode> { ControllerCode.DPadDown, ControllerCode.LeftStickDown } },
        { InputAction.Right, new List<ControllerCode> { ControllerCode.DPadRight, ControllerCode.LeftStickRight } },
        { InputAction.Interact, new List<ControllerCode> { ControllerCode.FaceButtonDown } },
        { InputAction.Pause, new List<ControllerCode> { ControllerCode.Select, ControllerCode.Start } }
    };

    [Serializable]
    public struct RumbleProfile
    {
        [Tooltip("Unique name to reference when triggering rumble in code.")]
        public string name;
        [Tooltip("Rumble strength (0 to 65,535).")]
        public ushort intensity;
        [Tooltip("Duration of rumble in seconds (for most actions, < 0.25).")]
        public float duration;
    }
    // All available rumble configs
    [SerializeField, Tooltip("Configure different rumble profiles for different in-game events.")]
    private List<RumbleProfile> rumbleProfiles;

    // Record of for given keyboard key's sprite
    // By default, we use TMP to overlay the character using the active localization font
    [Serializable]
    public struct KeyboardSprite
    {
        public KeyCode keyCode;
        public Sprite keySprite;
    }
    [SerializeField, Tooltip("Sprite overrides for special keys (ex: CTRL, CMD, SHIFT).")]
    private List<KeyboardSprite> keyboardSprites = new List<KeyboardSprite>();

    // Aliases for keyboard keys
    [SerializeField, Tooltip("Alias certain keys to be treated as the same key (ex: RSHIFT = LSHIFT).")]
    private Dictionary<KeyCode, KeyCode> keyCodeAliases = new Dictionary<KeyCode, KeyCode>
    {
        { KeyCode.RightShift, KeyCode.LeftShift },
        { KeyCode.RightAlt, KeyCode.LeftAlt },
        { KeyCode.RightControl, KeyCode.LeftControl },
        { KeyCode.RightMeta, KeyCode.LeftMeta },
        { KeyCode.KeypadPeriod, KeyCode.Period },
    };

    // KeyCodes to disable for input binding
    [SerializeField, Tooltip("Disable certain KeyCodes from being available in the binding menu.")]
    private List<KeyCode> disabledKeyCodes = new List<KeyCode>
    {
        KeyCode.Escape,
        KeyCode.Mouse0,
        KeyCode.Mouse1,
        KeyCode.None,
        KeyCode.LeftWindows,
        KeyCode.LeftMeta,
    };

    // List of all supported controllers
    // If left empty, the input manager will load all controllers from the Resources folder
    [SerializeField, Tooltip("List of all Input Actions that are part of the primary directional movement (ex: moving the character). Don't include menu inputs.")]
    private List<InputAction> movementInputActions;

    // List of all supported controllers
    // If left empty, the input manager will load all controllers from the Resources folder
    [SerializeField, Tooltip("List of the different controller types supported by the game.")]
    private List<ControllerConfig> controllerConfigs;
    // The default controller config to use if no controller is found
    [SerializeField, Tooltip("Default controller to use for glyphs if no controller is currently connected.")]
    private ControllerConfig defaultControllerConfig;

    // How many inputs can be allowed at once
    // Priority is assigned according to position in 'inputTypes'
    [SerializeField, Tooltip("Cap the number of inputs that can be held at the same time.")]
    private int maxSimultaneousInputs = 5;

    #endregion

    #region Private

    private static Vector3 lastMousePosition = Vector3.zero;

    private static Dictionary<InputAction, InputState> inputStatesMap = new Dictionary<InputAction, InputState>();

    // Inputs that are manually set by gameplay code, not by an external input device
    private static HashSet<InputAction> manualInputActions = new HashSet<InputAction>();

    // Key bindings are stored as a list of ints
    // >= 0 is a keyboard key
    // < 0 is a controller input
    private static Dictionary<InputAction, List<int>> keyBindings = new Dictionary<InputAction, List<int>>();
    private static Dictionary<InputAction, List<int>> defaultBindings = new Dictionary<InputAction, List<int>>();
    private const string keyBindingKey = "KeyBindings";

    // The last primary controller that was used
    private static ControllerConfig lastPrimaryController = null;

    // The controllers that are currently connected
    private static List<ControllerConfig> connectedControllers = new List<ControllerConfig>();
    // A bunch of boilerplate for quick lookups about connected controllers
    private struct ControllerBindingEntry<T>
    {
        public T data;
        public ControllerConfig config;
    }
    private static Dictionary<ControllerCode, List<ControllerBindingEntry<KeyCode>>> connectedControllerKeyBindings = new Dictionary<ControllerCode, List<ControllerBindingEntry<KeyCode>>>();
    private static Dictionary<KeyCode, ControllerCode> connectedControllerKeyCodeToControllerCode = new Dictionary<KeyCode, ControllerCode>();
    private static Dictionary<ControllerCode, List<ControllerBindingEntry<ControllerAxisBinding>>> connectedControllerAxisBindings = new Dictionary<ControllerCode, List<ControllerBindingEntry<ControllerAxisBinding>>>();

    // Quick lookup map of keyboard keys to their specified keys
    private static Dictionary<KeyCode, Sprite> keyboardKeySpritesDict = new Dictionary<KeyCode, Sprite>();

    private static float rumbleDuration = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // Initialize the input states map
            if (inputConfigs.Length > 0)
            {
                foreach (var inputState in inputConfigs)
                {
                    inputStatesMap[inputState.action] = inputState;
                }
            }
            else
            {
                Util.LogError("InputManager has no Input Configs set up. No inputs events will be fired.");
            }

            // Set starting cursor behavior
            Cursor.visible = Instance.cursorBehavior == CursorBehavior.Show;

            // Load controller configs if not specified
            if (controllerConfigs == null || controllerConfigs.Count == 0)
            {
                controllerConfigs = new List<ControllerConfig>(Resources.LoadAll<ControllerConfig>("Controllers"));
            }

            // Load existing control bindings
            if (PlayerPrefs.HasKey(keyBindingKey))
            {
                string json = PlayerPrefs.GetString(keyBindingKey);
                var savedBindings = JsonUtility.FromJson<SavedKeyBindingsList>(json);
                foreach (SavedKeyBindings binding in savedBindings.bindings)
                {
                    keyBindings[binding.action] = new List<int>(binding.keyCodes);
                }
            }

            FillUnassignedBindings(keyBindings);
            FillUnassignedBindings(defaultBindings, true);

            foreach (var binding in keyboardSprites)
            {
                keyboardKeySpritesDict[binding.keyCode] = binding.keySprite;
            }

            CheckMovementAxisBindings();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnApplicationQuit()
    {
        StopRumble();
    }

    private void Update()
    {
        PollForControllers();

        if (isInputManagerEnabled)
        {
            if (Instance.cursorBehavior == CursorBehavior.HideUntilMove)
            {
                //Cursor.visible = true;
            }
            //lastMousePosition = Input.mousePosition;
        }

        UpdateInputStates();
    }

    private void PollForControllers()
    {
        var newControllers = new List<ControllerConfig>();
        string[] joystickNames = Input.GetJoystickNames();
        foreach (string joystickName in joystickNames)
        {
            foreach (ControllerConfig controller in controllerConfigs)
            {
                bool found = false;
                foreach (var name in controller.searchTerms)
                {
                    if (joystickName.Contains(name))
                    {
                        newControllers.Add(controller);
                        found = true;
                        if (lastPrimaryController == null)
                        {
                            lastPrimaryController = controller;
                        }

                        break;
                    }
                }
                if (found) break;
            }
        }

        if (newControllers.Count < connectedControllers.Count)
        {
            if (newControllers.Count == 0)
            {
                SetPrimaryInputType(PrimaryInputType.Keyboard);
            }

            List<ControllerConfig> disconnectedControllers = new List<ControllerConfig>();
            foreach (var oldController in connectedControllers)
            {
                if (!newControllers.Contains(oldController))
                {
                    disconnectedControllers.Add(oldController);
                }
            }
            if (disconnectedControllers.Count == 0)
            {
                disconnectedControllers.Add(connectedControllers[connectedControllers.Count - 1]);
            }

            OnGamepadDisconnected?.Invoke(disconnectedControllers);
        }
        else if (newControllers.Count > connectedControllers.Count)
        {
            if (primaryInputType == PrimaryInputType.Unknown)
            {
                SetPrimaryInputType(PrimaryInputType.Controller);
            }

            List<ControllerConfig> connectedControllers = new List<ControllerConfig>();
            foreach (var newController in newControllers)
            {
                if (!connectedControllers.Contains(newController))
                {
                    connectedControllers.Add(newController);
                }
            }
            if (connectedControllers.Count == 0)
            {
                connectedControllers.Add(newControllers[newControllers.Count - 1]);
            }

            OnGamepadConnected?.Invoke(connectedControllers);
        }

        connectedControllers = newControllers;

        connectedControllerKeyBindings.Clear();
        connectedControllerAxisBindings.Clear();
        connectedControllerKeyCodeToControllerCode.Clear();
        for (int i = 0; i < connectedControllers.Count; i++)
        {
            foreach (var mapping in connectedControllers[i].keyBindings)
            {
                KeyCode keyCode = mapping.keyCode + i * (KeyCode.Joystick1Button0 - KeyCode.JoystickButton0);
                if (!connectedControllerKeyBindings.ContainsKey(mapping.code))
                {
                    connectedControllerKeyBindings[mapping.code] = new List<ControllerBindingEntry<KeyCode>>();
                }
                connectedControllerKeyBindings[mapping.code].Add(new ControllerBindingEntry<KeyCode> { data = keyCode, config = connectedControllers[i] });
                connectedControllerKeyCodeToControllerCode[keyCode] = mapping.code;
            }

            foreach (var axis in connectedControllers[i].axisBindings)
            {
                if (!connectedControllerAxisBindings.ContainsKey(axis.code))
                {
                    connectedControllerAxisBindings[axis.code] = new List<ControllerBindingEntry<ControllerAxisBinding>>();
                }
                connectedControllerAxisBindings[axis.code].Add(new ControllerBindingEntry<ControllerAxisBinding> { data = axis, config = connectedControllers[i] });
            }
        }

        // Update rumble
        if (rumbleDuration > 0f)
        {
            rumbleDuration -= Time.unscaledDeltaTime;
            if (rumbleDuration <= 0f)
            {
                StopRumble();
                rumbleDuration = 0f;
            }
        }
    }

    private void UpdateInputStates()
    {
        int simultaneousInputs = 0;
        foreach (InputState inputState in inputStatesMap.Values)
        {
            bool input = false;

            if (manualInputActions.Contains(inputState.action))
            {
                input = true;
            }
            else if (simultaneousInputs <= maxSimultaneousInputs)
            {
                foreach (int _code in keyBindings[inputState.action])
                {
                    if (Util.InputCodeIsKeyboard(_code))
                    {
                        KeyCode code = (KeyCode)_code;
                        if (Input.GetKey(code))
                        {
                            SetPrimaryInputType(PrimaryInputType.Keyboard);
                            input = true;
                        }
                    }
                    else
                    {
                        ControllerCode code = (ControllerCode)_code;
                        if (connectedControllerKeyBindings.ContainsKey(code))
                        {
                            foreach (ControllerBindingEntry<KeyCode> entry in connectedControllerKeyBindings[code])
                            {
                                if (Input.GetKey(entry.data))
                                {
                                    SetPrimaryInputType(PrimaryInputType.Controller);
                                    lastPrimaryController = entry.config;
                                    input = true;
                                    break;
                                }
                            }
                        }

                        if (connectedControllerAxisBindings.ContainsKey(code))
                        {
                            foreach (ControllerBindingEntry<ControllerAxisBinding> axis in connectedControllerAxisBindings[code])
                            {
                                if (Util.IsAxisPressed(axis.data))
                                {
                                    SetPrimaryInputType(PrimaryInputType.Controller);
                                    lastPrimaryController = axis.config;
                                    input = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (input)
            {
                if ((inputState.allowRepeatsWhenHeld || inputState.numRepeatedPresses == 0) && inputState.nextInputTime <= float.Epsilon)
                {
                    // Delay until the next input is determined by the curve
                    // Put keyframes at 1-unit intervals to fine tune the curve, or make it a flat line to have a constant rate
                    inputState.nextInputTime = inputState.repeatDelay.Evaluate(inputState.numRepeatedPresses);

                    inputState.pressedThisFrame = true;
                    inputState.numRepeatedPresses++;
                }
                else
                {
                    inputState.pressedThisFrame = false;
                    if (inputState.allowRepeatsWhenHeld)
                    {
                        inputState.nextInputTime -= Time.unscaledDeltaTime;
                    }
                }

                inputState.held = true;
                inputState.releasedThisFrame = false;
                simultaneousInputs++;
            }
            else if (inputState.held)
            {
                // It had been held, but not anymore
                inputState.pressedThisFrame = false;
                inputState.releasedThisFrame = inputState.held;
                inputState.held = false;
                inputState.numRepeatedPresses = 0;
                inputState.nextInputTime = 0f;
            }
        }

        manualInputActions.Clear();
    }

    private void SetPrimaryInputType(PrimaryInputType inputType)
    {
        primaryInputType = inputType;

        if (Instance.cursorBehavior == CursorBehavior.HideUntilMove)
        {
            Cursor.visible = false;
        }
    }

    private static bool ControllerCodeAssignedToInputAction(ControllerCode code, InputAction action)
    {
        return keyBindings.ContainsKey(action) && keyBindings[action].Contains((int)code);
    }

    private static void CheckMovementAxisBindings()
    {
        lStickBoundToMovement = Instance.movementInputActions.All((action) => 
            ControllerCodeAssignedToInputAction(ControllerCode.LeftStickUp, action) ||
            ControllerCodeAssignedToInputAction(ControllerCode.LeftStickDown, action) ||
            ControllerCodeAssignedToInputAction(ControllerCode.LeftStickLeft, action) ||
            ControllerCodeAssignedToInputAction(ControllerCode.LeftStickRight, action)
        );
        rStickBoundToMovement = Instance.movementInputActions.All((action) => 
            ControllerCodeAssignedToInputAction(ControllerCode.RightStickUp, action) ||
            ControllerCodeAssignedToInputAction(ControllerCode.RightStickDown, action) ||
            ControllerCodeAssignedToInputAction(ControllerCode.RightStickLeft, action) ||
            ControllerCodeAssignedToInputAction(ControllerCode.RightStickRight, action)
        );
        dPadBoundToMovement = Instance.movementInputActions.All((action) => 
            ControllerCodeAssignedToInputAction(ControllerCode.DPadUp, action) ||
            ControllerCodeAssignedToInputAction(ControllerCode.DPadDown, action) ||
            ControllerCodeAssignedToInputAction(ControllerCode.DPadLeft, action) ||
            ControllerCodeAssignedToInputAction(ControllerCode.DPadRight, action)
        );
    }

    private static KeyCode GetNormalizedKeyCode(KeyCode keyCode)
    {
        if (keyCode >= KeyCode.JoystickButton0)
        {
            return KeyCode.JoystickButton0 + (keyCode - KeyCode.JoystickButton0) % (KeyCode.Joystick1Button0 - KeyCode.JoystickButton0);
        }

        return keyCode;
    }

    private static void SaveKeyBindings()
    {
        var savedBindings = new List<SavedKeyBindings>();
        foreach (var key in keyBindings.Keys)
        {
            savedBindings.Add(new SavedKeyBindings
            {
                action = key,
                keyCodes = keyBindings[key].ToArray()
            });
        }
        SavedKeyBindingsList savedBindingsList = new SavedKeyBindingsList
        {
            bindings = savedBindings
        };
        string json = JsonUtility.ToJson(savedBindingsList);
        PlayerPrefs.SetString(keyBindingKey, json);
        PlayerPrefs.Save();

        CheckMovementAxisBindings();
    }

    private static void FillUnassignedBindings(Dictionary<InputAction, List<int>> bindings, bool forceChange = false)
    {
        foreach (var action in Enum.GetValues(typeof(InputAction)))
        {
            if (!bindings.ContainsKey((InputAction)action) && Instance.defaultKeyBindings.ContainsKey((InputAction)action))
            {
                // merge the controller and keyboard default bindings
                bindings[(InputAction)action] = new List<int>(Instance.defaultKeyBindings[(InputAction)action].Select(code => (int)code));
                bindings[(InputAction)action].AddRange(Instance.defaultControllerBindings[(InputAction)action].Select(code => (int)code));
            }
        }
    }

    public static List<InputState> GetInputStates()
    {
        return Instance.inputConfigs.ToList();
    }

    public static bool TryGetKeySprite(int key, out Sprite sprite)
    {
        if (Util.InputCodeIsKeyboard(key) && keyboardKeySpritesDict.TryGetValue((KeyCode)key, out sprite))
        {
            return true;
        }

        if (primaryControllerConfig != null)
        {     
            foreach (var binding in primaryControllerConfig.keyBindings)
            {
                if (binding.code == (ControllerCode)key)
                {
                    sprite = binding.keySprite;
                    return true;
                }
            }

            foreach (var binding in primaryControllerConfig.axisBindings)
            {
                if (binding.code == (ControllerCode)key)
                {
                    sprite = binding.keySprite;
                    return true;
                }
            }
        }

        sprite = null;
        return false;
    }

    #endregion
}

// Basically KeyCode, but for controllers
// This is built for only 1 controller
public enum ControllerCode
{
    DPadUp = -1,
    DPadDown = -2,
    DPadLeft = -3,
    DPadRight = -4,
    FaceButtonUp = -5, // Using the Switch naming convention for face buttons
    FaceButtonDown = -6,
    FaceButtonLeft = -7,
    FaceButtonRight = -8,
    LeftShoulder = -9,
    RightShoulder = -10,
    LeftTrigger = -11,
    RightTrigger = -12,
    Select = -13,
    Start = -14,
    LeftStickClick = -15,
    RightStickClick = -16,
    LeftStickUp = -17,
    LeftStickDown = -18,
    LeftStickLeft = -19,
    LeftStickRight = -20,
    RightStickUp = -21,
    RightStickDown = -22,
    RightStickLeft = -23,
    RightStickRight = -24,
}
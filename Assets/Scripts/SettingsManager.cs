using UnityEngine;
using System;

namespace Eepy
{
    [DisallowMultipleComponent]
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        public static event Action OnSettingsChanged;

        public enum SettingsType
        {
            Int,
            String
        }

        #region VSync
        [HideInInspector]
        public static bool isVsyncEnabled { get; private set; }
        public const string VSyncKey = "VSync";
        public const int DefaultVSyncValue = 0;
        private static void ApplyVSync()
        {
            QualitySettings.vSyncCount = isVsyncEnabled ? 1 : 0;
        }
        public static void SetVSync(bool enabled)
        {
            isVsyncEnabled = enabled;
            SetInt(VSyncKey, enabled ? 1 : 0);
        }
        #endregion

        #region Display
        public enum DisplayMode
        {
            Fullscreen = 0,
            Windowed = 1
        }
        public static DisplayMode displayMode { get; private set; }
        public const string DisplayModeKey = "DisplayMode";
        public const int DefaultDisplayModeValue = 0;
        private static void ApplyDisplayMode()
        {
            switch (displayMode)
            {
                case DisplayMode.Fullscreen:
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                    break;
                case DisplayMode.Windowed:
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                    break;
            }
            Screen.fullScreen = displayMode == DisplayMode.Fullscreen;

            Util.ClampFramerateToDisplay();
        }
        public static void SetDisplayMode(DisplayMode mode)
        {
            displayMode = mode;
            SetInt(DisplayModeKey, (int)mode);
        }
        #endregion

        #region Volume

        #region Music
        public static int musicVolume { get; private set; }
        public const string MusicVolumeKey = "MusicVolume";
        public const int DefaultMusicVolumeValue = 80;
        private static void ApplyMusicVolume()
        {
            // TODO: Add your implementation here
        }
        public static void SetMusicVolume(int volume)
        {
            musicVolume = volume;
            SetInt(MusicVolumeKey, volume);
        }
        #endregion

        #region SFX
        public static int sfxVolume { get; private set; }
        public const string SFXVolumeKey = "SFXVolume";
        public const int DefaultSFXVolumeValue = 80;
        private static void ApplySFXVolume()
        {
            // TODO: Add your implementation here
        }
        public static void SetSFXVolume(int volume)
        {
            sfxVolume = volume;
            SetInt(SFXVolumeKey, volume);
        }
        #endregion

        #region Master
        public static int mainVolume { get; set; }
        public const string MainVolumeKey = "MainVolume";
        public const int DefaultMainVolumeValue = 80;
        private static void ApplyMainVolume()
        {
            // TODO: Add your implementation here
        }
        public static void SetMainVolume(int volume)
        {
            mainVolume = volume;
            SetInt(MainVolumeKey, volume);
        }
        #endregion

        #endregion

        #region Language
        public static string language { get; private set; }
        public static bool isLanguageSet { get; private set; } = false;
        public const string LanguageKey = "GameLanguage";
        public const string DefaultLanguageValue = "en";
        private static void ApplyLanguage()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.LoadLanguage(language);
            }
        }
        public static void SetLanguage(string code)
        {
            language = code;
            isLanguageSet = true;
            SetString(LanguageKey, code);
        }
        #endregion

        #region Resolution
        public static string resolution { get; private set; }
        public const string ResolutionKey = "Resolution";
        private static void ApplyResolution()
        {
            if (resolution != null)
            {
                string[] res = resolution.Split('x');
                int width = int.Parse(res[0]);
                int height = int.Parse(res[1]);
                Screen.SetResolution(width, height, displayMode == DisplayMode.Fullscreen);

                Util.ClampFramerateToDisplay();
            }
        }
        public static void SetResolution(int width, int height)
        {
            resolution = ResolutionToString(width, height);
            SetString(ResolutionKey, resolution);
        }
        public static string ResolutionToString(int width, int height)
        {
            return $"{width}x{height}";
        }
        #endregion

        #region Rumble
        public static int rumble { get; private set; }
        public const string RumbleKey = "Rumble";
        public const int DefaultRumbleValue = 8;
        private static void ApplyRumble()
        {
            // Do nothing for now since InputManager handles the rumble
        }
        public static void SetRumble(int intensity)
        {
            rumble = intensity;
            SetInt(RumbleKey, intensity);
        }
        #endregion

        [SerializeField]
        private float saveDelay = 1f;

        private static float saveTimer = 0f;
        private static bool isApplicationInFullscreen = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;

                isVsyncEnabled = PlayerPrefs.GetInt(VSyncKey, 0) == 1;
                ApplyVSync();

                displayMode = (DisplayMode)PlayerPrefs.GetInt(DisplayModeKey, (int)DisplayMode.Fullscreen);
                ApplyDisplayMode();
                isApplicationInFullscreen = Screen.fullScreen;

                musicVolume = PlayerPrefs.GetInt(MusicVolumeKey, DefaultMusicVolumeValue);
                ApplyMusicVolume();
                sfxVolume = PlayerPrefs.GetInt(SFXVolumeKey, DefaultSFXVolumeValue);
                ApplySFXVolume();
                mainVolume = PlayerPrefs.GetInt(MainVolumeKey, DefaultMainVolumeValue);
                ApplyMainVolume();

                string langNotSet = "not-set";
                language = PlayerPrefs.GetString(LanguageKey, langNotSet);
                if (language != langNotSet)
                {
                    isLanguageSet = true;
                }
                else
                {
                    language = DefaultLanguageValue;
                }
                ApplyLanguage();

                resolution = PlayerPrefs.GetString(ResolutionKey, $"{Screen.currentResolution.width}x{Screen.currentResolution.height}");
                ApplyResolution();

                rumble = PlayerPrefs.GetInt(RumbleKey, DefaultRumbleValue);
                ApplyRumble();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            if (!isLanguageSet && LocalizationManager.IsLocalizationEnabled())
            {
                language = SteamManager.GetCurrentGameLanguage();
                Util.Log("Auto-setting language to user's language: " + language);
                ApplyLanguage();
            }
        }

        private void Update()
        {
            if (isApplicationInFullscreen != Screen.fullScreen)
            {
                SetInt(DisplayModeKey, (int)(Screen.fullScreen ? DisplayMode.Fullscreen : DisplayMode.Windowed));
                isApplicationInFullscreen = Screen.fullScreen;
                OnSettingsChanged?.Invoke();
            }

            if (saveTimer > 0f)
            {
                saveTimer -= Time.unscaledDeltaTime;
                if (saveTimer <= 0f)
                {
                    Save();
                }
            }
        }

        private static void Save()
        {
            saveTimer = 0f;
            PlayerPrefs.Save();
        }

        public static int SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            saveTimer = Instance.saveDelay;
            switch (key)
            {
                case VSyncKey:
                    isVsyncEnabled = value == 1;
                    ApplyVSync();
                    break;
                case DisplayModeKey:
                    displayMode = (DisplayMode)value;
                    ApplyDisplayMode();
                    break;
                case MusicVolumeKey:
                    musicVolume = value;
                    ApplyMusicVolume();
                    break;
                case SFXVolumeKey:
                    sfxVolume = value;
                    ApplySFXVolume();
                    break;
                case MainVolumeKey:
                    mainVolume = value;
                    ApplyMainVolume();
                    break;
                case RumbleKey:
                    rumble = value;
                    ApplyRumble();
                    break;
            }

            OnSettingsChanged?.Invoke();

            return value;
        }
        public static string SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            saveTimer = Instance.saveDelay;
            switch (key)
            {
                case LanguageKey:
                    language = value;
                    ApplyLanguage();
                    break;
                case ResolutionKey:
                    resolution = value;
                    ApplyResolution();
                    break;
            }

            OnSettingsChanged?.Invoke();

            return value;
        }

        public static int GetInt(string key)
        {
            switch (key)
            {
                case VSyncKey:
                    return isVsyncEnabled ? 1 : 0;
                case DisplayModeKey:
                    return (int)displayMode;
                case MusicVolumeKey:
                    return musicVolume;
                case SFXVolumeKey:
                    return sfxVolume;
                case MainVolumeKey:
                    return mainVolume;
                case RumbleKey:
                    return rumble;
                default:
                    Debug.LogWarning($"SettingsManager: GetInt() - Key '{key}' not found. Returning default value.");
                    return PlayerPrefs.GetInt(key, 0);
            }
        }

        public static string GetString(string key)
        {
            switch (key)
            {
                case LanguageKey:
                    return language;
                case ResolutionKey:
                    return resolution;
                default:
                    Debug.LogWarning($"SettingsManager: GetString() - Key '{key}' not found. Returning default value.");
                    return PlayerPrefs.GetString(key, string.Empty);
            }
        }
    }
};
# Eepy: An Opinionated Unity Starter Kit

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/wakeupingear/eepy/actions)
[![Version](https://img.shields.io/badge/version-v1.0.0-blue)](https://github.com/wakeupingear/eepy/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

All the boilerplate you'll need for your Unity game. Opinionated, simple, battle-tested on Steam, and free.

## Features

*   **A Better Input System**
    *   Define typesafe input actions in code.
    *   Configure per-key settings like input repeating and thresholds.
    *   **Replaces Rewired**

*   **Native Controller Support**
    *   Out-of-the-box support for DualShock, DualSense, Xbox, and Steam Deck controllers.
    *   Easily create custom configs for niche or unsupported controllers.
    *   Optional vibration with the Steam Input API

*   **Menu System:**
    *   A collection of UI primitives to build your game's menus.
    *   Pre-built prefabs for essential menus like Pause, Input Remapping, Settings, and Localization.

*   **Multiplatform Build System:**
    *   Define custom configurations for different build targets, streamlining the process of deploying to multiple platforms.
    *   **Replaces Unity 6's Build Profiles**

*   **Settings System:**
    *   Preconfigured handlers for most common game settings (resolution, volume levels, etc).
    *   Easily define and serialize custom settings.

*   **Steam Integration:**
    *   Built-in support for Steamworks features like achievements and stats.
    *   Powered by the amazing [Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET) by [@rlabrecque](https://github.com/rlabrecque)

## Getting Started

### Prerequisites

*   Unity 2021.3 or later
    * Earlier Editor versions should also work but are currently untested.
*   [Text Mesh Pro](https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/manual/index.html)
    * Included by default in new Unity projects!
*   Don't use the New Input System

### Installation

1.  [**Download the Latest Release**](https://github.com/wakeupingear/eepy/releases)
2.  **Import the UnityPackage:**
    *   Launch your Unity project.
    *   Open the package importer: `Assets -> Import Package -> Custom Package`.
    *   Select the downloaded `.unitypackage` file.
    *   Choose which files to import. It's recommended that you select all of them to start, as many of the assets depend on each other.

## Documentation

Here's the basics. More comprehensive docs coming soon.

### Manager Objects

All Manager objects are treated as Singletons, with most methods/variables being accessible through a static reference (ex: InputManager.GetKeyDown()).

To set these up all at once, just add the [Singletons](EepyDemoProject/Assets/Package/Prefabs/Singletons.prefab) prefab to your scene.

#### [InputManager](EepyDemoProject/Assets/Package/Scripts/Input/InputManager.cs)

This polls all connected devices (keyboard and controllers) for inputs.

The actual inputs are defined in the [InputAction](EepyDemoProject/Assets/Package/Scripts/Input/InputAction.cs) enum, and represent actions (ex: left, jump, etc) instead of specific key bindings. This is similar to the New Input System, but properly typesafe since it uses an enum. 

You can check the actions with the following static functions:
*   `InputManager.GetKey(InputAction)` - returns whether the action is being held.
*   `InputManager.GetKeyDown(InputAction)` - returns whether the action was pressed this frame.
*   `InputManager.GetKeyUp(InputAction)` - returns whether the action was released this frame.
*   `InputManager.GetKeyHoldTime(InputAction)` - returns whether how long in seconds the action has been held for.
*   `InputManager.GetKeyState(InputAction)` - returns the full state of the action. This has various other properties 

InputManager also lets you set the following options for each InputAction in the editor:
*   `allowRepeatsWhenHeld` - whether an action should repeatedly trigger the 'Pressed' event when held for long periods of time. This is useful for directional inputs, menu inputs, or other actions that can be triggered in rapid succession.
*   `repeatDelay` - an animation curve for how long to wait between repeated 'Pressed' events. A straight line will have the same delay every time, while a downward line will decrease the delay with every successive input.
*   `allowRebinding` - whether the action can be rebound by the player in the Controls Menu.

InputManager also has various functions to track key bindings. Each action can have multiple bindings, which can be a mix of keyboard and controller keys. This is all abstracted away, so you don't have to worry about the details!

#### [SettingsManager](EepyDemoProject/Assets/Package/Scripts/Settings/SettingsManager.cs)

This provides an interface into the game's settings. You can access the settings via a direct reference (ex: `SettingsManager.isVsyncEnabled`) or string/int reference (ex: `SettingsManager.GetString("VSync")`).

The following settings are included.
* Resolution
* Display - Fullscreen or Windowed
* VSync
* Volume Levels - Overall, Music, SFX
    * Note that Eepy doesn't have a built in audio solution. We use WWise for our last game, but we don't want to force that selection on you. Therefore, you'll need to hook these settings up yourself; stub methods are provided.
* Language
    * We use the [Web API](https://partner.steamgames.com/doc/store/localization/languages) standard for language codes.
* Controller Rumble - 0 to 10

You can subscribe to settings change events with the `SettingsManager.OnSettingsChanged` action. This will be broadcast whenever any setting changes.

#### [Localization Manager](EepyDemoProject/Assets/Package/Scripts/Localization/LocalizationManager.cs)

This handles the game's current translation. For most small games, you probably won't need this! Just set `isLocalizationEnabled` to `false` and you'll be all set.

There's 2 ways to actually display localized text:
* [Text Component](EepyDemoProject/Assets/Package/Prefabs/UI/Primatives/Text.prefab) - this takes in a `localizationKey` that refers to a specific localized string, and saves it to the attached TextMeshPro Text.
* `LocalizationManager.Get(localizationKey)` - this lets you get the localized text in code.

##### Importing Translations

The game's translations are stored as one big CSV (ex: this project's [translations.csv](EepyDemoProject/Assets/Package/Resources/translations.csv)). Each column is a language; each row is a specific translation entry, with a `localizationKey` as the first column.

**To save your changes, you'll need to manually regenerate the translation ScriptableObjects after editing the CSV.** This is easy to do:
1.  Open the Translation Generator: `Window -> Translation Generator`.
2.  Enter the path to your `translations.csv` and the path to your `Resources/Translations` folder.
3.  Press 'Generate Translations'
4.  For each custom font (ex: Chinese, Japanese, etc), you'll also need to update their atlas texture:
    *   Select the TMP Font Asset in the Project Window.
    *   Click 'Update Atlas Textures'.
    *   Set 'Character Set' to 'Characters from File'.
    *   Select the corresponding `.txt` file for that language. This contains every character required for that language, which ensures that our generated font SDF has every character.
    *   Click 'Generate Font Alias'.
    *   Click 'Save'.

#### [Steam Manager](EepyDemoProject/Assets/Package/Scripts/Steam/SteamManager.cs)

This is an extended version of Steamworks.NET. We added the following helpers on top:
*   `UnlockAchievement(id)` - unlock a specific achievement.
*   `IncrementStat(id)` - increment a specific numeric stat.
*   `GetStat(id)` - get the current value of a specific stat.
*   `GetCurrentGameLanguage()` - get the language provided to the game by Steam. This default's to the Steam Client's language, but players can override it on a per-game basis.
*   `IsSteamRunningOnSteamDeck()` - returns whether you're running on Steam Deck. Useful for disabling certain menu options or optimization purposes.

There's also an action - `OnSteamOverlayChanged` - that fires when the Steam overlay opens or closes. You'll probably want to automatically pause the game when this is broadcast with `true`.

### UI System

Eepy includes a basic UI system for all of your menu needs. To start, just add the [Gameplay UI Prefab](EepyDemoProject/Assets/Package/Prefabs/UI/Gameplay%20UI.prefab) to your scene. This contains a canvas and a script to manage all UI elements, as well as the following starter menus.

#### Starter Menus

We include a group of prebuilt menus to handle common pause menu needs:

##### [Pause Menu](EepyDemoProject/Assets/Package/Prefabs/UI/Pause%20Menu.prefab) 

The menu that opens when the player pauses the game. Think of this as a 'root menu' for all the other menus. Contains sublinks to the Settings and Controls menus.

##### [Settings Menu](EepyDemoProject/Assets/Package/Prefabs/UI/Settings%20Menu.prefab)

A menu with preconfigured settings toggles. Contains sublinks to the Resolution and Language menus.

##### [Resolution Menu](EepyDemoProject/Assets/Package/Prefabs/UI/Resolution%20Menu.prefab)

A menu that lets the player choose an available resolution. The actual resolution buttons are dynamically created at runtime based on the computer's display.

##### [Controls Menu](EepyDemoProject/Assets/Package/Prefabs/UI/Controls%20Menu.prefab)

A menu to view the key bindings for all `InputAction`s. The actual buttons are dynamically created at runtime. For inputs that are marked as rebindable in `InputManager`, the corresponding buttons are sublinks to the Change Binding Menu.

##### [Change Binding Menu](EepyDemoProject/Assets/Package/Prefabs/UI/Change%20Binding%20Menu.prefab)

A menu to add and remove key bindings for whichever action is set in the `inputAction` variable.

In most cases, this should only be opened from the Controls Menu since its starting state (the `inputAction`) has to be set externally.

##### [Language Menu](EepyDemoProject/Assets/Package/Prefabs/UI/Language%20Menu.prefab)

A menu that lets the player choose an available language. The actual language buttons are dynamically created at runtime based on which languages are specified in `LocalizationManager`.

##### [Confirm Menu](EepyDemoProject/Assets/Package/Prefabs/UI/Confirm%20Menu.prefab)

A reusable menu to display a yes/no confirmation popup. 

Before opening the menu with `GameplayUI.OpenMenu`, you should first call either `SetTitleTextAndAction` or `SetTitleLocalizationKeyAndAction` to set both the title of the menu and the 'Yes Button' click behavior.

#### Primitives

Under the hood, these are the building blocks to make more menu screens - simple and easily overridden.

##### [Text](EepyDemoProject/Assets/Package/Prefabs/UI/Primatives/Text.prefab)

Text object with functions for setting the text directly or with a `localizationKey`.

If localization is enabled, the font for this primitive is set to the active language's global font, which is necessary so that the text can display the right glyphs for a given language.

##### [Title Text](EepyDemoProject/Assets/Package/Prefabs/UI/Primatives/Title%20Text.prefab)

Variant of the `Text` primitive with a larger font size.

##### [Spacer](EepyDemoProject/Assets/Package/Prefabs/UI/Primatives/Spacer.prefab)

A constant sized spacer. Useful in horizontal/vertical layouts to provide consistent spacing between groups of items (ex: separate the title of a menu from the buttons).

##### [Horizontal List](EepyDemoProject/Assets/Package/Prefabs/UI/Primatives/Horizontal%20List.prefab) + [Vertical List](EepyDemoProject/Assets/Package/Prefabs/UI/Primatives/Vertical%20List.prefab)

Centered lists with a constant sized gap.

##### [Text Button](EepyDemoProject/Assets/Package/Prefabs/UI/Primatives/Text%20Button.prefab)

A Text Button that can be clicked, either by mouse or by a directional input. Buttons can be linked to each other so you can navigate between them with directional inputs.

They also emit three UnityEvent actions, which can be bound in both code or the Editor:
*   `OnClick` - when the button is clicked, either by an interact input or the mouse.
*   `OnFocus` - when the button is focused, either by a directional input or the mouse hover.
*   `OnUnfocus` - when the button is unfocused.

All of these functions are provided by the underlying [MenuButton](EepyDemoProject/Assets/Package/Scripts/UI/MenuButton.cs). This lets you make other buttons besides just Text Buttons, but we only ended up needing Text Buttons for our game.

##### [Text Button](EepyDemoProject/Assets/Package/Prefabs/UI/Primatives/Span%20Text%20Button.prefab)

A wide variant of Text Button intended to span the full center width of a menu, with text on the left side and an empty slot on the right side. This is used in many places throughout the UI, like for the Setting Toggle.

##### [Setting Toggle](EepyDemoProject/Assets/Package/Prefabs/UI/Primatives/Setting%20Toggle.prefab)

Animated slider toggle to change a value. This is configured to change a given setting and automatically subscribes to changes for that setting. The labels can be optionally localized or left as their original text.

##### [Menu](EepyDemoProject/Assets/Package/Prefabs/UI/Primatives/Menu.prefab)

A template for a menu screen. The screen itself defaults to a full screen UI panel with a vertical list in the center, although this can all be changed.

To create a new menu, you can follow these steps:
1.  Create a prefab variant of **Menu**.
2.  Set up your buttons, text, and other UI elements.
3.  Add the prefab variant as a child to the **Gameplay UI Prefab**, which will ensure that it's available in game.
4.  (Optional but highly recommended) Create a new script that inherits from the [MenuScreen Script](EepyDemoProject/Assets/Package/Scripts/UI/MenuScreen.cs), then add a member variable of that new script type to the [GameplayUI Script](EepyDemoProject/Assets/Package/Scripts/UI/GameplayUI.cs) and assign it in the prefab.
    *   This is useful since it's likely that you'll want to add custom code to the menu, especially for button clicks. Creating a child of `MenuScreen` gives you access to some useful member variables and lets you run code when the menu is opened/closed

This is part of a larger pattern that encourages you to break your UI up into discrete screens, which can be linked to each other in a stack. In code, you can call `GameplayUI.OpenMenu(screen)` to open a given screen.

For example, to open the pause menu, you can call this function:
```c#
GameplayUI.OpenMenu(GameplayUI.pauseMenu);
```

By default, screens animate in with a fade and slide. Both can be configured in the Inspector, and you can write your own implementation by replacing the `StateChangeAnimationCoroutine` function. 

There are 4 overridable functions that are triggered at the start of the animation depending on the desired end state; these are useful to run code when the menu state is changing:
-   `OnOpened` - called when the menu is opened and added to the top of the stack of open menus.
-   `OnReopened` - called when the menu, after being covered up on the stack, returns to the top after the menu above it is popped.
-   `OnClosed` - called when the menu is closed and popped off the top of the stack.
-   `OnCoveredUp` - called when the menu, after being on the top of the stack, is covered up by another menu.

### Multiplatform Build System

This is a clone of Unity 6's [Build Profiles](https://docs.unity3d.com/6000.1/Documentation/Manual/build-profiles.html), but a bit better.

#### How to Use

1.  Open the Build Profiles window: `Window -> Build Profiles`.
2.  Select one of the available profiles.
3.  Click 'Build Selected Profile'.

We include the following profiles out of the box:
*   Windows - Standalone and Steam
*   Mac - Standalone and Steam
*   Linux - Standalone and Steam
*   WebGL

#### How it Works

Each [Build Profile](/EepyDemoProject/Assets/Package/Editor/Build%20Tools/BuildProfile.cs) is stored as a Scriptable Object containing various fields about the target platform, as well as a list of [Build Defines](/EepyDemoProject/Assets/Package/Editor/Build%20Tools/BuildDefine.cs). Each Build Define is, itself, a Scriptable Object with a list of [scripting defines](https://docs.unity3d.com/6000.1/Documentation/Manual/custom-scripting-symbols.html). This lets you reuse groups of scripting defines (ex: the defines for all Steam targets) across multiple different platforms.

NOTE: Xcode project generation is currently broken, so you'll need to use the normal Build Window for those use cases.

## Contributing

We welcome contributions from the community!

However, we can't guarantee that all major changes that go beyond bug fixes will be accepted. Eepy is intentionally minimal and opinionated, and while there are many other features that could be bundled here, we want to be selected about what makes the cut.

For contribution instructions, please visit our [Contributing doc](CONTRIBUTING.md).

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

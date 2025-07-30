# Eepy
All the boilerplate you'll need for your Unity game. Opinionated, simple, and battle-tested on Steam.

Featuring:
* A Better Input System
    * Define typesafe input actions in code, then use everywhere
    * Abstract away input devices (keyboard, controllers)
    * Configure per-key settings (input repeating, thresholds)
* Native Controller Support
    * Out-of-the-box support for Dualshock, Dualsense, Xbox, and Steam Deck
    * Custom configs for easily adding niche controllers
    * Rumble using Steam Input
* Menu System
    * UI Primitives
    * Pause Menu Prefab
    * Input Remapping Menu Prefab
    * Settings Menu Prefab
    * Localization Menu Prefab
* Multiplatform Build System
    * Custom configs for defining 
* Settings System
    * Includes most common game settings (resolution, volume levels, etc)
* Steam Integration
    * Achievements
    * Stats

## Integration

### 0. Prerequisites

First, Eepy is an all-or-nothing solution. There is some dependency between scripts, as they were written as a system, not just as individual snippets. This is what lets it cover so many bases, but it makes it difficult to integrate into

With that out of the way:
* Use Unity 2021 or later
    * Older versions should work, but we haven't tested it and therefore can't guarantee that everything will work as expected.
* Install Text Mesh Pro
* Don't use the New Input System

### 1. Import the Eepy UnityPackage

### 2. Set Script Prioritizes

All of the manager scripts included here should run at a higher priority compared to the rest of your gameplay code. In most cases this is not strictly required, but it's highly recommended to avoid unnecessary delays.

To change this, go to `Project Settings -> Script Execution Order`, click the `+` button, add the following scripts, and drag them above the `Default Time` block in this order:

```
SettingsManager
InputManager
LocalizationManager
```

## Overview

## Misc Things to Know

- **All fonts are variable by default.** This ensures that all font decoration (bold, italic, etc) are supported. If your game only needs one style of text decoration, you can download the single weight versions from [Google Fonts](https://fonts.google.com/) and shave off 25% to 75% of the fonts' disk space.
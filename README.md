# Unity RuntimeDebugUI System

A powerful, data-driven debug UI system for Unity that enables real-time parameter tweaking with persistent settings. Perfect for gameplay tuning, balancing, and development workflows.

![Unity Version](https://img.shields.io/badge/Unity-2022.3%2B-blue)
![License](https://img.shields.io/badge/License-MIT-green)
![Platform](https://img.shields.io/badge/Platform-All%20Unity%20Platforms-lightgrey)

## ✨ Features

- 🎛️ **Real-time parameter tweaking** with sliders, toggles, and info displays
- 💾 **Intelligent auto-save system** with performance optimization and visual feedback
- 📱 **Full mobile support** with multiple trigger options
- 🖱️ **Runtime tooltip system** that works in builds
- 📂 **Organized tab system** with automatic scrolling
- 🎯 **Data-driven configuration** - no hardcoded UI elements
- 🔧 **Easy inheritance pattern** for project-specific implementations
- 📊 **Clean JSON serialization** with configurable decimal precision
- 🌍 **Cross-platform compatibility** (Windows, macOS, Linux, mobile, WebGL)
- ⚡ **Performance optimized** with debounced saving and minimal disk I/O
- 🎨 **Professional UI** with custom editor and visual status indicators

## 🚀 Installation

### Method 1: Package Manager (Git URL) - **Recommended**

1. **Install RuntimeDebugUI via Git URL:**
   ```
   Window → Package Manager → Add package from git URL
   https://github.com/hisham-CSS/unity-runtimedebugui.git
   ```

2. **That's it!** The package will be installed automatically with all dependencies.

### Method 2: Unity Package Manager (Local)

1. **Download the repository:**
   - Download ZIP from GitHub or clone: `git clone https://github.com/hisham-CSS/unity-runtimedebugui.git`

2. **Install via Package Manager:**
   ```
   Window → Package Manager → Add package from disk
   Select the package.json file from the downloaded folder
   ```

### Method 3: Manual Installation

1. **Add Newtonsoft JSON package:**
   ```
   Window → Package Manager → Add package by name
   com.unity.nuget.newtonsoft-json
   ```

2. **Download and copy files:**
   - Download the repository from GitHub
   - Copy the `Runtime` and `Editor` folder contents to your project:
     - `Runtime/DebugUI.cs` → `Assets/Scripts/DebugUI/`
     - `Runtime/UI/DebugUI.uxml` → `Assets/Scripts/DebugUI/UI/`
     - `Runtime/UI/DebugUI.uss` → `Assets/Scripts/DebugUI/UI/`
	 - `Editor/DebugUIEditor.cs` → `Assets/Scripts/DebugUI/Editor`
     - `Runtime/UI/DebugUIPropertyDrawer.cs` → `Assets/Scripts/DebugUI/Editor`

3. **Copy Assembly Definition (Optional but recommended):**
   - Copy the `CatSplatStudios.DebugUI.asmdef` and `CatSplatStudios.DebugUI.Editor.asmdef` in your DebugUI and Editor folders respectively.

## 🎮 Quick Start

### Basic Setup

1. **Create your debug UI class:**
   ```csharp
   using UnityEngine;
   using RuntimeDebugUI; // Add this namespace

   public class MyGameDebugUI : DebugUI
   {
       [Header("Game References")]
       [SerializeField] private PlayerController player;
       
       protected override void ConfigureTabs()
       {
           AddTab(ConfigurePlayerTab());
       }
       
       private DebugTabConfig ConfigurePlayerTab()
       {
           var tab = new DebugTabConfig
           {
               name = "Player",
               displayName = "Player Settings"
           };
           
           tab.controls.Add(new DebugControlConfig
           {
               name = "MoveSpeed",
               displayName = "Move Speed *", // * indicates auto-saved
               tooltip = "Player movement speed",
               type = DebugControlConfig.ControlType.Slider,
               saveValue = true, // Auto-save this value
               minValue = 0f,
               maxValue = 20f,
               getter = () => player.moveSpeed,
               setter = (value) => player.moveSpeed = value
           });
           
           return tab;
       }
   }
   ```

2. **Setup in scene:**
   - Create a GameObject with a `UIDocument` component
   - Assign `DebugUI.uxml` to the UIDocument's Source Asset
   - Add your debug UI script to the same GameObject
   - Assign the UIDocument reference in the inspector

3. **Test it:**
   - **Desktop:** Press `F1` (default toggle key) to show/hide the debug panel
   - **Mobile:** Use touch gestures or on-screen button (see Mobile Support section)
   - Adjust values in real-time
   - Values marked with `*` are automatically saved

## ⚡ Auto-Save System

The system features an intelligent auto-save system with multiple modes for optimal performance:

### Auto-Save Modes

#### 1. **Debounced** (Default - Recommended)
- **How it works:** Saves 2 seconds after you stop changing values
- **Performance:** 99% reduction in disk writes during active use
- **Best for:** Most projects - balances safety with performance
- **Visual feedback:** Orange save button when changes are pending

#### 2. **Interval**
- **How it works:** Saves every 30 seconds if there are unsaved changes
- **Performance:** Predictable save timing with minimal writes
- **Best for:** Long debugging sessions with frequent changes

#### 3. **Manual**
- **How it works:** Only saves when you click the save button or on app events
- **Performance:** Zero automatic disk writes
- **Best for:** Maximum control and performance-critical scenarios

#### 4. **Immediate** (Legacy)
- **How it works:** Saves immediately on every change (original behavior)
- **Performance:** Poor - not recommended for production
- **Best for:** Backward compatibility only

### Visual Feedback

The system provides clear visual indicators for save status:

- **Save Button States:**
  - Normal: "Save Settings" (default styling)
  - Pending: "Save Settings *" (orange background)
  - Saving: "Saved!" (green background, temporarily disabled)

- **Status Indicator:** (bottom-right corner)
  - "Unsaved Changes" (orange) - when modifications are pending
  - "Saving..." (yellow) - during save operations
  - "Saved" (green) - confirmation that auto-hides after 2 seconds

### Configuration

```csharp
[Header("Auto-Save Configuration")]
[SerializeField] private AutoSaveMode autoSaveMode = AutoSaveMode.Debounced;
[SerializeField] private float autoSaveDelay = 2f; // Debounced delay
[SerializeField] private float autoSaveInterval = 30f; // Interval timing
[SerializeField] private bool saveOnApplicationPause = true; // Mobile pause
[SerializeField] private bool saveOnApplicationFocus = true; // Window focus loss
[SerializeField] private bool saveOnDestroy = true; // Component destruction
```

## 📱 Mobile Support

The system includes comprehensive mobile support with multiple trigger options:

### Mobile Trigger Types

#### 1. **Multi-Touch Gesture** (Default)
- **How it works:** Tap with 3 fingers simultaneously to toggle the debug panel
- **Best for:** Quick access during gameplay without UI clutter
- **Configuration:**
  ```csharp
  [Header("Mobile Support")]
  [SerializeField] private MobileTriggerType mobileTriggerType = MobileTriggerType.TouchGesture;
  [SerializeField] private int touchCount = 3; // Number of fingers required
  ```

#### 2. **Touch and Hold**
- **How it works:** Touch and hold anywhere on screen for 2 seconds
- **Best for:** Deliberate access to prevent accidental triggers
- **Configuration:**
  ```csharp
  [SerializeField] private MobileTriggerType mobileTriggerType = MobileTriggerType.TouchAndHold;
  [SerializeField] private float touchHoldTime = 2f; // Hold duration in seconds
  ```

#### 3. **On-Screen Toggle Button**
- **How it works:** Always-visible button in the top-right corner
- **Best for:** Maximum accessibility and discoverability
- **Configuration:**
  ```csharp
  [SerializeField] private MobileTriggerType mobileTriggerType = MobileTriggerType.OnScreenButton;
  [SerializeField] private string toggleButtonText = "Debug"; // Button text
  ```
  
### Mobile Configuration

```csharp
[Header("Mobile Support")]
[SerializeField] private bool enableMobileSupport = true;
[SerializeField] private MobileTriggerType mobileTriggerType = MobileTriggerType.TouchGesture;
[SerializeField] private int touchCount = 3; // For multi-touch trigger
[SerializeField] private float touchHoldTime = 2f; // For touch and hold trigger
[SerializeField] private bool showToggleButton = true; // Show on-screen button
[SerializeField] private string toggleButtonText = "Debug";

public enum MobileTriggerType
{
    TouchGesture,    // Multi-touch (e.g., 3 finger tap)
    TouchAndHold,    // Touch and hold for X seconds
    OnScreenButton   // Always visible toggle button
}
```

### Platform Support

- ✅ **iOS** - All trigger types supported
- ✅ **Android** - All trigger types supported  
- ✅ **WebGL Mobile** - Touch events work in mobile browsers
- ✅ **Desktop** - Keyboard shortcuts + mobile triggers available

## 🎨 Custom Editor

The package includes a professional custom editor that provides:

### Enhanced Inspector Experience

- **Conditional Field Display:** Mobile and serialization options only appear when enabled
- **Organized Sections:** Collapsible foldout groups for better navigation
- **Real-time Help:** Contextual guidance that updates based on your settings
- **Smart Validation:** Automatic range clamping and error prevention
- **Save Location Preview:** Shows exactly where files will be saved

### Editor Sections

1. **UI Configuration** - Core setup options
2. **Mobile Support** - Touch input configuration (only visible when enabled)
3. **Serialization** - File saving options (only visible when enabled)
4. **Auto-Save Configuration** - Performance and timing settings
5. **Tooltip System** - Hover behavior configuration

The custom editor automatically appears when you select any DebugUI component in the inspector - no additional setup required!

## 📖 Detailed Usage

### Control Types

#### Slider Control
```csharp
new DebugControlConfig
{
    name = "JumpHeight",
    displayName = "Jump Height *",
    tooltip = "How high the player can jump",
    type = DebugControlConfig.ControlType.Slider,
    saveValue = true,
    minValue = 0f,
    maxValue = 10f,
    getter = () => player.jumpHeight,
    setter = (value) => player.jumpHeight = value
}
```

#### Toggle Control
```csharp
new DebugControlConfig
{
    name = "GodMode",
    displayName = "God Mode *",
    tooltip = "Player takes no damage",
    type = DebugControlConfig.ControlType.Toggle,
    saveValue = true,
    boolGetter = () => player.isInvincible,
    boolSetter = (value) => player.isInvincible = value
}
```

#### Info Display
```csharp
new DebugControlConfig
{
    name = "PlayerPosition",
    displayName = "Position",
    tooltip = "Current player world position",
    type = DebugControlConfig.ControlType.InfoDisplay,
    stringGetter = () => $"({player.transform.position.x:F1}, {player.transform.position.y:F1})"
}
```

### Section Organization

Group related controls using the `sectionName` property:

```csharp
// Movement section
new DebugControlConfig
{
    name = "MoveSpeed",
    displayName = "Move Speed *",
    sectionName = "Basic Movement", // Groups controls under this header
    saveValue = true,
    // ... other properties
},
new DebugControlConfig
{
    name = "JumpHeight", 
    displayName = "Jump Height *",
    sectionName = "Basic Movement", // Same section
    saveValue = true,
    // ... other properties
},
// Advanced section
new DebugControlConfig
{
    name = "WallJumpForce",
    displayName = "Wall Jump Force *", 
    sectionName = "Advanced Movement", // New section
    saveValue = true,
    // ... other properties
}
```

### Auto-Save Configuration

Controls marked with `saveValue = true` will automatically:
- Save when their value changes
- Load on startup
- Display a `*` indicator in the UI
- Include "(Auto-saved)" in their tooltip

#### File Locations

The system intelligently chooses save locations:

**Desktop Platforms (Preferred):**
- **Editor:** `ProjectFolder/DebugSettings/DebugUISettings.json`
- **Windows Build:** `GameFolder/DebugSettings/DebugUISettings.json`
- **macOS Build:** Next to the .app bundle
- **Linux Build:** Next to the executable

**Fallback (All Platforms):**
- Uses `Application.persistentDataPath` if accessible location fails

#### Serialization Configuration

```csharp
[Header("Serialization")]
[SerializeField] private bool enableSerialization = true;
[SerializeField] private string saveFileName = "DebugUISettings.json";
[SerializeField] private bool saveToPlayerPrefs = false; // Use PlayerPrefs instead of files
[SerializeField] private bool preferAccessibleLocation = true; // Try accessible location first
[SerializeField] private string customSaveFolder = "DebugSettings";
[SerializeField] private int jsonDecimalPlaces = 3; // Clean decimal formatting
```

### Tooltip System

The system includes a custom tooltip implementation that works in runtime builds:

```csharp
[Header("Tooltip System")]
[SerializeField] private float tooltipDelay = 0.5f; // Delay before showing
[SerializeField] private Vector2 tooltipOffset = new Vector2(10, -10); // Offset from mouse
```

Tooltips automatically show:
- Control description from the `tooltip` property
- "(Auto-saved)" indicator for saved controls
- Smart positioning to stay within screen bounds

## 🎮 Example Implementation

Here's a complete example from a platformer game:

```csharp
using UnityEngine;
using RuntimeDebugUI;

public class PlayerDebugUI : DebugUI
{
    [Header("Player References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private CameraFollow cameraFollow;

    protected override void Start()
    {
        // Get references
        if (player == null)
            player = FindObjectOfType<PlayerController>();
        
		// Call base class in order to setup the functionality for the window
        base.Start();
    }

    protected override void ConfigureTabs()
    {
        AddTab(ConfigureMovementTab());
        AddTab(ConfigureDebugInfoTab());
    }

    private DebugTabConfig ConfigureMovementTab()
    {
        var tab = new DebugTabConfig
        {
            name = "Movement",
            displayName = "Movement"
        };

        tab.controls.AddRange(new[]
        {
            new DebugControlConfig
            {
                name = "MoveSpeed",
                displayName = "Move Speed *",
                tooltip = "Base movement speed of the player",
                sectionName = "Basic Movement",
                type = DebugControlConfig.ControlType.Slider,
                saveValue = true,
                minValue = 0f,
                maxValue = 20f,
                getter = () => player.moveSpeed,
                setter = (value) => player.moveSpeed = value
            },
            new DebugControlConfig
            {
                name = "Acceleration",
                displayName = "Acceleration *",
                tooltip = "How quickly the player reaches max speed",
                sectionName = "Basic Movement",
                type = DebugControlConfig.ControlType.Slider,
                saveValue = true,
                minValue = 0f,
                maxValue = 50f,
                getter = () => player.acceleration,
                setter = (value) => player.acceleration = value
            }
        });

        return tab;
    }

    private DebugTabConfig ConfigureDebugInfoTab()
    {
        var tab = new DebugTabConfig
        {
            name = "DebugInfo",
            displayName = "Debug Info"
        };

        tab.controls.AddRange(new[]
        {
            new DebugControlConfig
            {
                name = "PlayerPosition",
                displayName = "Position",
                tooltip = "Current player world position",
                type = DebugControlConfig.ControlType.InfoDisplay,
                stringGetter = () => $"({player.transform.position.x:F1}, {player.transform.position.y:F1}, {player.transform.position.z:F1})"
            },
            new DebugControlConfig
            {
                name = "PlayerVelocity",
                displayName = "Velocity",
                tooltip = "Current player velocity",
                type = DebugControlConfig.ControlType.InfoDisplay,
                stringGetter = () => $"{player.velocity.magnitude:F1} m/s"
            }
        });

        return tab;
    }
}
```

## ⚙️ Configuration Reference

### DebugControlConfig Properties

| Property | Type | Description |
|----------|------|-------------|
| `name` | string | Unique identifier for the control |
| `displayName` | string | Text shown in the UI (add * for saved controls) |
| `tooltip` | string | Tooltip text (optional) |
| `type` | ControlType | Slider, Toggle, or InfoDisplay |
| `sectionName` | string | Groups controls under section headers |
| `saveValue` | bool | Whether to auto-save this control |
| `minValue` | float | Minimum slider value |
| `maxValue` | float | Maximum slider value |
| `getter` | Func<float> | Function to get current value |
| `setter` | Action<float> | Function to set new value |
| `boolGetter` | Func<bool> | Function to get bool value (toggles) |
| `boolSetter` | Action<bool> | Function to set bool value (toggles) |
| `stringGetter` | Func<string> | Function to get display string (info) |

### UI Configuration

| Property | Type | Description |
|----------|------|-------------|
| `toggleKey` | KeyCode | Key to show/hide the debug panel (default: F1) |
| `showOnStart` | bool | Whether to show panel on startup |
| `panelTitle` | string | Title displayed in the header |

### Mobile Support Configuration

| Property | Type | Description |
|----------|------|-------------|
| `enableMobileSupport` | bool | Enable mobile touch input handling |
| `mobileTriggerType` | MobileTriggerType | TouchGesture, TouchAndHold, or OnScreenButton |
| `touchCount` | int | Number of fingers for multi-touch gesture |
| `touchHoldTime` | float | Hold duration for touch-and-hold trigger |
| `showToggleButton` | bool | Show on-screen toggle button |
| `toggleButtonText` | string | Text displayed on the toggle button |

### Auto-Save Configuration

| Property | Type | Description |
|----------|------|-------------|
| `autoSaveMode` | AutoSaveMode | Immediate, Debounced, Interval, or Manual |
| `autoSaveDelay` | float | Delay after last change before saving (Debounced mode) |
| `autoSaveInterval` | float | Maximum time between saves (Interval mode) |
| `saveOnApplicationPause` | bool | Save when app is paused (mobile) |
| `saveOnApplicationFocus` | bool | Save when app loses focus |
| `saveOnDestroy` | bool | Save when component is destroyed |

### Serialization Configuration

| Property | Type | Description |
|----------|------|-------------|
| `enableSerialization` | bool | Enable auto-save functionality |
| `saveFileName` | string | Name of the save file |
| `saveToPlayerPrefs` | bool | Use PlayerPrefs instead of files |
| `preferAccessibleLocation` | bool | Try accessible location first |
| `customSaveFolder` | string | Custom folder name for saves |
| `jsonDecimalPlaces` | int | Decimal precision in JSON |

## 🎨 Customization

### Styling

The UI uses Unity's UI Toolkit (USS). Modify `DebugUI.uss` to customize:
- Colors and transparency
- Fonts and sizes  
- Layout and spacing
- Mobile-responsive breakpoints

### Extending Functionality

Create custom control types by extending the base system:

```csharp
public enum CustomControlType
{
    ColorPicker,
    Vector3Field,
    DropdownList
}

// Extend DebugControlConfig with custom properties
// Implement custom UI creation in your derived DebugUI class
```

## 🔧 Troubleshooting

### Common Issues

**"Package not found" when using Git URL**
- Ensure you have Git installed and accessible from Unity
- Try using HTTPS instead of SSH: `https://github.com/hisham-CSS/unity-runtimedebugui.git`
- Check your internet connection and firewall settings

**"Newtonsoft.Json not found"**
- Install the Newtonsoft JSON package via Package Manager: `com.unity.nuget.newtonsoft-json`

**"UIDocument reference is null"**
- Ensure UIDocument component has `DebugUI.uxml` assigned
- Check that the UIDocument is on the same GameObject as your debug script

**"Controls not appearing"**
- Verify `ConfigureTabs()` is calling `AddTab()` for each tab
- Check that getter/setter functions are not null
- Ensure control names are unique within each tab
- Add the `using RuntimeDebugUI;` namespace to your script

**"Settings not saving"**
- Verify `enableSerialization = true`
- Check that controls have `saveValue = true`
- Ensure write permissions for the save location

**"Mobile triggers not working"**
- Verify `enableMobileSupport = true`
- Check that you're testing on an actual mobile device or mobile simulator
- For touch-and-hold, ensure you're holding still (movement cancels the gesture)
- For multi-touch, ensure all fingers touch simultaneously

**"Tooltips not showing"**
- Tooltips require mouse hover - they don't work with touch input
- Check that `tooltip` property is set on controls
- Verify tooltip delay settings

**Save files not found**
- Check the console for the actual save location path
- On desktop builds, look next to the executable
- Enable "Open Folder" button to navigate directly to save location

**Performance issues during debugging**
- Switch to `AutoSaveMode.Debounced` or `AutoSaveMode.Manual`
- Reduce `autoSaveDelay` if saves feel too slow
- Consider using PlayerPrefs instead of files for better performance

**Custom editor not showing**
- Ensure the Editor scripts are in an `Editor` folder
- Check that the `DebugUI.Editor.asmdef` is properly configured
- Restart Unity if the editor doesn't appear immediately

## 🔄 Updating the Package

### Via Package Manager (Git URL)
1. Go to `Window → Package Manager`
2. Select "In Project" and find "Runtime Debug UI"
3. Click "Update" if available, or remove and re-add the Git URL

### Manual Update
1. Delete the old files from your project
2. Follow the installation steps again with the new version

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE.md) file for details.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

### Development Setup
1. Clone the repository
2. Follow the manual installation steps.
4. Make your changes in the new project.
5. Test thoroughly
6. Copy your changes back to the cloned repository.
7. Submit a PR

## 🙏 Acknowledgments

- Built for the Unity community
- Inspired by developer tools and debug interfaces
- Thanks to all who wish to contribute or test this package

## 📞 Support

- **Issues:** [GitHub Issues](https://github.com/hisham-css/unity-runtimedebugui/issues)
- **Discussions:** [GitHub Discussions](https://github.com/hisham-css/unity-runtimedebugui/discussions)
- **Discord:** [Cat Splat Studios Discord](https://discord.gg/MXcPNkBWxf)

---

**Happy debugging!** 🎮✨
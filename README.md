# Unity RuntimeDebugUI System

A powerful, data-driven debug UI system for Unity that enables real-time parameter tweaking with persistent settings. Perfect for gameplay tuning, balancing, and development workflows.

![Unity Version](https://img.shields.io/badge/Unity-2022.3%2B-blue)
![License](https://img.shields.io/badge/License-MIT-green)
![Platform](https://img.shields.io/badge/Platform-All%20Unity%20Platforms-lightgrey)

## ✨ Features

- 🎛️ **Real-time parameter tweaking** with sliders, toggles, and info displays
- 💾 **Smart auto-save system** with accessible file locations
- 📱 **Cross-platform compatibility** (Windows, macOS, Linux, mobile)
- 🖱️ **Runtime tooltip system** that works in builds
- 📂 **Organized tab system** with automatic scrolling
- 🎯 **Data-driven configuration** - no hardcoded UI elements
- 🔧 **Easy inheritance pattern** for project-specific implementations
- 📊 **Clean JSON serialization** with configurable decimal precision

## 🚀 Quick Start

### Prerequisites

- Unity 2022.3 or later
- Newtonsoft JSON package (`com.unity.nuget.newtonsoft-json`)

### Installation

1. **Add Newtonsoft JSON package:**
   ```
   Window → Package Manager → Add package by name
   com.unity.nuget.newtonsoft-json
   ```

2. **Import the DebugUI system:**
   - Copy `DebugUI.cs`, `DebugUI.uxml`, and `DebugUI.uss` to your project
   - Place them in a `Scripts/DebugUI/` folder (recommended)

3. **Create your debug UI class:**
   ```csharp
   using UnityEngine;

   public class MyGameDebugUI : DebugUI
   {
       [Header("Game References")]
       [SerializeField] private PlayerController player;
       
       protected override void ConfigureTabs()
       {
           AddTab(ConfigurePlayerTab());
           AddTab(ConfigureGameplayTab());
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
               displayName = "Move Speed",
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

4. **Setup in scene:**
   - Create a GameObject with a `UIDocument` component
   - Assign `DebugUI.uxml` to the UIDocument's Source Asset
   - Add your debug UI script to the same GameObject
   - Assign the UIDocument reference in the inspector

## 📖 Detailed Usage

### Control Types

#### Slider Control
```csharp
new DebugControlConfig
{
    name = "JumpHeight",
    displayName = "Jump Height",
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
    displayName = "God Mode",
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
    displayName = "Move Speed",
    sectionName = "Basic Movement", // Groups controls under this header
    // ... other properties
},
new DebugControlConfig
{
    name = "JumpHeight", 
    displayName = "Jump Height",
    sectionName = "Basic Movement", // Same section
    // ... other properties
},
// Advanced section
new DebugControlConfig
{
    name = "WallJumpForce",
    displayName = "Wall Jump Force", 
    sectionName = "Advanced Movement", // New section
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

#### Configuration Options

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
        
        base.Start();
    }

    protected override void ConfigureTabs()
    {
        AddTab(ConfigureMovementTab());
        AddTab(ConfigureJumpTab());
        AddTab(ConfigureCameraTab());
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
                displayName = "Move Speed",
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
                displayName = "Acceleration",
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
| `displayName` | string | Text shown in the UI |
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
| `toggleKey` | KeyCode | Key to show/hide the debug panel |
| `showOnStart` | bool | Whether to show panel on startup |
| `panelTitle` | string | Title displayed in the header |

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
- Responsive breakpoints

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

**"Newtonsoft.Json not found"**
- Install the Newtonsoft JSON package via Package Manager

**"UIDocument reference is null"**
- Ensure UIDocument component has `DebugUI.uxml` assigned
- Check that the UIDocument is on the same GameObject as your debug script

**"Controls not appearing"**
- Verify `ConfigureTabs()` is calling `AddTab()` for each tab
- Check that getter/setter functions are not null
- Ensure control names are unique within each tab

**"Settings not saving"**
- Verify `enableSerialization = true`
- Check that controls have `saveValue = true`
- Ensure write permissions for the save location

**"Tooltips not showing"**
- Tooltips require mouse hover - they don't work with touch input
- Check that `tooltip` property is set on controls
- Verify tooltip delay settings

### Performance Considerations

- Info displays update every frame - keep string operations lightweight
- Large numbers of controls may impact performance - consider splitting into multiple tabs
- Auto-save triggers on every value change - avoid rapid updates if possible

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

### Development Setup

1. Clone the repository
2. Open in Unity 2022.3+
3. Install Newtonsoft JSON package
4. Open the example scene to test changes

## 🙏 Acknowledgments

- Built for the Unity community
- Inspired by developer tools and debug interfaces
- Thanks to all contributors and testers

## 📞 Support

- **Issues:** [GitHub Issues](https://github.com/yourusername/unity-debugui/issues)
- **Discussions:** [GitHub Discussions](https://github.com/yourusername/unity-debugui/discussions)
- **Discord:** [Your Discord Server](https://discord.gg/yourserver)

---

**Happy debugging!** 🎮✨
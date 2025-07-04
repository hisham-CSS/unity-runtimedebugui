﻿using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using System.IO;
using Newtonsoft.Json;


namespace RuntimeDebugUI
{
    #region Helper Classes
    /// <summary>
    /// Configuration classes for the debug UI system
    /// </summary>
    [Serializable]
    public class DebugControlConfig
    {
        public string name;
        public string displayName;
        public string tooltip;
        public ControlType type;
        public string sectionName; // Which section this control belongs to

        // Serialization options
        public bool saveValue = false; // Whether to save/load this control's value
        public string saveKey; // Custom save key (optional, defaults to tab.name + "." + control.name)

        public enum ControlType
        {
            Slider,
            Toggle,
            InfoDisplay
        }

        // Slider properties
        public float minValue;
        public float maxValue;
        public float defaultValue;
        public Func<float> getter;
        public Action<float> setter;

        // Toggle properties
        public bool defaultBoolValue;
        public Func<bool> boolGetter;
        public Action<bool> boolSetter;

        // Info display properties
        public Func<string> stringGetter;
    }
    [Serializable]
    public class DebugTabConfig
    {
        public string name;
        public string displayName;
        public List<DebugControlConfig> controls = new List<DebugControlConfig>();
    }
    /// <summary>
    /// Serializable data structure for saving debug values
    /// </summary>
    [Serializable]
    public class DebugUIData
    {
        public List<SavedValue> savedValues = new List<SavedValue>();
    }
    [Serializable]
    public class SavedValue
    {
        public string key;
        public float floatValue;
        public bool boolValue;
        public DebugControlConfig.ControlType type;
    }
    #endregion

    /// <summary>
    /// Generic debug UI system for tweaking any values at runtime.
    /// Uses a data-driven approach for easy configuration of tabs and controls.
    /// Perfect for debugging game mechanics, tweaking parameters, and monitoring values.
    /// </summary>
    public class DebugUI : MonoBehaviour
    {
        #region Enums
        public enum AutoSaveMode
        {
            Immediate,      // Save immediately on every change (original behavior)
            Debounced,      // Save after delay when user stops changing values
            Interval,       // Save at regular intervals only
            Manual          // Only save manually or on app events
        }
        public enum MobileTriggerType
        {
            TouchGesture,    // Multi-touch (e.g., 3 finger tap)
            TouchAndHold,    // Touch and hold for X seconds
            OnScreenButton   // Always visible toggle button
        }
        #endregion

        #region Inspector Configuration
        [Header("UI Configuration")]
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private KeyCode toggleKey = KeyCode.F1;
        [SerializeField] private bool showOnStart = false;
        [SerializeField] protected string panelTitle = "Debug Settings";

        [Header("Mobile Support")]
        [SerializeField] private bool enableMobileSupport = true;
        [SerializeField] private MobileTriggerType mobileTriggerType = MobileTriggerType.TouchGesture;
        [SerializeField] private int touchCount = 3; // For multi-touch trigger
        [SerializeField] private float touchHoldTime = 2f; // For touch and hold trigger
        [SerializeField] private bool showToggleButton = true; // Show on-screen button
        [SerializeField] private string toggleButtonText = "Debug";

        [Header("Serialization")]
        [SerializeField] private bool enableSerialization = true;
        [SerializeField] private string saveFileName = "DebugUISettings.json";
        [SerializeField] private bool saveToPlayerPrefs = false; // Alternative to file saving
        [SerializeField] private bool preferAccessibleLocation = true; // Try to save in accessible location first
        [SerializeField] private string customSaveFolder = "DebugSettings"; // Custom folder name for organization
        [SerializeField] private int jsonDecimalPlaces = 3; // Number of decimal places in JSON output

        [Header("Auto-Save Configuration")]
        [SerializeField] private AutoSaveMode autoSaveMode = AutoSaveMode.Debounced;
        [SerializeField] private float autoSaveDelay = 2f; // Delay after last change before saving
        [SerializeField] private float autoSaveInterval = 30f; // Maximum time between saves
        [SerializeField] private bool saveOnApplicationPause = true;
        [SerializeField] private bool saveOnApplicationFocus = true;
        [SerializeField] private bool saveOnDestroy = true;

        [Header("Tooltip System")]
        [SerializeField] private float tooltipDelay = 0.5f; // Delay before showing tooltip
        [SerializeField] private Vector2 tooltipOffset = new Vector2(10, -10); // Offset from mouse position 
        #endregion

        #region Private Fields
        // Private fields
        private VisualElement root;
        private VisualElement debugPanel;
        private bool isVisible = false;
        private string saveFilePath;
        private string actualSaveLocation; // Where the file was actually saved
        private bool usingFallbackLocation = false;

        // Auto-save system fields
        private bool hasUnsavedChanges = false;
        private float lastChangeTime = 0f;
        private float lastSaveTime = 0f;

        // Save status indicator fields
        private Label saveStatusIndicator;
        private Coroutine saveStatusCoroutine;

        // Class field to store save button reference
        private Button saveButton;
        private Coroutine saveButtonResetCoroutine;

        // Mobile support fields
        private float[] touchStartTimes;
        private bool isTouchHolding = false;
        private float touchHoldStartTime;
        private Button mobileToggleButton;

        // UI containers
        private VisualElement tabButtonsContainer;
        private VisualElement tabContentContainer;

        // Configuration
        protected List<DebugTabConfig> tabConfigs = new List<DebugTabConfig>();
        private Dictionary<string, VisualElement> tabElements = new Dictionary<string, VisualElement>();
        private Dictionary<string, Button> tabButtons = new Dictionary<string, Button>();
        private string currentActiveTab = "";

        // Original values for reset functionality
        private Dictionary<string, float> originalValues = new Dictionary<string, float>();
        private Dictionary<string, bool> originalBoolValues = new Dictionary<string, bool>();

        // Smart serialization data
        private DebugUIData debugData = new DebugUIData();

        // Tooltip system
        private VisualElement tooltipContainer;
        private Label tooltipLabel;
        private VisualElement currentHoveredElement;
        private string currentTooltipText;
        private float tooltipTimer;
        private bool tooltipVisible = false;
        #endregion

        protected virtual void Start()
        {
            // Determine the best save file path
            if (enableSerialization)
            {
                DetermineSaveLocation();
            }

            ConfigureTabs();

            // Load saved values before storing originals
            if (enableSerialization)
            {
                LoadValues();
            }

            StoreOriginalValues();
            InitializeUI();
            SetupEventHandlers();
            SetupTooltipSystem();
            SetupMobileSupport();

            // Set initial visibility
            isVisible = showOnStart;
            debugPanel.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }
        private void Awake()
        {
            // Validate UIDocument reference
            if (uiDocument == null)
            {
                Debug.LogError("DebugUI: UIDocument reference is missing.");
                enabled = false;
                return;
            }

            root = uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("DebugUI: Root visual element not found.");
                enabled = false;
                return;
            }
        }
        private void Update()
        {
            // Toggle UI visibility with key press
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleVisibility();
            }

            // Handle mobile input
            if (enableMobileSupport)
            {
                HandleMobileInput();
            }

            // Update info displays if visible
            if (isVisible)
            {
                UpdateInfoDisplays();
                UpdateTooltipSystem();
            }

            // Handle auto-save logic
            if (enableSerialization && hasUnsavedChanges)
            {
                HandleAutoSave();
            }
        }
        private void OnDestroy()
        {
            // Auto-save when the component is destroyed
            if (enableSerialization && saveOnDestroy && hasUnsavedChanges)
            {
                SaveValues();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            // Auto-save when the application is paused (mobile)
            if (!pauseStatus && enableSerialization && saveOnApplicationPause && hasUnsavedChanges)
            {
                SaveValues();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            // Save when app loses focus
            if (!hasFocus && enableSerialization && saveOnApplicationFocus && hasUnsavedChanges)
            {
                SaveValues();
            }
        }

        #region Tooltip System

        /// <summary>
        /// Setup the custom tooltip system for runtime use
        /// </summary>
        private void SetupTooltipSystem()
        {
            // Create tooltip container
            tooltipContainer = new VisualElement();
            tooltipContainer.name = "TooltipContainer";
            tooltipContainer.AddToClassList("tooltip-container");
            tooltipContainer.style.position = Position.Absolute;
            tooltipContainer.style.display = DisplayStyle.None;
            tooltipContainer.pickingMode = PickingMode.Ignore; // Don't interfere with mouse events

            // Create tooltip label
            tooltipLabel = new Label();
            tooltipLabel.AddToClassList("tooltip-label");
            tooltipContainer.Add(tooltipLabel);

            // Add to root (on top of everything)
            root.Add(tooltipContainer);
        }

        /// <summary>
        /// Update the tooltip system each frame
        /// </summary>
        private void UpdateTooltipSystem()
        {
            if (currentHoveredElement != null && !string.IsNullOrEmpty(currentTooltipText))
            {
                tooltipTimer += Time.deltaTime;

                if (tooltipTimer >= tooltipDelay && !tooltipVisible)
                {
                    ShowTooltip(currentTooltipText);
                }

                if (tooltipVisible)
                {
                    UpdateTooltipPosition();
                }
            }
        }

        /// <summary>
        /// Register tooltip for an element
        /// </summary>
        private void RegisterTooltip(VisualElement element, string tooltipText)
        {
            if (string.IsNullOrEmpty(tooltipText)) return;

            element.RegisterCallback<MouseEnterEvent>(evt => {
                currentHoveredElement = element;
                currentTooltipText = tooltipText;
                tooltipTimer = 0f;
            });

            element.RegisterCallback<MouseLeaveEvent>(evt => {
                if (currentHoveredElement == element)
                {
                    HideTooltip();
                    currentHoveredElement = null;
                    currentTooltipText = null;
                    tooltipTimer = 0f;
                }
            });
        }

        /// <summary>
        /// Show the tooltip with specified text
        /// </summary>
        private void ShowTooltip(string text)
        {
            if (tooltipVisible) return;

            tooltipLabel.text = text;
            tooltipContainer.style.display = DisplayStyle.Flex;
            tooltipVisible = true;
            UpdateTooltipPosition();
        }

        /// <summary>
        /// Hide the tooltip
        /// </summary>
        private void HideTooltip()
        {
            if (!tooltipVisible) return;

            tooltipContainer.style.display = DisplayStyle.None;
            tooltipVisible = false;
        }

        /// <summary>
        /// Update tooltip position to follow mouse
        /// </summary>
        private void UpdateTooltipPosition()
        {
            if (!tooltipVisible) return;

            Vector2 mousePosition = Input.mousePosition;

            // Convert screen coordinates to UI coordinates
            Vector2 localMousePosition = RuntimePanelUtils.ScreenToPanel(
                root.panel,
                new Vector2(mousePosition.x, Screen.height - mousePosition.y)
            );

            // Apply offset and ensure tooltip stays within screen bounds
            Vector2 tooltipPosition = localMousePosition + tooltipOffset;

            // Get tooltip size
            Vector2 tooltipSize = new Vector2(
                tooltipContainer.resolvedStyle.width,
                tooltipContainer.resolvedStyle.height
            );

            // Adjust position to keep tooltip on screen
            if (tooltipPosition.x + tooltipSize.x > root.resolvedStyle.width)
            {
                tooltipPosition.x = localMousePosition.x - tooltipSize.x - tooltipOffset.x;
            }

            if (tooltipPosition.y + tooltipSize.y > root.resolvedStyle.height)
            {
                tooltipPosition.y = localMousePosition.y - tooltipSize.y - tooltipOffset.y;
            }

            tooltipContainer.style.left = tooltipPosition.x;
            tooltipContainer.style.top = tooltipPosition.y;
        }

        #endregion

        #region Smart File Location System

        /// <summary>
        /// Intelligently determine the best save location based on platform and accessibility
        /// </summary>
        private void DetermineSaveLocation()
        {
            if (!preferAccessibleLocation)
            {
                // User prefers standard persistent data path
                saveFilePath = Path.Combine(Application.persistentDataPath, saveFileName);
                actualSaveLocation = saveFilePath;
                usingFallbackLocation = false;
                Debug.Log($"DebugUI: Using persistent data path: {saveFilePath}");
                return;
            }

            // Try to find an accessible location first
            string accessiblePath = GetAccessibleSavePath();

            if (!string.IsNullOrEmpty(accessiblePath))
            {
                saveFilePath = accessiblePath;
                actualSaveLocation = accessiblePath;
                usingFallbackLocation = false;
                Debug.Log($"DebugUI: Using accessible location: {saveFilePath}");
            }
            else
            {
                // Fall back to persistent data path
                saveFilePath = Path.Combine(Application.persistentDataPath, saveFileName);
                actualSaveLocation = saveFilePath;
                usingFallbackLocation = true;
                Debug.LogWarning($"DebugUI: Accessible location not available, using fallback: {saveFilePath}");
            }

                Debug.Log(saveFilePath);
        }

        /// <summary>
        /// Get an accessible save path based on the current environment
        /// </summary>
        private string GetAccessibleSavePath()
        {
            try
            {
#if UNITY_EDITOR
                // In editor: Save relative to project folder
                string projectPath = Directory.GetParent(Application.dataPath).FullName;
                string editorSavePath = Path.Combine(projectPath, customSaveFolder, saveFileName);

                // Test if we can write to this location
                if (TestWriteAccess(Path.GetDirectoryName(editorSavePath)))
                {
                    return editorSavePath;
                }
#else
                // In build: Try to save relative to executable
                if (Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    // Windows: Save next to the .exe file
                    string exeDirectory = Path.GetDirectoryName(Application.dataPath); // dataPath in build points to Data folder
                    string buildSavePath = Path.Combine(exeDirectory, customSaveFolder, saveFileName);
                
                    // Test if we can write to this location
                    if (TestWriteAccess(Path.GetDirectoryName(buildSavePath)))
                    {
                        return buildSavePath;
                    }
                }
                else if (Application.platform == RuntimePlatform.OSXPlayer)
                {
                    // macOS: Save in a reasonable location (next to .app bundle)
                    string appPath = Application.dataPath;
                    string appDirectory = Directory.GetParent(Directory.GetParent(Directory.GetParent(appPath).FullName).FullName).FullName;
                    string macSavePath = Path.Combine(appDirectory, customSaveFolder, saveFileName);
                
                    if (TestWriteAccess(Path.GetDirectoryName(macSavePath)))
                    {
                        return macSavePath;
                    }
                }
                else if (Application.platform == RuntimePlatform.LinuxPlayer)
                {
                    // Linux: Save next to executable
                    string exeDirectory = Path.GetDirectoryName(Application.dataPath);
                    string linuxSavePath = Path.Combine(exeDirectory, customSaveFolder, saveFileName);
                
                    if (TestWriteAccess(Path.GetDirectoryName(linuxSavePath)))
                    {
                        return linuxSavePath;
                    }
                }
#endif
            }
            catch (Exception e)
            {
                Debug.LogWarning($"DebugUI: Error determining accessible save path: {e.Message}");
            }

            return null; // Couldn't find accessible location
        }

        /// <summary>
        /// Test if we can write to a directory
        /// </summary>
        private bool TestWriteAccess(string directoryPath)
        {
            try
            {
                // Create directory if it doesn't exist
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Try to write a test file
                string testFile = Path.Combine(directoryPath, "write_test.tmp");
                File.WriteAllText(testFile, "test");

                // Clean up test file
                if (File.Exists(testFile))
                {
                    File.Delete(testFile);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Serialization
        /// <summary>
        /// Get the save key for a control (uses custom key if provided, otherwise generates one)
        /// </summary>
        private string GetSaveKey(DebugTabConfig tabConfig, DebugControlConfig control)
        {
            return !string.IsNullOrEmpty(control.saveKey) ? control.saveKey : $"{tabConfig.name}.{control.name}";
        }
        /// <summary>
        /// Force save immediately regardless of auto-save settings
        /// </summary>
        public void ForceSave()
        {
            if (enableSerialization)
            {
                SaveValues();
                Debug.Log("DebugUI: Manual save completed");
            }
        }
        /// <summary>
        /// Save all serializable control values with improved auto-save logic
        /// </summary>
        public void SaveValues()
        {
            if (!enableSerialization) return;
            ShowSavingIndicator();

            debugData.savedValues.Clear();

            foreach (var tabConfig in tabConfigs)
            {
                foreach (var control in tabConfig.controls)
                {
                    if (!control.saveValue) continue;

                    var savedValue = new SavedValue
                    {
                        key = GetSaveKey(tabConfig, control),
                        type = control.type
                    };

                    switch (control.type)
                    {
                        case DebugControlConfig.ControlType.Slider:
                            if (control.getter != null)
                            {
                                savedValue.floatValue = control.getter();
                                debugData.savedValues.Add(savedValue);
                            }
                            break;
                        case DebugControlConfig.ControlType.Toggle:
                            if (control.boolGetter != null)
                            {
                                savedValue.boolValue = control.boolGetter();
                                debugData.savedValues.Add(savedValue);
                            }
                            break;
                    }
                }
            }

            if (saveToPlayerPrefs)
            {
                SaveToPlayerPrefs();
            }
            else
            {
                SaveToFile();
            }

            lastSaveTime = Time.time;
            hasUnsavedChanges = false;

            // Update save button appearance
            UpdateSaveButtonAppearance();
            ShowSavedIndicator();
        }
        /// <summary>
        /// Load all serializable control values
        /// </summary>
        public void LoadValues()
        {
            if (!enableSerialization) return;

            if (saveToPlayerPrefs)
            {
                LoadFromPlayerPrefs();
            }
            else
            {
                LoadFromFile();
            }

            // Apply loaded values
            var valueDict = new Dictionary<string, SavedValue>();
            foreach (var savedValue in debugData.savedValues)
            {
                valueDict[savedValue.key] = savedValue;
            }

            foreach (var tabConfig in tabConfigs)
            {
                foreach (var control in tabConfig.controls)
                {
                    if (!control.saveValue) continue;

                    string key = GetSaveKey(tabConfig, control);
                    if (valueDict.TryGetValue(key, out var savedValue))
                    {
                        switch (control.type)
                        {
                            case DebugControlConfig.ControlType.Slider:
                                control.setter?.Invoke(savedValue.floatValue);
                                break;
                            case DebugControlConfig.ControlType.Toggle:
                                control.boolSetter?.Invoke(savedValue.boolValue);
                                break;
                        }
                    }
                }
            }
        }
        private void SaveToFile()
        {
            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(saveFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Configure JSON settings for clean float formatting
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    FloatFormatHandling = FloatFormatHandling.String,
                    FloatParseHandling = FloatParseHandling.Double
                };

                // Custom converter to control decimal places
                settings.Converters.Add(new FloatConverter(jsonDecimalPlaces));

                string json = JsonConvert.SerializeObject(debugData, settings);
                File.WriteAllText(saveFilePath, json);

                string locationInfo = usingFallbackLocation ? " (fallback location)" : " (accessible location)";
                Debug.Log($"DebugUI: Settings saved to {saveFilePath}{locationInfo}");
            }
            catch (Exception e)
            {
                Debug.LogError($"DebugUI: Failed to save settings - {e.Message}");

                // If we failed and weren't using fallback, try fallback location
                if (!usingFallbackLocation && preferAccessibleLocation)
                {
                    Debug.Log("DebugUI: Attempting to save to fallback location...");
                    string fallbackPath = Path.Combine(Application.persistentDataPath, saveFileName);
                    try
                    {
                        string json = JsonConvert.SerializeObject(debugData, new JsonSerializerSettings
                        {
                            Formatting = Formatting.Indented,
                            FloatFormatHandling = FloatFormatHandling.String,
                            FloatParseHandling = FloatParseHandling.Double,
                            Converters = { new FloatConverter(jsonDecimalPlaces) }
                        });
                        File.WriteAllText(fallbackPath, json);
                        Debug.Log($"DebugUI: Settings saved to fallback location: {fallbackPath}");
                    }
                    catch (Exception fallbackException)
                    {
                        Debug.LogError($"DebugUI: Failed to save to fallback location - {fallbackException.Message}");
                    }
                }
            }
        }
        private void LoadFromFile()
        {
            try
            {
                if (!File.Exists(saveFilePath))
                {
                    Debug.Log($"DebugUI: No save file found at {saveFilePath}");
                    return;
                }

                string json = File.ReadAllText(saveFilePath);
                debugData = JsonConvert.DeserializeObject<DebugUIData>(json);

                if (debugData?.savedValues == null)
                {
                    debugData = new DebugUIData();
                    Debug.LogWarning("DebugUI: Invalid save file format");
                    return;
                }

                Debug.Log($"DebugUI: Loaded {debugData.savedValues.Count} saved values from {saveFilePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"DebugUI: Failed to load settings - {e.Message}");
                debugData = new DebugUIData();
            }
        }
        private void SaveToPlayerPrefs()
        {
            try
            {
                // Configure JSON settings for clean float formatting
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    FloatFormatHandling = FloatFormatHandling.String,
                    FloatParseHandling = FloatParseHandling.Double
                };

                // Custom converter to control decimal places
                settings.Converters.Add(new FloatConverter(jsonDecimalPlaces));

                string json = JsonConvert.SerializeObject(debugData, settings);
                PlayerPrefs.SetString("DebugUI_Settings", json);
                PlayerPrefs.Save();
                Debug.Log($"DebugUI: Settings saved to PlayerPrefs ({debugData.savedValues.Count} values)");
            }
            catch (Exception e)
            {
                Debug.LogError($"DebugUI: Failed to save to PlayerPrefs - {e.Message}");
            }
        }
        private void LoadFromPlayerPrefs()
        {
            try
            {
                if (!PlayerPrefs.HasKey("DebugUI_Settings"))
                {
                    Debug.Log("DebugUI: No PlayerPrefs data found");
                    return;
                }

                string json = PlayerPrefs.GetString("DebugUI_Settings");
                debugData = JsonConvert.DeserializeObject<DebugUIData>(json);

                if (debugData?.savedValues == null)
                {
                    debugData = new DebugUIData();
                    Debug.LogWarning("DebugUI: Invalid PlayerPrefs data format");
                    return;
                }

                Debug.Log($"DebugUI: Loaded {debugData.savedValues.Count} saved values from PlayerPrefs");
            }
            catch (Exception e)
            {
                Debug.LogError($"DebugUI: Failed to load from PlayerPrefs - {e.Message}");
                debugData = new DebugUIData();
            }
        }
        /// <summary>
        /// Clear all saved settings
        /// </summary>
        public void ClearSavedSettings()
        {
            if (saveToPlayerPrefs)
            {
                PlayerPrefs.DeleteKey("DebugUI_Settings");
                PlayerPrefs.Save();
                Debug.Log("DebugUI: PlayerPrefs settings cleared");
            }
            else
            {
                if (File.Exists(saveFilePath))
                {
                    File.Delete(saveFilePath);
                    Debug.Log($"DebugUI: Settings file deleted: {saveFilePath}");
                }
            }

            debugData = new DebugUIData();
        }
        /// <summary>
        /// Open the folder containing the save file (desktop platforms only)
        /// </summary>
        public void OpenSaveFolder()
        {
            if (saveToPlayerPrefs)
            {
                Debug.Log("DebugUI: Using PlayerPrefs - no file folder to open");
                return;
            }

            try
            {
                string folderPath = Path.GetDirectoryName(saveFilePath);

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                System.Diagnostics.Process.Start("explorer.exe", folderPath.Replace('/', '\\'));
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                System.Diagnostics.Process.Start("open", folderPath);
#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
                System.Diagnostics.Process.Start("xdg-open", folderPath);
#else
                Debug.Log($"DebugUI: Save folder: {folderPath}");
#endif
            }
            catch (Exception e)
            {
                Debug.LogError($"DebugUI: Failed to open save folder - {e.Message}");
            }
        }
        private void HandleAutoSave()
        {
            float timeSinceLastChange = Time.time - lastChangeTime;
            float timeSinceLastSave = Time.time - lastSaveTime;

            switch (autoSaveMode)
            {
                case AutoSaveMode.Immediate:
                    // Original behavior - save immediately
                    SaveValues();
                    hasUnsavedChanges = false;
                    break;

                case AutoSaveMode.Debounced:
                    // Save after delay when user stops changing values
                    if (timeSinceLastChange >= autoSaveDelay)
                    {
                        SaveValues();
                        hasUnsavedChanges = false;
                    }
                    break;

                case AutoSaveMode.Interval:
                    // Save at regular intervals
                    if (timeSinceLastSave >= autoSaveInterval)
                    {
                        SaveValues();
                        hasUnsavedChanges = false;
                    }
                    break;

                case AutoSaveMode.Manual:
                    // Only save on app events or manual save
                    break;
            }
        }
        private void MarkAsChanged()
        {
            hasUnsavedChanges = true;
            lastChangeTime = Time.time;

            // Update save button appearance
            UpdateSaveButtonAppearance();
            UpdateSaveStatusIndicator();
        }
        private void AddSerializationButtons()
        {
            var footer = root.Q<VisualElement>("Footer");
            if (footer == null) return;

            // Create manual save button
            saveButton = new Button();
            saveButton.name = "save-button";
            saveButton.AddToClassList("footer-button");
            UpdateSaveButtonAppearance(); // Set initial appearance

            // Set up click handler
            saveButton.clicked += OnSaveButtonClicked;
            footer.Insert(0, saveButton);

            // Clear button
            var clearButton = new Button(() => {
                ClearSavedSettings();
                // Optionally reload to show cleared state
                LoadValues();
                UpdateSaveButtonAppearance(); // Update button after clearing
            })
            {
                text = "Clear Saved"
            };
            clearButton.AddToClassList("footer-button");
            footer.Insert(1, clearButton);

            // Open folder button (desktop only)
            if (!saveToPlayerPrefs && (Application.platform == RuntimePlatform.WindowsPlayer ||
                                       Application.platform == RuntimePlatform.OSXPlayer ||
                                       Application.platform == RuntimePlatform.LinuxPlayer ||
                                       Application.isEditor))
            {
                var openFolderButton = new Button(() => OpenSaveFolder())
                {
                    text = "Open Folder"
                };
                openFolderButton.AddToClassList("footer-button");
                footer.Insert(2, openFolderButton);
            }
        }
        private void OnSaveButtonClicked()
        {
            ForceSave();

            // Visual feedback
            saveButton.text = "Saved!";
            saveButton.SetEnabled(false);
            saveButton.RemoveFromClassList("unsaved-changes");
            saveButton.AddToClassList("save-feedback");

            // Reset button after delay
            if (saveButtonResetCoroutine != null)
            {
                StopCoroutine(saveButtonResetCoroutine);
            }
            saveButtonResetCoroutine = StartCoroutine(ResetSaveButtonCoroutine());
        }
        private System.Collections.IEnumerator ResetSaveButtonCoroutine()
        {
            yield return new WaitForSeconds(1.5f);

            if (saveButton != null)
            {
                saveButton.SetEnabled(true);
                saveButton.RemoveFromClassList("save-feedback");
                UpdateSaveButtonAppearance();
            }

            saveButtonResetCoroutine = null;
        }

        private void UpdateSaveButtonAppearance()
        {
            if (saveButton == null) return;

            if (hasUnsavedChanges)
            {
                saveButton.text = "Save Settings *";
                saveButton.AddToClassList("unsaved-changes");
                saveButton.RemoveFromClassList("save-feedback");
            }
            else
            {
                saveButton.text = "Save Settings";
                saveButton.RemoveFromClassList("unsaved-changes");
                saveButton.RemoveFromClassList("save-feedback");
            }
        }
        // Add this method to create the save status indicator
        private void SetupSaveStatusIndicator()
        {
            // Create save status indicator
            saveStatusIndicator = new Label();
            saveStatusIndicator.name = "save-status-indicator";
            saveStatusIndicator.AddToClassList("save-status");
            saveStatusIndicator.text = "Unsaved Changes";

            // Add to root
            var footer = root.Q<VisualElement>("Footer");
            footer.Add(saveStatusIndicator);

            // Set initial visibility based on current state
            if (hasUnsavedChanges)
            {
                saveStatusIndicator.AddToClassList("visible");
                saveStatusIndicator.AddToClassList("unsaved");
            }
        }

        // Add this method to update the indicator state
        private void UpdateSaveStatusIndicator()
        {
            if (saveStatusIndicator == null) return;

            if (hasUnsavedChanges)
            {
                saveStatusIndicator.text = "Unsaved Changes";
                saveStatusIndicator.RemoveFromClassList("saved");
                saveStatusIndicator.RemoveFromClassList("saving");
                saveStatusIndicator.AddToClassList("unsaved");
                saveStatusIndicator.AddToClassList("visible");
            }
            else
            {
                saveStatusIndicator.RemoveFromClassList("visible");
                saveStatusIndicator.RemoveFromClassList("unsaved");
                saveStatusIndicator.RemoveFromClassList("saving");
            }
        }
        // Add this method to show saving state
        private void ShowSavingIndicator()
        {
            if (saveStatusIndicator == null) return;

            saveStatusIndicator.text = "Saving...";
            saveStatusIndicator.RemoveFromClassList("unsaved");
            saveStatusIndicator.RemoveFromClassList("saved");
            saveStatusIndicator.AddToClassList("saving");
            saveStatusIndicator.AddToClassList("visible");
        }

        // Add this method to show saved confirmation
        private void ShowSavedIndicator()
        {
            if (saveStatusIndicator == null) return;

            saveStatusIndicator.text = "Saved";
            saveStatusIndicator.RemoveFromClassList("unsaved");
            saveStatusIndicator.RemoveFromClassList("saving");
            saveStatusIndicator.AddToClassList("saved");
            saveStatusIndicator.AddToClassList("visible");

            // Hide after 2 seconds
            if (saveStatusCoroutine != null)
            {
                StopCoroutine(saveStatusCoroutine);
            }
            saveStatusCoroutine = StartCoroutine(HideSaveStatusAfterDelay());
        }

        // Add this coroutine to hide the indicator after showing "Saved"
        private System.Collections.IEnumerator HideSaveStatusAfterDelay()
        {
            yield return new WaitForSeconds(2f);

            if (saveStatusIndicator != null)
            {
                saveStatusIndicator.RemoveFromClassList("visible");
                saveStatusIndicator.RemoveFromClassList("saved");
            }

            saveStatusCoroutine = null;
        }

        #endregion

        #region UI Initialization and Event Handling
        private void InitializeUI()
        {
            // Get UI containers
            debugPanel = root.Q<VisualElement>("DebugPanel");
            tabButtonsContainer = root.Q<VisualElement>("TabButtons");
            tabContentContainer = root.Q<VisualElement>("TabContentContainer");

            if (tabButtonsContainer == null || tabContentContainer == null)
            {
                Debug.LogError("DebugUI: Required UI containers not found.");
                return;
            }

            // Set panel title
            var headerText = root.Q<Label>(className: "header-text");
            if (headerText != null)
            {
                headerText.text = panelTitle;
            }

            // Create tabs and buttons
            foreach (var tabConfig in tabConfigs)
            {
                CreateTab(tabConfig);
            }

            // Show first tab by default
            if (tabConfigs.Count > 0)
            {
                ShowTab(tabConfigs[0].name);
            }
            SetupSaveStatusIndicator();
        }
        /// <summary>
        /// Override this method to configure your tabs and controls.
        /// This is where you define what appears in your debug UI.
        /// </summary>
        protected virtual void ConfigureTabs()
        {
            // Example configuration - replace with your own
            var exampleTab = new DebugTabConfig
            {
                name = "Example",
                displayName = "Example Tab"
            };

            exampleTab.controls.AddRange(new[]
            {
                new DebugControlConfig
                {
                    name = "ExampleFloat",
                    displayName = "Example Float",
                    tooltip = "This is an example float value",
                    type = DebugControlConfig.ControlType.Slider,
                    sectionName = "Example Settings",
                    minValue = 0f,
                    maxValue = 10f,
                    defaultValue = 5f,
                    saveValue = true, // This value will be saved/loaded
                    getter = () => 5f, // Replace with your actual getter
                    setter = (value) => { /* Replace with your actual setter */ }
                },
                new DebugControlConfig
                {
                    name = "ExampleBool",
                    displayName = "Example Toggle",
                    tooltip = "This is an example boolean value",
                    type = DebugControlConfig.ControlType.Toggle,
                    sectionName = "Example Settings",
                    defaultBoolValue = false,
                    saveValue = true, // This value will be saved/loaded
                    boolGetter = () => false, // Replace with your actual getter
                    boolSetter = (value) => { /* Replace with your actual setter */ }
                }
            });

            tabConfigs.Add(exampleTab);
        }
        private void CreateTab(DebugTabConfig tabConfig)
        {
            // Create tab button
            var tabButton = new Button(() => ShowTab(tabConfig.name))
            {
                text = tabConfig.displayName
            };
            tabButton.AddToClassList("tab-button");
            tabButtonsContainer.Add(tabButton);
            tabButtons[tabConfig.name] = tabButton;

            // Create tab content container
            var tabContent = new VisualElement();
            tabContent.AddToClassList("tab-content");
            tabContent.style.display = DisplayStyle.None;

            // Create scroll view for tab content
            var scrollView = new ScrollView();
            scrollView.AddToClassList("tab-content-scroll");

            // Group controls by section
            string currentSection = null;
            foreach (var control in tabConfig.controls)
            {
                // Add section header if this control belongs to a new section
                if (!string.IsNullOrEmpty(control.sectionName) && control.sectionName != currentSection)
                {
                    var sectionHeader = new Label(control.sectionName);
                    sectionHeader.AddToClassList("section-header");
                    scrollView.Add(sectionHeader);
                    currentSection = control.sectionName;
                }

                // Create control based on type
                switch (control.type)
                {
                    case DebugControlConfig.ControlType.Slider:
                        CreateSliderControl(scrollView, control);
                        break;
                    case DebugControlConfig.ControlType.Toggle:
                        CreateToggleControl(scrollView, control);
                        break;
                    case DebugControlConfig.ControlType.InfoDisplay:
                        CreateInfoControl(scrollView, control);
                        break;
                }
            }

            tabContent.Add(scrollView);
            tabContentContainer.Add(tabContent);
            tabElements[tabConfig.name] = tabContent;
        }
        private void ShowTab(string tabName)
        {
            // Hide all tabs
            foreach (var tab in tabElements.Values)
            {
                tab.style.display = DisplayStyle.None;
            }

            // Remove active class from all buttons
            foreach (var button in tabButtons.Values)
            {
                button.RemoveFromClassList("tab-button-active");
            }

            // Show selected tab
            if (tabElements.TryGetValue(tabName, out var selectedTab))
            {
                selectedTab.style.display = DisplayStyle.Flex;
                currentActiveTab = tabName;
            }

            // Add active class to selected button
            if (tabButtons.TryGetValue(tabName, out var selectedButton))
            {
                selectedButton.AddToClassList("tab-button-active");
            }
        }
        protected void AddTab(DebugTabConfig tabConfig)
        {
            tabConfigs.Add(tabConfig);
        }
        private void CreateSliderControl(VisualElement parent, DebugControlConfig config)
        {
            var container = new VisualElement();
            container.AddToClassList("slider-container");

            var label = new Label(config.displayName);

            // Build tooltip text
            string tooltipText = config.tooltip;
            if (config.saveValue && enableSerialization)
            {
                label.text += " *";
                if (!string.IsNullOrEmpty(tooltipText))
                {
                    tooltipText += " (Auto-saved)";
                }
                else
                {
                    tooltipText = "Auto-saved";
                }
            }

            // Register tooltip for runtime
            if (!string.IsNullOrEmpty(tooltipText))
            {
                RegisterTooltip(label, tooltipText);
            }

            container.Add(label);

            var sliderContainer = new VisualElement();
            sliderContainer.AddToClassList("slider-with-value");

            var slider = new Slider(config.minValue, config.maxValue);
            slider.AddToClassList("slider");
            slider.name = config.name;
            slider.value = config.getter != null ? config.getter() : config.defaultValue;

            var valueField = new FloatField();
            valueField.name = config.name + "Field";
            valueField.value = slider.value;
            valueField.AddToClassList("value-field");

            slider.RegisterValueChangedCallback(evt => {
                valueField.SetValueWithoutNotify(evt.newValue);
                config.setter?.Invoke(evt.newValue);

                // Mark as changed instead of immediate save
                if (enableSerialization && config.saveValue)
                {
                    MarkAsChanged();
                }
            });

            valueField.RegisterValueChangedCallback(evt => {
                float clampedValue = Mathf.Clamp(evt.newValue, config.minValue, config.maxValue);
                valueField.SetValueWithoutNotify(clampedValue);
                slider.SetValueWithoutNotify(clampedValue);
                config.setter?.Invoke(clampedValue);

                // Mark as changed instead of immediate save
                if (enableSerialization && config.saveValue)
                {
                    MarkAsChanged();
                }
            });

            sliderContainer.Add(slider);
            sliderContainer.Add(valueField);
            container.Add(sliderContainer);
            parent.Add(container);
        }

        private void CreateToggleControl(VisualElement parent, DebugControlConfig config)
        {
            var container = new VisualElement();
            container.AddToClassList("toggle-container");

            var toggle = new Toggle(config.displayName);
            toggle.name = config.name;
            toggle.value = config.boolGetter != null ? config.boolGetter() : config.defaultBoolValue;

            // Build tooltip text
            string tooltipText = config.tooltip;
            if (config.saveValue && enableSerialization)
            {
                toggle.text += " *";
                if (!string.IsNullOrEmpty(tooltipText))
                {
                    tooltipText += " (Auto-saved)";
                }
                else
                {
                    tooltipText = "Auto-saved";
                }
            }

            // Register tooltip for runtime
            if (!string.IsNullOrEmpty(tooltipText))
            {
                RegisterTooltip(toggle, tooltipText);
            }

            toggle.RegisterValueChangedCallback(evt => {
                config.boolSetter?.Invoke(evt.newValue);

                // Mark as changed instead of immediate save
                if (enableSerialization && config.saveValue)
                {
                    MarkAsChanged();
                }
            });

            container.Add(toggle);
            parent.Add(container);
        }
        private void CreateInfoControl(VisualElement parent, DebugControlConfig config)
        {
            var container = new VisualElement();
            container.AddToClassList("info-container");

            var label = new Label(config.displayName);
            label.AddToClassList("info-label");

            var valueLabel = new Label();
            valueLabel.AddToClassList("info-value");
            valueLabel.name = config.name + "Value";

            // Register tooltip for runtime
            if (!string.IsNullOrEmpty(config.tooltip))
            {
                RegisterTooltip(container, config.tooltip);
            }

            container.Add(label);
            container.Add(valueLabel);
            parent.Add(container);
        }
        private void UpdateInfoDisplays()
        {
            foreach (var tabConfig in tabConfigs)
            {
                foreach (var control in tabConfig.controls)
                {
                    if (control.type == DebugControlConfig.ControlType.InfoDisplay)
                    {
                        var valueLabel = root.Q<Label>(control.name + "Value");
                        if (valueLabel != null && control.stringGetter != null)
                        {
                            valueLabel.text = control.stringGetter();
                        }
                    }
                }
            }
        }
        private void SetupEventHandlers()
        {
            // Close button
            var closeButton = root.Q<Button>("CloseButton");
            if (closeButton != null)
            {
                closeButton.clicked += ToggleVisibility;
            }

            // Reset button
            var resetButton = root.Q<Button>("ResetButton");
            if (resetButton != null)
            {
                resetButton.clicked += ResetToOriginalValues;
            }

            // Add serialization buttons if enabled
            if (enableSerialization)
            {
                AddSerializationButtons();
            }
        }
        #endregion

        #region Mobile Support
        private void SetupMobileSupport()
        {
            if (!enableMobileSupport) return;

            // Initialize touch tracking arrays
            touchStartTimes = new float[10]; // Support up to 10 touches

            // Create on-screen toggle button if needed
            if (mobileTriggerType == MobileTriggerType.OnScreenButton || showToggleButton)
            {
                CreateMobileToggleButton();
            }
        }
        private void CreateMobileToggleButton()
        {
            // Create a floating toggle button for mobile
            mobileToggleButton = new Button(() => ToggleVisibility());
            mobileToggleButton.text = toggleButtonText;
            mobileToggleButton.AddToClassList("mobile-toggle-button");

            // Position it in the top-right corner
            mobileToggleButton.style.position = Position.Absolute;
            mobileToggleButton.style.top = 10;
            mobileToggleButton.style.right = 10;
            mobileToggleButton.style.width = 80;
            mobileToggleButton.style.height = 40;
            mobileToggleButton.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            mobileToggleButton.style.color = Color.white;
            mobileToggleButton.style.borderTopLeftRadius = 5;
            mobileToggleButton.style.borderTopRightRadius = 5;
            mobileToggleButton.style.borderBottomLeftRadius = 5;
            mobileToggleButton.style.borderBottomRightRadius = 5;

            // Initially show button only if panel is hidden
            mobileToggleButton.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;

            root.Add(mobileToggleButton);
        }
        private void HandleMobileInput()
        {
            switch (mobileTriggerType)
            {
                case MobileTriggerType.TouchGesture:
                    HandleMultiTouchGesture();
                    break;
                case MobileTriggerType.TouchAndHold:
                    HandleTouchAndHold();
                    break;
                case MobileTriggerType.OnScreenButton:
                    // Button is always visible, no additional input handling needed
                    break;
            }
        }
        private void HandleMultiTouchGesture()
        {
            if (Input.touchCount == touchCount)
            {
                bool allTouchesStartedThisFrame = true;
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (Input.GetTouch(i).phase != TouchPhase.Began)
                    {
                        allTouchesStartedThisFrame = false;
                        break;
                    }
                }

                if (allTouchesStartedThisFrame)
                {
                    ToggleVisibility();
                }
            }
        }
        private void HandleTouchAndHold()
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    isTouchHolding = true;
                    touchHoldStartTime = Time.time;
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    isTouchHolding = false;
                }
                else if (isTouchHolding && touch.phase == TouchPhase.Stationary)
                {
                    if (Time.time - touchHoldStartTime >= touchHoldTime)
                    {
                        ToggleVisibility();
                        isTouchHolding = false; // Prevent multiple triggers
                    }
                }
            }
            else
            {
                isTouchHolding = false;
            }
        }
        #endregion

        #region Helper Methods
        private void StoreOriginalValues()
        {
            originalValues.Clear();
            originalBoolValues.Clear();

            foreach (var tabConfig in tabConfigs)
            {
                foreach (var control in tabConfig.controls)
                {
                    string key = $"{tabConfig.name}.{control.name}";

                    switch (control.type)
                    {
                        case DebugControlConfig.ControlType.Slider:
                            if (control.getter != null)
                            {
                                originalValues[key] = control.getter();
                            }
                            break;
                        case DebugControlConfig.ControlType.Toggle:
                            if (control.boolGetter != null)
                            {
                                originalBoolValues[key] = control.boolGetter();
                            }
                            break;
                    }
                }
            }

            Debug.Log($"DebugUI: Stored {originalValues.Count} original float values and {originalBoolValues.Count} original bool values");
        }
        private void ResetToOriginalValues()
        {
            Debug.Log("DebugUI: Resetting to original values...");

            foreach (var tabConfig in tabConfigs)
            {
                foreach (var control in tabConfig.controls)
                {
                    string key = $"{tabConfig.name}.{control.name}";

                    switch (control.type)
                    {
                        case DebugControlConfig.ControlType.Slider:
                            if (originalValues.TryGetValue(key, out float originalValue))
                            {
                                // Set the data value
                                control.setter?.Invoke(originalValue);

                                // Update the UI elements to reflect the reset value
                                var slider = root.Q<Slider>(control.name);
                                if (slider != null)
                                {
                                    slider.SetValueWithoutNotify(originalValue);
                                }
                                var floatField = root.Q<FloatField>(control.name + "Field");
                                if (floatField != null)
                                {
                                    floatField.SetValueWithoutNotify(originalValue);
                                }
                            }
                            break;
                        case DebugControlConfig.ControlType.Toggle:
                            if (originalBoolValues.TryGetValue(key, out bool originalBoolValue))
                            {
                                // Set the data value
                                control.boolSetter?.Invoke(originalBoolValue);

                                // Update the UI element to reflect the reset value
                                var toggle = root.Q<Toggle>(control.name);
                                if (toggle != null)
                                {
                                    toggle.SetValueWithoutNotify(originalBoolValue);
                                }
                            }
                            break;
                    }
                }
            }

            Debug.Log("DebugUI: Reset to original values complete");

            // Auto-save the reset values if serialization is enabled
            if (enableSerialization)
            {
                SaveValues();
            }
        }
        private void ToggleVisibility()
        {
            isVisible = !isVisible;
            debugPanel.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;

            // Update mobile toggle button visibility - show when panel is hidden, hide when panel is visible
            if (mobileToggleButton != null)
            {
                mobileToggleButton.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;
            }

            // Hide tooltip when UI is hidden
            if (!isVisible)
            {
                HideTooltip();
            }
        }
        /// <summary>
        /// Show the debug UI - these are hooks for external scripts to control visibility
        /// </summary>
        public void Show()
        {
            isVisible = true;
            debugPanel.style.display = DisplayStyle.Flex;
        }
        /// <summary>
        /// Hide the debug UI - these are hooks for external scripts to control visibility
        /// </summary>
        public void Hide()
        {
            isVisible = false;
            debugPanel.style.display = DisplayStyle.None;
            HideTooltip();
        }
        /// <summary>
        /// Check if the debug UI is currently visible - this can be used by external scripts to determine visibility state
        /// </summary>
        public bool IsVisible => isVisible;
        #endregion
    }

    #region JSON Serialization Helpers
    /// <summary>
    /// Custom JSON converter for formatting floats with specific decimal places
    /// </summary>
    public class FloatConverter : JsonConverter<float>
    {
        private readonly int decimalPlaces;

        public FloatConverter(int decimalPlaces)
        {
            this.decimalPlaces = decimalPlaces;
        }

        public override void WriteJson(JsonWriter writer, float value, JsonSerializer serializer)
        {
            // Round to specified decimal places and write as string to preserve formatting
            string formattedValue = value.ToString($"F{decimalPlaces}");
            writer.WriteValue(float.Parse(formattedValue));
        }

        public override float ReadJson(JsonReader reader, Type objectType, float existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            // Standard float reading
            if (reader.Value == null) return 0f;
            return Convert.ToSingle(reader.Value);
        }
    }
    #endregion
}
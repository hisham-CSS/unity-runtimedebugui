using UnityEditor;
using UnityEngine;
using static RuntimeDebugUI.DebugUI;

namespace RuntimeDebugUI.Editor
{
    [CustomEditor(typeof(DebugUI), true)]
    public class DebugUIEditor : UnityEditor.Editor
    {
        private SerializedProperty uiDocument;
        private SerializedProperty toggleKey;
        private SerializedProperty showOnStart;
        private SerializedProperty panelTitle;

        // Mobile Support
        private SerializedProperty enableMobileSupport;
        private SerializedProperty mobileTriggerType;
        private SerializedProperty touchCount;
        private SerializedProperty touchHoldTime;
        private SerializedProperty showToggleButton;
        private SerializedProperty toggleButtonText;

        // Serialization
        private SerializedProperty enableSerialization;
        private SerializedProperty saveFileName;
        private SerializedProperty saveToPlayerPrefs;
        private SerializedProperty preferAccessibleLocation;
        private SerializedProperty customSaveFolder;
        private SerializedProperty jsonDecimalPlaces;

        // Auto-Save Configuration
        private SerializedProperty autoSaveMode;
        private SerializedProperty autoSaveDelay;
        private SerializedProperty autoSaveInterval;
        private SerializedProperty saveOnApplicationPause;
        private SerializedProperty saveOnApplicationFocus;
        private SerializedProperty saveOnDestroy;

        // Tooltip System
        private SerializedProperty tooltipDelay;
        private SerializedProperty tooltipOffset;

        // Foldout states
        private bool showUIConfiguration = true;
        private bool showMobileSupport = true;
        private bool showSerialization = true;
        private bool showAutoSave = true;
        private bool showTooltipSystem = true;


        private void OnEnable()
        {
            // UI Configuration
            uiDocument = serializedObject.FindProperty("uiDocument");
            toggleKey = serializedObject.FindProperty("toggleKey");
            showOnStart = serializedObject.FindProperty("showOnStart");
            panelTitle = serializedObject.FindProperty("panelTitle");

            // Mobile Support
            enableMobileSupport = serializedObject.FindProperty("enableMobileSupport");
            mobileTriggerType = serializedObject.FindProperty("mobileTriggerType");
            touchCount = serializedObject.FindProperty("touchCount");
            touchHoldTime = serializedObject.FindProperty("touchHoldTime");
            showToggleButton = serializedObject.FindProperty("showToggleButton");
            toggleButtonText = serializedObject.FindProperty("toggleButtonText");

            // Serialization
            enableSerialization = serializedObject.FindProperty("enableSerialization");
            saveFileName = serializedObject.FindProperty("saveFileName");
            saveToPlayerPrefs = serializedObject.FindProperty("saveToPlayerPrefs");
            preferAccessibleLocation = serializedObject.FindProperty("preferAccessibleLocation");
            customSaveFolder = serializedObject.FindProperty("customSaveFolder");
            jsonDecimalPlaces = serializedObject.FindProperty("jsonDecimalPlaces");

            // Auto-Save Configuration
            autoSaveMode = serializedObject.FindProperty("autoSaveMode");
            autoSaveDelay = serializedObject.FindProperty("autoSaveDelay");
            autoSaveInterval = serializedObject.FindProperty("autoSaveInterval");
            saveOnApplicationPause = serializedObject.FindProperty("saveOnApplicationPause");
            saveOnApplicationFocus = serializedObject.FindProperty("saveOnApplicationFocus");
            saveOnDestroy = serializedObject.FindProperty("saveOnDestroy");

            // Tooltip System
            tooltipDelay = serializedObject.FindProperty("tooltipDelay");
            tooltipOffset = serializedObject.FindProperty("tooltipOffset");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(5);

            // Header
            EditorGUILayout.LabelField("Runtime Debug UI System", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Data-driven debug panel with mobile support", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);

            // UI Configuration Section
            showUIConfiguration = EditorGUILayout.BeginFoldoutHeaderGroup(showUIConfiguration, "UI Configuration");
            if (showUIConfiguration)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(uiDocument, new GUIContent("UI Document", "UIDocument component reference"));
                EditorGUILayout.PropertyField(toggleKey, new GUIContent("Toggle Key", "Keyboard key to show/hide debug panel"));
                EditorGUILayout.PropertyField(showOnStart, new GUIContent("Show On Start", "Whether to show the debug panel when the game starts"));
                EditorGUILayout.PropertyField(panelTitle, new GUIContent("Panel Title", "Title displayed in the debug panel header"));
                EditorGUILayout.Space(5);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Mobile Support Section
            showMobileSupport = EditorGUILayout.BeginFoldoutHeaderGroup(showMobileSupport, "Mobile Support");
            if (showMobileSupport)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(enableMobileSupport, new GUIContent("Enable Mobile Support", "Enable touch input handling for mobile devices"));

                if (enableMobileSupport.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.Space(3);

                    EditorGUILayout.PropertyField(mobileTriggerType, new GUIContent("Trigger Type", "How users open the debug panel on mobile"));

                    // Show relevant options based on trigger type
                    DebugUI.MobileTriggerType triggerType = (DebugUI.MobileTriggerType)mobileTriggerType.enumValueIndex;

                    switch (triggerType)
                    {
                        case DebugUI.MobileTriggerType.TouchGesture:
                            EditorGUILayout.PropertyField(touchCount, new GUIContent("Touch Count", "Number of fingers required for multi-touch gesture"));
                            if (touchCount.intValue < 1) touchCount.intValue = 1;
                            if (touchCount.intValue > 5) touchCount.intValue = 5;
                            break;

                        case DebugUI.MobileTriggerType.TouchAndHold:
                            EditorGUILayout.PropertyField(touchHoldTime, new GUIContent("Hold Time", "How long to hold touch (in seconds)"));
                            if (touchHoldTime.floatValue < 0.5f) touchHoldTime.floatValue = 0.5f;
                            if (touchHoldTime.floatValue > 10f) touchHoldTime.floatValue = 10f;
                            break;

                        case DebugUI.MobileTriggerType.OnScreenButton:
                            EditorGUILayout.PropertyField(toggleButtonText, new GUIContent("Button Text", "Text displayed on the toggle button"));
                            break;
                    }

                    // Show toggle button option for all types
                    EditorGUILayout.Space(3);
                    EditorGUILayout.PropertyField(showToggleButton, new GUIContent("Show Toggle Button", "Display an on-screen button (useful as backup or primary method)"));

                    if (showToggleButton.boolValue && triggerType != DebugUI.MobileTriggerType.OnScreenButton)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(toggleButtonText, new GUIContent("Button Text", "Text displayed on the toggle button"));
                        EditorGUI.indentLevel--;
                    }

                    EditorGUI.indentLevel--;

                    // Help box with trigger type explanation
                    EditorGUILayout.Space(5);
                    string helpText = GetMobileTriggerHelpText(triggerType);
                    EditorGUILayout.HelpBox(helpText, MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("Mobile support is disabled. Enable to show touch input options.", MessageType.None);
                }

                EditorGUILayout.Space(5);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Serialization Section
            showSerialization = EditorGUILayout.BeginFoldoutHeaderGroup(showSerialization, "Serialization");
            if (showSerialization)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(enableSerialization, new GUIContent("Enable Serialization", "Auto-save and load debug values between sessions"));

                if (enableSerialization.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.Space(3);

                    EditorGUILayout.PropertyField(saveToPlayerPrefs, new GUIContent("Use PlayerPrefs", "Save to PlayerPrefs instead of files (cross-platform but less accessible)"));

                    if (!saveToPlayerPrefs.boolValue)
                    {
                        // File-based serialization options
                        EditorGUILayout.PropertyField(saveFileName, new GUIContent("Save File Name", "Name of the JSON file to save settings"));
                        EditorGUILayout.PropertyField(preferAccessibleLocation, new GUIContent("Prefer Accessible Location", "Try to save next to executable for easy access"));
                        EditorGUILayout.PropertyField(customSaveFolder, new GUIContent("Custom Save Folder", "Folder name for organizing save files"));

                        // Show current save location info
                        EditorGUILayout.Space(3);
                        string locationInfo = GetSaveLocationInfo();
                        EditorGUILayout.HelpBox(locationInfo, MessageType.Info);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Settings will be saved to PlayerPrefs (registry on Windows, plist on macOS).", MessageType.Info);
                    }

                    EditorGUILayout.Space(3);
                    EditorGUILayout.PropertyField(jsonDecimalPlaces, new GUIContent("JSON Decimal Places", "Number of decimal places in saved JSON (for clean formatting)"));
                    if (jsonDecimalPlaces.intValue < 0) jsonDecimalPlaces.intValue = 0;
                    if (jsonDecimalPlaces.intValue > 10) jsonDecimalPlaces.intValue = 10;

                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.HelpBox("Serialization is disabled. Values will not be saved between sessions.", MessageType.None);
                }

                EditorGUILayout.Space(5);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Auto-Save Configuration Section
            showAutoSave = EditorGUILayout.BeginFoldoutHeaderGroup(showAutoSave, "Auto-Save Configuration");
            if (showAutoSave)
            {
                if (enableSerialization.boolValue)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.PropertyField(autoSaveMode, new GUIContent("Auto-Save Mode", "How and when to automatically save changes"));

                    AutoSaveMode saveMode = (AutoSaveMode)autoSaveMode.enumValueIndex;

                    EditorGUI.indentLevel++;
                    switch (saveMode)
                    {
                        case AutoSaveMode.Immediate:
                            EditorGUILayout.HelpBox("Saves immediately on every value change. May cause performance issues with frequent changes.", MessageType.Warning);
                            break;

                        case AutoSaveMode.Debounced:
                            EditorGUILayout.PropertyField(autoSaveDelay, new GUIContent("Save Delay", "Seconds to wait after last change before saving"));
                            if (autoSaveDelay.floatValue < 0.1f) autoSaveDelay.floatValue = 0.1f;
                            if (autoSaveDelay.floatValue > 30f) autoSaveDelay.floatValue = 30f;
                            EditorGUILayout.HelpBox($"Will save {autoSaveDelay.floatValue:F1} seconds after you stop changing values. Recommended for most use cases.", MessageType.Info);
                            break;

                        case AutoSaveMode.Interval:
                            EditorGUILayout.PropertyField(autoSaveInterval, new GUIContent("Save Interval", "Maximum seconds between automatic saves"));
                            if (autoSaveInterval.floatValue < 5f) autoSaveInterval.floatValue = 5f;
                            if (autoSaveInterval.floatValue > 300f) autoSaveInterval.floatValue = 300f;
                            EditorGUILayout.HelpBox($"Will save every {autoSaveInterval.floatValue:F0} seconds if there are unsaved changes.", MessageType.Info);
                            break;

                        case AutoSaveMode.Manual:
                            EditorGUILayout.HelpBox("Only saves on app events (pause, focus loss, destroy) or manual save button. Use for maximum control.", MessageType.Info);
                            break;
                    }
                    EditorGUI.indentLevel--;

                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("App Event Saving", EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(saveOnApplicationPause, new GUIContent("Save on App Pause", "Save when app is paused (mobile)"));
                    EditorGUILayout.PropertyField(saveOnApplicationFocus, new GUIContent("Save on Focus Loss", "Save when app loses focus"));
                    EditorGUILayout.PropertyField(saveOnDestroy, new GUIContent("Save on Destroy", "Save when component is destroyed"));
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.HelpBox("Auto-save configuration requires serialization to be enabled.", MessageType.None);
                }
                EditorGUILayout.Space(5);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Tooltip System Section
            showTooltipSystem = EditorGUILayout.BeginFoldoutHeaderGroup(showTooltipSystem, "Tooltip System");
            if (showTooltipSystem)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(tooltipDelay, new GUIContent("Tooltip Delay", "Delay before showing tooltip on hover (seconds)"));
                if (tooltipDelay.floatValue < 0f) tooltipDelay.floatValue = 0f;
                if (tooltipDelay.floatValue > 5f) tooltipDelay.floatValue = 5f;

                EditorGUILayout.PropertyField(tooltipOffset, new GUIContent("Tooltip Offset", "Offset from mouse position (pixels)"));

                EditorGUILayout.Space(3);
                EditorGUILayout.HelpBox("Tooltips work in runtime builds and show control descriptions with auto-save indicators.", MessageType.Info);
                EditorGUILayout.Space(5);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Footer info
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Runtime Debug UI v1.3.0", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.LabelField("Created by Hisham Ata", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.LabelField("Configure tabs and controls in ConfigureTabs() method", EditorStyles.centeredGreyMiniLabel);

            serializedObject.ApplyModifiedProperties();
        }

        private string GetMobileTriggerHelpText(DebugUI.MobileTriggerType triggerType)
        {
            switch (triggerType)
            {
                case DebugUI.MobileTriggerType.TouchGesture:
                    return $"Multi-touch: Tap with {touchCount.intValue} finger{(touchCount.intValue > 1 ? "s" : "")} simultaneously to toggle the debug panel. Good for hidden access during gameplay.";

                case DebugUI.MobileTriggerType.TouchAndHold:
                    return $"Touch & Hold: Touch and hold anywhere for {touchHoldTime.floatValue:F1} seconds to toggle. Prevents accidental triggers.";

                case DebugUI.MobileTriggerType.OnScreenButton:
                    return "On-Screen Button: Always visible toggle button in the top-right corner. Maximum accessibility and discoverability.";

                default:
                    return "Select a trigger type to see details.";
            }
        }

        private string GetSaveLocationInfo()
        {
            if (preferAccessibleLocation.boolValue)
            {
                return $"Will try to save to: GameFolder/{customSaveFolder.stringValue}/{saveFileName.stringValue}\nFalls back to persistent data path if not accessible.";
            }
            else
            {
                return $"Will save to: Application.persistentDataPath/{saveFileName.stringValue}\nStandard Unity persistent data location.";
            }
        }
    }
}
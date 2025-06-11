using UnityEngine;
using UnityEditor;
using System.Diagnostics;

namespace RuntimeDebugUI.Editor
{
    /// <summary>
    /// Custom property drawer for mobile trigger type with helpful descriptions
    /// </summary>
    [CustomPropertyDrawer(typeof(DebugUI.MobileTriggerType))]
    public class MobileTriggerTypeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw the enum popup
            Rect enumRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(enumRect, property, label);

            // Add description below
            Rect descRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);

            string description = GetTriggerDescription((DebugUI.MobileTriggerType)property.enumValueIndex);
            EditorGUI.LabelField(descRect, description, EditorStyles.helpBox);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + 2; // Enum + description + spacing
        }

        private string GetTriggerDescription(DebugUI.MobileTriggerType triggerType)
        {
            switch (triggerType)
            {
                case DebugUI.MobileTriggerType.TouchGesture:
                    return "Multi-finger tap gesture (configurable finger count)";
                case DebugUI.MobileTriggerType.TouchAndHold:
                    return "Touch and hold for specified duration";
                case DebugUI.MobileTriggerType.OnScreenButton:
                    return "Always visible toggle button";
                default:
                    return "";
            }
        }
    }
}


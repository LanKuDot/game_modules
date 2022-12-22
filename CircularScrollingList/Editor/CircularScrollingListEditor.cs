using UnityEditor;
using UnityEngine;

namespace AirFishLab.ScrollingList.Editor
{
    [CustomEditor(typeof(CircularScrollingList))]
    [CanEditMultipleObjects]
    public class CircularScrollingListEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertyField("_listBank");
            DrawPropertyField("_listBoxes", true);
            DrawSetting();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPropertyField(string path, bool includeChildren = false)
        {
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(path), includeChildren);
        }

        #region Setting Property Drawer

        private SerializedProperty _settingProperty;
        private SerializedProperty _controlMode;

        private SerializedProperty GetSettingProperty(string path)
        {
            return _settingProperty.FindPropertyRelative(path);
        }

        private void DrawSettingProperty(string path)
        {
            EditorGUILayout.PropertyField(GetSettingProperty(path));
        }

        private void DrawSetting()
        {
            _settingProperty = serializedObject.FindProperty("_setting");
            _settingProperty.isExpanded =
                EditorGUILayout.Foldout(_settingProperty.isExpanded, "Setting");
            if (!_settingProperty.isExpanded)
                return;

            ++EditorGUI.indentLevel;

            _controlMode = GetSettingProperty("_controlMode");
            DrawListMode();
            EditorGUILayout.Space();
            DrawListAppearance();
            EditorGUILayout.Space();
            DrawEvents();

            --EditorGUI.indentLevel;
        }

        private void DrawListMode()
        {
            EditorGUILayout.LabelField("List Mode", EditorStyles.boldLabel);
            DrawSettingProperty("_listType");
            DrawSettingProperty("_controlMode");

            ++EditorGUI.indentLevel;
            if (HasFlag(
                    _controlMode.intValue,
                    (int)CircularScrollingList.ControlMode.Drag)) {
                DrawSettingProperty("_alignMiddle");
            }

            if (HasFlag(
                    _controlMode.intValue,
                    (int)CircularScrollingList.ControlMode.MouseWheel)) {
                DrawSettingProperty("_reverseDirection");
            }
            --EditorGUI.indentLevel;

            DrawSettingProperty("_direction");
            DrawSettingProperty("_centeredContentID");
            DrawSettingProperty("_centerSelectedBox");
            DrawSettingProperty("_reverseOrder");
            DrawSettingProperty("_initializeOnStart");
        }

        private void DrawListAppearance()
        {
            EditorGUILayout.LabelField("List Appearance", EditorStyles.boldLabel);
            DrawSettingProperty("_boxDensity");
            DrawSettingProperty("_boxPositionCurve");
            DrawSettingProperty("_boxScaleCurve");
            if (HasFlag(
                    _controlMode.intValue,
                    (int)CircularScrollingList.ControlMode.Drag))
                DrawSettingProperty("_boxVelocityCurve");
            DrawSettingProperty("_boxMovementCurve");
        }

        private void DrawEvents()
        {
            var boldFoldout =
                new GUIStyle(EditorStyles.foldout) {
                    fontStyle = FontStyle.Bold
                };

            var onBoxClick = GetSettingProperty("_onBoxClick");
            onBoxClick.isExpanded =
                EditorGUILayout.Foldout(
                    onBoxClick.isExpanded, "List Events", true, boldFoldout);

            if (!onBoxClick.isExpanded)
                return;

            ++EditorGUI.indentLevel;
            DrawSettingProperty("_onBoxClick");
            DrawSettingProperty("_onCenteredContentChanged");
            DrawSettingProperty("_onCenteredBoxChanged");
            DrawSettingProperty("_onMovementEnd");
            --EditorGUI.indentLevel;
        }

        #endregion

        #region Utility

        private bool HasFlag(int enumIntValue, int flagValue)
        {
            return (enumIntValue & flagValue) != 0;
        }

        #endregion
    }
}

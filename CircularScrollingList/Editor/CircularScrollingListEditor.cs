using UnityEditor;
using UnityEngine;

namespace AirFishLab.ScrollingList.Editor
{
    [CustomEditor(typeof(CircularScrollingList))]
    [CanEditMultipleObjects]
    public class CircularScrollingListEditor : UnityEditor.Editor
    {
        private bool _toGenerateBoxes;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertyField("_listBank");
            DrawBoxSetting();
            DrawListSetting();

            if (_toGenerateBoxes)
                GenerateBoxesAndArrange();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPropertyField(string path, bool includeChildren = false)
        {
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(path), includeChildren);
        }

        #region Box Property Drawer

        private SerializedProperty _boxSettingProperty;

        private SerializedProperty GetBoxSettingProperty(string path)
        {
            return _boxSettingProperty.FindPropertyRelative(path);
        }

        private void DrawBoxSettingProperty(string path)
        {
            EditorGUILayout.PropertyField(GetBoxSettingProperty(path));
        }

        private void DrawBoxSetting()
        {
            _boxSettingProperty = serializedObject.FindProperty("_boxSetting");
            _boxSettingProperty.isExpanded =
                EditorGUILayout.Foldout(_boxSettingProperty.isExpanded, "Box Setting");
            if (!_boxSettingProperty.isExpanded)
                return;

            ++EditorGUI.indentLevel;

            DrawBoxSettingProperty("_boxRootTransform");
            DrawBoxSettingProperty("_boxPrefab");
            DrawBoxSettingProperty("_numOfBoxes");
            _toGenerateBoxes = GUILayout.Button(
                new GUIContent(
                    "Generate Boxes and Arrange",
                    "Generate the boxes under the specified box root transform and "
                    + "arrange them according to the list setting"));

            DrawListBoxes();

            --EditorGUI.indentLevel;
        }

        private void DrawListBoxes()
        {
            const string showBoxesText = "Show the Boxes";
            const string hideBoxesText = "Hide the Boxes";

            var listBoxesProperty = serializedObject.FindProperty("_listBoxes");
            var toShowBoxes = listBoxesProperty.isExpanded;
            if (GUILayout.Button(toShowBoxes ? hideBoxesText : showBoxesText)) {
                toShowBoxes = !toShowBoxes;
                listBoxesProperty.isExpanded = toShowBoxes;
            }

            if (toShowBoxes)
                DrawPropertyField("_listBoxes", true);
        }

        #endregion

        #region List Setting Property Drawer

        private SerializedProperty _listSettingProperty;
        private SerializedProperty _focusingPosition;
        private SerializedProperty _controlMode;

        private SerializedProperty GetListSettingProperty(string path)
        {
            return _listSettingProperty.FindPropertyRelative(path);
        }

        private void DrawListSettingProperty(string path)
        {
            EditorGUILayout.PropertyField(GetListSettingProperty(path));
        }

        private void DrawListSetting()
        {
            _listSettingProperty = serializedObject.FindProperty("_listSetting");
            _listSettingProperty.isExpanded =
                EditorGUILayout.Foldout(_listSettingProperty.isExpanded, "List Setting");
            if (!_listSettingProperty.isExpanded)
                return;

            ++EditorGUI.indentLevel;

            _focusingPosition = GetListSettingProperty("_focusingPosition");
            _controlMode = GetListSettingProperty("_controlMode");
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
            DrawListSettingProperty("_listType");
            DrawListSettingProperty("_direction");
            DrawListSettingProperty("_controlMode");

            ++EditorGUI.indentLevel;
            if (HasFlag(
                    _controlMode.intValue,
                    (int)CircularScrollingList.ControlMode.Pointer)) {
                DrawListSettingProperty("_alignAtFocusingPosition");
            }

            if (HasFlag(
                    _controlMode.intValue,
                    (int)CircularScrollingList.ControlMode.MouseWheel)) {
                DrawListSettingProperty("_reverseScrollingDirection");
            }
            --EditorGUI.indentLevel;

            DrawListSettingProperty("_focusingPosition");

            ++EditorGUI.indentLevel;
            if (_focusingPosition.intValue ==
                (int)CircularScrollingList.FocusingPosition.Center)
                DrawListSettingProperty("_reverseContentOrder");
            --EditorGUI.indentLevel;

            DrawListSettingProperty("_initFocusingContentID");
            DrawListSettingProperty("_focusSelectedBox");
            DrawListSettingProperty("_initializeOnStart");
        }

        private void DrawListAppearance()
        {
            EditorGUILayout.LabelField("List Appearance", EditorStyles.boldLabel);
            DrawListSettingProperty("_boxDensity");
            DrawListSettingProperty("_boxPositionCurve");
            DrawListSettingProperty("_boxScaleCurve");
            if (HasFlag(
                    _controlMode.intValue,
                    (int)CircularScrollingList.ControlMode.Pointer))
                DrawListSettingProperty("_boxVelocityCurve");
            DrawListSettingProperty("_boxMovementCurve");
        }

        private void DrawEvents()
        {
            var boldFoldout =
                new GUIStyle(EditorStyles.foldout) {
                    fontStyle = FontStyle.Bold
                };

            var onBoxSelected = GetListSettingProperty("_onBoxSelected");
            onBoxSelected.isExpanded =
                EditorGUILayout.Foldout(
                    onBoxSelected.isExpanded, "List Events", true, boldFoldout);

            if (!onBoxSelected.isExpanded)
                return;

            ++EditorGUI.indentLevel;
            DrawListSettingProperty("_onBoxSelected");
            DrawListSettingProperty("_onFocusingBoxChanged");
            DrawListSettingProperty("_onMovementEnd");
            --EditorGUI.indentLevel;
        }

        #endregion

        #region Generate Boxes

        private void GenerateBoxesAndArrange()
        {
            var scrollingList = target as CircularScrollingList;
            if (!scrollingList)
                return;

            Undo.RecordObject(scrollingList, "Generate Boxes and Arrange");
            scrollingList.GenerateBoxesAndArrange();
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

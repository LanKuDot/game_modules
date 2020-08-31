using UnityEditor;
using ControlMode = ListPositionCtrl.ControlMode;

[CustomEditor(typeof(ListPositionCtrl))]
[CanEditMultipleObjects]
public class ListPositionCtrlEditor : Editor
{
	private SerializedProperty GetProperty(string proptyName)
	{
		return serializedObject.FindProperty(proptyName);
	}

	private void SetPropertyField(string proptyName, bool includeChildren = false)
	{
		EditorGUILayout.PropertyField(
			serializedObject.FindProperty(proptyName), includeChildren);
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		/* Basic configuration */
		SetPropertyField("listType");
		SetPropertyField("controlMode");
		if (GetProperty("controlMode").enumValueIndex == (int) ControlMode.Drag) {
			++EditorGUI.indentLevel;
			SetPropertyField("alignMiddle");
			--EditorGUI.indentLevel;
		}
		SetPropertyField("direction");
		SetPropertyField("listBoxes", true);
		SetPropertyField("listBank");
		SetPropertyField("centeredContentID");

		/* Appearance */
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("List Appearance", EditorStyles.boldLabel);
		SetPropertyField("boxDensity");
		SetPropertyField("boxPositionCurve");
		SetPropertyField("boxScaleCurve");
		SetPropertyField("boxMovementCurve");

		/* Events */
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("List Event", EditorStyles.boldLabel);
		SetPropertyField("onBoxClick");

		serializedObject.ApplyModifiedProperties();
	}
}

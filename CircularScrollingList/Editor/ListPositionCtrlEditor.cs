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

	private void SetPropertyField(string proptyName)
	{
		EditorGUILayout.PropertyField(serializedObject.FindProperty(proptyName));
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		/* Basic configuration */
		SetPropertyField("listType");
		SetPropertyField("controlMode");
		if (GetProperty("controlMode").enumValueIndex == (int) ControlMode.Drag) {
			SetPropertyField("alignMiddle");
		}
		SetPropertyField("direction");
		SetPropertyField("listBoxes");
		SetPropertyField("listBank");
		SetPropertyField("centeredContentID");

		/* Appearance */
		EditorGUILayout.Space(5);
		EditorGUILayout.LabelField("List Appearance", EditorStyles.boldLabel);
		SetPropertyField("boxDensity");
		SetPropertyField("boxPositionCurve");
		SetPropertyField("boxScaleCurve");
		SetPropertyField("boxMovementCurve");

		/* Events */
		EditorGUILayout.Space(5);
		EditorGUILayout.LabelField("List Event", EditorStyles.boldLabel);
		SetPropertyField("onBoxClick");

		serializedObject.ApplyModifiedProperties();
	}
}

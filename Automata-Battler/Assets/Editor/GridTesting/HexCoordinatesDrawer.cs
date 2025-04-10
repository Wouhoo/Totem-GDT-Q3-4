using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(HexCoordinates))]

//class to SHOW a cell's coordinates without them being editable
public class HexCoordinatesDrawer : PropertyDrawer 
{

    public override void OnGUI (
		Rect position, SerializedProperty property, GUIContent label
	) {
		HexCoordinates coordinates = new HexCoordinates(
			property.FindPropertyRelative("x").intValue,
			property.FindPropertyRelative("y").intValue
		);
		EditorGUI.LabelField(position, label.text, coordinates.ToString());
	}
}
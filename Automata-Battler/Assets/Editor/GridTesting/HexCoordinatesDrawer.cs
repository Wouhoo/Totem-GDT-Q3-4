using UnityEditor;
using UnityEngine;

//I just wanted X and Z to be editable while Y shows up as well (not editable)
//I have no idea how this works or why all of this was necessary
// In ChatGPT we trust ~Lars
[CustomPropertyDrawer(typeof(HexCoordinates))]
public class HexCoordinatesDrawer : PropertyDrawer {
    const float PADDING = 4f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        // First line: X and Z fields
        Rect firstLine = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        float halfWidth = (firstLine.width - PADDING) * 0.5f;
        Rect xRect = new Rect(firstLine.x, firstLine.y, halfWidth, firstLine.height);
        Rect zRect = new Rect(firstLine.x + halfWidth + PADDING, firstLine.y, halfWidth, firstLine.height);

        var xProp = property.FindPropertyRelative("x");
        var zProp = property.FindPropertyRelative("z");

        xProp.intValue = EditorGUI.IntField(xRect, new GUIContent("Hex X"), xProp.intValue);
        zProp.intValue = EditorGUI.IntField(zRect, new GUIContent("Hex Z"), zProp.intValue);

        // Second line: display read-only ToString() (which includes Y)
        Rect secondLine = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + PADDING * 0.5f,
                                   position.width, EditorGUIUtility.singleLineHeight);
        var coords = new HexCoordinates(xProp.intValue, zProp.intValue);
        EditorGUI.LabelField(secondLine, $"Hex Coordinate: {coords.ToString()}");

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        // Two lines + padding between them
        return EditorGUIUtility.singleLineHeight * 2 + PADDING * 0.5f;
    }
}
using UnityEditor;

/// <summary>
/// A custom editor for the PathData object that allows the viewing of the lists and nodes
/// </summary>
[CustomEditor(typeof(PathData))]
public class PathDataEditor : Editor {
    private SerializedProperty pathsListProperty;

    private void OnEnable() {
        pathsListProperty = serializedObject.FindProperty("pathsList");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        EditorGUILayout.PropertyField(pathsListProperty);
        serializedObject.ApplyModifiedProperties();
    }
}
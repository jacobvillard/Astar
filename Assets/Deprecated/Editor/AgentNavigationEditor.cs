using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// A custom editor for agent navigation that displays nodes
/// </summary>
[CustomEditor(typeof(AgentNavigation))]
public class AgentNavigationEditor : Editor {
    public override void OnInspectorGUI() {
        serializedObject.Update();

        //Display default properties
        DrawDefaultInspector();
        
        //Space between default and custom properties
        EditorGUILayout.Space();

        //Get the target object
        var agentNavigation = (AgentNavigation)target;

        //Display Path and NextPath properties
        EditorGUILayout.LabelField("Path", EditorStyles.boldLabel);
        DisplayNodeList(agentNavigation.Path);
        
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Next Path", EditorStyles.boldLabel);
        DisplayNodeList(agentNavigation.NextPath);

        serializedObject.ApplyModifiedProperties();
    }
    
    /// <summary>
    /// Displays each node and its position
    /// </summary>
    /// <param name="nodeList"></param>
    private void DisplayNodeList(List<Node> nodeList) {
        if (nodeList == null)return;

        for (var i = 0; i < nodeList.Count; i++) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Node " + i + " Position", GUILayout.Width(120));
            EditorGUILayout.Vector3Field("", nodeList[i].Position);
            EditorGUILayout.EndHorizontal();
        }
    }

}
using System.Collections.Generic;
using Grid;
using UnityEditor;
using UnityEngine;

/// <summary>
/// A custom editor for agent navigation that displays nodes
/// </summary>
[CustomEditor(typeof(AgentNav))]
public class AgentNavEditor : Editor {
    public override void OnInspectorGUI() {
        serializedObject.Update();

        //Display default properties
        DrawDefaultInspector();
        
        //Space between default and custom properties
        EditorGUILayout.Space();

        //Get the target object
        var agentNavigation = (AgentNav)target;

        //Display Path and NextPath properties
        EditorGUILayout.LabelField("Path", EditorStyles.boldLabel);
        DisplayNodeList(agentNavigation.Path);
        
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Clusters", EditorStyles.boldLabel);
        DisplayClusterList(agentNavigation.ClusterPath);

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
    
    /// <summary>
    /// Displays each cluster and its position
    /// </summary>
    /// <param name="clusterList"></param>
    private void DisplayClusterList(List<Cluster> clusterList) {
        if (clusterList == null)return;

        for (var i = 0; i < clusterList.Count; i++) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Node " + i + " Position", GUILayout.Width(120));
            EditorGUILayout.Vector3Field("", clusterList[i].GetPosition());
            EditorGUILayout.EndHorizontal();
        }
    }

}
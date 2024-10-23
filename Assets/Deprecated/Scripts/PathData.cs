using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores a list of lists that store nodes
/// </summary>
[CreateAssetMenu(fileName = "Path Data", menuName = "Path Data")]
public class PathData : ScriptableObject
{
    public List<NodeList> pathsList;
}

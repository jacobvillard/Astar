using System;
using System.Collections.Generic;
using UnityEngine;

namespace Grid {
    /// <summary>
    /// Creates a grid of nodes
    /// </summary>
    public class GridCreator : MonoBehaviour {
        //Serialized grid creation references
        [Header("Grid Creation"), SerializeField] private int gridHeight, gridWidth;
        [SerializeField] private Vector3Int startPos;
        public int clusterSize = 10;
        public Node[,] gridPosition;
        public Cluster[,] clusters;

    

        //Gizmos references
        [Header("Gizmo Colors"), SerializeField] 
        private Color walkableColor = Color.green, unWalkableColor = Color.red, clusterColor = Color.yellow, entranceColor = Color.white;
   

        private void Awake() {
            CreateGrid();
            CreateClusters(clusterSize);
        }

        /// <summary>
        /// Creates a grid using input fields
        /// </summary>
        private void CreateGrid() {
            gridPosition = new Node[gridWidth, gridHeight];
        
            //Destroys any preexisting nodes
            foreach (Transform childObject in gameObject.transform){
                Destroy(childObject.gameObject);
            }
        
            //Creates a node at every coordinate
            for (var h = 0; h < gridHeight; h++){
                for (var w = 0; w < gridWidth; w++) {
                    var posVector = new Vector3Int(w, 0, h)+startPos;
                    var node = new Node(posVector); 
                    node.CheckIfNodeIsWalkable(posVector);
                    gridPosition[w, h] = node;
                }
            }
        }
    
        /// <summary>
        /// Only draw when the game object is selected to prevent frame drop during testing
        /// </summary>
        private void OnDrawGizmosSelected() {
            DrawGridGizmos();
            DrawClusterGizmos();
        }

        /// <summary>
        /// Draws a cube for every node and colours it according to its current state.
        /// </summary>
        private void DrawGridGizmos() {
            try {
                if (gridPosition == null) return;
                for (var h = 0; h < gridHeight; h++) {
                    for (var w = 0; w < gridWidth; w++) {
                        var position = startPos + new Vector3(w, 0, h);
                        var isWalkable = gridPosition[w, h].IsWalkable;
                
                        //Assign colours depending upon state
                        Gizmos.color = isWalkable ? walkableColor : unWalkableColor;
                
                        // Draw a wire cube at each grid square
                        Gizmos.DrawWireCube(position, Vector3.one); 
                    }
                }
            }
            catch {
                //Failed to show gizmos
            }
        }
    
        /// <summary>
        /// Draws the clusters and entrances
        /// </summary>
        private void DrawClusterGizmos() {
            try {
                //Draw Clusters
                Gizmos.color = clusterColor; 
                foreach (var cluster in clusters) {
                    if (cluster == null) continue;

                    //Calculate the center position of the cluster
                    var center = new Vector3(
                        cluster.clusterPosition.x * clusterSize + (clusterSize / 2f)- 0.5f,
                        0, 
                        cluster.clusterPosition.y * clusterSize + (clusterSize / 2f)- 0.5f
                    ) + startPos;
                
                    var size = new Vector3(clusterSize, 0.1f, clusterSize); 
                    Gizmos.DrawWireCube(center + Vector3.up * 0.05f, size); //Draw slightly above the grid for visibility
                }
            
                //Draw entrances
                Gizmos.color = entranceColor;
                foreach (var cluster in clusters) {
                    foreach (var entrance in cluster.entrances) {
                        var pos = new Vector3(entrance.Position.x, 0, entrance.Position.z) + startPos;
                        Gizmos.DrawCube(pos, new Vector3(0.9f, 0.2f, 0.9f)); //Smaller cube for entrances
                    }
                }
            }
            catch {
                //Failed to show gizmos
            }

        

       
        }
    
        /// <summary>
        /// Creates clusters
        /// </summary>
        /// <param name="clusterSize"></param>
        private void CreateClusters(int clusterSize) {
            var clustersAcross = gridWidth / clusterSize; //Assuming gridWidth is divisible by clusterSize
            var clustersDown = gridHeight / clusterSize;
            clusters = new Cluster[clustersAcross, clustersDown];

            for (var y = 0; y < clustersDown; y++) {
                for (var x = 0; x < clustersAcross; x++) {
                    var clusterPosition = new Vector3Int(x, y, 0);
                    var newCluster = new Cluster(clusterPosition);
                    newCluster.clusterActualPosition = new Vector3(
                        clusterPosition.x * clusterSize + (clusterSize / 2f)- 0.5f,
                        0, 
                        clusterPosition.y * clusterSize + (clusterSize / 2f)- 0.5f
                    ) + startPos;
                    clusters[x, y] = newCluster;

                    //Determine the range of nodes that belong to this cluster
                    var nodeStartX = x * clusterSize;
                    var nodeEndX = (x + 1) * clusterSize;
                    var nodeStartY = y * clusterSize;
                    var nodeEndY = (y + 1) * clusterSize;

                    for (var nodeY = nodeStartY; nodeY < nodeEndY; nodeY++) {
                        for (var nodeX = nodeStartX; nodeX < nodeEndX; nodeX++) {
                            var node = gridPosition[nodeX, nodeY];
                            newCluster.nodes.Add(node);
                            node.cluster = newCluster; 

                            //Determine if this node is an entrance/exit
                            if (IsNodeAnEntrance(nodeX, nodeY, nodeStartX, nodeEndX, nodeStartY, nodeEndY)) {
                                newCluster.entrances.Add(node);
                            }
                        }
                    }
                }
            }
        }
    
        /// <summary>
        /// Returns if a node is an entrance, on the edge and walkable
        /// </summary>
        /// <param name="nodeX"></param>
        /// <param name="nodeY"></param>
        /// <param name="startX"></param>
        /// <param name="endX"></param>
        /// <param name="startY"></param>
        /// <param name="endY"></param>
        /// <returns></returns>
        private bool IsNodeAnEntrance(int nodeX, int nodeY, int startX, int endX, int startY, int endY) {
            if (!gridPosition[nodeX, nodeY].IsWalkable) return false;
            return nodeX == startX || nodeX == endX - 1 || nodeY == startY || nodeY == endY - 1;
        }
    }


    /// <summary>
    /// Information about the node
    /// </summary>
    [Serializable]
    public class Node {
        /// <summary>
        /// Position of the node
        /// </summary>
        public Vector3Int Position;
        /// <summary>
        /// Distance from starting node
        /// </summary>
        public double GCost;
        /// <summary>
        /// /// Distance from end node
        /// </summary>
        public double HCost;
        /// <summary>
        /// Total Cost
        /// </summary>
        public double FCost;
        /// <summary>
        /// The parent node
        /// </summary>
        public Node Parent;
        /// <summary>
        /// If the node can be accessed by the agent
        /// </summary>
        public bool IsWalkable;
        /// <summary>
        /// The cluster the node belongs to
        /// </summary>
        public Cluster cluster;
    
        public Node(Vector3Int position, bool isWalkable = true, Node parent = null, float gCost = 0, float fCost = 0, float hCost = 0) {
            Position = position;
            FCost = fCost;
            GCost = gCost;
            HCost = hCost;
            IsWalkable = isWalkable;
            Parent = parent;
        }
        /// <summary>
        /// Checks if the node is walkable via a ray-cast
        /// </summary>
        public void CheckIfNodeIsWalkable(Vector3Int pos) {
            //Does the ray intersect any objects with the tag wall, ray cast max agent height
            if (!Physics.Raycast(pos, Vector3.up, out var hit, 2.75f)) return;
            if(hit.transform.CompareTag("wall"))IsWalkable = false;
        }
    }

    /// <summary>
    /// A list of node, required for being serialization in scriptable objects.
    /// </summary>
    [Serializable]
    public class NodeList {
        public List<Node> nodes;
    }

    /// <summary>
    /// A partition of the large grid
    /// </summary>
    public class Cluster {
        public List<Node> nodes = new();
        public Vector3Int clusterPosition;
        public Vector3 clusterActualPosition;
        public List<Node> entrances = new();
    
        public float GCost { get; set; } = float.MaxValue;
        public float FCost { get; set; } = float.MaxValue;
        public Cluster Parent { get; set; }

        //Constructor to initialize width and height based on clusterSize
        public Cluster(Vector3Int position) {
            clusterPosition = position;
        }
    
        public Vector3 GetPosition() {
            return clusterActualPosition;
        }
    }
}
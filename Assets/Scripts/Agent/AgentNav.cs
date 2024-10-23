using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class AgentNav : MonoBehaviour
{
    //Serialized References & Variables
    [Header("Agent Speed"), Range(1, 20), SerializeField] private float speed = 10;
    private GameObject gridCreator;
    private bool isCalculatingClusterPath;
    private bool isCalculatingPath;
    private Node startNode, endNode;
    private Node startNodeCluster, endNodeCluster;
    private Node[,] grid;
    private Cluster[,] clusters;
    public List<Node> Path{ get; set; }
    public List<Cluster> ClusterPath{ get; set; }
    
    [Header("Fine Tuning"), SerializeField]
    private int iterationsPerFrame = 100;

    private void Awake() {
        gridCreator = Singleton<GameManager>.Instance.gridCreator;
    }

    private void Start() {
        CreateGrid();//Initialise grid
        StartCalculatingClusterPath();//Start calculating first path
    }

    /// <summary>
    /// Starts calculating a path between two clusters, set all values beforehand
    /// </summary>
    private void StartCalculatingClusterPath() {
        isCalculatingClusterPath = true;
        var agentPos = transform.position;
        var startNodeVector = new Vector3Int (Mathf.RoundToInt(agentPos.x),0, Mathf.RoundToInt(agentPos.z));//Sets start node based upon current position.
        startNode = grid[startNodeVector.x, startNodeVector.z];
        endNode = GenerateTargetPos();
        ClusterPath = CalculateClusterPath(startNode.cluster, endNode.cluster);
    }

    /// <summary>
    /// Initialises a 2d grid array taken from the grid creator
    /// </summary>
    private void CreateGrid(){
        grid = gridCreator.GetComponent<GridCreator>().gridPosition;
        clusters = gridCreator.GetComponent<GridCreator>().clusters;
    }
    
    /// <summary>
    /// Returns a cluster path using a* pathfinding
    /// </summary>
    /// <param name="startCluster"></param>
    /// <param name="endCluster"></param>
    /// <returns></returns>
    private List<Cluster> CalculateClusterPath(Cluster startCluster, Cluster endCluster) {
        var openClusters = new List<Cluster> { startCluster };
        var closedClusters = new HashSet<Cluster>();

        //Initialize all clusters' scores
        foreach (var cluster in clusters) {
            cluster.GCost = float.MaxValue;
            cluster.FCost = float.MaxValue;
            cluster.Parent = null;
        }

        startCluster.GCost = 0;
        startCluster.FCost = GetClusterManhattanDistance(startCluster, endCluster);

        while (openClusters.Count > 0) {
            var currentCluster = openClusters.OrderBy(c => c.FCost).First();

            if (currentCluster == endCluster) {
                isCalculatingClusterPath = false;
                return RetraceClusterPath(currentCluster);
            }

            openClusters.Remove(currentCluster);
            closedClusters.Add(currentCluster);

            foreach (var neighbor in GetNeighbouringClusters(currentCluster)) {
                if (closedClusters.Contains(neighbor)) continue;

                var tentativeGScore = currentCluster.GCost + GetClusterManhattanDistance(currentCluster, neighbor);

                if (!(tentativeGScore < neighbor.GCost)) continue;
                neighbor.Parent = currentCluster;
                neighbor.GCost = tentativeGScore;
                neighbor.FCost = neighbor.GCost + GetClusterManhattanDistance(neighbor, endCluster);

                if (!openClusters.Contains(neighbor)) {
                    openClusters.Add(neighbor);
                }
            }
        }
        return null; // Return null if no path is found
       
    }
    
    /// <summary>
    /// Gets neighbouring clusters
    /// </summary>
    /// <param name="cluster"></param>
    /// <returns></returns>
    private List<Cluster> GetNeighbouringClusters(Cluster cluster) {
        var neighbors = new List<Cluster>();
        var pos = cluster.clusterPosition;
        var x = pos.x;
        var y = pos.y;
        
        if (y > 0 && HasValidConnection(cluster, clusters[x, y - 1])) neighbors.Add(clusters[x, y - 1]);
        if (y < clusters.GetLength(1) - 1 && HasValidConnection(cluster, clusters[x, y + 1])) neighbors.Add(clusters[x, y + 1]);
        if (x < clusters.GetLength(0) - 1 && HasValidConnection(cluster, clusters[x + 1, y])) neighbors.Add(clusters[x + 1, y]);
        if (x > 0 && HasValidConnection(cluster, clusters[x - 1, y])) neighbors.Add(clusters[x - 1, y]);
        
        return neighbors;
    }
    
    /// <summary>
    /// Ensure there are entrances/exits between two clusters
    /// </summary>
    /// <param name="fromCluster"></param>
    /// <param name="toCluster"></param>
    /// <returns></returns>
    private bool HasValidConnection(Cluster fromCluster, Cluster toCluster) {
        return (from fromEntrance in fromCluster.entrances
            from toEntrance in toCluster.entrances
            where Mathf.Abs(fromEntrance.Position.x - toEntrance.Position.x) +
                Mathf.Abs(fromEntrance.Position.z - toEntrance.Position.z) == 1
            where fromEntrance.IsWalkable is true and true
            select fromEntrance).Any();
    }
    
    /// <summary>
    /// Gets manhattan distance for clusters
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private float GetClusterManhattanDistance(Cluster a, Cluster b) {
        var centerA = a.GetPosition();
        var centerB = b.GetPosition();
        return Mathf.Abs(centerA.x - centerB.x) + Mathf.Abs(centerA.z - centerB.z);
    }
    
    /// <summary>
    /// Reconstructs the path of the clusters
    /// </summary>
    /// <param name="clusterParent"></param>
    /// <param name="current"></param>
    /// <returns></returns>
    private List<Cluster> RetraceClusterPath(Cluster endCluster) {
        Debug.Log("Path found");
        var path = new List<Cluster>();
        var currentCluster = endCluster;

        while (currentCluster != null) {
            path.Add(currentCluster);
            currentCluster = currentCluster.Parent;
        }

        path.Reverse();
        return path;
    }

    
    
    /// <summary>
    /// Generates a random node for the agent to walk to
    /// </summary>
    private Node GenerateTargetPos(){
        Node temp = null;
        while (temp == null){//Only assigns a new targetPos if the node is walkable and the minimum distance requirements are met.
            endNode = new Node(new Vector3Int(Random.Range(0, grid.GetLength(0)), 3,Random.Range(0, grid.GetLength(1))));
            temp = grid[endNode.Position.x, endNode.Position.z];
            if(!temp.IsWalkable || temp.cluster == startNode.cluster)temp = null;//Sets the target node to null so it can be regenerated
        }
        return temp;
    }


    private void Update() {
        Movement();
    }
    
    /// <summary>
    /// Movement for the agent
    /// </summary>
    private void Movement() {
        //Higher level cluster checks
        if (ClusterPath == null || ClusterPath.Count < 2 && isCalculatingClusterPath == false ) StartCalculatingClusterPath();
        if (Path == null || Path.Count < 1 && isCalculatingPath == false )
            if (ClusterPath != null && ClusterPath.Count > 1)StartCalculatingPath(ClusterPath[0], ClusterPath[1]);
        
        //pathfinding within each cluster
        if(Path == null || Path.Count <  1 || isCalculatingPath )return;
        var nextNode = Path[0];
        var position = nextNode.Position;
        var pos = new Vector3(position.x, 1.5f, position.z);
        transform.LookAt(new Vector3(nextNode.Position.x, nextNode.Position.y + 1.5f, nextNode.Position.z));
        //Move towards the destination
        transform.position = Vector3.MoveTowards(transform.position, pos, speed * Time.deltaTime);
            
        //Check if the agent has reached the destination 
        if ((Vector3.Distance(transform.position, pos) < 0.05f)) Path.RemoveAt(0);
        
    }
    
    /// <summary>
    /// Calculates a path between current cluster and the next cluster
    /// </summary>
    /// <param name="currentCluster"></param>
    /// <param name="nextCluster"></param>
    private void StartCalculatingPath(Cluster currentCluster, Cluster nextCluster) {
        if (startNodeCluster == null)
        {
            var agentPos = transform.position;
            var startNodeVector = new Vector3Int (Mathf.RoundToInt(agentPos.x),0, Mathf.RoundToInt(agentPos.z));//Sets start node based upon current position.
            startNodeCluster = grid[startNodeVector.x, startNodeVector.z];
        }
        else
        {
            startNodeCluster = endNodeCluster;
        }
        isCalculatingPath = true;
        foreach (var nextEntrance in nextCluster.entrances) {
            foreach (var currentEntrance in currentCluster.entrances) {
                if (Mathf.Abs(currentEntrance.Position.x - nextEntrance.Position.x) + Mathf.Abs(currentEntrance.Position.z - nextEntrance.Position.z) <= 1) {
                    endNodeCluster = nextEntrance;
                }
            }
        }
        
        
        StartCoroutine(CalculatePath(startNodeCluster, endNodeCluster));

    }


    
     // ReSharper disable Unity.PerformanceAnalysis
    /// <summary>
    /// Calculates a path between the start and end node if possible
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    private IEnumerator CalculatePath(Node start, Node end) {
        var startNodeCp = grid[start.Position.x, start.Position.z];
        var endNodeCp = grid[end.Position.x, end.Position.z];
        var openNodes = new List<Node> { startNodeCp };
        var closedNodes = new HashSet<Node>();
        var iteration = 0;
        List<Node> returnedPath = null;
        
        
        //While open nodes is not empty or null
        while (openNodes is { Count: > 0 }){
            var currentNode = GetFCost(openNodes);

            //If there is a path to the end node then exit loop, update agent path and pathdata
            if (currentNode == endNodeCp) {
                var retracedPath = RetraceNodePath(startNodeCp, endNodeCp); 
                Path = retracedPath;
                isCalculatingPath = false;
                ClusterPath.RemoveAt(0);
                yield break;
            }

            //Move the current node from open to closed
            openNodes.Remove(currentNode);
            closedNodes.Add(currentNode);

            //Find the neighbours of the current node and assign node values
            foreach (var neighbour in GetNodeNeighbours(currentNode)){
                if (neighbour == currentNode.Parent || closedNodes.Contains(neighbour)) continue;
                var newGCost = (int)(currentNode.GCost + GetManhattanDistance(currentNode.Position, neighbour.Position));
                if (!(newGCost < neighbour.GCost) && openNodes.Contains(neighbour)) continue;
                var newHCost =GetManhattanDistance(neighbour.Position, endNodeCp.Position);
                neighbour.GCost = newGCost;
                neighbour.HCost = newHCost;
                neighbour.FCost = newGCost + neighbour.HCost;
                neighbour.Parent = currentNode;
                if (!openNodes.Contains(neighbour))openNodes.Add(neighbour);
            }
            
            //Reduces computational load by waiting for the next frame after 100 iterations
            iteration++;
            if (!(iteration >= iterationsPerFrame)) continue;
            yield return 0;
            iteration = 0;
        }
    }

    /// <summary>
    /// Returns the cheapest node to move to
    /// </summary>
    /// <param name="nodeArray"></param>
    /// <returns></returns>
    private static Node GetFCost(IReadOnlyList<Node> nodeArray){
        var lowestCostNode = nodeArray[0];

        //Replaces the lowest cost node if its cheaper
        for (var i = 1; i < nodeArray.Count; i++){
            if (nodeArray[i].FCost < lowestCostNode.FCost) lowestCostNode = nodeArray[i];
        }
        return lowestCostNode;
    }

    /// <summary>
    /// Gets the Manhattan distance between two nodes
    /// </summary>
    /// <param name="node"></param>
    /// <param name="node2"></param>
    /// <returns></returns>
    private static double GetManhattanDistance(Vector3 node, Vector3 node2){
        //Debug.Log("Called GetManhattanDistance");
        return (node.x - node2.x) + (node.y - node2.y);
    }

    /// <summary>
    /// Retraces and returns the path the agent will take
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="endNode"></param>
    /// <returns></returns>
    private static List<Node> RetraceNodePath(Node startNode, Node endNode){
        var retracedPath = new List<Node>();
        var currentNode = endNode;
        
        //Adds the parent node of each node starting from the endNode
        while (currentNode != null && currentNode != startNode && retracedPath.Count < 100) {
            retracedPath.Add(currentNode);
            currentNode = currentNode.Parent;
        }
        retracedPath.Reverse();
        return retracedPath;
    }

    /// <summary>s
    /// Return the neighbours of a given node
    /// </summary>
    /// <param name="node"></param>
    /// <param name="neighbours"></param>
    /// <returns></returns>
    private List<Node> GetNodeNeighbours(Node node) {
        var neighbours = new List<Node>();
        var maxX = grid.GetLength(0);
        var maxY = grid.GetLength(1);
        var x = Mathf.RoundToInt(node.Position.x);
        var y = Mathf.RoundToInt(node.Position.z);
        
        //Checks each neighbour is in bounds and walkable before adding it
        if (y + 1 < maxY && grid[x, y + 1].IsWalkable) neighbours.Add(grid[x, y + 1]);  
        if (x + 1 < maxX && grid[x + 1, y].IsWalkable) neighbours.Add(grid[x + 1, y]);
        if (y - 1 >= 0 && grid[x, y - 1].IsWalkable) neighbours.Add(grid[x, y - 1]);  
        if (x - 1 >= 0 && grid[x - 1, y].IsWalkable)neighbours.Add(grid[x - 1, y]); 
        return neighbours;
    }
    
}


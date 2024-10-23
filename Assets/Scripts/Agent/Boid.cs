using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Boid : MonoBehaviour {
    public float speed = 5.0f;
    public float detectionRadius = 5.0f;
    public LayerMask obstacleLayer;
    public float rayDistance = 5.0f;
    public float smoothTime = 0.5f; 

    private Vector3 velocity = Vector3.zero;
    private const float NeighborDistance = 3.0f;

    //Weights
    public float alignmentWeight = 1f;
    public float cohesionWeight = 1f;
    public float separationWeight = 1f;
    public float obstacleAvoidanceWeight = 1f;

    private void Update() {
        var context = GetNearbyObjects();
        var alignmentVector = CalculateAlignmentVector(context) * alignmentWeight;
        var cohesionVector = CalculateCohesionVector(context) * cohesionWeight;
        var separationVector = CalculateSeparationVector(context) * separationWeight;
        var avoidanceVector = ObstacleAvoidanceDirection() * obstacleAvoidanceWeight;
        var moveDirection = alignmentVector + cohesionVector + separationVector + avoidanceVector;
        
        if (moveDirection != Vector3.zero)moveDirection = moveDirection.normalized;
        var smoothedDirection = Vector3.SmoothDamp(transform.forward, moveDirection, ref velocity, smoothTime).normalized;
        
        transform.position += smoothedDirection * (speed * Time.deltaTime);
        if (smoothedDirection != Vector3.zero) transform.forward = smoothedDirection;

    }
    
    /// <summary>
    /// Gets objects withing an overlap sphere.
    /// </summary>
    /// <returns></returns>
    private List<Transform> GetNearbyObjects() {
        var contextColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        return (from c in contextColliders where c != GetComponent<Collider>() select c.transform).ToList();
    }

    //Align
    private static Vector3 CalculateAlignmentVector(IEnumerable<Transform> context) {
        var alignmentVector = context.Aggregate(Vector3.zero, (current, item) => current + item.forward);
        return alignmentVector.normalized;
    }

    //Cohere
    private Vector3 CalculateCohesionVector(IReadOnlyCollection<Transform> context) {
        var cohesionVector = context.Aggregate(Vector3.zero, (current, item) => current + item.position);
        cohesionVector /= context.Count;
        cohesionVector -= transform.position;
        return cohesionVector.normalized;
    }

    //Separate
    private Vector3 CalculateSeparationVector(IEnumerable<Transform> context) {
        var separationVector = context.Where(item => Vector3.Distance(item.position, transform.position) < NeighborDistance).Aggregate(Vector3.zero, (current, item) => current + (transform.position - item.position));
        return separationVector.normalized;
    }
    
    /// <summary>
    /// Avoid flying into things
    /// </summary>
    /// <returns></returns>
    private Vector3 ObstacleAvoidanceDirection() {
        float[] rayAngles = { 0, 30, -30, 60, -60 }; 
        var avoidanceDirection = Vector3.zero;

        foreach (var angle in rayAngles) {
            var direction = Quaternion.Euler(0, angle, 0) * transform.forward;
            if (Physics.Raycast(transform.position, direction, out _, rayDistance, obstacleLayer)) continue;
            avoidanceDirection = direction;
            break; 
        }
        //If all directions are blocked go backwards
        if (avoidanceDirection == Vector3.zero && Physics.Raycast(transform.position, transform.forward, out _, rayDistance, obstacleLayer))avoidanceDirection = -transform.forward;
        return avoidanceDirection.normalized;
    }
}

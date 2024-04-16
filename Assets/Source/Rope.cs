using UnityEngine;
using static Utils;
using static UnityEngine.Mathf;
using static UnityEngine.Physics;

public class Rope : MonoBehaviour{
    [SerializeField] private Transform firstPos; 
    [SerializeField] private int nodesCount = 30;
    [SerializeField] private float targetDistance = 1f;
    [SerializeField] private float maxForceDistance = 2f;
    [SerializeField] private float maxForce = 10f;
    [SerializeField] private float force = 10f;
    [SerializeField] private float repulsion = 10f;
    [SerializeField] private float damping = 1f;
    [SerializeField] private float gravity = 1f;
    [SerializeField] private float iterationCount = 3;

    private GameObject _ropeHandler;

    private RopeNode _nodePrefab;
    private RopeNode[] _nodes;
    private LineRenderer _lr;
    
    private void Awake(){
        _lr = GetComponent<LineRenderer>();
    
        _ropeHandler = new GameObject("RopeHandler: " + gameObject.name);
    
        _nodePrefab = GetPrefab("BaseRopeNode").GetComponent<RopeNode>();
    
        _nodes = new RopeNode[nodesCount];
        _lr.positionCount = nodesCount;
        
        _nodes[0] = Instantiate(_nodePrefab, transform);
        _nodes[0].transform.position = firstPos.position;
        _nodes[0].neighbourIndexes = new int[]{1};
        _lr.SetPosition(0, _nodes[0].transform.position);        
    
        for (int i = 1; i < nodesCount; i++){
            _nodes[i] = Instantiate(_nodePrefab, _ropeHandler.transform);
            _nodes[i].transform.position = firstPos.position - transform.up * targetDistance * i + Random.onUnitSphere;
            _nodes[i].canMove = true;
            
            if (i == nodesCount - 1){
                _nodes[i].neighbourIndexes = new int[]{i - 1};
            } else{
                _nodes[i].neighbourIndexes = new int[]{i - 1, i + 1};
            }
            
            _lr.SetPosition(i, _nodes[i].transform.position);        
        }
    }
    
    private void FixedUpdate(){
        for (int iteration = 1; iteration <= iterationCount; iteration++){
            for (int i = 0; i < nodesCount; i++){
                RopeNode node = _nodes[i];
                
                if (!node.canMove) continue;
                
                //node.velocity = Vector3.zero;
                //Debug.Log(node.neighbourIndexes.Length);
                for (int j = 0; j < node.neighbourIndexes.Length; j++){
                    RopeNode neighbour = _nodes[node.neighbourIndexes[j]];
                    if (false && !neighbour.canMove){
                        continue;
                    }
                    
                    Vector3 vecToMe = node.transform.position - neighbour.transform.position;
                    float vecLength = vecToMe.magnitude;
                    Vector3 dirToMe = vecToMe / vecLength;
                    
                    float difference = (targetDistance - vecLength) / vecLength;
                    
                    node.velocity += vecToMe * 0.5f * maxForce * difference;
                    neighbour.velocity -= vecToMe * 0.5f * maxForce * difference;
                    
                    //var target = neighbour.transform.position + dirToMe * targetDistance;
                    //float powerMultiplier = Clamp(vecLength / targetDistance, 0, maxForce);
                    //neighbour.velocity += dirToMe * powerMultiplier * maxForce * Time.fixedDeltaTime / iterationCount;
                    //Vector3 targetVelocityVector = (target - node.transform.position);
                    /*
                    if (vecLength >= targetDistance && Vector3.Dot(targetVelocityVector, node.velocity) >= maxForce){
                        continue;
                    }
                    */
                    //node.velocity += targetVelocityVector * Clamp(powerMultiplier * powerMultiplier, 0, maxForce) * force;// * powerMultiplier;
                }
            }
            
            for (int i = 0; i < nodesCount; i++){
                RopeNode node = _nodes[i];
                if (!node.canMove){
                    _lr.SetPosition(i, node.transform.position);
                    continue;
                }
                
                
                //node.velocity *= 1f - (1.1f - (i / nodesCount)) * damping * Time.fixedDeltaTime / iterationCount;
                node.velocity += Vector3.down * gravity * Time.fixedDeltaTime / iterationCount;
                node.velocity *= 1f - damping * Time.fixedDeltaTime / iterationCount;
                CalculateNodeCollisions(ref node);
                node.transform.position += node.velocity * Time.fixedDeltaTime / iterationCount;
                
                _lr.SetPosition(i, node.transform.position);
            }
        }
    }
    
    private void CalculateNodeCollisions(ref RopeNode node){
        var deltaVelocity = node.velocity * Time.fixedDeltaTime / iterationCount;
        RaycastHit[] environmentHits = SphereCastAll(node.transform.position, node.sphere.radius, node.velocity.normalized, deltaVelocity.magnitude, Layers.Environment);
        
        for (int i = 0; i < environmentHits.Length; i++){
            //node.transform.position += environmentHits[i].normal * node.sphere.radius;
            node.velocity += environmentHits[i].normal * node.velocity.magnitude * 1.1f;
            //node.velocity = Vector3.ClampMagnitude(node.velocity, 60);
        }
    }
}

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

    private RopeNode _nodePrefab;
    private RopeNode[] _nodes;
    private LineRenderer _lr;
    
    private void Awake(){
        _lr = GetComponent<LineRenderer>();
    
        _nodePrefab = GetPrefab("BaseRopeNode").GetComponent<RopeNode>();
    
        _nodes = new RopeNode[nodesCount];
        _lr.positionCount = nodesCount;
        
        _nodes[0] = Instantiate(_nodePrefab, transform);
        _nodes[0].transform.position = firstPos.position;
        _nodes[0].neighbourIndexes = new int[]{1};
        _lr.SetPosition(0, _nodes[0].transform.position);        
    
        for (int i = 1; i < nodesCount; i++){
            _nodes[i] = Instantiate(_nodePrefab, transform);
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
    
    private void Update(){
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
                var target = neighbour.transform.position + dirToMe * targetDistance;
                
                
                float powerMultiplier = Clamp(vecLength / targetDistance, 0, maxForce);
                //neighbour.velocity += dirToMe * powerMultiplier * maxForce * Time.deltaTime;
                Vector3 targetVelocityVector = (target - node.transform.position);
                
                if (Vector3.Dot(targetVelocityVector, node.velocity) >= maxForce){
                    continue;
                }
                node.velocity += targetVelocityVector * Clamp(powerMultiplier * powerMultiplier, 0, maxForce) * force;// * powerMultiplier;
            }
        }
        
        for (int i = 0; i < nodesCount; i++){
            RopeNode node = _nodes[i];
            if (!node.canMove){
                _lr.SetPosition(i, node.transform.position);
                continue;
            }
            
            
            //node.velocity *= 1f - (1.1f - (i / nodesCount)) * damping * Time.deltaTime;
            node.velocity += Vector3.down * gravity * Time.deltaTime;
            node.velocity *= 1f - damping * Time.deltaTime;
            CalculateNodeCollisions(ref node);
            node.transform.position += node.velocity * Time.deltaTime;
            _lr.SetPosition(i, node.transform.position);
        }
    }
    
    private void CalculateNodeCollisions(ref RopeNode node){
        var deltaVelocity = node.velocity * Time.deltaTime;
        RaycastHit[] environmentHits = SphereCastAll(node.transform.position, node.sphere.radius, node.velocity.normalized, deltaVelocity.magnitude, Layers.Environment);
        
        for (int i = 0; i < environmentHits.Length; i++){
            //node.transform.position += environmentHits[i].normal * node.sphere.radius;
            node.velocity += environmentHits[i].normal * node.velocity.magnitude;
            //node.velocity = Vector3.ClampMagnitude(node.velocity, 60);
        }
    }
}

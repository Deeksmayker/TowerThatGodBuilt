using UnityEngine;
using static Utils;
using static UnityEngine.Mathf;
using static UnityEngine.Physics;

public class Rope : MonoBehaviour{
    [SerializeField] private Transform firstPos; 
    [SerializeField] private int nodesCount = 30;
    [SerializeField] private float targetDistance = 1f;
    [SerializeField] private float maxForce = 10f;
    [SerializeField] private float damping = 1f;
    [SerializeField] private float gravity = 1f;
    [SerializeField] private float iterationCount = 3;
    [SerializeField] private int connectionPointsCount = 5;

    private float _lifetime;

    private static GameObject _globalRopeHandler;
    private GameObject _myRopeHandler;

    private RopeNode _nodePrefab;
    private RopeNode[] _nodes;
    private LineRenderer _lr;
    
    private void Awake(){
        if (!_globalRopeHandler){
            _globalRopeHandler = new GameObject("GlobalRopeSiblingsHandler");
        }
    
        _lr = GetComponent<LineRenderer>();
    
        _myRopeHandler = new GameObject("RopeHandler: " + gameObject.name);
        _myRopeHandler.transform.SetParent(_globalRopeHandler.transform, true);
    
        _nodePrefab = GetPrefab("BaseRopeNode").GetComponent<RopeNode>();
    
        _nodes = new RopeNode[nodesCount];
        _lr.positionCount = nodesCount * connectionPointsCount - connectionPointsCount;
        
        _nodes[0] = Instantiate(_nodePrefab, transform);
        _nodes[0].transform.position = firstPos.position;
        _nodes[0].neighbourIndexes = new int[]{1};
    
        for (int i = 1; i < nodesCount; i++){
            _nodes[i] = Instantiate(_nodePrefab, _myRopeHandler.transform);
            _nodes[i].transform.position = firstPos.position + Random.onUnitSphere;//firstPos.position - transform.up * targetDistance * i + Random.onUnitSphere;
            _nodes[i].canMove = true;
            _nodes[i].GetComponent<MeshRenderer>().enabled = false;
            
            if (i == nodesCount - 1){
                _nodes[i].neighbourIndexes = new int[]{i - 1};
            } else{
                _nodes[i].neighbourIndexes = new int[]{i - 1, i + 1};
            }
        }
        
        SetLineRendererPositions();
    }
    
    private void FixedUpdate(){
        for (int iteration = 1; iteration <= iterationCount; iteration++){
            for (int i = 0; i < nodesCount; i++){
                RopeNode node = _nodes[i];
                
                for (int j = 0; j < node.neighbourIndexes.Length; j++){
                    RopeNode neighbour = _nodes[node.neighbourIndexes[j]];
                    
                    Vector3 vecToMe = node.transform.position - neighbour.transform.position;
                    float vecLength = vecToMe.magnitude;
                    Vector3 dirToMe = vecToMe / vecLength;
                    
                    float difference = (targetDistance - vecLength) / vecLength;
                    
                    if (node.canMove && !node.stopOnCollision){
                        node.velocity += vecToMe * 0.5f * maxForce * difference;
                    }
                    if (neighbour.canMove && !neighbour.stopOnCollision){
                        neighbour.velocity -= vecToMe * 0.5f * maxForce * difference;
                    }
                }
            }
            
            for (int i = 0; i < nodesCount; i++){
                RopeNode node = _nodes[i];
                if (!node.canMove){
                    _lr.SetPosition(i, node.transform.position);
                    continue;
                }
                
                //node.velocity *= 1f - (1.1f - (i / nodesCount)) * damping * Time.fixedDeltaTime / iterationCount;
                float gravityValue = node.stopOnCollision ? gravity * 0.25f : gravity;
                node.velocity += Vector3.down * gravityValue * Time.fixedDeltaTime / iterationCount;
                if (!node.stopOnCollision){
                    node.velocity *= 1f - damping * Time.fixedDeltaTime / iterationCount;
                }
                CalculateNodeCollisions(ref node);
                node.transform.position += node.velocity * Time.fixedDeltaTime / iterationCount;
            }
        }
        
        //Render lines
        SetLineRendererPositions();
        
        _lifetime += Time.fixedDeltaTime;
        if (_lifetime >= 2 && _nodes[0].stopOnCollision){
            DestroyRope();
            return;
        }
    }
    
    private void SetLineRendererPositions(){
        Vector3 previousLineVec = Vector3.zero;
        for (int i = 0; i < nodesCount - 1; i++){
            _lr.SetPosition(i * connectionPointsCount, _nodes[i].transform.position);
            
            if (i == 0){
                previousLineVec = _nodes[1].transform.position - _nodes[0].transform.position;
            }
            
            Vector3 middlePos = _nodes[i].transform.position + previousLineVec * 0.5f;
            for (int j = 1; j < connectionPointsCount - 1; j++){ 
                _lr.SetPosition(i * connectionPointsCount + j, Bezie(_nodes[i].transform.position, middlePos, _nodes[i+1].transform.position, ((float)j) / connectionPointsCount));
            }
            if (i > 0){
                previousLineVec = _nodes[i].transform.position - _nodes[i-1].transform.position;
            }
            _lr.SetPosition((i + 1) * connectionPointsCount - 1, _nodes[i+1].transform.position);
        }
    }
    
    public void DestroyRope(){
        Destroy(_myRopeHandler);
        Destroy(gameObject);
    }
    
    public void SetVelocityToFirstNode(Vector3 velocity){
        _nodes[0].canMove = true;
        _nodes[0].stopOnCollision = true;
        _nodes[0].velocity = velocity;
    }
    
    private RaycastHit[] _collisionHits = new RaycastHit[10];
    private void CalculateNodeCollisions(ref RopeNode node){
        var deltaVelocity = node.velocity * Time.fixedDeltaTime / iterationCount;
        Utils.ClearArray(_collisionHits);
        int hitsCount = SphereCastNonAlloc(node.transform.position, node.sphere.radius, node.velocity.normalized, _collisionHits, deltaVelocity.magnitude, Layers.Environment);
        
        for (int i = 0; i < hitsCount; i++){
            //node.transform.position += _collisionHits[i].normal * node.sphere.radius;
            node.velocity += _collisionHits[i].normal * node.velocity.magnitude * 1.1f;
            
            if (node.stopOnCollision){
                node.velocity = Vector3.zero;
                node.canMove = false;
                node.stopOnCollision = false;
                return;
            }
            //node.velocity = Vector3.ClampMagnitude(node.velocity, 60);
        }
    }
}

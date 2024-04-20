using UnityEngine;
using static Utils;
using static UnityEngine.Mathf;
using static UnityEngine.Physics;

public class Rope : MonoBehaviour{
    [SerializeField] private Transform firstPos; 
    [SerializeField] private int nodesCount = 30;
    [SerializeField] private float targetDistance = 1f;
    [SerializeField] private float strength = 1f;
    [SerializeField] private float airFriction = 0.5f;
    [SerializeField] private float gravity = 1f;
    [SerializeField] private float iterationCount = 3;
    [SerializeField] private int connectionPointsCount = 5;

    private float _lifetime;
    private bool _sleeping;

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
    
        string ropeHandlerName = "RopeHandler: ";
        if (transform.parent){
            ropeHandlerName += transform.parent.name;
        } else{
            ropeHandlerName += gameObject.name;
        }
    
        _myRopeHandler = new GameObject(ropeHandlerName);
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
    
    private void OnEnable(){
        _myRopeHandler.SetActive(true);
    }
    
    private void OnDisable(){
        if (_myRopeHandler){
            _myRopeHandler.SetActive(false);
        }
    }
    
    private void ApplyGravity(ref RopeNode node){
        var gravityValue = node.stopOnCollision ? gravity * 0.25f : gravity;
        node.forces += Vector3.down * gravityValue * node.mass;
    }
    
    private void ApplyAirFriction(ref RopeNode node){
        if (node.stopOnCollision){
            return;
        }
        node.forces -= node.velocity * airFriction;
    }
    
    private void UpdatePosition(ref RopeNode node, float delta){
        if (!node.canMove){
            return;
        }
        
        node.oldPosition = node.transform.position;
        node.velocity += (node.forces / node.mass) * delta;
        node.transform.position += node.velocity * delta;
    }
    
    private void MoveNode(ref RopeNode node, Vector3 vec){
        if (!node.canMove){
            return;
        }
        
        node.transform.position += vec;
    }
    
    private void SolveConstraint(ref RopeNode node1, ref RopeNode node2){
        Vector3 vecToFirst = node1.transform.position - node2.transform.position;
        float distance = vecToFirst.magnitude;
        if (distance > targetDistance){
            //broken = distance > targetDistance * max_elongation_ratio;
            Vector3 dir = vecToFirst / distance;
            float distDiff = targetDistance - distance;
            
            Vector3 powerVec = -(distDiff * strength) / (node1.mass + node2.mass) * dir;
            if (!node1.stopOnCollision){
                MoveNode(ref node1, -powerVec / node1.mass);
            }
            if (!node2.stopOnCollision){
                MoveNode(ref node2, powerVec / node2.mass);
            }
        }
    }
    
    private void UpdateDerivative(ref RopeNode node, float delta){
        node.velocity = (node.transform.position - node.oldPosition) / delta;
        node.forces = Vector3.zero;
    }
    
    private void FixedUpdate(){
        for (int i = 0; i < nodesCount; i++){
            RopeNode node = _nodes[i];
            
            ApplyGravity(ref node);
            ApplyAirFriction(ref node);
            UpdatePosition(ref node, Time.fixedDeltaTime);
            CalculateNodeCollisions(ref node);
        }
        
        for (int iteration = 1; iteration <= iterationCount; iteration++){
            for (int i = 0; i < nodesCount; i++){
                RopeNode node = _nodes[i];
                for (int j = 0; j < _nodes[i].neighbourIndexes.Length; j++){
                    RopeNode neighbour = _nodes[node.neighbourIndexes[j]];
                    SolveConstraint(ref node, ref neighbour);
                }
                
                CalculateNodeCollisions(ref node);
            }
        }
        
        for (int i = 0; i < nodesCount; i++){
            RopeNode node = _nodes[i];
            
            UpdateDerivative(ref node, Time.fixedDeltaTime);
        }
        
        SetLineRendererPositions();
        
        _lifetime += Time.fixedDeltaTime;
        if (_lifetime >= 5 && _nodes[0].stopOnCollision){
            DestroyRope();
            return;
        }
    
    return;
    /*
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
                        node.velocity += vecToMe * 0.5f * strength * difference;
                    }
                    if (neighbour.canMove && !neighbour.stopOnCollision){
                        neighbour.velocity -= vecToMe * 0.5f * strength * difference;
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
    */
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
    
    private void CalculateNodeCollisions(ref RopeNode node){
        (Collider[], int) collidersNearby = Utils.CollidersInRadius(node.transform.position, node.sphere.radius, Layers.Environment);
        for (int i = 0; i < collidersNearby.Item2; i++){
            //node.transform.position += _collisionHits[i].normal * node.sphere.radius;
            Vector3 colPoint = collidersNearby.Item1[i].ClosestPoint(node.transform.position);
            Vector3 vecToNode = node.transform.position - colPoint;
            Vector3 dirToNode = vecToNode.normalized;
            node.transform.position += colPoint - (node.transform.position - dirToNode * node.sphere.radius);
            
            if (node.stopOnCollision){
                node.velocity = Vector3.zero;
                node.canMove = false;
                node.stopOnCollision = false;
                return;
            }
        }
        
        /*
        RaycastHit[] velocityHits = SphereCastAll(node.transform.position, node.sphere.radius, node.velocity.normalized, (node.velocity * Time.fixedDeltaTime).magnitude, Layers.Environment);
        
        for (int i = 0; i < velocityHits.Length; i++){
            Vector3 vecToNode = node.transform.position - velocityHits[i].point;
            Vector3 dirToNode = vecToNode.normalized;

            node.transform.position += velocityHits[i].point - (node.transform.position - velocityHits[i].normal * node.sphere.radius);
        }
        */
    }
}

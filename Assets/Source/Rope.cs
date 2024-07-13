using Source.Utils;
using UnityEngine;
using static Source.Utils.Utils;
using static UnityEngine.Mathf;
using static UnityEngine.Physics;

public class Rope : MonoBehaviour{
    public Transform firstPos; 
    public Transform endPos; 
    public int nodesCount = 30;
    public float targetDistance = 1f;
    public float strength = 1f;
    public float airFriction = 0.5f;
    public float gravity = 1f;
    public float iterationCount = 3;
//    publicte int connectionPointsCount = 5;
    public int collisionSamples = 1;
    public float multiplier = 1f;

    public float lifetime;
    public bool sleeping;
    public float sleepCountdown;
    
    private static GameObject _globalRopeHandler;
    private GameObject _myRopeHandler;

    public int index = -1;
    
    private RopeNode _nodePrefab;
    public RopeNode[] nodes;
    public LineRenderer lr;
    
    private void Awake(){
        if (!_globalRopeHandler){
            _globalRopeHandler = new GameObject("GlobalRopeSiblingsHandler");
        }
    
        lr = GetComponent<LineRenderer>();
    
        string ropeHandlerName = "RopeHandler: ";
        if (transform.parent){
            ropeHandlerName += transform.parent.name;
        } else{
            ropeHandlerName += gameObject.name;
        }
    
        _myRopeHandler = new GameObject(ropeHandlerName);
        _myRopeHandler.transform.SetParent(_globalRopeHandler.transform, true);
    
        _nodePrefab = GetPrefab("BaseRopeNode").GetComponent<RopeNode>();
    
        nodes = new RopeNode[nodesCount];
        lr.positionCount = nodesCount;// * connectionPointsCount - connectionPointsCount;
        
        nodes[0] = Instantiate(_nodePrefab, transform);
        nodes[0].transform.position = firstPos.position;
        nodes[0].neighbourIndexes = new int[]{1};
        nodes[0].oldPosition = nodes[0].transform.position;
        nodes[0].framePreviousPos = nodes[0].transform.position;
        
    
        for (int i = 1; i < nodesCount; i++){
            nodes[i] = Instantiate(_nodePrefab, _myRopeHandler.transform);
            nodes[i].transform.position = firstPos.position + Random.onUnitSphere;//firstPos.position - transform.up * targetDistance * i + Random.onUnitSphere;
            nodes[i].canMove = true;
            nodes[i].GetComponent<MeshRenderer>().enabled = false;
            nodes[i].oldPosition = nodes[i].transform.position;
            nodes[i].framePreviousPos = nodes[i].transform.position;
            
            
            if (i == nodesCount - 1){
                nodes[i].neighbourIndexes = new int[]{i - 1};
                if (endPos){
                    nodes[i].canMove = false;
                    nodes[i].transform.SetParent(endPos, true);
                    nodes[i].transform.position = endPos.position;
                }
            } else{
                nodes[i].neighbourIndexes = new int[]{i - 1, i + 1};
            }
        }
        
        //SetLineRendererPositions();
    }
    
    private void Start(){
        RopeManager.Instance.AddRope(this);   
    }
    
    private Vector3 _endPos;
    public void SetEndPos(Vector3 pos){
        //endPos = targetTransform;
        _endPos = pos;
        if (pos == Vector3.zero){
            nodes[nodes.Length - 1].canMove = true;
            nodes[nodes.Length - 1].transform.position = nodes[nodes.Length - 1].transform.position;
            //nodes[nodes.Length - 1].velocity = Vector3.zero;
            SetVelocityToNodes(Vector3.zero);
            //nodes[nodes.Length - 1].transform.SetParent(_myRopeHander, true);
            return;
        }
        nodes[nodes.Length - 1].canMove = false;
        //nodes[nodes.Length - 1].transform.SetParent(endPos, true);
        nodes[nodes.Length - 1].transform.position = pos;
    }
    
    public void SetVelocityToNodes(Vector3 value){
        for (int i = 0; i < nodes.Length; i++){
            nodes[i].velocity = value;
        }
    }
    
    public Vector3 EndPos(){
        return nodes[nodes.Length - 1].transform.position;
    }
    
    private void OnEnable(){
        _myRopeHandler.SetActive(true);
    }
    
    private void OnDisable(){
        if (_myRopeHandler){
            _myRopeHandler.SetActive(false);
        }
    }
    
    public void SetGravity(float value){
        gravity = value;
    }
    
    public void SetStrength(float value){
        strength = value;
    }
    
    public void DestroyRope(float time = 0){
        nodes[0].canMove = true;
        nodes[nodesCount-1].canMove = true;
        
        //RopeManager.Instance.RemoveRope(index);
        
        sleepCountdown = 2f;
        
        // Destroy(_myRopeHandler, time);
        // Destroy(gameObject, time);
    }
    
    public void SetVelocityToFirstNode(Vector3 velocity){
        nodes[0].canMove = true;
        nodes[0].stopOnCollision = true;
        nodes[0].velocity = velocity;
    }
    
    public void LockLastNode(Transform targetTransform, Vector3 position){
        nodes[nodesCount-1].transform.SetParent(targetTransform, true);
        nodes[nodesCount-1].transform.position = position;
        nodes[nodesCount-1].canMove = false;
    }
    
    public void LockFirstNode(Transform targetTransform, Vector3 position){
        nodes[0].transform.SetParent(targetTransform, true);
        nodes[0].transform.position = position;
        nodes[0].canMove = false;
    }
    
    public RopeNode FirstNode(){
        return nodes[0];
    }
    
    public Vector3 EndToStartDirection(){
        return (nodes[0].transform.position - nodes[nodesCount-1].transform.position).normalized;
    }
}

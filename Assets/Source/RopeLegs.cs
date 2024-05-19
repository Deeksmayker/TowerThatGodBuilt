using UnityEngine;
using static UnityEngine.Physics;
using static UnityEngine.Mathf;
using static Source.Utils.Utils;

public class RopeLegs : MonoBehaviour{
    [SerializeField] private float stepDistance = 5;
    [SerializeField] private float legLength = 20;
    [SerializeField] private float moveTime = 0.5f;
    [SerializeField] private float stepHeight = 1f;
    
    [SerializeField] private float connectedGravity = -5000f;
    [SerializeField] private float disconnectedGravity = -50f;
    
    [SerializeField] private float connectedStrength = 1.6f;
    [SerializeField] private float disconnectedStrength = 0.1f;
    
    [SerializeField] private Rope[] ropes;
    
    public Leg[] legs;
    
    private void Awake(){
        legs = new Leg[ropes.Length];
        for (int i = 0; i < legs.Length; i++){
            legs[i] = new Leg();
            legs[i].rope = ropes[i];
            legs[i].rope.SetGravity(disconnectedGravity);
            legs[i].rope.SetStrength(disconnectedStrength);
        }
        
        _lastPosition = transform.position;
    }
    
    private float _previousDelta;
    private float _unscaledDelta;
    
    private Vector3 _lastPosition;
    private void Update(){
        MakeGoodFrameUpdate(UpdateAll, ref _previousDelta, ref _unscaledDelta);
    }
    
    private void UpdateAll(float delta){
        Vector3 velocity = transform.position - _lastPosition;
        for (int i = 0; i < ropes.Length; i++){
            if (legs[i].moving){
                legs[i].moveT += delta / moveTime;
                Vector3 currentEndPos = Vector3.Lerp(legs[i].startMovePoint, legs[i].targetMovePoint, EaseInOutQuad(legs[i].moveT));
                float heightDifference = legs[i].targetMovePoint.y - legs[i].startMovePoint.y;
                float upper = legs[i].startMovePoint.y + heightDifference + stepHeight;
                float startHeight = legs[i].moveT <= 0.5f ? legs[i].startMovePoint.y : upper;
                float targetHeight = legs[i].moveT <= 0.5f
                                   ? upper
                                   : legs[i].targetMovePoint.y;
                float currentMoveT = legs[i].moveT <= 0.5f ? legs[i].moveT * 2f : (legs[i].moveT - 0.5f) * 2f;
                currentEndPos.y = Lerp(startHeight, targetHeight, currentMoveT); 
                legs[i].rope.SetEndPos(currentEndPos);
                if (legs[i].moveT >= 1){
                    legs[i].moveT = 0;
                    legs[i].moving = false;
                    legs[i].lastMoveTimer = 0;
                    legs[i].standPoint = legs[i].targetMovePoint;
                    legs[i].connected = true;
                    legs[i].rope.SetGravity(connectedGravity);
                    legs[i].rope.SetStrength(connectedStrength);
                } else{
                    continue;
                }
            } else{
                legs[i].lastMoveTimer += delta;
            }
        
            if (Hit(legs[i].rope.transform, out var hit, velocity)){
                if (legs[i].lastMoveTimer > 0.1f
                                                && Vector3.Distance(hit.point, legs[i].targetMovePoint) > stepDistance
                                                && !legs[(i + 1) % ropes.Length].moving
                                                && !legs[(i - 1 + ropes.Length) % ropes.Length].moving){
                    legs[i].startMovePoint = legs[i].rope.EndPos();
                    legs[i].targetMovePoint = hit.point;
                    legs[i].normal = hit.normal;
                    legs[i].moving = true;
                    
                    legs[i].grounded = Vector3.Angle(hit.normal, Vector3.up) <= 30;
                    
                    break;
                }
            } else if (Vector3.Distance(legs[i].standPoint, legs[i].rope.transform.position + legs[i].rope.transform.forward * legLength) > legLength){
                legs[i].connected = false;
                legs[i].rope.SetEndPos(Vector3.zero);
                legs[i].rope.SetGravity(disconnectedGravity);
                legs[i].rope.SetStrength(disconnectedStrength);
            }
        }        
        
        _lastPosition = transform.position;
    }
    
    private bool Hit(Transform startTransform, out RaycastHit hit, Vector3 velocity){
        Vector3 velocityAddDirection = velocity.normalized;
        velocityAddDirection.y = 0;
        if (Raycast(startTransform.position + Vector3.up, startTransform.forward + velocityAddDirection, out hit, legLength, Layers.Environment)){
            return true;
        } 
        // if (Raycast(startTransform.position + Vector3.up, startTransform.right, out hit, legLength, Layers.Environment)){
        //     return true;
        // } 
        // if (Raycast(startTransform.position + Vector3.up, -startTransform.right, out hit, legLength, Layers.Environment)){
        //     return true;
        // } 
        if (Raycast(startTransform.position + Vector3.up, startTransform.forward - startTransform.right + velocityAddDirection, out hit, legLength, Layers.Environment)){
            return true;
        } 
        if (Raycast(startTransform.position + Vector3.up, startTransform.forward + startTransform.right + velocityAddDirection, out hit, legLength, Layers.Environment)){
            return true;
        } 
        return false;
    }
}

public class Leg{
    public Rope rope;
    public Vector3 startMovePoint;
    public Vector3 targetMovePoint;
    public Vector3 standPoint;
    public Vector3 normal;
    public bool moving;
    public bool connected;
    public bool grounded;
    public float moveT;
    public float lastMoveTimer;
}

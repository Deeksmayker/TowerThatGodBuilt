using UnityEngine;
using System;
using static UnityEngine.Physics;
using static UnityEngine.Mathf;
using static Source.Utils.Utils;

public class RopeLegs : MonoBehaviour{
    [SerializeField] private float baseStepDistance = 5;
    [SerializeField] private float legLength = 20;
    [SerializeField] private float checkRadius = 25f;
    [SerializeField] private float moveTime = 0.5f;
    [SerializeField] private float stepHeight = 1f;
    
    public float speedThreshold = 30;
    public float beforeThresholdAngle = 60;
    public float afterThresholdAngle = 70;
    
    [SerializeField] private Rope[] ropes;
    public IKLegs[] ikLegs;
    
    public bool separateWishPositions;
    public Transform[] wishPositions;
    
    [Header("Rope Settings")]
    [SerializeField] private float connectedGravity = -5000f;
    [SerializeField] private float disconnectedGravity = -50f;
    
    [SerializeField] private float connectedStrength = 1.6f;
    [SerializeField] private float disconnectedStrength = 0.1f;
    
    public bool updateByMyself = true;
    
    public Leg[] legs;
    
    private void Awake(){
        legs = new Leg[ropes.Length + ikLegs.Length];
        for (int i = 0; i < ropes.Length; i++){
            legs[i] = new Leg();
            legs[i].baseTransform = ropes[i].transform;
            legs[i].right = legs[i].baseTransform.localPosition.x > 0;
            legs[i].ropeVisual = true;
            if (separateWishPositions){
                legs[i].wishTransform = wishPositions[i];
            } else{
                legs[i].wishTransform = legs[i].baseTransform;
            }
        }
        
        for (int i = ropes.Length; i < legs.Length; i++){
            legs[i] = new Leg();
            legs[i].baseTransform = ikLegs[i - ropes.Length].transform;
            legs[i].right = legs[i].baseTransform.localPosition.x > 0;
            legs[i].ikVisual = true;
            if (separateWishPositions){
                legs[i].wishTransform = wishPositions[i];
            } else{
                legs[i].wishTransform = legs[i].baseTransform;
            }
        }
        
        for (int i = 0; i < ropes.Length; i++){
            ropes[i].SetGravity(disconnectedGravity);
            ropes[i].SetStrength(disconnectedStrength);
        }
        
        _lastPosition = transform.position;
    }
    
    private float _previousDelta;
    private float _unscaledDelta;
    
    private Vector3 _lastPosition;
    private void Update(){
        if (!updateByMyself){
            return;
        }
    
        Vector3 velocity = transform.position - _lastPosition;
        velocity /= Time.deltaTime;
        //MakeGoodFrameUpdate(UpdateAll, ref _previousDelta, ref _unscaledDelta);
        UpdateAll(Time.deltaTime, velocity);
        _lastPosition = transform.position;
    }
    
    public void UpdateAll(float delta, Vector3 velocity){
        for (int i = 0; i < legs.Length; i++){
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
                
                if (legs[i].ropeVisual){
                    ropes[i].SetEndPos(currentEndPos);
                } else if (legs[i].ikVisual){
                    //ikLegs[i - ropes.Length].targetPoint.position = currentEndPos;
                }
                
                legs[i].currentTargetPoint = currentEndPos;
                if (legs[i].moveT >= 1){
                    legs[i].moveT = 0;
                    legs[i].moving = false;
                    legs[i].lastMoveTimer = 0;
                    legs[i].standPoint = legs[i].targetMovePoint;
                    legs[i].connected = true;
                    if (legs[i].ropeVisual){
                        ropes[i].SetGravity(connectedGravity);
                        ropes[i].SetStrength(connectedStrength);
                    }
                } else{
                    continue;
                }
            } else{
                legs[i].lastMoveTimer += delta;
            }
            
            float stepDistance = baseStepDistance;
            
            if (velocity.magnitude <= 10){
                stepDistance = 0.1f;
            } else if (velocity.magnitude < speedThreshold * 0.5f){
                stepDistance = LerpUnclamped(0.1f, baseStepDistance, velocity.magnitude / (speedThreshold * 0.5f));
            }
        
            if (Hit(legs[i], out ColInfo colInfo, velocity)){
                if (legs[i].lastMoveTimer > 0.1f
                                                && Vector3.Distance(colInfo.point, legs[i].targetMovePoint) > stepDistance
                                                && !legs[(i + 1) % legs.Length].moving
                                                && !legs[(i - 1 + legs.Length) % legs.Length].moving){
                    //legs[i].startMovePoint = legs[i].rope.EndPos();
                    legs[i].startMovePoint = legs[i].currentTargetPoint;
                    legs[i].targetMovePoint = colInfo.point;
                    legs[i].normal = colInfo.normal;
                    legs[i].moving = true;
                    
                    legs[i].grounded = Vector3.Angle(colInfo.normal, Vector3.up) <= 30;
                    
                    break;
                }
            } else if (Vector3.Distance(legs[i].standPoint, legs[i].baseTransform.position + legs[i].baseTransform.forward * legLength) > legLength){
                // legs[i].connected = false;
                
                // //ikLegs[i].targetPoint.position = Vector3.zero;
                
                // if (legs[i].ropeVisual){
                //     ropes[i].SetEndPos(Vector3.zero);
                //     ropes[i].SetGravity(disconnectedGravity);
                //     ropes[i].SetStrength(disconnectedStrength);
                // }
                
                // legs[i].currentTargetPoint = Vector3.zero;
            }
            
            //ikLegs[i - ropes.Length].UpdateIK(legs[i].currentTargetPoint);
        }        
        
        for (int i = 0; i < ikLegs.Length; i++){
            ikLegs[i].UpdateIK(legs[i + ropes.Length].currentTargetPoint);
        }
    }
    
    public void SetIkTarget(int index, Vector3 target){
        legs[index + ropes.Length].currentTargetPoint = target;
        ikLegs[index].UpdateIK(target);
    }
    
    public void StopMoving(){
        for (int i = 0; i < legs.Length; i++){
            legs[i].moving = false;
        }
    }
    
    private bool Hit(Leg leg, out ColInfo colInfo, Vector3 velocity){
        return GroundHit(leg.wishTransform, out colInfo, velocity);
        // float speed = velocity.magnitude;
        // // float speedProgress = Clamp01(speed / speedThreshold);30      
        // Vector3 velocityNorm = velocity.normalized;
        // Vector3 velocityRight = Quaternion.Euler(0, 90, 0) * velocityNorm;
        
        // float dirRotationAngle = 0;
        
        // if (speed > speedThreshold){
        //     dirRotationAngle = afterThresholdAngle;
        // } else if (speed > 5){
        //     dirRotationAngle = Lerp(0, beforeThresholdAngle, EaseOutCubic(speed / speedThreshold));
        // }
        // Vector3 checkDirection = leg.wishTransform.forward;
        // if (dirRotationAngle > 0){
        //     checkDirection = Quaternion.AngleAxis(-dirRotationAngle, velocityRight) * checkDirection;
        // }
        
        // if (Raycast(leg.wishTransform.position, checkDirection, out var hit1, legLength, Layers.Environment)){
        //     colInfo = new ColInfo();
        //     colInfo.normal = hit1.normal;
        //     colInfo.point = hit1.point;
        //     return true;
        // }
        // // colInfo = null;
        // // return false;
        
        // ColInfo[] colInfos = ColInfoInRadius(leg.wishTransform.position, checkRadius, Layers.Environment);
        
        // for (int i = 0; i < colInfos.Length; i++){
        //     if (Vector3.Angle(colInfos[i].normal, Vector3.up) <= 30){
        //         colInfo = colInfos[i];
        //         return true;
        //     }
        // }
        
        // colInfo = null;
        // return false;
    }
    
    public bool GroundHit(Transform targetTransform, out ColInfo colInfo, Vector3 velocity){
        float speed = velocity.magnitude;
        // float speedProgress = Clamp01(speed / speedThreshold);30      
        Vector3 velocityNorm = velocity.normalized;
        Vector3 velocityRight = Quaternion.Euler(0, 90, 0) * velocityNorm;
        
        float dirRotationAngle = 0;
        
        if (speed > speedThreshold){
            dirRotationAngle = afterThresholdAngle;
        } else if (speed > 5){
            dirRotationAngle = Lerp(0, beforeThresholdAngle, EaseOutCubic(speed / speedThreshold));
        }
        Vector3 checkDirection = targetTransform.forward;
        if (dirRotationAngle > 0){
            checkDirection = Quaternion.AngleAxis(-dirRotationAngle, velocityRight) * checkDirection;
        }
        
        if (Raycast(targetTransform.position, checkDirection, out var hit1, legLength, Layers.Environment)){
            colInfo = new ColInfo();
            colInfo.normal = hit1.normal;
            colInfo.point = hit1.point;
            return true;
        }
        
        ColInfo[] colInfos = ColInfoInRadius(targetTransform.position, checkRadius, Layers.Environment);
        
        for (int i = 0; i < colInfos.Length; i++){
            if (Vector3.Angle(colInfos[i].normal, Vector3.up) <= 30){
                colInfo = colInfos[i];
                return true;
            }
        }
        
        colInfo = null;
        return false;

    }
}

[Serializable]
public class Leg{
    //public Rope rope;
    public Transform baseTransform;
    public Transform wishTransform;
    public Vector3 startMovePoint;
    public Vector3 targetMovePoint;
    public Vector3 currentTargetPoint;
    public Vector3 standPoint;
    public Vector3 normal;
    public bool right;
    public bool moving;
    public bool connected;
    public bool grounded;
    
    public bool ropeVisual;
    public bool ikVisual;
    
    public float moveT;
    public float lastMoveTimer;
}

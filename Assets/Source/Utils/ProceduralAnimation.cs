using UnityEngine;
using System;
using static UnityEngine.Physics;
using static UnityEngine.Mathf;
using static Source.Utils.Utils;


public static class ProceduralAnimation{
    private static float _previousDelta;
    private static float _unscaledDelta;
    
    private static Vector3 _lastPosition;

    private static void UpdateLeg(float delta, Leg1[] legs, ref Transform targetTransform, Vector3 velocity, float legLength, float checkRadius, float stepDistance, float stepHeight, float moveTime){
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
                
                // if (legs[i].ropeVisual){
                //     ropes[i].SetEndPos(currentEndPos);
                // } else if (legs[i].ikVisual){
                //     //ikLegs[i - ropes.Length].targetPoint.position = currentEndPos;
                // }
                
                legs[i].currentTargetPoint = currentEndPos;
                if (legs[i].moveT >= 1){
                    legs[i].moveT = 0;
                    legs[i].moving = false;
                    legs[i].lastMoveTimer = 0;
                    legs[i].standPoint = legs[i].targetMovePoint;
                    legs[i].connected = true;
                    // if (legs[i].ropeVisual){
                    //     ropes[i].SetGravity(connectedGravity);
                    //     ropes[i].SetStrength(connectedStrength);
                    // }
                } else{
                    continue;
                }
            } else{
                legs[i].lastMoveTimer += delta;
            }
        
            if (Hit(targetTransform, legs[i], out ColInfo colInfo, velocity, legLength, checkRadius )){
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
                legs[i].connected = false;
                
                //ikLegs[i].targetPoint.position = Vector3.zero;
                
                // if (legs[i].ropeVisual){
                //     ropes[i].SetEndPos(Vector3.zero);
                //     ropes[i].SetGravity(disconnectedGravity);
                //     ropes[i].SetStrength(disconnectedStrength);
                // }
                
                legs[i].currentTargetPoint = Vector3.zero;
            }
            
            //ikLegs[i - ropes.Length].UpdateIK(legs[i].currentTargetPoint);
        }        
        
        // for (int i = 0; i < ikLegs.Length; i++){
        //     ikLegs[i].UpdateIK(legs[i + ropes.Length].currentTargetPoint);
        // }
        
        //_lastPosition = transform.position;
    }
    
    private static bool Hit(Transform targetTransform, Leg1 leg, out ColInfo colInfo, Vector3 velocity, float legLength, float checkRadius){
        Vector3 velocityAddDirection = velocity*7;
        
        if (Raycast(leg.baseTransform.position, leg.baseTransform.forward + velocityAddDirection, out var hit1, legLength, Layers.Environment)){
            colInfo = new ColInfo();
            colInfo.normal = hit1.normal;
            colInfo.point = hit1.point;
            return true;
                 
        }
        
        float startOffset = leg.right ? 8 : -8;
        
        Vector3 rightVector = targetTransform.right;
        rightVector += leg.baseTransform.forward;
        rightVector.y = 0;
        rightVector = rightVector.normalized;
        
        Vector3 rayStart = leg.baseTransform.position + rightVector * startOffset + Vector3.up * 8;
        Vector3 rayDirection = (Vector3.down);
        
        if (Raycast(rayStart, rayDirection + velocityAddDirection, out var hit, legLength, Layers.Environment)){
            colInfo = new ColInfo();
            colInfo.normal = hit.normal;
            colInfo.point = hit.point;
            return true;
        }
        
        ColInfo[] colInfos = ColInfoInRadius(rayStart, checkRadius, Layers.Environment);
        
        //DrawSphere(rayStart, checkRadius, Color.green);
        
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
public class Leg1{
    //public Rope rope;
    public Transform baseTransform;
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

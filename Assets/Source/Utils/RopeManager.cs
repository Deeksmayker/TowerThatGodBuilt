using UnityEngine;
using System.Collections.Generic;
using static Source.Utils.Utils;

public class RopeManager : MonoBehaviour{
    public static RopeManager Instance;
    
    private List<Rope> _ropes = new();
    
    private void Awake(){
        if (Instance && Instance != this){   
            Instance = null;
        }
        
        Instance = this;
        
        
    }
    
    public void AddRope(Rope rope){
        rope.index = _ropes.Count;
        _ropes.Add(rope);
    }
    
    public void RemoveRope(int index){
        _ropes.RemoveAt(index);
    }
    
    private void ApplyGravity(ref RopeNode node, ref Rope rope){
        var gravityValue = node.stopOnCollision ? rope.gravity * 0.25f : rope.gravity;
        node.forces += Vector3.down * gravityValue * node.mass;
    }

    private void ApplyAirFriction(ref RopeNode node, ref Rope rope){
        if (node.stopOnCollision){
            return;
        }
        node.forces -= node.velocity * rope.airFriction;
    }
    
    private void UpdatePosition(ref RopeNode node, float dt){
        if (!node.canMove){
            return;
        }
        
        node.oldPosition = node.transform.position;
        node.velocity += (node.forces / node.mass) * dt;
        node.transform.position += node.velocity * dt;
    }
    
    private void MoveNode(ref RopeNode node, Vector3 vec){
        if (!node.canMove){
            return;
        }
        
        node.transform.position += vec;
    }
    
    private void SolveConstraint(ref RopeNode node1, ref RopeNode node2, ref Rope rope){
        Vector3 vecToFirst = node1.transform.position - node2.transform.position;
        float distance = vecToFirst.magnitude;
        if (distance > rope.targetDistance*rope.multiplier){
            //broken = distance > rope.targetDistance * max_elongation_ratio;
            Vector3 dir = vecToFirst / distance;
            float distDiff = rope.targetDistance*rope.multiplier - distance;
            
            Vector3 powerVec = -(distDiff * rope.strength) / (node1.mass + node2.mass) * dir;
            if (!node1.stopOnCollision){
                MoveNode(ref node1, -powerVec / node1.mass);
            }
            if (!node2.stopOnCollision){
                MoveNode(ref node2, powerVec / node2.mass);
            }
        }
    }
    
    private void UpdateDerivative(ref RopeNode node, float dt){
        node.velocity = (node.transform.position - node.oldPosition) / dt;
        node.forces = Vector3.zero;
    }
    
    private float _previousDelta;
    private float _unscaledDelta;
    private void Update(){
        if (GAME_DELTA_SCALE <= 0){
            return;
        }
        MakeFixedUpdate(UpdateAll, ref _previousDelta, ref _unscaledDelta);
    }
    
    // private void FixedUpdate(){
    //     UpdateAll(Time.fixedDeltaTime);
    // }    
    
    private void UpdateAll(float dt){
        for (int r = 0; r < _ropes.Count; r++){
            if (_ropes[r] == null){
                _ropes.RemoveAt(r);
                continue;
            }
            
            Rope rope = _ropes[r];   
            
            if (rope.sleeping){
                continue;
            }
        
            UpdateRope(ref rope, dt);
        }
    }
    
    private void UpdateRope(ref Rope rope, float dt){
        if (rope.sleepCountdown > 0){
            rope.sleepCountdown -= dt;
            
            if (rope.sleepCountdown <= 0){
                rope.sleeping = true;
                return;
            } else{
                float t = rope.sleepCountdown / 2f;
                dt *= Mathf.Lerp(0f, 1f, t * t);
            }
        }
    
        for (int i = 0; i < rope.nodesCount; i++){
            RopeNode node = rope.nodes[i];
            
            node.frameStartPos = node.transform.position;
            
            ApplyGravity(ref node, ref rope);
            ApplyAirFriction(ref node, ref rope);
            UpdatePosition(ref node, dt);
            //CalculateNodeCollisions(ref node);
        }
        
        for (int iteration = 1; iteration <= rope.iterationCount; iteration++){
            for (int i = 0; i < rope.nodesCount; i++){
                RopeNode node = rope.nodes[i];
                for (int j = 0; j < rope.nodes[i].neighbourIndexes.Length; j++){
                    RopeNode neighbour = rope.nodes[node.neighbourIndexes[j]];
                    SolveConstraint(ref node, ref neighbour, ref rope);
                }
                
                CalculateNodeCollisions(ref node, ref rope);
            }
        }
        
        for (int i = 0; i < rope.nodesCount; i++){
            RopeNode node = rope.nodes[i];
            
            UpdateDerivative(ref node, dt);
            
            node.framePreviousPos = node.transform.position;
            
            rope.lr.SetPosition(i, node.transform.position);
        }
        
        //SetLineRendererPositions();
        
        rope.lifetime += dt;
        if (rope.lifetime >= 5 && rope.nodes[0].stopOnCollision){
            rope.DestroyRope();
            return;
        }
    }
    
    
    private void CalculateNodeCollisions(ref RopeNode node, ref Rope rope){
        if (!node.canMove){
            return;
        }
    
        Vector3 startPos = node.framePreviousPos;
        Vector3 targetPos = node.transform.position;
        Vector3 resultPos = targetPos;
        for (int sample = 1; sample <= rope.collisionSamples; sample++){
            resultPos = Vector3.Lerp(startPos, targetPos, (float)sample / (float)rope.collisionSamples);
            
            (Collider[], int) collidersNearby = CollidersInRadius(resultPos, node.sphere.radius, Layers.Environment);
            for (int i = 0; i < collidersNearby.Item2; i++){
                Vector3 colPoint = collidersNearby.Item1[i].ClosestPoint(resultPos);
                Vector3 vecToNode = resultPos - colPoint;
                Vector3 dirToNode = vecToNode.normalized;
                var added = colPoint - (resultPos - dirToNode * node.sphere.radius);
                startPos += added;
                targetPos += added;
                
                if (node.stopOnCollision){
                    node.velocity = Vector3.zero;
                    node.canMove = false;
                    node.stopOnCollision = false;
                    node.transform.position = targetPos;
                    return;
                }
            }
            
            node.transform.position = targetPos;
        }
    }        

    // private void SetLineRendererPositions(){
    //     Vector3 previousLineVec = Vector3.zero;
    //     for (int i = 0; i < rope.nodesCount - 1; i++){
    //         rope.lr.SetPosition(i * connectionPointsCount, rope.nodes[i].transform.position);
            
    //         if (i == 0){
    //             previousLineVec = rope.nodes[1].transform.position - rope.nodes[0].transform.position;
    //         }
            
    //         Vector3 middlePos = rope.nodes[i].transform.position + previousLineVec * 0.5f;
    //         for (int j = 1; j < connectionPointsCount - 1; j++){ 
    //             rope.lr.SetPosition(i * connectionPointsCount + j, Bezie(rope.nodes[i].transform.position, middlePos, rope.nodes[i+1].transform.position, ((float)j) / connectionPointsCount));
    //         }
    //         if (i > 0){
    //             previousLineVec = rope.nodes[i].transform.position - rope.nodes[i-1].transform.position;
    //         }
    //         rope.lr.SetPosition((i + 1) * connectionPointsCount - 1, rope.nodes[i+1].transform.position);
    //     }
    // }
}

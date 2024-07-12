using System;
using UnityEngine;
using Array = System.Array;
using static UnityEngine.Mathf;
using static UnityEngine.Physics;

namespace Source.Utils
{
    public static class Utils{
        public static float GAME_DELTA_SCALE = 1;

        public const float FIXED_DELTA = 0.0083333333f;
        public const float MIN_FRAME_DELTA = 0.01666666f;
        public const float EPSILON = 0.00001f;

        public static void ToggleCursor(bool canISeeYou){
            var lockState = canISeeYou ? CursorLockMode.None : CursorLockMode.Locked;
        
            Cursor.visible = canISeeYou;
            Cursor.lockState = lockState;
        }
    
        public static GameObject GetPrefab(string name){
            var prefab = Resources.Load("Prefabs/" + name) as GameObject;
            if (prefab == null) Debug.LogError("Wrong prefab name - " + name);
            return prefab;
        }
    
        public static Transform CameraTransform(){
            return Camera.main.transform;
        }
        
        public static void MakeGoodFrameUpdate(Action<float> update, ref float previousDt, ref float unscaledDt){
            float fullDelta = Time.deltaTime * GAME_DELTA_SCALE;
            unscaledDt = Time.unscaledDeltaTime * GAME_DELTA_SCALE;
            fullDelta += previousDt;
            previousDt = 0;
            
            if (fullDelta > MIN_FRAME_DELTA){
                float delta = MIN_FRAME_DELTA * GAME_DELTA_SCALE;
                while (fullDelta > MIN_FRAME_DELTA){
                    update(delta);
                    fullDelta -= delta;
                    unscaledDt = 0;
                }
                previousDt = fullDelta;
            } else{
                update(fullDelta);
            }
        }
        
        public static void MakeFixedUpdate(Action<float> update, ref float previousDt, ref float unscaledDt){
            float fullDelta = Time.unscaledDeltaTime + previousDt;
            previousDt = 0;
            
            if (fullDelta < FIXED_DELTA){
                previousDt = fullDelta;
                return;
            }
            
            fullDelta = Clamp(fullDelta, 0, 0.1f);
            
            while (fullDelta >= FIXED_DELTA){
                float dt = FIXED_DELTA * Time.timeScale * GAME_DELTA_SCALE;
                if (dt == 0){
                    return;
                }
                
                if (Time.timeScale > 1){
                    while (dt >= FIXED_DELTA){
                        update(FIXED_DELTA);
                        dt -= FIXED_DELTA;
                    }
                
                    //if (dt > 0) update(dt);
                    previousDt += dt;
                } else{
                    update(dt);   
                }
                fullDelta -= FIXED_DELTA;
            }
            
            previousDt = fullDelta;
        }
    
        public static Collider ClosestCollider(Vector3 distanceToWhom, (Collider[], int) colliders, GameObject excludedObject = null){
            var minDistance = 1000000000f;
            int indexOfMin = 0;
        
            for (int i = 0; i < colliders.Item2; i++){
                Transform colliderToCheck = colliders.Item1[i].transform;
                if (colliderToCheck.parent){
                    colliderToCheck = colliderToCheck.parent;
                }
        
                if (excludedObject && excludedObject.name == colliderToCheck.gameObject.name){
                    continue;
                }
                var distance = Vector3.Distance(colliders.Item1[i].transform.position, distanceToWhom);
                if (distance < minDistance){
                    minDistance = distance;
                    indexOfMin = i;
                }
            }
        
            return colliders.Item1[indexOfMin];
        }
    
        private static Collider[] _targetColliders = new Collider[20];
        private static Collider[] _targetCollidersBig = new Collider[100];
    
        public static void MoveSphereOutCollision(Transform targetTransform, float radius, LayerMask layers){
            ColInfo[] colInfo = ColInfoInRadius(targetTransform.position, radius, layers);
        
            for (int i = 0; i < colInfo.Length; i++){
                targetTransform.position += colInfo[i].point - (targetTransform.position - colInfo[i].normal * radius);
            }
        }
        
        public static void MoveSphereOutCollision(ref Vector3 targetPos, float radius, LayerMask layers){
            ColInfo[] colInfo = ColInfoInRadius(targetPos, radius, layers);
        
            for (int i = 0; i < colInfo.Length; i++){
                targetPos += colInfo[i].point - (targetPos - colInfo[i].normal * radius);
            }
        }
    
        public static (Collider[], int) CollidersInRadius(Vector3 position, float radius, LayerMask layers){
            ClearArray(_targetColliders);
            int count = OverlapSphereNonAlloc(position, radius, _targetColliders, layers);
    
            return (_targetColliders, count);
        }
    
        public static (Collider[], int) CollidersInCapsule(Vector3 position1, Vector3 position2, float radius, LayerMask layers){
            ClearArray(_targetColliders);
            int count = OverlapCapsuleNonAlloc(position1, position2, radius, _targetColliders, layers);
    
            return (_targetColliders, count);
        }
        
        public static (Collider[], int) CollidersInCapsuleBig(Vector3 position1, Vector3 position2, float radius, LayerMask layers){
            ClearArray(_targetCollidersBig);
            int count = OverlapCapsuleNonAlloc(position1, position2, radius, _targetCollidersBig, layers);
    
            return (_targetCollidersBig, count);
        }
    
        public static (Collider[], int) CollidersInBoxBig(Vector3 center, Vector3 size, Quaternion rotation, LayerMask layers){
            ClearArray(_targetCollidersBig);
            int count = OverlapBoxNonAlloc(center, size, _targetCollidersBig, rotation, layers);
    
            return (_targetCollidersBig, count);
        }
        
        public static Enemy GetClosestEnemy(Vector3 position, GameObject excludedObject = null){
            (Collider[], int) enemiesInRange = CollidersInRadius(position, 1000, Layers.EnemyHurtBox);
            if (enemiesInRange.Item2 > 0){
                var closestEnemy = ClosestCollider(position, enemiesInRange, excludedObject);
                var toEnemy = closestEnemy.transform.position - position;
            
                if (false && Raycast(position, toEnemy * 0.9f, Layers.Environment)){
                    return null;
                }
            
                return closestEnemy.GetComponentInParent<Enemy>();
            }
        
            return null;
        }
        
        public static ColInfo[] ColInfoInRadius(Vector3 nextPosition, float radius, LayerMask layers){
            (Collider[], int) colliders = CollidersInRadius(nextPosition, radius, layers);
            ColInfo[] result = new ColInfo[colliders.Item2];
            
            for (int i = 0; i < colliders.Item2; i++){
                result[i] = new ColInfo();
                result[i].col = colliders.Item1[i];
                result[i].point = colliders.Item1[i].ClosestPoint(nextPosition);
                result[i].vecToTarget = (nextPosition - result[i].point);
                result[i].normal = result[i].vecToTarget.normalized;
            }
            
            return result;
        }
        
        public static ColInfo[] ColInfoInCapsule(Vector3 nextPosition, Vector3 capsulePos1, Vector3 capsulePos2, float radius, Vector3 velocity, LayerMask layers){
            (Collider[], int) colliders = CollidersInCapsule(capsulePos1, capsulePos2, radius, layers);
            ColInfo[] result = new ColInfo[colliders.Item2];
            
            for (int i = 0; i < colliders.Item2; i++){
                result[i] = new ColInfo();
                result[i].col = colliders.Item1[i];
                
                Collider col = result[i].col;
                
                Vector3 closest = result[i].col.ClosestPoint(nextPosition);
                Vector3 closestPos1 = result[i].col.ClosestPoint(capsulePos1);
                Vector3 closestPos2 = result[i].col.ClosestPoint(capsulePos2);
                
                Vector3 vecToClosest = nextPosition - closest;
                Vector3 vecToClosestPos1 = capsulePos1 - closestPos1;
                Vector3 vecToClosestPos2 = capsulePos2 - closestPos2;
                
                // if (vecToClosestPos1.normalized.Equals(Vector3.down)){;
                //     Debug.Log("that's right");
                //     capsulePos1 = capsulePos2;
                //     vecToClosestPos1 = vecToClosestPos2;   
                //     closestPos1 = closestPos2;
                // }
                
                // if (vecToClosest.normalized.Equals(Vector3.down)){
                //     Debug.Log("FSD");
                // }
                
                if (Vector3.Dot(velocity, vecToClosest) >= 0){
                    closest = closestPos2;
                    vecToClosest = vecToClosestPos2;
                }
                
                if (Vector3.Dot(velocity, vecToClosestPos1) < 0 && vecToClosestPos1.sqrMagnitude < vecToClosest.sqrMagnitude){
                    closest = closestPos1; 
                    vecToClosest = vecToClosestPos1;
                }
                if (Vector3.Dot(velocity, vecToClosestPos2) < 0 && vecToClosestPos2.sqrMagnitude < vecToClosest.sqrMagnitude){
                    closest = closestPos2; 
                    vecToClosest = vecToClosestPos2;
                }
                
                result[i].point = closest;
                result[i].vecToTarget = vecToClosest;
                result[i].normal = result[i].vecToTarget.normalized;
            }
            
            return result;
        }
        
        public static bool CapsuleCollided(Transform targetTransform, CapsuleCollider capsule, LayerMask layers){
            var sphereCenter1 = targetTransform.position - targetTransform.up * capsule.height * 0.5f + capsule.radius * targetTransform.up + capsule.center;
            var sphereCenter2 = targetTransform.position + targetTransform.up * capsule.height * 0.5f - capsule.radius * targetTransform.up + capsule.center;
            
            Debug.Log(targetTransform.up);
            
            return CheckCapsule(sphereCenter1, sphereCenter2, capsule.radius, layers);
        }
        
        public static ColInfo[] ColInfoInCapsule(Vector3 nextPosition, Transform targetTransform, CapsuleCollider capsule, Vector3 velocity, LayerMask layers){
            var sphereCenter1 = nextPosition - targetTransform.up * capsule.height * 0.5f + capsule.radius * targetTransform.up + capsule.center;
            var sphereCenter2 = nextPosition + targetTransform.up * capsule.height * 0.5f - capsule.radius * targetTransform.up + capsule.center;

            return ColInfoInCapsule(nextPosition, sphereCenter1, sphereCenter2, capsule.radius, velocity, layers);
        }
        
        public static void CapsuleSphereCenters(CapsuleCollider capsule, out Vector3 pos1, out Vector3 pos2){
            Transform targetTransform = capsule.transform;
            pos1 = targetTransform.position - targetTransform.up * capsule.height * 0.5f + capsule.radius * targetTransform.up + capsule.center;
            pos2 = targetTransform.position + targetTransform.up * capsule.height * 0.5f - capsule.radius * targetTransform.up + capsule.center;
        }
    
        public static bool MoveToPosition(ref Transform targetTransform, ref float timer, float timeToMove, Vector3 startPosition, Vector3 endPosition, bool backwards, Func<float, float> easeFunction){
            float t = 0;
        
            if (backwards){
                timer -= Time.deltaTime;
                t = 1f - timer / timeToMove;
            
                Vector3 tmpPos = startPosition;
                startPosition = endPosition;
                endPosition = tmpPos;
            } else{
                timer += Time.deltaTime;
                t = timer / timeToMove;
            }
            targetTransform.position = Vector3.LerpUnclamped(startPosition, endPosition, easeFunction(t));

            if ((backwards && t <= 0) || (!backwards && t >= 1)){
                return true;
            }
    
            return false;
        }

    
        public static void ClearArray(Array arr){
            Array.Clear(arr, 0, arr.Length);
        }
    
        public static Vector3 Bezie(Vector3 startPos, Vector3 middlePos, Vector3 endPos, float t){
            var ab = Vector3.Lerp(startPos, middlePos, t);
            var bc = Vector3.Lerp(middlePos, endPos, t);

            return Vector3.Lerp(ab, bc, t);
        }
        
        //Drawing 
        
        private static readonly Vector4[] s_UnitSphere = MakeUnitSphere(16);

        // Square with edge of length 1
        private static readonly Vector4[] s_UnitSquare =
        {
            new Vector4(-0.5f, 0.5f, 0, 1),
            new Vector4(0.5f, 0.5f, 0, 1),
            new Vector4(0.5f, -0.5f, 0, 1),
            new Vector4(-0.5f, -0.5f, 0, 1),
        };

        private static Vector4[] MakeUnitSphere(int len)
        {
            Debug.Assert(len > 2);
            var v = new Vector4[len * 3];
            for (int i = 0; i < len; i++)
            {
                var f = i / (float)len;
                float c = Mathf.Cos(f * (float)(Math.PI * 2.0));
                float s = Mathf.Sin(f * (float)(Math.PI * 2.0));
                v[0 * len + i] = new Vector4(c, s, 0, 1);
                v[1 * len + i] = new Vector4(0, c, s, 1);
                v[2 * len + i] = new Vector4(s, 0, c, 1);
            }
            return v;
        }
        
        public static void DrawSphere(Vector4 pos, float radius, Color color)
        {
            Vector4[] v = s_UnitSphere;
            int len = s_UnitSphere.Length / 3;
            for (int i = 0; i < len; i++)
            {
                var sX = pos + radius * v[0 * len + i];
                var eX = pos + radius * v[0 * len + (i + 1) % len];
                var sY = pos + radius * v[1 * len + i];
                var eY = pos + radius * v[1 * len + (i + 1) % len];
                var sZ = pos + radius * v[2 * len + i];
                var eZ = pos + radius * v[2 * len + (i + 1) % len];
                Debug.DrawLine(sX, eX, color);
                Debug.DrawLine(sY, eY, color);
                Debug.DrawLine(sZ, eZ, color);
            }
        }
    
        //Ease functions
        public static float EaseInOutQuad(float x){
            return x < 0.5 ? 2 * x * x : 1 - Pow(-2 * x + 2, 2) / 2;
        }
    
        public static float EaseOutQuint(float x){
            return 1f - Pow(1 - x, 5);
        }
        
        public static float EaseOutCubic(float x){
            return 1f - Pow(1 - x, 3);
        }
    
        public static float EaseInOutCubic(float x){
            return x < 0.5f ? 4f * x * x * x : 1f - Pow(-2f * x + 2f, 3f) / 2f;
        }
    
        public static float EaseOutElastic(float x){
            float c4 = (2 * PI) / 3;
        
            return x == 0 ? 0
                : (x == 1
                    ? 1
                    : Pow(2f, -10 * x) * Sin((x * 10 - 0.75f) * c4) + 1f);
        }
    
        public static float EaseOutBounce(float x){
            var n1 = 7.5625f;
            var d1 = 2.75f;
        
            if (x < 1f / d1) {
                return n1 * x * x;
            } else if (x < 2 / d1) {
                return n1 * (x -= 1.5f / d1) * x + 0.75f;
            } else if (x < 2.5f / d1) {
                return n1 * (x -= 2.25f / d1) * x + 0.9375f;
            } else {
                return n1 * (x -= 2.625f / d1) * x + 0.984375f;
            }
        }
    
        public static float EaseInOutBounce(float x) {
            return x < 0.5f
                ? (1f - EaseOutBounce(1f - 2f * x)) / 2f
                : (1f + EaseOutBounce(2f * x - 1f)) / 2f;

        }
    }
}

public class ColInfo{
    public Collider col;
    public Vector3 point;
    public Vector3 vecToTarget;
    public Vector3 normal;
}

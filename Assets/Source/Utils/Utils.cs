using UnityEngine;
using System;
using Array = System.Array;
using static UnityEngine.Mathf;
using static UnityEngine.Physics;

public static class Utils{
    public const float EPSILON = 0.000001f;

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
    
    public static Transform GetCameraTransform(){
        return Camera.main.transform;
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
        (Collider[], int) collidersNearby = CollidersInRadius(targetTransform.position, radius, layers);
        
        for (int i = 0; i < collidersNearby.Item2; i++){
            Collider col = collidersNearby.Item1[i];
            
            Vector3 colPoint = col.ClosestPoint(targetTransform.position);
            Vector3 vecToMe = (targetTransform.position - colPoint);
            Vector3 dirToMe = vecToMe.normalized;
            
            targetTransform.position += colPoint - (targetTransform.position - dirToMe * radius);
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
    
    //Ease functions
    public static float EaseInOutQuad(float x){
        return x < 0.5 ? 2 * x * x : 1 - Pow(-2 * x + 2, 2) / 2;
    }
    
    public static float EaseOutQuint(float x){
        return 1 - Pow(1 - x, 5);
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

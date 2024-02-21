using UnityEngine;
using static UnityEngine.Mathf;

public static class Utils{
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
    
    public static Collider GetClosestFromColliders(Vector3 distanceToWhom, Collider[] colliders, GameObject excludeGameObject = null){
        var minDistance = 1000000000f;
        int indexOfMin = 0;
        
        for (int i = 0; i < colliders.Length; i++){
            if (excludeGameObject && excludeGameObject.name == colliders[i].transform.parent.gameObject.name){
                continue;
            }
            var distance = Vector3.Distance(colliders[i].transform.position, distanceToWhom);
            if (distance < minDistance){
                minDistance = distance;
                indexOfMin = i;
            }
        }
        
        return colliders[indexOfMin];
    }
    
    //Ease functions
    public static float EaseInOutQuad(float x){
        return x < 0.5 ? 2 * x * x : 1 - Pow(-2 * x + 2, 2) / 2;
    }
    
    public static float EaseOutQuint(float x){
        return 1 - Pow(1 - x, 5);
    }
    
    public static float EaseOutElastic(float x){
        float c4 = (2 * PI) / 3;
        
        return x == 0 ? 0
          : (x == 1
          ? 1
          : Pow(2f, -10 * x) * Sin((x * 10 - 0.75f) * c4) + 1f);
    }
}

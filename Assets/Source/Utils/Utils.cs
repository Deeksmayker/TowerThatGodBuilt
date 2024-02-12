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

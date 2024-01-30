using UnityEngine;

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
}

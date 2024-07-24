using UnityEngine;

public class Assets : MonoBehaviour{
    public static Assets Instance;
    
    private void Awake(){
        if (Instance && Instance != this){
            Instance = null;   
        }
        
        Instance = this;
    }
    
    public Material whiteMaterial;
    
    public Material White() => whiteMaterial;
}

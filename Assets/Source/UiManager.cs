using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour{
    public static UiManager Instance;
    
    private void Awake(){
        if (Instance && Instance != this){
            Instance = null;
        }
        
        Instance = this;
    }
    
    public Image speedVignette;
}

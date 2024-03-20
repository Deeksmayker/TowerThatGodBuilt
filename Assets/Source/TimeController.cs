using UnityEngine;

public class TimeController : MonoBehaviour{
    public static TimeController Instance;

    private float _hitStopCountdown;
    
    private void Awake(){
        if (Instance && Instance != this){
            Instance = null;
            return;
        }
        
        Instance = this;
    }
    
    private void Update(){
        if (_hitStopCountdown > 0){
            _hitStopCountdown -= Time.unscaledDeltaTime;
            
            if (_hitStopCountdown > 0) Time.timeScale = 0;
            else {
                Time.timeScale = 1;
                _hitStopCountdown = 0;
            }
            
        }
    }
    
    public void AddHitStop(float time){
        _hitStopCountdown += time;
    }
}

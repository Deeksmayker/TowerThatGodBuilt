using UnityEngine;
using static UnityEngine.Mathf;

public class TimeController : MonoBehaviour{
    public static TimeController Instance;

    private float _hitStopCountdown;
    
    private bool _slowingDown;
    
    private void Awake(){
        if (Instance && Instance != this){
            Instance = null;
            return;
        }
        
        Instance = this;
    }
    
    private void Update(){
        if (_slowingDown){
            Time.timeScale -= Time.deltaTime;
            Clamp01(Time.timeScale);
            return;
        }
    
        if (_hitStopCountdown > 0){
            _hitStopCountdown -= Time.unscaledDeltaTime;
            
            if (_hitStopCountdown > 0) Time.timeScale = 0;
            else {
                Time.timeScale = 1;
                _hitStopCountdown = 0;
            }
            
        }
    }
    
    public void SlowToZero(){
        _slowingDown = true;
    }
    
    public void AddHitStop(float time){
        _hitStopCountdown += time;
    }
}

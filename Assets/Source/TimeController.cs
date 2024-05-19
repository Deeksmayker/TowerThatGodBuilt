using UnityEngine;
using static UnityEngine.Mathf;

public class TimeController : MonoBehaviour{
    public static TimeController Instance;

    private float _hitStopCountdown;
    
    private float _targetTimeScale = 1f;
    
    private bool _slowingDown;
    
    private void Awake(){
        if (Instance && Instance != this){
            Instance = null;
            return;
        }
        
        Instance = this;
        Time.timeScale = _targetTimeScale;
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
                Time.timeScale = _targetTimeScale;
                _hitStopCountdown = 0;
            }
            
        }
    }
    
    public void SlowToZero(){
        _slowingDown = true;
    }
    
    public void AddHitStop(float time){
        _hitStopCountdown += time;
        _hitStopCountdown = Clamp(_hitStopCountdown, 0, 0.1f);
    }
    
    public void SetTargetTimeScale(float value){
        _targetTimeScale = value;
        Time.timeScale = _targetTimeScale;
    }
}

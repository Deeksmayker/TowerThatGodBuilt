using UnityEngine;
using System;
using static UnityEngine.Mathf;

public class PlayerCameraController : MonoBehaviour{
    public static PlayerCameraController Instance;

    [SerializeField] private float followSpeed = 60;

    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Transform xRotationTarget;
    [SerializeField] private Transform yRotationTarget;
    [SerializeField] private Transform rollTransform;
    [SerializeField] private Transform pitchTransform;
    
    [SerializeField] private float sense;
    [SerializeField] private float rmbSenseMultiplier = 0.1f;
    [SerializeField] private float maxRoll, maxPitch;
    [SerializeField] private float rollChangeSpeed, pitchChangeSpeed;
    
    [Header("Shake")]
    [SerializeField] private CameraShakers shakers;
    
    private float _currentSenseMultiplier = 1;
    
    private float _targetRoll, _targetPitch;
    
    private PlayerController _player;
    
    private void Awake(){
        if (Instance && Instance != this){
            Instance = null;
            return;
        }
        
        Instance = this;
        
        shakers.baseShake.shakeTransform = (new GameObject("BaseShakeTransform")).transform;
        shakers.baseShake.shakeTransform.parent = Utils.GetCameraTransform().parent;
        Utils.GetCameraTransform().parent = shakers.longShake.shakeTransform;
        shakers.rapidShake.shakeTransform = (new GameObject("RapidShakeTransform")).transform;
        shakers.rapidShake.shakeTransform.parent = shakers.baseShake.shakeTransform;
        Utils.GetCameraTransform().parent = shakers.longShake.shakeTransform;
        shakers.longShake.shakeTransform = (new GameObject("LongShakeTransform")).transform;
        shakers.longShake.shakeTransform.parent = shakers.rapidShake.shakeTransform;
        Utils.GetCameraTransform().parent = shakers.longShake.shakeTransform;
        
        _player = FindObjectOfType<PlayerController>();
    }
    
    private void Start(){
        Utils.ToggleCursor(false);
        
        switch (_player.playerClass){
            case PlayerClass.Attacker:
                rmbSenseMultiplier = 1;
                break;
            case PlayerClass.Balanced:
                rmbSenseMultiplier = 0.2f;
                break;
        }
    }
    
    private void Update(){
        Look();        
        PitchAndRoll();
        Shake();
    }
    
    private void LateUpdate(){
        transform.position = Vector3.Lerp(transform.position, cameraTarget.position, followSpeed * Time.deltaTime);
    }
    
    private void Shake(){
        UpdateShake(shakers.baseShake);
        UpdateShake(shakers.rapidShake);
        UpdateShake(shakers.longShake);
    }
    
    private void UpdateShake(ShakeSettings shake){
        var currentShake = Pow(shake.trauma, shake.traumaExponent);
        
        if (currentShake <= 0){
            shake.shakeTransform.localRotation = Quaternion.Euler(Vector3.zero);
            return;
        }
        
        Vector3 previousShakeRotation = shake.lastShakeRotation;
        
        shake.lastShakeRotation = new Vector3(
            shake.MaxShake.x * PerlinNoise(2, Time.time * shake.changingSpeed),
            shake.MaxShake.y * PerlinNoise(3, Time.time * shake.changingSpeed),
            shake.MaxShake.z * PerlinNoise(4, Time.time * shake.changingSpeed)
        ) * currentShake;
        
        shake.shakeTransform.localRotation = Quaternion.Euler(shake.shakeTransform.localEulerAngles + shake.lastShakeRotation - previousShakeRotation);
        
        shake.trauma = Clamp01(shake.trauma - Time.deltaTime * shake.traumaDecreaseSpeed);
    }
    
    private void Look(){
        if (Input.GetMouseButton(1)){
            _currentSenseMultiplier = rmbSenseMultiplier;
        }
        if (Input.GetMouseButtonUp(1)){
            _currentSenseMultiplier = 1;
        }
    
        var xInputRotation =  Input.GetAxis("Mouse X") * sense * _currentSenseMultiplier;
        var yInputRotation = -Input.GetAxis("Mouse Y") * sense * _currentSenseMultiplier;
        
        xRotationTarget.localRotation *= Quaternion.Euler(yInputRotation, 0, 0);
        yRotationTarget.localRotation *= Quaternion.Euler(0, xInputRotation, 0);
        
        //xRotationTarget.localRotation = ClampRotationAroundXAxis(xRotationTarget.rotation);
    }
    
    private void PitchAndRoll(){
        _targetRoll  = -Input.GetAxisRaw("Horizontal") * maxRoll;
        _targetPitch = Input.GetAxisRaw("Vertical") * maxPitch;
        
        _targetPitch = Mathf.Clamp(_targetPitch, -maxPitch, 0);

        rollTransform.localRotation  = Quaternion.Lerp(rollTransform.localRotation, Quaternion.Euler(new Vector3(0, 0, _targetRoll)), rollChangeSpeed * Time.deltaTime);
        pitchTransform.localRotation  = Quaternion.Lerp(pitchTransform.localRotation, Quaternion.Euler(new Vector3(_targetPitch, 0, 0)), pitchChangeSpeed * Time.deltaTime);
    }
    
    private Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, 90, -90);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }   
    
    public void ShakeCameraBase(float stress){
        shakers.baseShake.trauma = Clamp01(shakers.baseShake.trauma + stress);
    }
    
    public void ShakeCameraRapid(float stress){
        shakers.rapidShake.trauma = Clamp01(shakers.rapidShake.trauma + stress);
    }
    
    public void ShakeCameraLong(float stress){
        shakers.longShake.trauma = Clamp01(shakers.longShake.trauma + stress);
    }
}

[Serializable]
public class CameraShakers{
    public ShakeSettings baseShake;
    public ShakeSettings rapidShake;
    public ShakeSettings longShake;
}

[Serializable]
public class ShakeSettings{
    public Transform shakeTransform;
    public float traumaExponent = 2;
    public float traumaDecreaseSpeed = 0.5f;
    public float changingSpeed = 3f;
    public Vector3 MaxShake = Vector3.one * 5;
    
    public float trauma;
    public Vector3 lastShakeRotation;
}

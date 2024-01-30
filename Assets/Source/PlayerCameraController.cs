using UnityEngine;

public class PlayerCameraController : MonoBehaviour{
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Transform xRotationTarget;
    [SerializeField] private Transform yRotationTarget;
    
    [SerializeField] private float sense;
    
    private void Start(){
        Utils.ToggleCursor(false);
    }
    
    private void Update(){
        Look();        
    }
    
    private void LateUpdate(){
        transform.position = Vector3.Lerp(transform.position, cameraTarget.position, 30 * Time.deltaTime);
    }
    
    private void Look(){
        var xInputRotation =  Input.GetAxis("Mouse X") * sense;
        var yInputRotation = -Input.GetAxis("Mouse Y") * sense;
        
        xRotationTarget.localRotation *= Quaternion.Euler(yInputRotation, 0, 0);
        yRotationTarget.localRotation *= Quaternion.Euler(0, xInputRotation, 0);
        
        //xRotationTarget.localRotation = ClampRotationAroundXAxis(xRotationTarget.rotation);
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
}

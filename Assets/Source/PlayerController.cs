using UnityEngine;

public class PlayerController : MonoBehaviour{
    [SerializeField] private Transform directionTransform;
    
    [SerializeField] private float speed;
    [SerializeField] private float acceleration;
    [SerializeField] private float gravity;
    
    private Vector3 _lastVelocity;
    private Vector3 _velocity;
    
    private Vector3 _moveInput;
    
    private CapsuleCollider _collider;
    
    private void Awake(){
        _collider = GetComponent<CapsuleCollider>();
    }
    
    private void Update(){
        _moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        _moveInput.Normalize();
        
        var wishVelocity = directionTransform.right * _moveInput.x + directionTransform.forward * _moveInput.z;
        wishVelocity *= speed;
        _velocity.x = wishVelocity.x;
        _velocity.z = wishVelocity.z;
    
        _velocity += Vector3.down * gravity * Time.deltaTime;
        
        CalculateCollisions(ref _velocity);
        transform.Translate(_velocity * Time.deltaTime);
    }
    
    private void CalculateCollisions(ref Vector3 velocity){
        var sphereCenter1 = transform.position - Vector3.up * _collider.height * 0.5f;
        var sphereCenter2 = transform.position + Vector3.up * _collider.height * 0.5f;
        
        var deltaVelocity = velocity * Time.deltaTime;
        
        RaycastHit[] velocityHits = Physics.CapsuleCastAll(sphereCenter1, sphereCenter2, _collider.radius, velocity.normalized, deltaVelocity.magnitude, Layers.Environment);
        
        for (int i = 0; i < velocityHits.Length; i++){
            velocity -= velocityHits[i].normal * Vector3.Dot(velocity, velocityHits[i].normal);
        }
        
        if (Physics.CheckCapsule(sphereCenter1, sphereCenter2, _collider.radius, Layers.Environment)){
            velocity.y =   _collider.height;
        }
        
    }
}

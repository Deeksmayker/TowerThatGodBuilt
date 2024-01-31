using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour{
    public class PlayerBall{
        public int index = -1;
        public Transform transform;
        public SphereCollider collider;
        public Vector3 velocity;
        public Vector3 angularVelocity;
    }
    [Header("Player")]
    [SerializeField] private Transform directionTransform;
    [SerializeField] private float speed;
    [SerializeField] private float acceleration;
    [SerializeField] private float gravity;
    [SerializeField] private float jumpForce;
    
    private bool _grounded;
    
    private Vector3 _lastVelocity;
    private Vector3 _velocity;
    private Vector3 _moveInput;
    
    private CapsuleCollider _collider;
    
    [Header("Balls")]
    [SerializeField] private float maxBallSpeed;
    [SerializeField] private float ballGravity;
    [SerializeField] private float angularVelocityPower;
    [SerializeField] private float angularVelocitySense;
    [SerializeField] private float maxAngularVelocity = 40;
    
    private Vector3 _currentStartAngularVelocity;
    
    private LineRenderer     _ballPredictionLineRenderer;
    private GameObject       _playerBallPrefab;
    private List<PlayerBall> _balls = new();
    
    private void Awake(){
        _collider = GetComponent<CapsuleCollider>();
        
        _playerBallPrefab           = Utils.GetPrefab("PlayerBall");
        _ballPredictionLineRenderer = Instantiate(Utils.GetPrefab("PredictionTrail")).GetComponent<LineRenderer>();
    }
    
    private void Update(){
        _moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        _moveInput.Normalize();
        
        var wishVelocity = directionTransform.right * _moveInput.x + directionTransform.forward * _moveInput.z;
            wishVelocity*= speed;
        _velocity.x      = wishVelocity.x;
        _velocity.z      = wishVelocity.z;
    
        var gravityMultiplierProgress = Mathf.InverseLerp(0, jumpForce, _velocity.y);
        var gravityMultiplier         = Mathf.Lerp(1, 2, gravityMultiplierProgress * gravityMultiplierProgress);
        _velocity += Vector3.down * gravity * gravityMultiplier * Time.deltaTime;
        
        CalculatePlayerCollisions(ref _velocity);
        
        if (Input.GetKeyDown(KeyCode.Space) && _grounded){
            _velocity.y += jumpForce;
        }
        
        transform.Translate(_velocity * Time.deltaTime);
        
        if (transform.position.y < -30){
            transform.position = Vector3.up * 10;
        }
        
        UpdateBalls();
    }
    
    private void CalculatePlayerCollisions(ref Vector3 velocity){
        var sphereCenter1 = transform.position - Vector3.up * _collider.height * 0.5f;
        var sphereCenter2 = transform.position + Vector3.up * _collider.height * 0.5f;
        
        var deltaVelocity = velocity * Time.deltaTime;
        
        RaycastHit[] velocityHits = Physics.CapsuleCastAll(sphereCenter1, sphereCenter2, _collider.radius, velocity.normalized, deltaVelocity.magnitude, Layers.Environment);
        
        bool foundGround = false;
        
        for (int i = 0; i < velocityHits.Length; i++){
            velocity -= velocityHits[i].normal * Vector3.Dot(velocity, velocityHits[i].normal);
            
            if (Vector3.Angle(velocityHits[i].normal, Vector3.up) <= 30){
                foundGround = true;
            }
        }
        
        _grounded = foundGround;
        
        if (Physics.CheckCapsule(sphereCenter1, sphereCenter2, _collider.radius, Layers.Environment)){
            velocity.y = 50;
            _grounded = true;
        }
    }
    
    private void UpdateBalls(){
        if (Input.GetMouseButton(1)){
            var imaginaryBall = SpawnPlayerBall();
            imaginaryBall.transform.gameObject.name += "IMAGINE";
            
            _currentStartAngularVelocity += new Vector3(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * angularVelocitySense;
            _currentStartAngularVelocity.x = Mathf.Clamp(_currentStartAngularVelocity.x, -maxAngularVelocity, maxAngularVelocity);
            _currentStartAngularVelocity.y = Mathf.Clamp(_currentStartAngularVelocity.y, -maxAngularVelocity, maxAngularVelocity);
            
            imaginaryBall.angularVelocity = _currentStartAngularVelocity;
            
            var iterationCount = 50;
            var step = 0.02f;
            
            _ballPredictionLineRenderer.positionCount = iterationCount;
            for (int i = 0; i < iterationCount; i++){
                _ballPredictionLineRenderer.SetPosition(i, imaginaryBall.transform.position);
                UpdateBall(imaginaryBall, step, true);
            }
            imaginaryBall.collider.enabled = false;
            Destroy(imaginaryBall.transform.gameObject);
            Destroy(imaginaryBall.collider);
            
            Time.timeScale = 0.05f;
        } else{
            _currentStartAngularVelocity = Vector3.Lerp(_currentStartAngularVelocity, Vector3.zero, Time.deltaTime * 4);
            Time.timeScale = 1f;
        }
        if (Input.GetMouseButtonUp(1)){
            _ballPredictionLineRenderer.positionCount = 0;
        }
    
        if (Input.GetMouseButtonDown(0)){
            PlayerBall newBall = SpawnPlayerBall();
            newBall.index = _balls.Count;
            newBall.angularVelocity = _currentStartAngularVelocity;
            _balls.Add(newBall);
        }
        
        for (int i = 0; i < _balls.Count; i++){
            UpdateBall(_balls[i], Time.deltaTime);    
        }
    }
    
    private void UpdateBall(PlayerBall ball, float delta, bool imaginaryBall = false){
        ball.velocity += Vector3.down * ballGravity * delta;
        ball.velocity += (ball.transform.up * ball.angularVelocity.x + ball.transform.right * ball.angularVelocity.y) * angularVelocityPower * delta;
        
        ball.transform.forward = ball.velocity;
    
        CalculateBallCollisions(ball, delta, imaginaryBall);
    
        ball.transform.Translate(ball.velocity * delta, Space.World);
    }
    
    private void CalculateBallCollisions(PlayerBall ball, float delta, bool imaginaryBall = false){
        //Layers. - gives us proper flag, but gameObject.layer gives us layer number from unity editor
        var deltaVelocity = ball.velocity * delta;
        
        var hitableLayers = Layers.PlayerBallHitable;
        
        if (imaginaryBall){
            hitableLayers &= ~(int)Layers.PlayerProjectile;
        }
        RaycastHit[] velocityHits = Physics.SphereCastAll(ball.transform.position, ball.collider.radius, ball.velocity.normalized, deltaVelocity.magnitude, hitableLayers);
        
        for (int i = 0; i < velocityHits.Length; i++){
            if (velocityHits[i].transform == ball.transform) continue;
            
            bool hitBallLayer = ((1 << velocityHits[i].transform.gameObject.layer) & (int)Layers.PlayerProjectile) > 0;
            if (hitBallLayer){
            }
            
            var enemy = velocityHits[i].collider.GetComponentInParent<Enemy>();
            if (enemy){
                if (!imaginaryBall){
                    enemy.TakeHit(velocityHits[i].collider);
                }
            }
            
        
            ball.velocity = Vector3.Reflect(ball.velocity, velocityHits[i].normal);
        }
        /*
        if (Physics.CheckSphere(ball.transform.position, ball.collider.radius, Layers.Environment)){
            ball.velocity.y = 50;
        }
        */
    }
    
    private PlayerBall SpawnPlayerBall(){
        var newBall = new PlayerBall();
        newBall.transform = Instantiate(_playerBallPrefab, Utils.GetCameraTransform().position + Utils.GetCameraTransform().forward, Quaternion.identity).transform;
        newBall.collider = newBall.transform.GetComponent<SphereCollider>();
        newBall.velocity = Utils.GetCameraTransform().forward * maxBallSpeed;
        return newBall;        
    }
    
    public bool IsGrounded(){
        return _grounded;
    }
}

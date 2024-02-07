using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour{
    [Serializable]
    public class PlayerBall{
        public int index = -1;
        public int bounceCount;
        public float lifeTime;
        public Transform transform;
        public SphereCollider collider;
        public bool hitEnemy;
        public Vector3 velocity;
        public Vector3 velocityNormalized;
        public Vector3 velocityUp;
        public Vector3 velocityRight;
        public Vector3 angularVelocity;
    }
    [Header("Player")]
    [SerializeField] private Transform directionTransform;
    [SerializeField] private float baseSpeed, sprintSpeed;
    [SerializeField] private float sprintStaminaDrain;
    [SerializeField] private float groundAcceleration, groundDeceleration, airAcceleration, airDeceleration;
    [SerializeField] private float friction;
    [SerializeField] private float gravity;
    [SerializeField] private float minJumpForce, maxJumpForce;
    [SerializeField] private float timeToChargeMaxJump;
    [SerializeField] private float jumpChargeStaminaDrain;
    [SerializeField] private float jumpForwardBoost;
    [SerializeField] private float coyoteTime, jumpBufferTime;
    [SerializeField] private float maxStamina;
    [SerializeField] private float staminaRecoveryRate;
    
    private float _currentFriction;
    private float _currentStamina;
    private float _jumpChargeProgress;
    private float _currentSpeed;
    private float _timeSinceGrounded;
    private float _jumpBufferTimer;
    
    private bool _needToJump;
    
    private Slider _staminaSlider;
    
    private bool _grounded;
    
    private Vector3 _lastVelocity;
    private Vector3 _playerVelocity;
    private Vector3 _moveInput;
    
    private CapsuleCollider _collider;
    
    [Header("Balls")]
    [SerializeField] private float ballCollectRadius;
    [SerializeField] private float shootCooldown;
    [SerializeField] private float maxBallSpeed;
    [SerializeField] private float ballGravity;
    [SerializeField] private float angularVelocityPower;
    [SerializeField] private float angularVelocitySense;
    [SerializeField] private float maxAngularVelocity;
    [SerializeField] private float angularVelocityDecreaseRate;
    [SerializeField] private float findEnemiesRadius;
    
    private Vector3 _currentStartAngularVelocity;
    
    private float _shootCooldownTimer;
    
    private LineRenderer     _ballPredictionLineRenderer;
    private GameObject       _playerBallPrefab;
    private List<PlayerBall> _balls = new();
    
    [Header("Debug")]
    [SerializeField] private bool showPlayerStats;
    
    private TextMeshProUGUI _speedTextMesh;
    
    private void Awake(){
        Application.targetFrameRate = 200;
        _collider = GetComponent<CapsuleCollider>();
        
        _playerBallPrefab           = Utils.GetPrefab("PlayerBall");
        _ballPredictionLineRenderer = Instantiate(Utils.GetPrefab("PredictionTrail")).GetComponent<LineRenderer>();
        
        _currentStamina  = maxStamina;
        _currentSpeed    = baseSpeed;
        _currentFriction = friction;
        
        _staminaSlider = GameObject.FindWithTag("StaminaSlider").GetComponent<Slider>();
        
        _speedTextMesh = GameObject.FindWithTag("SpeedText").GetComponent<TextMeshProUGUI>();
        
        if (!showPlayerStats){
            _speedTextMesh.gameObject.SetActive(false);
        }
    }
    
    private void Update(){
        _moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        
        var wishDirection = new Vector3(_moveInput.x, 0, _moveInput.z);
        wishDirection = directionTransform.right * wishDirection.x + directionTransform.forward * wishDirection.z;
        wishDirection.Normalize();

        if (IsGrounded()){
            _timeSinceGrounded = 0;
        
            GroundMove(wishDirection);
            
            if (_jumpBufferTimer > 0){
                Jump(wishDirection);
            }
        } else{
            _timeSinceGrounded += Time.deltaTime;
        
            AirMove(wishDirection);
        }
        
        var gravityMultiplierProgress = Mathf.InverseLerp(0, minJumpForce, _playerVelocity.y);
        var gravityMultiplier         = Mathf.Lerp(1, 2, gravityMultiplierProgress * gravityMultiplierProgress);
        _playerVelocity += Vector3.down * gravity * gravityMultiplier * Time.deltaTime;
        
        if (Input.GetKey(KeyCode.Space)){   
            if (_currentStamina > 0 && _jumpChargeProgress < 1){
                _jumpChargeProgress += Time.unscaledDeltaTime / timeToChargeMaxJump;
                _currentStamina -= Time.deltaTime * jumpChargeStaminaDrain;
                _jumpBufferTimer = 0;
                _currentFriction = friction * 0.5f;
            } else{
                _currentFriction = friction;
            }
        } 
        if (Input.GetKey(KeyCode.LeftShift)){
            if (_currentStamina > 0){
                _currentSpeed = sprintSpeed;
                _currentStamina -= Time.deltaTime * sprintStaminaDrain;
            } else{
                _currentSpeed = baseSpeed;
            }
        }
        
        if (_jumpBufferTimer > 0){
            _jumpBufferTimer -= Time.deltaTime;
            if (_jumpBufferTimer <= 0){
                _currentStamina += _jumpChargeProgress * timeToChargeMaxJump * jumpChargeStaminaDrain;
            }
        }
        
        if (Input.GetKeyUp(KeyCode.Space)){
            if (IsGrounded() || _timeSinceGrounded <= coyoteTime || _jumpBufferTimer > 0){
                Jump(wishDirection);
            } else{
                _jumpBufferTimer = jumpBufferTime;
            }
            _currentFriction = friction;
        }
        
        if (Input.GetKeyUp(KeyCode.LeftShift)){
            _currentSpeed = baseSpeed;
        }
        
        if (!Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.LeftShift)){
            _currentStamina = Mathf.Clamp(_currentStamina + staminaRecoveryRate * Time.deltaTime, 0, maxStamina);
        }
        
        _staminaSlider.value = _currentStamina / maxStamina;

        
        CalculatePlayerCollisions(ref _playerVelocity);
        
        transform.Translate(_playerVelocity * Time.deltaTime);
        
        if (transform.position.y < -30){
            transform.position = Vector3.up * 10;
        }
        
        UpdateBalls();
        BulletTime();
        
        if (showPlayerStats){
            var horizontalSpeed = (new Vector3(_playerVelocity.x, 0, _playerVelocity.z)).magnitude;
            _speedTextMesh.text = "Horizontal: " + horizontalSpeed;
        }
    }
    
    private void Jump(Vector3 wishDirection){
        _playerVelocity.y += Mathf.Lerp(minJumpForce, maxJumpForce, _jumpChargeProgress);
        _playerVelocity += wishDirection * jumpForwardBoost;
        _jumpBufferTimer = 0;
        _jumpChargeProgress = 0;
    }
    
    private void GroundMove(Vector3 wishDirection){
        var wishSpeed = wishDirection.sqrMagnitude * _currentSpeed;
        
        ApplyFriction();
        
        var directionDotVelocity = Vector3.Dot(wishDirection, _playerVelocity.normalized);
        var acceleration = directionDotVelocity < 0.5f ? groundDeceleration : groundAcceleration;
        
        Accelerate(wishDirection, wishSpeed, acceleration);
    }
    
    private void AirMove(Vector3 wishDirection){
        var wishSpeed = wishDirection.sqrMagnitude * _currentSpeed;
        
        var directionDotVelocity = Vector3.Dot(wishDirection, _playerVelocity.normalized);
        var acceleration = directionDotVelocity < 0f ? airDeceleration : airAcceleration;

        Accelerate(wishDirection, wishSpeed, acceleration);
    }
    
    private void Accelerate(Vector3 targetDirection, float wishSpeed, float acceleration){
        var speedInWishDirection = Vector3.Dot(_playerVelocity, targetDirection);
        
        var speedDifference = wishSpeed - speedInWishDirection;        
        
        if (speedDifference <= 0){
            return;
        }
        
        var accelerationSpeed = acceleration * speedDifference * Time.deltaTime;  
        if (accelerationSpeed > speedDifference){
            accelerationSpeed = speedDifference;
        }
        
        _playerVelocity.x += targetDirection.x * accelerationSpeed;
        _playerVelocity.z += targetDirection.z * accelerationSpeed;
    }
    
    private void ApplyFriction(){
        float speed = _playerVelocity.magnitude;
        /*
        float speedDrop = Mathf.Pow(_currentFriction, Time.deltaTime * _currentFriction);
        
        if (speedDrop < 0) speedDrop = 0;
        
        float newSpeedMultiplier = speed - speedDrop;
        
        if (newSpeedMultiplier > 0){
            newSpeedMultiplier /= speed;  
        } else{
            newSpeedMultiplier = 0;
        }
        */
        var newSpeedMultiplier = 1 / (1 + (Time.deltaTime * _currentFriction));
        
        _playerVelocity.x *= newSpeedMultiplier;
        _playerVelocity.z *= newSpeedMultiplier;
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
        if (_shootCooldownTimer > 0){
            _shootCooldownTimer -= Time.deltaTime;
        }
    
        if (Input.GetMouseButton(1)){
            PredictAndDrawBallTrajectory();
        } else{
            //_currentStartAngularVelocity = Vector3.Lerp(_currentStartAngularVelocity, Vector3.zero, Time.deltaTime * 4);
        }
        if (Input.GetMouseButtonUp(1)){
            _ballPredictionLineRenderer.positionCount = 0;
        }
    
        if (Input.GetMouseButtonDown(0) && _shootCooldownTimer <= 0){
            PlayerBall newBall = SpawnPlayerBall();
            newBall.index = _balls.Count;
            newBall.angularVelocity = _currentStartAngularVelocity;
            _balls.Add(newBall);
            
            _shootCooldownTimer = shootCooldown;
        }
        
        _currentStartAngularVelocity = Vector3.Lerp(_currentStartAngularVelocity, Vector3.zero, Time.unscaledDeltaTime * 0.5f);
        
        
        for (int i = 0; i < _balls.Count; i++){
            if (_balls[i].transform == null){
                //_balls.RemoveAt(i);
                continue;
            }
            UpdateBall(_balls[i], Time.deltaTime);    
        }
    }
    
    private void UpdateBall(PlayerBall ball, float delta, bool imaginaryBall = false){
        ball.velocity += Vector3.down * ballGravity * delta;
        ball.angularVelocity = Vector3.Lerp(ball.angularVelocity, Vector3.zero, delta * angularVelocityDecreaseRate);
        
        ball.velocityNormalized = ball.velocity.normalized;
        
        ball.velocityRight = Quaternion.Euler(0, 90, 0) * ball.velocityNormalized;
        ball.velocityUp    = Quaternion.AngleAxis(-90, ball.velocityRight)* ball.velocityNormalized;
        
        ball.velocity += (ball.velocityUp * ball.angularVelocity.x + ball.velocityRight * ball.angularVelocity.y) * angularVelocityPower * delta;
        
        //ball.transform.forward = ball.velocity;
        
        ball.lifeTime += delta;
    
        CalculateBallCollisions(ball, delta, imaginaryBall);
    
        ball.transform.Translate(ball.velocity * delta, Space.World);
        
        if (imaginaryBall || (ball.lifeTime < 1 && !ball.hitEnemy)) return;
        var playerToBallVector = ball.transform.position - transform.position;
        if (playerToBallVector.sqrMagnitude < ballCollectRadius * ballCollectRadius){
            Destroy(ball.transform.gameObject);
            //_balls.RemoveAt(ball.index);
            if (_playerVelocity.y < 10){
                _playerVelocity.y = 30;
            } else{
                _playerVelocity.y += 20;
            }
        }
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
            
            ball.bounceCount++;
            
            bool hitBallLayer = ((1 << velocityHits[i].transform.gameObject.layer) & (int)Layers.PlayerProjectile) > 0;
            if (hitBallLayer){
            }
            
            var enemy = velocityHits[i].collider.GetComponentInParent<Enemy>();
            if (enemy){
                if (!imaginaryBall){
                    enemy.TakeHit(velocityHits[i].collider);
                    ball.hitEnemy = true;
                } else{
                    Animations.Instance.ChangeMaterialColor(enemy.gameObject, Colors.PredictionHitColor * 3, 0.02f);
                }
            }
            
            var reflectVectorNormalized = Vector3.Reflect(ball.velocity, velocityHits[i].normal).normalized;
            
            var enemiesInRange = Physics.OverlapSphere(ball.transform.position, findEnemiesRadius, Layers.EnemyHurtBox);
            bool foundEnemy = false;
            
            float ballSpeed = ball.velocity.magnitude;
            
            for (int e = 0; i < enemiesInRange.Length; i++){
                //if (velocityHits[i].normal.y == 1) return;
                var ballToEnemyVectorNormalized = (enemiesInRange[e].transform.position - ball.transform.position).normalized;
                if (Vector3.Dot(ballToEnemyVectorNormalized, reflectVectorNormalized) > 1f - (ball.bounceCount * 0.1f)){
                    ball.velocity = ballToEnemyVectorNormalized * ballSpeed;
                    foundEnemy = true;
                }
            }
            if (!foundEnemy){
                //ball.angularVelocity += reflectVectorNormalized * ballSpeed * 0.5f;
                //ball.angularVelocity.x = Mathf.Abs(ball.angularVelocity.x);
                ball.velocity = reflectVectorNormalized * ballSpeed * 0.6f;
                //ball.velocity.y += 10f;
            }
            
            //ball.velocity += ball.transform.forward * ball.angularVelocity.x + ball.transform.right * ball.angularVelocity.y;
        }
        /*
        if (Physics.CheckSphere(ball.transform.position, ball.collider.radius, Layers.Environment)){
            ball.velocity.y = 50;
        }
        */
    }
    
    private void PredictAndDrawBallTrajectory(){
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
    }
    
    private PlayerBall SpawnPlayerBall(){
        var newBall = new PlayerBall();
        newBall.transform = Instantiate(_playerBallPrefab, Utils.GetCameraTransform().position + Utils.GetCameraTransform().forward, Quaternion.identity).transform;
        newBall.collider = newBall.transform.GetComponent<SphereCollider>();
        newBall.velocity = Utils.GetCameraTransform().forward * maxBallSpeed;// + _playerVelocity;
        return newBall;        
    }
    
    private void BulletTime(){
        if (Input.GetKey(KeyCode.Q)){
            Time.timeScale = 0.05f;
        } 
        if (Input.GetKeyUp(KeyCode.Q)){
            Time.timeScale = 1f;
        }
    }
    
    public bool IsGrounded(){
        return _grounded;
    }
    
    private void OnDrawGizmosSelected(){
        Gizmos.color = Color.blue;
        
        Gizmos.DrawWireSphere(transform.position, ballCollectRadius);
    }
}

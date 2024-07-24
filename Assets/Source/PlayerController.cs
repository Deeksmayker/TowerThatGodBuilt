using UnityEngine;
using System.Collections.Generic;
using System;
using Source.Features.SceneEditor.Controllers;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using static UnityEngine.Mathf;
using static UnityEngine.Physics;
using static Source.Utils.Utils;
using static EnemyType;

public class PlayerController : MonoBehaviour{
    public Transform cameraTarget;
    
    private Vector3 _baseCamTargetLocalPos;
    
    private float _surfaceAngle;
    private Vector3 _groundNormal;
    
    [Header("Player")]
    [SerializeField] private Transform directionTransform;
    
    [NonSerialized] public float baseSpeed = 30; 
    [NonSerialized] public float sprintSpeed = 80;
    [NonSerialized] public float sprintTime = 1.5f;
    //[NonSerialized] public float sprintStaminaDrain = 10;
    [NonSerialized] public float groundAcceleration = 15;
    [NonSerialized] public float groundDeceleration = 5;
    [NonSerialized] public float airAcceleration = 2;
    [NonSerialized] public float airDeceleration = 1;
    [NonSerialized] public float friction = 100;
    [NonSerialized] public float gravity = 80;
    [NonSerialized] public float jumpForce = 60;
    [NonSerialized] public float jumpForwardBoost = 0;
    [NonSerialized] public float coyoteTime = 0.2f;
    [NonSerialized] public float jumpBufferTime = 0.05f;
    
    //hook
    [NonSerialized] public float hookPullDelay = 0.4f;
    [NonSerialized] public float hookPullPower = 40;
    [NonSerialized] public float hookUpPower = 60;
    public float hookBallDetectRadius = 7;
    
    //Kick
    [NonSerialized] public float kickHitBoxLength = 20;
    [NonSerialized] public float kickHitBoxWidth = 20;
    
    [Header("Balls")]
    [NonSerialized] public float maxBallSpeed = 100;
    [NonSerialized] public float ballGravity = 50;
    [NonSerialized] public float angularVelocityPower = 2;
    [NonSerialized] public float angularVelocitySense = 2;
    [NonSerialized] public float maxAngularVelocity = 100;
    [NonSerialized] public float angularVelocityDecreaseRate = 1;
    [NonSerialized] public float findEnemiesRadius = 100;

    
    public float stepDistance = 8f;
    public float stepCamPower = 2f;
    
    public Transform body;
    //public IKLegs[] legs;
    public RopeLegs legs;
    
    private Vector3 _lastPosition;
    private float _distanceWalked;
    
    private List<int> _alreadyHitByKick = new();
    private float _kickPerformingCountdown;
    private float _kickCooldownCountdown;
    
    private float _kickDuration = 0.5f;
    private float _kickCooldown = 0.8f;
    private float _kickPower = 200f;
    
    private ParticleSystem _kickHitParticles;
    
    private float _currentFriction;
    private float _currentSpeed;
    private float _airTime;
    private float _timeSinceJump;
    private float _jumpBufferCountdown;
    
    private float _playerTimeScale = 1;
    
    private bool _holdingBall;
    private bool _catchedBallOnInput;
    private float _holdingBallTime;
    private float _timeSinceHoldingBall;
    
    private bool _needToJump;
    //private bool _sprinting;
    private float _sprintCountdown;
    
    private Rope _ropePrefab;
    
    //private Slider _staminaSlider;
    private Text   _ballCounterTextMesh;
    
    private bool _grounded;
    
    //private Vector3 _lastVelocity;
    public Vector3 playerVelocity;
    private float   _playerSpeed;
    public Vector3 moveInput;
    
    private CapsuleCollider _capsule;
    private float _startCapsuleHeight;
    
    private KeyCode _bulletTimeKey  = KeyCode.Q;
    private KeyCode _collectBallKey = KeyCode.E;
    
    private Vector3 _currentStartAngularVelocity;
    
    private ParticleSystem _ballHitParticles;
    
    private float _shootCooldownTimer;
    
    private PlayerBall _ballInHold;
    
    //Ropes
    private Rope _hookRope;
    private PlayerBall _hookPullingBall;
    private bool _hookPulled;
    private bool _pulledBall;
    private float _hookTimer;
    private float _hookFlyTimer;
    private Vector3 _hookTargetPoint;
    private GameObject _scopeObject;
    
    private Collider[] _imaginaryBallAreas;

    private Vector3 _spawnPosition;
    
    private LineRenderer     _ballPredictionLineRenderer;
    private PlayerBall       _playerBallPrefab;
    private List<PlayerBall> _balls = new();
    
    [Header("Particles")]
    public ParticleSystem[] slidingParticles;
    
    [Header("Audio")]
    public AudioSource slidingSource;
    public AudioClip landingClip;
    
    [Header("Debug")]
    [SerializeField] private bool showPlayerStats;
    
    private PlayerCameraController _playerCamera;
    
    [NonSerialized] public PlayerCameraController pCam;
    [NonSerialized] public UiManager ui;
    [NonSerialized] public Sound sound;
    [NonSerialized] public TimeController time;
    [NonSerialized] public Particles particles;

    //Input
    private bool _jumpQueued;
    private bool _sprintQueued;
    private bool _hookQueued;
    private bool _ropeQueued;
    
    private Text _speedText;
    private Text _horizontalSpeedText;
    private Text _groundedText;
    private Text _surfaceAngleText;
    
    private void Awake(){
        Application.targetFrameRate = 200;
        _capsule = GetComponent<CapsuleCollider>();
        
        _playerBallPrefab           = GetPrefab("PlayerBall").GetComponent<PlayerBall>();
        _ropePrefab                 = GetPrefab("PlayerRope").GetComponent<Rope>();
        _scopeObject                = Instantiate(GetPrefab("ScopePrefab"));
        _ballPredictionLineRenderer = Instantiate(GetPrefab("PredictionTrail")).GetComponent<LineRenderer>();
        
//        _currentStamina  = maxStamina;
        _currentSpeed    = baseSpeed;
        _currentFriction = friction;
        
        // GameObject staminaObject = GameObject.FindWithTag("StaminaSlider");
        // if (staminaObject){
        //     _staminaSlider       = staminaObject.GetComponent<Slider>();
        // }
        _ballCounterTextMesh = GameObject.FindWithTag("BallCounter")?.GetComponent<Text>();
        
        
        //_kickModelParticle = GameObject.FindWithTag("KickLeg").GetComponent<ParticleSystem>();
        
        _imaginaryBallAreas = new Collider[10];
        
        //_currentBallCount = maxBallCount;
        // if (_ballCounterTextMesh){
        //     _ballCounterTextMesh.text = _currentBallCount.ToString();
        // }
        _speedText    = GameObject.FindWithTag("SpeedText")?.GetComponent<Text>();
        _horizontalSpeedText    = GameObject.FindWithTag("HorizontalSpeedText")?.GetComponent<Text>();
        _groundedText = GameObject.FindWithTag("GroundedText")?.GetComponent<Text>();
        _surfaceAngleText = GameObject.FindWithTag("SurfaceAngleText")?.GetComponent<Text>();
        
        if (!showPlayerStats){
            _speedText.gameObject.SetActive(false);
            _groundedText.gameObject.SetActive(false);
            _surfaceAngleText.gameObject.SetActive(false);
        }
        
        _playerCamera = FindObjectOfType<PlayerCameraController>();
        
        //Application.targetFrameRate = 120;
    }

    private void OnEnable()
    {
        SceneLoader.BallSpawnerFound += OnBallSpawnerFound;
    }
    
    private void OnDisable()
    {
        SceneLoader.BallSpawnerFound -= OnBallSpawnerFound;
    }

    private void Start(){
        _startCapsuleHeight = _capsule.height;
    
        _baseCamTargetLocalPos = cameraTarget.localPosition;
    
        _kickHitParticles  = Particles.Instance.GetParticles("KickHitParticles");
        _ballHitParticles  = Particles.Instance.GetParticles("BallHitParticles");
        
        _spawnPosition = transform.position;
        
        PlayerBall[] ballsOnScene = FindObjectsOfType<PlayerBall>();
        
        for (int i = 0; i < ballsOnScene.Length; i++){
            PlayerBall ball = ballsOnScene[i];
            //@HACK: Somebody duplicates balls
            if (ball.initialized){
                break;
            }
        
            //ball.sleeping = true;
            InitBall(ref ball);
        }
        
        _lastPosition = transform.position;
        
        sound = Sound.Instance;
        time = TimeController.Instance;
        particles = Particles.Instance;
        pCam = PlayerCameraController.Instance;
        ui = UiManager.Instance;
    }
    
    private float _previousDelta;
    private float _unscaledDelta;
    
    private void Update(){
        if (GAME_DELTA_SCALE <= 0){
            return;
        }
        
                
           // _unscaledDelta = Time.unscaledDeltaTime;
           // UpdateAll(Time.deltaTime);
        MakeFixedUpdate(UpdateAll, ref _previousDelta, ref _unscaledDelta);
        DebugStuff();
    }
    
    private void UpdateAll(float dt){
        //var dt = dt * _playerTimeScale;
        
        moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        
        var wishDirection = new Vector3(moveInput.x, 0, moveInput.z);
        wishDirection = directionTransform.right * wishDirection.x + directionTransform.forward * wishDirection.z;
        wishDirection.Normalize();
        _playerSpeed = playerVelocity.magnitude;

        if (IsGrounded()){
            _airTime = 0;
        
            GroundMove(dt, wishDirection);
            
            playerVelocity = Vector3.ProjectOnPlane(playerVelocity, _groundNormal);
        } else{
            _airTime += dt;
        
            AirMove(dt, wishDirection);
            
            var gravityMultiplierProgress = InverseLerp(0, jumpForce, playerVelocity.y);
            var gravityMultiplier         = Lerp(1, 2, gravityMultiplierProgress * gravityMultiplierProgress);
            playerVelocity += Vector3.down * gravity * gravityMultiplier * dt;
        }
        
        _timeSinceJump += dt;
        
        UpdateAbilities(dt, wishDirection);
        UpdateRopeMovement(dt);
                
        _playerSpeed = playerVelocity.magnitude;
        
        CalculatePlayerCollisions(ref playerVelocity, dt);
        
        _playerSpeed = playerVelocity.magnitude;
        
        transform.Translate(playerVelocity * dt);
        
        _playerCamera.UpdateAll(dt);
        _playerCamera.LateUpdateAll(dt);
        
        UpdateSteps(dt);
        
        if (transform.position.y < -30){
            transform.position = _spawnPosition;
        }
        
        UpdateBalls(dt);
        
        if (!_holdingBall){
            _timeSinceHoldingBall += dt;
        } else{
            _timeSinceHoldingBall = 0;
        }
        
        UpdateKick(dt);
        BulletTime();
        
        if (!_holdingBall){
            //_playerTimeScale = Lerp(_playerTimeScale, 1, _unscaledDelta * 5);
        }
        
        //DebugStuff();
        
        _lastPosition = transform.position;
        
        //wind sound
        {
            float a = 40;
            float b = 100;
            float t = InverseLerp(a, b, _playerSpeed);
            float mult = 2f;
            if (t < sound.windSource.volume){
                mult = 6f;
            }
            sound.windSource.volume = Lerp(sound.windSource.volume, t * 0.6f, dt * mult);
            float pitch = Lerp(0.5f, 1.2f, t * t);
            sound.windSource.pitch = Lerp(sound.windSource.pitch, pitch, dt * 2f);
        }
    }
    
    private Vector3 _legTarget;
    private float _kickTimer;
    
    private Vector3 _legStart;
    private Vector3 _legBack ;
    private Vector3 _lastKickTarget;

    private void UpdateSteps(float dt){
        Vector3 velocityNorm = playerVelocity.normalized;
        Vector3 velocityRight = Quaternion.Euler(0, 90, 0) * velocityNorm;
        IKLegs[] ikLegs = legs.ikLegs;
        
        Vector3 camTargetLocalPos = _baseCamTargetLocalPos;
                        
        if (IsGrounded() && (moveInput != Vector3.zero && _playerSpeed > EPSILON)){
            camTargetLocalPos = _baseCamTargetLocalPos + Vector3.down * (_sprintCountdown > 0 ? 1.25f : 0.75f);
        }
        
        bool kick = _kickedSomething;

        int iFrom = 0;
        int iTo = ikLegs.Length;
        if (kick){
            iTo--;
        }
        
        if (kick){
            Collider[] targets = KickTargets();
            if (targets.Length > 0){
                _lastKickTarget = transform.InverseTransformPoint(_kickedCol.ClosestPoint(transform.position));                
            } 
        
            legs.StopMoving();
            if (_kickTimer == 0f){
                _legStart = transform.InverseTransformPoint(ikLegs[1].lastTarget);
                _legBack = _legStart - transform.forward * 4f + transform.right * 2f;
            }
        
            _kickTimer += dt;
            
            float backTime = 0.025f;
            float forwardTime = 0.75f;
            float t = 0f;
            if (_kickTimer <= backTime){
                t = _kickTimer / backTime;
                _legTarget = Vector3.Lerp(_legStart, _legBack, t * t);
            } else{
                t = (_kickTimer - backTime) / forwardTime;
                _legTarget = Vector3.Lerp(_legBack, _lastKickTarget, EaseInOutBounce(t));
            }
            
            legs.SetIkTarget(1, transform.TransformPoint(_legTarget));
        } else{
            _kickTimer = 0;
        }

        if (IsGrounded()){
            if ((moveInput != Vector3.zero || _playerSpeed <= EPSILON) && !kick){
                legs.UpdateAll(dt, playerVelocity);
                slidingSource.volume = 0;
                for (int i = 0; i < slidingParticles.Length; i++){
                    slidingParticles[i].Pause();
                }
            } else{
                legs.StopMoving();
                for (int i = iFrom; i < iTo; i++){
                    if (legs.GroundHit(ikLegs[i].startPoint, out ColInfo colInfo, playerVelocity)){
                        legs.SetIkTarget(i, Vector3.Lerp(ikLegs[i].lastTarget, colInfo.point, dt * 6f));
                        slidingParticles[i].Play();
                        slidingParticles[i].transform.position = legs.IkTargetPoint(i);
                    } else{
                        legs.SetIkTarget(i, Vector3.Lerp(ikLegs[i].lastTarget, ikLegs[i].startPoint.position + ikLegs[i].startPoint.forward * 4f, dt * 10));
                    }
                }
                
                slidingSource.volume = Lerp(slidingSource.volume, Lerp(0, 0.05f, _playerSpeed / 20), dt * 5f);
                slidingSource.pitch = Lerp(slidingSource.pitch, Lerp(0.7f, 1.3f, _playerSpeed / 20), dt * 5f);
            }
        } else{
            slidingSource.volume = 0;
            for (int i = 0; i < slidingParticles.Length; i++){
                //slidingParticles[i].gameObject.SetActive(false);
                slidingParticles[i].Pause();
            }
            
            legs.StopMoving();
            float legLength = 3f;
            for (int i = iFrom; i < iTo; i++){
                Vector3 dir = ikLegs[i].startPoint.forward * legLength;
                
                float horizontalSpeed = (playerVelocity - Vector3.up * playerVelocity.y).magnitude;
                
                float t = _airTime / 2f;
                
                if (horizontalSpeed < 15){
                    t = 0;
                }
                
                dir *= Lerp(1f, 1.8f, t * t);
                float angle = Lerp(0, 90f, t);// * t);
                dir = Quaternion.AngleAxis(-angle, velocityRight) * dir;
                Vector3 targetPos = ikLegs[i].startPoint.position + dir;
                legs.SetIkTarget(i, Vector3.Lerp(ikLegs[i].lastTarget, targetPos, dt * _playerSpeed * Clamp01(_airTime * 2)));
            }
        }
        
        cameraTarget.localPosition = Vector3.Lerp(cameraTarget.localPosition, camTargetLocalPos, dt * 5);
    
        if (!IsGrounded() || moveInput.Equals(Vector3.zero)){
            return;
        }
        
        _distanceWalked += (transform.position - _lastPosition).magnitude;
        _distanceWalked = Mathf.Clamp(_distanceWalked, 0, stepDistance * 2);
        if (_distanceWalked >= stepDistance){
            float stepPower = _sprintCountdown > 0 ? stepCamPower * 2 : stepCamPower;
            //walk step
            _distanceWalked -= stepDistance;
            //PlayerSound.Instance.PlaySound(_footstepClips[Random.Range(0, _footstepClips.Length)], 0.1f, Random.Range(1f, 1.5f));
            Vector3 randomCamSpeed = Random.onUnitSphere * stepPower;
            randomCamSpeed.z = 0;
            randomCamSpeed.x *= 0.5f;
            randomCamSpeed.y *= 0.1f;//Clamp(randomCamSpeed.y, -stepCamPower, stepCamPower * 0.5f);
            pCam.AddStepCamVelocity(-transform.up * stepPower + randomCamSpeed);
        }
    }
    
    private void UpdateRopeMovement(float dt){
        if (!Input.GetKey(KeyCode.Space)){
            return;
        }
        
        float ropeCheckRadius = 5f;
        float ropeDamping = 2f;
        float ropeTargetSpeed = 40;
        float ropeAcceleration = 120;
        bool foundOne = false;
        (Collider[], int) ropeColliders = CollidersInRadius(transform.position, ropeCheckRadius, Layers.Rope);
        for (int i = 0; i < ropeColliders.Item2; i++){
            if (ropeColliders.Item1[i].TryGetComponent<RopeNode>(out var ropeNode)){
                ropeNode.velocity.x += playerVelocity.x * dt * 10;
                ropeNode.velocity.z += playerVelocity.z * dt * 10;
                ropeNode.velocity.y -= playerVelocity.y * dt * 10;
                foundOne = true;
            }
        }
        if (foundOne){
            playerVelocity.x *= 1f - ropeDamping * dt;
            playerVelocity.z *= 1f - ropeDamping * dt;
            playerVelocity.y = MoveTowards(playerVelocity.y, gravity * 0.5f, gravity * 2 * dt);
        }
        if (CheckSphere(transform.position, ropeCheckRadius, Layers.Rope)){
        }
    }

    
    private void UpdateAbilities(float dt, Vector3 wishDirection){
        if (_jumpBufferCountdown > 0){
            _jumpBufferCountdown -= dt;
            if (_jumpBufferCountdown <= 0){
                //_currentStamina += _jumpChargeProgress * timeToChargeMaxJump * jumpChargeStaminaDrain;
                //_jumpChargeProgress = 0;
            }
        }
        
        if (Input.GetKeyDown(KeyCode.Space)){
            //Coyote jump
            if (IsGrounded() || (_airTime <= coyoteTime && _timeSinceJump > 1f)){
                Jump(wishDirection);
            } else{
                _jumpBufferCountdown = jumpBufferTime;
            }
            //_currentFriction = friction;
        } else if (IsGrounded() && _jumpBufferCountdown > 0){
            Jump(wishDirection);
        }
        
        //Sprint logic
        if (_sprintCountdown > 0){
            _sprintCountdown -= dt;
            if (_sprintCountdown <= 0){
                _sprintCountdown = 0;
                _currentSpeed = baseSpeed;
            }
            
            Color vColor = ui.speedVignette.color;
            float t = 1f - (_sprintCountdown / sprintTime);
            float grow = 0.1f;
            
            if (t <= grow){
                vColor.a = Lerp(0f, .7f, EaseInOutCubic(t / grow));
            } else{
                vColor.a = Lerp(.7f, 0f, EaseInCubic((t - grow) / (1f - grow)));
            }
            
            ui.speedVignette.color = vColor;
        }
        
        if (_sprintCountdown <= 0 && Input.GetKeyDown(KeyCode.LeftShift)){
            _sprintCountdown = sprintTime;
            _currentSpeed = sprintSpeed;
            pCam.ShakeCameraRapid(1f);
            // Color vColor = ui.speedVignette.color;
            // vColor.a = 1f;
            // ui.speedVignette.color = vColor;
            
            sound.Play(sound.sprintActivation, .15f, 1.75f);
        }
        
        //Rope logics
        if (Input.GetKeyDown(KeyCode.C)){
            Rope rope = Instantiate(_ropePrefab, BallStartPosition(), Quaternion.identity);
            rope.SetVelocityToFirstNode(CameraTransform().forward * maxBallSpeed + playerVelocity * 0.5f);
        }
        if (_hookTimer <= 0){
            PlayerBall ballInHookRange = null;
            _hookTargetPoint = Vector3.zero;
            bool foundGroundPoint = false;
            if (SphereCast(CameraTransform().position, hookBallDetectRadius, CameraTransform().forward, out var hit, 100, Layers.PlayerProjectile)){
                if (hit.transform.TryGetComponent<PlayerBall>(out var playerBall)){
                    ballInHookRange = playerBall;
                    _scopeObject.transform.position = hit.point;
                    _hookTargetPoint = hit.point;
                }
            } else if (Raycast(CameraTransform().position, CameraTransform().forward, out var hit1, 300, Layers.Environment)){
                _scopeObject.transform.position = hit1.point;
                _hookTargetPoint = hit1.point;
                foundGroundPoint = true;
            } else if (SphereCast(CameraTransform().position, hookBallDetectRadius, CameraTransform().forward, out var hit2, 100, Layers.Environment)){
                _scopeObject.transform.position = hit2.point;
                _hookTargetPoint = hit2.point;
                foundGroundPoint = true;
            } else{
                //_hookTargetPoint = CameraTransform().position + CameraTransform().forward * 100;
                _scopeObject.transform.position = CameraTransform().position + CameraTransform().forward * 1000;
            }
            
            if (Input.GetKeyDown(KeyCode.V) && _hookTimer <= 0){
                if (foundGroundPoint || ballInHookRange){
                    //hookTargetPos = _hookTargetPoint;
                    _hookRope = Instantiate(_ropePrefab, transform.position - transform.forward, Quaternion.identity);
                    _hookRope.LockLastNode(CameraTransform(), transform.position - transform.forward);
                    _hookRope.SetVelocityToFirstNode(CameraTransform().forward * 20);
                    GameObject firstNodeObject = _hookRope.FirstNode().gameObject;
                    //Animations.Instance.MoveObject(ref firstNodeObject, _hookTargetPoint, hookPullDelay, false, 0, (a) => Sqrt(a));
                }
                
                if (ballInHookRange){
                    _hookPullingBall = ballInHookRange;
                    _pulledBall = true;
                }
            }
        }
        
        if (_hookRope){
            if (_hookTimer <= hookPullDelay){
                Vector3 targetPoint = _hookTargetPoint;
                if (_hookPullingBall){
                    targetPoint = _hookPullingBall.transform.position;
                    _scopeObject.transform.position = targetPoint;
                }
                var ropeTransform = _hookRope.FirstNode().transform;
                MoveToPosition(ref ropeTransform, ref _hookFlyTimer, hookPullDelay, BallStartPosition(), targetPoint, false, (a) => Sqrt(a));
            } else{
                _scopeObject.transform.position = _hookRope.FirstNode().transform.position;
            }
            _hookTimer += dt;
            if (_hookTimer >= hookPullDelay && !_hookPulled){
                _hookPulled = true;
                if (_hookPullingBall){
                    _ballInHold = _hookPullingBall;
                    _hookPullingBall = null;
                    _hookRope.LockFirstNode(_ballInHold.transform, _ballInHold.transform.position);
                } else{
                    playerVelocity = _hookRope.EndToStartDirection() * hookPullPower;
                }
                playerVelocity.y = Clamp(playerVelocity.y, 0, 40) + hookUpPower;
            }
            if (_hookTimer >= hookPullDelay * 2){
                _hookPulled = false;
                _hookRope.DestroyRope(1);
                _hookRope = null;
                _hookTimer = 0;
                _hookFlyTimer = 0;
            }
        }
    }
    
    private void UpdateKick(float dt){
        if (_kickCooldownCountdown > 0){
            _kickCooldownCountdown -= dt;
            
            if (_kickCooldownCountdown <= _kickCooldown * 0.7f){
                _kickedSomething = false;
            }
        }
        
        if (_kickCooldownCountdown > 0){
            return;
        }
        
        if (_kickPerformingCountdown <= 0 && Input.GetMouseButtonDown(0)){
            _kickPerformingCountdown = _kickDuration;
            //_kickModelParticle.Emit(1);
        }
        
        if (_kickPerformingCountdown <= 0){
            return;
        }
        
        Collider[] targets = KickTargets();
        
        for (int i = 0; i < targets.Length; i++){
            _kickedSomething = true;
            _kickedCol = targets[i];
            
            if (_kickPerformingCountdown >= _kickDuration - 0.1f){
                break;
            }
            //Ball kick
            var ball = targets[i].GetComponentInParent<PlayerBall>();
            if (ball){
                ball.groundBounceCount = 0;
                ball.lifeTime = 0;
                StopHoldingBall();
                SetKickVelocityToBall(ref ball);
                
                time.AddHitStop(0.05f);
                pCam.ShakeCameraBase(0.7f);
                
                Vector3 targetScale = Vector3.one * 0.5f;
                targetScale.z *= ball.velocity.magnitude * 0.2f;
                Animations.Instance.ChangeScale(ball.transform.gameObject, targetScale, 0.1f, true, 0.3f, EaseOutQuint);
                
                float colliderSizeProgress = Clamp01(_playerSpeed / 50);
                ball.sphere.radius = Lerp(1, 3, colliderSizeProgress);
                
                _kickPerformingCountdown = 0;
                _kickCooldownCountdown = _kickCooldown;
            }
        }
        
        _kickPerformingCountdown -= dt;
        if (_kickPerformingCountdown <= 0){
            _kickPerformingCountdown = 0;
            _kickCooldownCountdown = _kickCooldown;
        }
    }
    
    private Collider _kickedCol;
    private bool _kickedSomething;
    private void UpdateKick1(float dt){
        if (_kickCooldownCountdown > 0){
            _kickCooldownCountdown -= dt;
        }
        if (_kickCooldownCountdown > 0){
            return;
        }
    
        if (_kickPerformingCountdown <= 0 && Input.GetMouseButtonDown(0)){
            _kickPerformingCountdown = _kickDuration;
            //_kickModelParticle.Emit(1);
        }
        
        if (_kickPerformingCountdown > 0){
            //_kickPerformingCountdown -= dt;
            
            Collider[] targets = KickTargets();            
            
            Vector3 target = Vector3.zero;
            
            if (targets.Length > 0){
                target = targets[0].ClosestPoint(KickCenter());
            } else{
                target = KickEnd();
            }
            
            legs.SetIkTarget(1, Vector3.Lerp(legs.ikLegs[1].lastTarget, target, dt * 20));
            _lastKickTarget = target;
        }
        
        if (_kickPerformingCountdown > 0){
            _kickPerformingCountdown -= dt;
        
            Collider[] targets = KickTargets();            
            
            for (int i = 0; i < targets.Length; i++){
                int targetHash = targets[i].transform.parent ? targets[i].transform.parent.GetHashCode() : targets[i].GetHashCode();
                if (_alreadyHitByKick.Contains(targetHash)){
                    continue;
                }
                _alreadyHitByKick.Add(targetHash);
                
                Particles.Instance.SpawnAndPlay(_kickHitParticles, targets[i].ClosestPoint(transform.position));
                
                if (targets[i].transform.GetComponent<WinGate>()){
                    Win(targets[i].ClosestPoint(transform.position));
                    return;
                }
                
                //Ball kick
                var ball = targets[i].GetComponentInParent<PlayerBall>();
                if (ball){
                    ball.groundBounceCount = 0;
                    ball.lifeTime = 0;
                    StopHoldingBall();
                    SetKickVelocityToBall(ref ball);
                    
                    time.AddHitStop(0.05f);
                    pCam.ShakeCameraBase(0.7f);
                    
                    Vector3 targetScale = Vector3.one * 0.5f;
                    targetScale.z *= ball.velocity.magnitude * 0.2f;
                    Animations.Instance.ChangeScale(ball.transform.gameObject, targetScale, 0.1f, true, 0.3f, EaseOutQuint);
                    
                    float colliderSizeProgress = Clamp01(_playerSpeed / 50);
                    ball.sphere.radius = Lerp(1, 3, colliderSizeProgress);
                    break;
                }
                
                var enemy = targets[i].GetComponentInParent<Enemy>();
                if (enemy){
                    enemy.TakeKick(CameraTransform().forward * _kickPower, targets[i].ClosestPoint(KickCenter()));
                    pCam.ShakeCameraBase(0.8f);
                    time.AddHitStop(0.1f);
                }
                
                if (targets[i].TryGetComponent<RopeNode>(out var ropeNode)){
                    ropeNode.velocity += CameraTransform().forward * _kickPower;
                }
            }
            
            if (_kickPerformingCountdown <= 0){
                _kickCooldownCountdown = _kickCooldown;
                _alreadyHitByKick.Clear();
            }
        }
    
    }
    
    public Vector3 KickCenter(){
        return transform.position + CameraTransform().forward * kickHitBoxLength * 0.5f;
    }
    
    public Vector3 KickEnd(){
        return transform.position + CameraTransform().forward * kickHitBoxLength;
    }
    
    private Collider[] KickTargets(float mult = 1){
        var kickHitBoxCenter = KickCenter() * mult;
        Collider[] targets = OverlapBox(kickHitBoxCenter, new Vector3(kickHitBoxWidth * mult, kickHitBoxWidth * mult, kickHitBoxLength * mult) * 0.5f, CameraTransform().rotation, Layers.PlayerKickHitable);
        
        return targets;
    }
    
    private bool TargetInKickRange(){
        var targets = KickTargets();
        for (int i = 0; i < targets.Length; i++){
            var ball = targets[i].GetComponentInParent<PlayerBall>();
            if (ball){
                return true;
            }
        }
        return false;
    }
    
    private void Jump(Vector3 wishDirection, float multiplier = 1){
        if (playerVelocity.y < 0) playerVelocity.y = 0;
        playerVelocity.y += jumpForce;
//        playerVelocity += wishDirection * jumpForwardBoost * multiplier;
        
        pCam.ShakeCameraLong(0.1f);
        pCam.AddCamVelocity(-transform.up * 80);
        
        _jumpBufferCountdown = 0;
        //_jumpChargeProgress = 0;
        _timeSinceJump = 0;
    }
    
    private void GroundMove(float dt, Vector3 wishDirection){
        var wishSpeed = wishDirection.sqrMagnitude * _currentSpeed;
        
        ApplyFriction(dt);
        
        var directionDotVelocity = Vector3.Dot(wishDirection, playerVelocity.normalized);
        var acceleration = directionDotVelocity < 0.5f ? groundDeceleration : groundAcceleration;
        
        Accelerate(dt, wishDirection, wishSpeed, acceleration);
        
        _playerSpeed = playerVelocity.magnitude;
        
        if (_playerSpeed > baseSpeed && directionDotVelocity < 0.1f){
            pCam.ShakeCameraRapid(dt * 5);
        }
        
        if (_playerSpeed <= EPSILON){
            playerVelocity = Vector3.zero;
        }
    }
    
    private void AirMove(float dt, Vector3 wishDirection){
        var wishSpeed = wishDirection.sqrMagnitude * _currentSpeed;
        
        var directionDotVelocity = Vector3.Dot(wishDirection, playerVelocity.normalized);
        var acceleration = directionDotVelocity < 0f ? airDeceleration : airAcceleration;

        Accelerate(dt, wishDirection, wishSpeed, acceleration);
        
        _playerSpeed = playerVelocity.magnitude;
    }
    
    private void Accelerate(float dt, Vector3 targetDirection, float wishSpeed, float acceleration){
        var speedInWishDirection = Vector3.Dot(playerVelocity, targetDirection);
        
        var speedDifference = wishSpeed - speedInWishDirection;        
        
        if (speedDifference <= 0){
            return;
        }
        
        var accelerationSpeed = acceleration * speedDifference * dt;
        if (accelerationSpeed > speedDifference){
            accelerationSpeed = speedDifference;
        }
        
        playerVelocity.x += targetDirection.x * accelerationSpeed;
        playerVelocity.z += targetDirection.z * accelerationSpeed;
    }
    
    private void ApplyFriction(float dt){
        Vector3 frictionForce = _currentFriction * -playerVelocity.normalized * dt;
        
        frictionForce = Vector3.ClampMagnitude(frictionForce, _playerSpeed);
        
        playerVelocity += frictionForce;
    }  
    
    private void CalculatePlayerCollisions(ref Vector3 velocity, float dt){
        Vector3 nextPosition = transform.position + velocity * dt;
        // var sphereCenter1 = nextPosition - transform.up * _capsule.height * 0.5f + _capsule.radius * transform.up;
        // var sphereCenter2 = nextPosition + transform.up * _capsule.height * 0.5f - _capsule.radius * transform.up;
        
        bool foundGround = false;
        
        bool wasGrounded = false;
        
        if (IsGrounded()){
            //nextPosition.y -= 1f;
            velocity -= _groundNormal;
            //nextPosition.y -= dt;
            wasGrounded = true;
        }
        
        ColInfo[] groundColliders = ColInfoInCapsule(nextPosition, transform, _capsule, velocity, Layers.Environment);
        
        for (int i = 0; i < groundColliders.Length; i++){
            if (Vector3.Dot(velocity, groundColliders[i].normal) >= 0){
                continue;
            }
            
            bool justGrounded = false;
            
            _surfaceAngle = Vector3.Angle(groundColliders[i].normal, transform.up);
            
            if (_surfaceAngle <= 55){
                _groundNormal = groundColliders[i].normal;
            
                foundGround = true;
                if (!_grounded && _airTime > 0.1f){
                    var landingSpeedProgress = -velocity.y / 75; 
                    pCam.ShakeCameraLong(landingSpeedProgress);
                    pCam.AddCamVelocity(-transform.up * 30 * landingSpeedProgress + Random.insideUnitSphere * 10 * landingSpeedProgress);
                    
                    sound.Play(landingClip, Lerp(0, 0.3f, playerVelocity.y / -100f));
                    justGrounded = true;
                    Vector3 normal = groundColliders[i].normal;
                    velocity = Vector3.ProjectOnPlane(velocity, normal);
                }
            }
            
             velocity -= groundColliders[i].normal * Vector3.Dot(velocity, groundColliders[i].normal);
        }
        
        // if (wasGrounded && !foundGround && playerVelocity.y < 20 && Raycast(transform.position, Vector3.down, out var hit12, 10f, Layers.Environment)){
        //     Debug.Log(velocity.magnitude);
        //     velocity = (Vector3.ProjectOnPlane(velocity, hit12.normal)).normalized * velocity.magnitude;
        //     Debug.Log("after: " + velocity.magnitude);
        //     foundGround = true;
        // }
        
        _grounded = foundGround;
    }
    
    private void UpdateBalls(float dt){
        // if (_currentBallCount < maxBallCount){
        //     _ballReloadCountdown -= dt;
        //     if (_ballReloadCountdown <= 0){
        //         _currentBallCount++;
        //         if (_ballCounterTextMesh){
        //             _ballCounterTextMesh.text = _currentBallCount.ToString();
        //         }
                
        //         if (_currentBallCount < maxBallCount){
        //             _ballReloadCountdown = ballReloadTime;
        //         }
        //     }
        // }
    
        if (_shootCooldownTimer > 0){
            _shootCooldownTimer -= dt;
        }
    
        var angularVelocityDecreaseMultiplier = 0.5f; 
        
        if (Input.GetMouseButtonDown(1)){
            _currentStartAngularVelocity = Vector3.zero;
        }
        
        UpdateHoldBall(dt);
        PredictAndDrawBallTrajectory();
        
        if (Input.GetMouseButton(1)){
//            PredictAndDrawBallTrajectory();
        } else{
            //_currentStartAngularVelocity = Vector3.Lerp(_currentStartAngularVelocity, Vector3.zero, dt * 4);
            angularVelocityDecreaseMultiplier = 2f;
        }
        if (Input.GetMouseButtonUp(1)){
            //_ballPredictionLineRenderer.positionCount = 0;
            //StopHoldingBall(true);
        }
    
        if (Input.GetMouseButton(1) && Input.GetMouseButtonDown(0) && _shootCooldownTimer <= 0 && !TargetInKickRange() && !_ballInHold){
            PlayerBall newBall = SpawnPlayerBall(BallStartPosition());
            //newBall.index = _balls.Count;
            //_balls.Add(newBall);
            
            //_currentBallCount--;
            // if (_ballCounterTextMesh){
            //     _ballCounterTextMesh.text = _currentBallCount.ToString();
            // }
            // if (_ballReloadCountdown <= 0){
            //     _ballReloadCountdown = 0.2f;
            // }
            
            //Debug
            _shootCooldownTimer = 0.2f;
        }
        
        _currentStartAngularVelocity = Vector3.Lerp(_currentStartAngularVelocity, Vector3.zero, _unscaledDelta * angularVelocityDecreaseMultiplier);
        
        
        for (int i = 0; i < _balls.Count; i++){
            if (_balls[i] == null || !_balls[i].gameObject.activeSelf){
                //_balls.RemoveAt(i);
                continue;
            }
            UpdateBall(_balls[i], dt);    
        }
    }
    
    private void UpdateBall(PlayerBall ball, float dt, bool imaginaryBall = false){
        if (ball.lifeTime < 1){
            ball.inHold = false;
        }
        
        if (ball.inHold){
            return;
        }
    
        ball.velocity += Vector3.down * ballGravity * dt;
        
        ball.angularVelocity.y = Lerp(ball.angularVelocity.y, 0, dt * angularVelocityDecreaseRate);
        ball.angularVelocity.x = Lerp(ball.angularVelocity.x, 0, dt * angularVelocityDecreaseRate * 2);
        
        ball.velocityNormalized = ball.velocity.normalized;
        
        ball.velocityRight = Quaternion.Euler(0, 90, 0) * ball.velocityNormalized;
        ball.velocityUp    = Quaternion.AngleAxis(-90, ball.velocityRight)* ball.velocityNormalized;
        
        ball.speed = ball.velocity.magnitude;
        
        ball.velocity += (ball.velocityUp * ball.angularVelocity.x + ball.velocityRight * ball.angularVelocity.y) * angularVelocityPower * dt;
        
        ball.transform.forward = ball.velocity;
        
        ball.lifeTime += dt;
        
        if (ball.groundBounceCount >= 1 
                && Raycast(ball.transform.position, ball.velocityNormalized, out var hit, 10, Layers.Environment) 
                && hit.normal.y == 1){
            ball.velocity *= 1f - dt * 5;
            /*
            if (ball.velocity.sqrMagnitude < 2 * 2){
                ball.velocity = Vector3.ClampMagnitude(ball.velocity, 1);
            }
            */
        }
    
        CalculateBallCollisions(ref ball, dt, imaginaryBall);
        
        ball.velocity.y = Clamp(ball.velocity.y, -150, 150);
        
        if (imaginaryBall){
            //We made it only for imaginary because otherwise WindGuy do it by himself
            //_enemiesController.CheckWindForImaginaryBall(ref ball, dt);
            
            Array.Clear(_imaginaryBallAreas, 0, _imaginaryBallAreas.Length);
            OverlapSphereNonAlloc(ball.transform.position, ball.sphere.radius, _imaginaryBallAreas, Layers.Area);
            
            for (int i = 0; i < _imaginaryBallAreas.Length; i++){
                if (_imaginaryBallAreas[i] == null){
                    continue;
                }
                
                if (_imaginaryBallAreas[i].TryGetComponent<WindArea>(out var wind)){
                    ball.velocity += wind.PowerVector(ball.transform.position) * dt;
                }
            }
        }
        
        if (!imaginaryBall){
            float ropeCheckRadius = 5f;
            (Collider[], int) ropesInRadius = CollidersInRadius(ball.transform.position, ropeCheckRadius, Layers.Rope);
            for (int i = 0; i < ropesInRadius.Item2; i++){
                if (ropesInRadius.Item1[i].TryGetComponent<RopeNode>(out var ropeNode)){
                    ropeNode.velocity += ball.velocity * dt * 20;
                }
            }
        }
    
        ball.transform.Translate(ball.velocity * dt, Space.World);
        
        if (imaginaryBall || (ball.lifeTime < 1 && !ball.hitEnemy)) return;
        
        /*
        if (!Input.GetKey(_collectBallKey)) return;
        //Ball collect logic
        var playerToBallVector = ball.transform.position - transform.position;
        if (playerToBallVector.sqrMagnitude < ballCollectRadius * ballCollectRadius){
            if (_currentBallCount < maxBallCount){
                _currentBallCount++;
                _ballCounterTextMesh.text = _currentBallCount.ToString();
            }
            
            DisableBall(ref ball);
            //_balls.RemoveAt(ball.index);
            if (playerVelocity.y < 10){
                playerVelocity.y = 30;
            } else{
                playerVelocity.y += 20;
            }
        }
        */
    }
    
    private void ReflectToPlayer(ref PlayerBall ball, Enemy enemy){
        ball.velocity = (transform.position - enemy.transform.position).normalized * 10;
        ball.velocity.y = 20;
        //ball.angularVelocity.x = Clamp(ball.angularVelocity.x, 0, 15);
        ball.angularVelocity = Vector3.zero;
    }
    
    private void ReflectToNearbyEnemy(ref PlayerBall ball, Enemy enemy){
        Enemy closestEnemy = GetClosestEnemy(ball.transform.position, enemy.gameObject);
        if (closestEnemy){
            var ballToEnemyVectorNormalized = (closestEnemy.transform.position - ball.transform.position).normalized;
            ball.velocity = ballToEnemyVectorNormalized * 200;
        } else{
            ReflectToPlayer(ref ball, enemy);
        }
        ball.angularVelocity = Vector3.zero;
    }
    
    private bool BallRicocheCharged(ref PlayerBall ball){
        if (ball.ricocheCharged && ball.chargedBounceCount < 3){
            ball.chargedBounceCount++;    
            if (ball.chargedBounceCount >= 3){
                ball.ricocheCharged = false;
                ball.chargedBounceCount = 0;
                ball.chargedParticles.Stop();
            }
            
            return true;
        }
        
        return false;
    }
    
    private void CalculateBallCollisions(ref PlayerBall ball, float dt, bool imaginaryBall = false){
        //Layers. - gives us proper flag, but gameObject.layer gives us layer number from unity editor
        Vector3 nextPosition = ball.transform.position + ball.velocity * dt;
        
        var hitableLayers = Layers.PlayerBallHitable;
        /*
        if (imaginaryBall){
            hitableLayers &= ~(int)Layers.PlayerProjectile;
        }
        */
//        RaycastHit[] enemyHits = SphereCastAll(ball.transform.position, ball.sphere.radius, ball.velocity.normalized, deltaVelocity.magnitude, Layers.EnemyHurtBox);
        
        (Collider[], int) enemyColliders = CollidersInRadius(nextPosition, ball.sphere.radius, Layers.EnemyHurtBox);

        for (int i = 0; i < enemyColliders.Item2; i++){
            Collider col = enemyColliders.Item1[i];
        
            if (col.transform == ball.transform) continue;
            
            Vector3 colPoint = col.ClosestPoint(ball.transform.position);
            Vector3 vecToBall = (ball.transform.position - colPoint);
            Vector3 normal = vecToBall.normalized;
            
            if (!imaginaryBall && col.transform.GetComponent<WinGate>()){
                Win(colPoint);
                return;
            }
            
            var enemy = col.GetComponentInParent<Enemy>();
            
            if (!enemy || enemy.hitImmuneCountdown > 0 || ball.speed < 10 || Vector3.Dot(normal, ball.velocity) > 0) continue;

            if (ball.velocity.sqrMagnitude > 25){
                ball.bounceCount++;
                
                //if (ball.bounceCount > 20) continue;
                
                if (!imaginaryBall){
                    var p = Particles.Instance.SpawnAndPlay(_ballHitParticles, colPoint + normal);
                    p.transform.rotation = Quaternion.LookRotation(normal);
                    p.Play();
                }
            }

            bool hitBallLayer = ((1 << col.transform.gameObject.layer) & (int)Layers.PlayerProjectile) > 0;
            if (hitBallLayer){
            }

            if (enemy){
                if (!imaginaryBall){
                    if (enemy.effectsCooldown <= 0){
                        var hitStopMultiplier = 1.0f;
                        if (ball.ricocheCharged){
                            hitStopMultiplier = 3.0f;
                        }
                        
                        time.AddHitStop(0.05f * hitStopMultiplier);
                        pCam.ShakeCameraBase(0.3f);
                    }

                    switch (enemy.type){
                        case BlockerType:
                            enemy.TakeKick(ball.velocity, col.ClosestPoint(ball.transform.position));
                            break;
                        case WindGuyType:
                            enemy.TakeKick(ball.velocity, col.ClosestPoint(ball.transform.position));
                            break;
                        case DefenderType:
                            enemy.TakeKick(ball.velocity, col.ClosestPoint(ball.transform.position));
                            break;
                        default:
                            enemy.TakeHit(col);
                            break;
                    }
                
                    //ball.hitEnemy = true;
                } else{
                    GameObject enemyObject = enemy.gameObject;
                    Animations.Instance.ChangeMaterialColor(ref enemyObject, Colors.PredictionHitColor * 3, 0.02f);
                }

                switch (enemy.type){
                    case DummyType:
                        if (BallRicocheCharged(ref ball)){
                            ReflectToNearbyEnemy(ref ball, enemy);
                        } else{
                            ReflectToPlayer(ref ball, enemy);
                        }
                        break;
                    case HorizontalShooterType:
                    case VerticalShooterType:
                        if (BallRicocheCharged(ref ball)){
                            ReflectToNearbyEnemy(ref ball, enemy);
                        } else{
                            ReflectToPlayer(ref ball, enemy);
                        }
                        break;
                    case RicocheType:
                        ball.ricocheCharged = true;
                        ball.chargedParticles.Play();
                        ReflectToNearbyEnemy(ref ball, enemy);
                        break;
                    default:
                        if (BallRicocheCharged(ref ball)){
                            ReflectToNearbyEnemy(ref ball, enemy);
                        } else{
                            ReflectToPlayer(ref ball, enemy);
                        }
                        //ball.velocity = Vector3.Reflect(ball.velocity, normal) * 0.5f;
                        break;
                }
            }
        }
            
//        RaycastHit[] otherHits = SphereCastAll(ball.transform.position, 0.5f, ball.velocity.normalized, deltaVelocity.magnitude, Layers.Environment | Layers.EnemyProjectile);
        
        nextPosition = ball.transform.position + ball.velocity * dt;
        
        (Collider[], int) otherColliders = CollidersInRadius(nextPosition, 0.5f, Layers.Environment | Layers.EnemyProjectile | Layers.Player);
        
        for (int i = 0; i < otherColliders.Item2; i++){
            Collider col = otherColliders.Item1[i];
            
            Vector3 colPoint = col.ClosestPoint(ball.transform.position);
            Vector3 vecToBall = (ball.transform.position - colPoint);
            Vector3 normal = vecToBall.normalized;
        
            EnemyProjectile enemyProjectile = col.GetComponentInParent<EnemyProjectile>();
            if (!imaginaryBall && ball.speed > 35){
                Particles.Instance.SpawnAndPlay(_ballHitParticles, colPoint);
            }
            
            if (enemyProjectile){
                ball.velocity = Vector3.Reflect(ball.velocity, normal);
                ball.velocity.y = 20;
                
                enemyProjectile.velocity = Vector3.Reflect(enemyProjectile.velocity, -normal);
                continue;
            }
            
            Vector3 velocityReflectedVec = Vector3.Reflect(ball.velocity, normal);
            float velocityMultiplier = 0.6f;
            if (velocityReflectedVec.y <= 5 && ball.speed <= 40){ 
                velocityMultiplier = 0.95f;
            }
            ball.velocity = velocityReflectedVec * velocityMultiplier;
            //ball.velocity = Vector3.ClampMagnitude(ball.velocity, 60);
            ball.angularVelocity = Vector3.zero;
        }
    }
    
    private void DisableBall(ref PlayerBall ball){
        ball.lifeTime = 0;
        ball.sphere.radius = 1;
        ball.bounceCount = 0;
        ball.groundBounceCount = 0;
        ball.hitEnemy = false;
        ball.inHold = false;
        ball.velocity = Vector3.zero;
        
        ball.gameObject.SetActive(false);
    }
    
    private PlayerBall BallInRange(float multiplier = 2){
        var kickTargets = KickTargets(multiplier);
        for (int i = 0; i < kickTargets.Length; i++){
            var ball = kickTargets[i].GetComponentInParent<PlayerBall>();
            if (ball && ball.lifeTime > 1){
                return ball;
            }
        }
        return null;
    }
    
    private void UpdateHoldBall(float dt){
        //_holdingBall = false;
        
        // if (Input.GetMouseButtonDown(1)){
        //     //_ballInHold = BallInRange();
        // }
        
        float snappingBallTime = 0.3f;
        
        if (Input.GetMouseButton(1) && !_catchedBallOnInput){
            _ballInHold = BallInRange(2);
            if (_ballInHold){
                _ballInHold.velocity = playerVelocity;
                GameObject ballObject = _ballInHold.gameObject;
                //Animations.Instance.MoveObject(ref ballObject, BallStartPosition(), 0.1f, false, 0, (a) => a * a);
                //Animations.Instance.ChangeMaterialColor(ref ballObject, Colors.BallHighlightColor, snappingBallTime);
                _catchedBallOnInput = true;
            }

            // if (!_ballInHold){
            //     _ballInHold = BallInRange();
            // }
            
            // if (_ballInHold){
            //     _holdingBallTime += dt;
            // }
        }
        
        if (Input.GetMouseButtonUp(1)){
            _catchedBallOnInput = false;
            //StopHoldingBall(true);
            StopHoldingBall(true);
        }
        
        if (_ballInHold){
            float maxHoldTime = snappingBallTime;
            if (_pulledBall){
                maxHoldTime *= 4;
            }
            float ballSnapSpeed = 20;
            if (_pulledBall){
                ballSnapSpeed = 5;
            }
            
            _holdingBallTime += dt;
            _ballInHold.transform.position = Vector3.Lerp(_ballInHold.transform.position, BallStartPosition(), dt * ballSnapSpeed * Clamp(_playerSpeed / baseSpeed, 1, 10));
            MoveSphereOutCollision(_ballInHold.transform, _ballInHold.sphere.radius, Layers.Environment);
            _ballInHold.velocity = playerVelocity;
        }
    }
    
    private void PredictAndDrawBallTrajectory(){
        // if (haveScope && Input.GetKey(KeyCode.Mouse1)){
        //     _currentStartAngularVelocity += new Vector3(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * angularVelocitySense;
        //     _currentStartAngularVelocity.x = Clamp(_currentStartAngularVelocity.x, -maxAngularVelocity, maxAngularVelocity);
        //     _currentStartAngularVelocity.y = Clamp(_currentStartAngularVelocity.y, -maxAngularVelocity, maxAngularVelocity);
        // }
        
        // if (haveScope){
        var imaginaryBall = SpawnPlayerBall(BallStartPosition());
        imaginaryBall.imaginary = true;
        
        PlayerBall ballInKickRange = _ballInHold ? _ballInHold : BallInRange(1);
        if (ballInKickRange){
            imaginaryBall.transform.position = ballInKickRange.transform.position;
            imaginaryBall.angularVelocity = ballInKickRange.angularVelocity;
            imaginaryBall.velocity = ballInKickRange.velocity;
        }
        
        if (ballInKickRange ||  Input.GetKey(KeyCode.Mouse1)){
            SetKickVelocityToBall(ref imaginaryBall);
            //imaginaryBall.gameObject.name += "IMAGINE";
            
            var iterationCount = 200;
            var step = 0.02f;
            
            _ballPredictionLineRenderer.positionCount = iterationCount;
            for (int i = 0; i < iterationCount; i++){
                _ballPredictionLineRenderer.SetPosition(i, imaginaryBall.transform.position);
                UpdateBall(imaginaryBall, step, true);
            }
        } else{
            _ballPredictionLineRenderer.positionCount = 0;
        }
        
        DisableBall(ref imaginaryBall);
        //}
    }
    
    private void StopHoldingBall(bool inheritVelocity = false){
        if (!_ballInHold) return;
    
        _ballInHold.inHold = false;
        _holdingBallTime = 0;
        _pulledBall = false;
        
        if (inheritVelocity) _ballInHold.velocity = playerVelocity;
        
        _ballInHold = null;
        //_holdingBall = false;
    }
    
    private PlayerBall SpawnPlayerBall(Vector3 spawnPosition){
        PlayerBall newBall = null;
        
        for (int i = 0; i < _balls.Count; i++){
            if (!_balls[i].gameObject.activeSelf){
                newBall = _balls[i];
                newBall.gameObject.SetActive(true);
                newBall.transform.position = spawnPosition;
                newBall.imaginary = false;
                newBall.chargedParticles.Stop();
                break;
            }
        }
        
        if (!newBall){
            newBall = Instantiate(_playerBallPrefab, spawnPosition, Quaternion.identity);
            InitBall(ref newBall);
        }
        
        MoveSphereOutCollision(newBall.transform, 0.5f, Layers.Environment);
        
        return newBall;        
    }
    
    private void InitBall(ref PlayerBall ball){
        if (ball.initialized){
            return;
        }
        ball.index = _balls.Count;
        ball.sphere = ball.GetComponent<SphereCollider>();
        ball.initialized = true;
        _balls.Add(ball);
        //Debug.Log(_balls.Count);
    }
    
    private Vector3 BallStartPosition(){
        return CameraTransform().position + CameraTransform().forward * 4 - CameraTransform().up * 2.5f;
    }
    
    private void SetKickVelocityToBall(ref PlayerBall ball){
        ball.velocity = CameraTransform().forward * maxBallSpeed + playerVelocity * 0.5f;
        ball.angularVelocity = _currentStartAngularVelocity;
    }
    
    private void OnBallSpawnerFound(Transform spawnPoint)
    {
        //var ball = Instantiate(_playerBallPrefab, spawnPoint.position, spawnPoint.rotation);
        //InitBall(ref ball);
            
        //_balls.Add(ball);
        SpawnPlayerBall(spawnPoint.position);
    }
    
    private void BulletTime(){
        if (Input.GetKey(_bulletTimeKey)){
            Time.timeScale = 0.05f;
            //_currentStamina -= _unscaledDelta * bulletTimeStaminaDrain;
        } 
        else if (Input.GetKeyUp(_bulletTimeKey)){
            Time.timeScale = 1f;
        }
    }
    
    public void Win(Vector3 pos){
        time.SlowToZero();
        for (int i = 0; i < 100; i++){
            Particles.Instance.SpawnAndPlay(_ballHitParticles, pos);
        }
    }
    
    public bool IsGrounded(){
        return _grounded;
    }
    
    public void ResetPosition(bool falling = false){
        if (!falling){
            return;
        }
        transform.position = _spawnPosition;
    }
    
    public List<PlayerBall> GetBalls(){
        return _balls;
    }
    
    public float TimeSinceGrounded(){
        return _airTime;
    }
    
    private void OnDrawGizmosSelected(){
        Gizmos.color = Color.blue;
        
        // if (_player == null){
        //     _player = attackerSettings;
        // }
        
        //Gizmos.DrawWireSphere(transform.position, ballCollectRadius);
        
        Gizmos.color = Color.red;
        
        
        var kickHitBoxCenter = transform.position + CameraTransform().forward * kickHitBoxLength * 0.5f;
        Gizmos.DrawWireCube(kickHitBoxCenter, new Vector3(kickHitBoxWidth, kickHitBoxWidth, kickHitBoxLength));
    }
    
    private void DebugStuff(){
        if (Input.GetKeyDown(KeyCode.Keypad5)){
            time.SetDebugTimeScale(5);
        }
        if (Input.GetKeyDown(KeyCode.Keypad2)){
            time.SetDebugTimeScale(2);
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad3)){
            time.SetDebugTimeScale(0.5f);
        }
        if (Input.GetKeyDown(KeyCode.Keypad4)){
            time.SetDebugTimeScale(0.1f);
        }
        //No other keys
        if (Input.GetKeyDown(KeyCode.L)){
            pCam.ShakeCameraLong(1);
        }
        
        if (Input.GetKeyDown(KeyCode.K)){
            pCam.ShakeCameraRapid(1);
        }
        
        if (Input.GetKeyDown(KeyCode.J)){
            pCam.ShakeCameraBase(1);
        }
        
        if (Input.GetKeyDown(KeyCode.T)){
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        
        if (Input.GetKeyDown(KeyCode.P)){
            GAME_DELTA_SCALE = GAME_DELTA_SCALE < 1 ? 1 : 0;
        }
        
        if (Input.GetKeyDown(KeyCode.H)){
            Jump(transform.up, 5);
        }
        
        if (showPlayerStats){
            //var horizontalSpeed = (new Vector3(playerVelocity.x, 0, playerVelocity.z)).magnitude;
            if (_speedText){
                _speedText.text = "Speed: " + playerVelocity.magnitude;
            }
            
            if (_horizontalSpeedText){
                _horizontalSpeedText.text = "HorizontalSpeed: " + (new Vector3(playerVelocity.x, 0, playerVelocity.z)).magnitude;
            }
            
            if (_groundedText){
                _groundedText.text = "Grounded: " + IsGrounded();
            }
            
            if (_surfaceAngleText){
                _surfaceAngleText.text = "Surface angle: " + _surfaceAngle;
            }
        }
    }
}

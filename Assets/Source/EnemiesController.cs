using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
using Source.Features.SceneEditor.Controllers;
using Array = System.Array;
using UnityEditor;
using static UnityEngine.Mathf;
using static UnityEngine.Physics;
using static Source.Utils.Utils;
using static EnemyType;
using static DodgeDirection;

public enum EnemyType{
    DummyType,
    VerticalShooterType,
    HorizontalShooterType,
    BlockerType,
    RicocheType,
    WindGuyType,
    DefenderType
}

[Serializable] 
public class Dummy{
    public Enemy enemy;
    public Vector3 dodgeStartPosition;
    public float ballDetectRadius = 24f;
    public float dodgeDistance    = 7f;
    public float dodgeTime        = 2f;
    public float dodgeTimer;
    public bool  dodging;
}

[Serializable]
public class Shooter{
    //shooter variation 1: horizontal dodge look at player, 2: vertical dodge look at player, 3: no dodge don't look at player
    public Enemy enemy;
    public Dummy dummyDodgeComponent;
    public float shootCooldown   = 5f;
    public int   burstShootCount = 3;
    public float shootDelay      = 0.1f;
    public float cooldownTimer;
    public float delayTimer;
    public int   shootedCount;
}

[Serializable]
public class Blocker{
    public Enemy enemy;
    public Vector3 pivotPosition;
    public float moveDistance = 25;
    public float cycleTime = 5;
    public float cycleProgress = 0.25f;
    
    public Vector3 startBlockPosition;
    public Vector3 endBlockPosition;
    public float ballDetectRadius = 25f;
    public float maxBlockDistance = 30f;
    public float blockTime = 1.5f;
    public float blockTimer;
    public float blockCooldown = 3f;
    public float blockCooldownCountdown;
    public bool blocking;
}

[Serializable]
public class Ricoche{
    public Enemy enemy;
    public LineRenderer targetLine;
    public Vector3 pivotPosition;
    public Vector3 orbitAxis;
    public Vector3 orbitPosition;
    public float orbitRadius = 75;
    public float orbitingSpeed = 20;
}

[Serializable]
public class WindGuy{
    public Enemy enemy;
    public WindArea windArea;
}

[Serializable] 
public class Defender{
    public Enemy e;
    public CapsuleCollider capsule;
    public bool grounded;
    
    public Transform parentTransform;
    public float restRadius = 10f;
}

public class EnemiesController : MonoBehaviour{
    [Header("Shooter")]
    [SerializeField] private float projectileStartSpeed;
    
    [Header("Wind guy")]
    [SerializeField] private float windGuyPower  = 20;
    [SerializeField] private float windGuyLength = 20;
    [SerializeField] private float windGuyWidth  = 5;
    
    private Collider[] _targetColliders;
    
    private Transform        _playerTransform;
    private PlayerController _player;
    private Vector3          _playerPosition;
    
    private EnemyProjectile _shooterProjectilePrefab;
    private Enemy _dummyPrefab;
    private Enemy _horizontalShooterPrefab;
    private Enemy _verticalShooterPrefab;
    private Enemy _blockerPrefab;
    private Enemy _ricochePrefab;
    private Enemy _windGuyPrefab;
    private Enemy _defenderPrefab;
    
    private ParticleSystem _baseDeadManParticles;
    
    private List<EnemyProjectile> _enemyProjectiles = new();
    private List<Dummy>           _dummies          = new();
    private List<Shooter>         _shooters         = new();
    private List<Blocker>         _blockers         = new();
    private List<Ricoche>         _ricoches         = new();
    private List<WindGuy>         _windGuys         = new();
    private List<Defender>        _defenders        = new();
    
    private List<Enemy> _enemies = new();
    
    private void Awake(){
        _shooterProjectilePrefab = GetPrefab("ShooterProjectile").GetComponent<EnemyProjectile>();
        //_dummyPrefab = GetPrefab("Dummy").GetComponent<EnemyProjectile>();
        _horizontalShooterPrefab = GetPrefab("HorizontalDummyShooter").GetComponent<Enemy>();
        _verticalShooterPrefab = GetPrefab("VerticalDummyShooter").GetComponent<Enemy>();
        _blockerPrefab  = GetPrefab("Blocker").GetComponent<Enemy>();
        _ricochePrefab  = GetPrefab("Ricoche").GetComponent<Enemy>();
        _windGuyPrefab  = GetPrefab("WindGuy").GetComponent<Enemy>();
        _defenderPrefab = GetPrefab("Defender").GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        SceneLoader.EnemySpawnerFound += OnEnemySpawnerFound;
    }
    
    private void OnDisable()
    {
        SceneLoader.EnemySpawnerFound -= OnEnemySpawnerFound;
    }

    private void OnEnemySpawnerFound(EnemyType type, Transform spawnPoint)
    {
        SpawnEnemy(type, spawnPoint.position, spawnPoint.rotation);
    }
    
    private void Start(){
        _baseDeadManParticles = Particles.Instance.GetParticles("BaseDeadManParticles");
        
        _playerTransform = FindObjectOfType<PlayerController>().transform;
        _player = _playerTransform.GetComponent<PlayerController>();
        
        _targetColliders = new Collider[20];
        
    
        var enemiesOnScene = FindObjectsOfType<Enemy>();
        
        for (int i = 0; i < enemiesOnScene.Length; i++){
            Enemy enemy = enemiesOnScene[i];
            //if (enemy.index < 0){
            InitEnemy(ref enemy);
            //}
        }
    }
    
    public void SpawnEnemy(EnemyType type, Vector3 position, Quaternion rotation){
        Enemy enemy = null;
    
        switch (type){
            case DummyType:
                enemy = Instantiate(_dummyPrefab, position, rotation);
                break;
            case VerticalShooterType:
                enemy = Instantiate(_verticalShooterPrefab, position, rotation);
                break;
            case HorizontalShooterType:
                enemy = Instantiate(_horizontalShooterPrefab, position, rotation);
                break;
            case BlockerType:
                enemy = Instantiate(_blockerPrefab, position, rotation);
                break;
            case RicocheType:
                enemy = Instantiate(_ricochePrefab, position, rotation);
                break;
            case WindGuyType:
                enemy = Instantiate(_windGuyPrefab, position, rotation);
                break;
            case DefenderType:
                enemy = Instantiate(_defenderPrefab, position, rotation);
                break;
            default:
                Debug.LogError("No enemy prefab of type: " + type);
                break;
        }
        
        InitEnemy(ref enemy);
    }
    
    private void InitEnemy(ref Enemy enemy){
        // if (enemy.index >= 0){
        //     return;
        // }
        
        if (enemy.initialized){
            return;
        }
    
        enemy.sphere = enemy.GetComponent<SphereCollider>();
        enemy.kickTrailParticles = Instantiate(Particles.Instance.GetParticles("KickTrailParticles"), enemy.transform);
    
        switch (enemy.type){
            case DummyType:
                _dummies.Add(new Dummy() { enemy = enemy });
                _dummies[_dummies.Count-1].dodgeStartPosition = _dummies[_dummies.Count-1].enemy.transform.position;
                _dummies[_dummies.Count-1].enemy.index = _dummies.Count-1;
                break;
            case VerticalShooterType:
            case HorizontalShooterType:
                var shooter = new Shooter() {enemy = enemy};
                shooter.enemy.index = _shooters.Count;
                shooter.cooldownTimer = shooter.shootCooldown;
                shooter.dummyDodgeComponent = new Dummy() {enemy = shooter.enemy};
                shooter.dummyDodgeComponent.dodgeStartPosition = shooter.enemy.transform.position;
                _shooters.Add(shooter);
                break;
            case BlockerType:
                _blockers.Add(new Blocker() {enemy = enemy});
                _blockers[_blockers.Count-1].pivotPosition = _blockers[_blockers.Count-1].enemy.transform.position;
                _blockers[_blockers.Count-1].enemy.index = _blockers.Count-1;
                break;
            case RicocheType:
                var ricoche = new Ricoche() {enemy = enemy};
                ricoche.pivotPosition = ricoche.enemy.transform.position;
                ricoche.enemy.index = _ricoches.Count;
                
                ricoche.orbitAxis = ricoche.enemy.transform.up;
                ricoche.orbitPosition = ricoche.enemy.transform.right * ricoche.orbitRadius;
                
                ricoche.targetLine = ricoche.enemy.GetComponent<LineRenderer>();
                
                _ricoches.Add(ricoche);
                break;
            case WindGuyType:
                var windGuy = new WindGuy() {enemy = enemy};
                windGuy.enemy.index = _windGuys.Count;
                windGuy.windArea = windGuy.enemy.GetComponentInChildren<WindArea>();
                windGuy.windArea.windPower = windGuyPower;
                windGuy.windArea.boxCollider.center = windGuy.enemy.transform.forward * windGuyLength * 0.5f;
                windGuy.windArea.SetColliderSize(new Vector3(windGuyWidth, windGuyWidth, windGuyLength));
                _windGuys.Add(windGuy);
                break;
            case DefenderType:
                var defender = new Defender() {e = enemy}; 
                defender.e.index = _defenders.Count;
            
                if (Raycast(defender.e.transform.position, Vector3.down, out var hit, 100, Layers.Environment)){
                    defender.e.transform.position = hit.point + hit.normal * 5;
                    defender.e.transform.rotation = Quaternion.LookRotation(defender.e.transform.forward, hit.normal);
                }
                
                GameObject defenderParentObj = new GameObject("DefenderParent");
                defenderParentObj.transform.position = defender.e.transform.position;
                defenderParentObj.transform.rotation = defender.e.transform.rotation;
                defender.parentTransform = defenderParentObj.transform;
                defender.e.transform.SetParent(defender.parentTransform, true);
                
                defender.capsule = defender.e.GetComponent<CapsuleCollider>();
                
                //defender.legs = defender.e.GetComponent<RopeLegs>().legs;
                
                _defenders.Add(defender);
                break;
            default:
                Debug.Log("No initialization for enemy of type: " + enemy.type);
                break;
        }
        
        enemy.initialized = true;
        _enemies.Add(enemy);
    }
    
    private float _previousDelta;
    private float _unscaledDelta;
    private void Update(){
        if (GAME_DELTA_SCALE <= 0){
            return;
        }
    
//         float fullDelta = Time.deltaTime * GAME_DELTA_SCALE;
//         //_unscaledDelta = Time.unscaledDeltaTime * GAME_DELTA_SCALE;
//         fullDelta += _previousDelta;
//         _previousDelta = 0;
        
//         if (fullDelta > MIN_FRAME_DELTA){
//             float dt = MIN_FRAME_DELTA * Time.timeScale * GAME_DELTA_SCALE;
//             while (fullDelta > MIN_FRAME_DELTA){
//                 UpdateAll(dt);
//                 fullDelta -= MIN_FRAME_DELTA;
// //                _unscaledDelta = 0;
//             }
//             _previousDelta = fullDelta;
//         } else{
//             UpdateAll(fullDelta);
//         }

        MakeGoodFrameUpdate(UpdateAll, ref _previousDelta, ref _unscaledDelta);
    }
    
    private void UpdateAll(float dt){
        _playerPosition = _playerTransform.position;
        UpdateRicoches(dt);
        UpdateShooters(dt);
        UpdateEnemyProjectiles(dt);
        UpdateDummies(dt);
        UpdateBlockers(dt);
        UpdateWindGuys(dt);
        UpdateDefenders(dt);
        
        UpdateDebug(dt);
    }
    
    private void UpdateDefenders(float dt){
        for (int i = 0; i < _defenders.Count; i++){
            var def = _defenders[i];
            
            if (!def.e.gameObject.activeSelf || def.e.dead){
                continue;
            }
            // MoveByVelocity(ref windGuy.enemy, dt);            
            // FlyByKick(ref windGuy.enemy, dt);
            EnemyCountdowns(ref def.e, dt);
            
            // if (EnemyHit(ref def.e)){
            //     continue;
            // }
            
            if (def.e.justTakeHit || def.e.takedKick){
                def.e.dead = true;
                continue;
            }
            
            var defTrans = def.e.transform;
            
            if (defTrans.transform.position.y < -1000){
                def.e.gameObject.SetActive(false);
                return;
            }
            
            //float gravity = 100;
            float vOffset = 5f;
            float moveSpeed = 100f;
            bool grounded = true;
            
            Vector3 normal = Vector3.zero;
            
            Vector3 vecToPlayer = _playerPosition - defTrans.position;
            float distanceToPlayer = vecToPlayer.magnitude;
            
            Vector3 horizontalVecToPlayer = vecToPlayer;
            horizontalVecToPlayer.y = 0;
            
            Vector3 targetLocalPos = def.parentTransform.InverseTransformPoint(_playerPosition);
            targetLocalPos.z = 0;
            targetLocalPos.y = defTrans.localPosition.y;
            
            Vector3 targetPos = def.parentTransform.TransformPoint(targetLocalPos);
            
            Vector3 nextPos = Vector3.MoveTowards(defTrans.position, targetPos, moveSpeed * dt);
            
            if (Raycast(nextPos + Vector3.up * 5, Vector3.down, out var hit, 40, Layers.Environment)){
                normal = hit.normal;
                defTrans.position = hit.point + Vector3.up * vOffset;
            } else{
                nextPos = defTrans.position;
            }
            
            
            if (normal != Vector3.zero){
                Quaternion targetRotation = Quaternion.LookRotation(horizontalVecToPlayer, normal); //need up
                defTrans.rotation = Quaternion.RotateTowards(defTrans.rotation, targetRotation, dt * 50);
            }
            //defTrans.position = nextPos;
            
            // ColInfo[] cols = ColInfoInCapsule(nextPosition, defTrans, def.capsule, def.e.velocity, Layers.Environment);
            
            // for (int j = 0; j < cols.Length; j++){
            //     if (Vector3.Dot(def.e.velocity, cols[j].normal) >= 0){
            //         continue;
            //     }
                
            //     def.e.velocity -= cols[j].normal * Vector3.Dot(def.e.velocity, cols[j].normal) * 1.1f;
            //     //def.e.angularVelocity.x *= -1f;
            // }
            
            // defTrans.Translate(def.e.velocity * dt, Space.World);
            
            // defTrans.Rotate(def.e.angularVelocity * dt);
            // def.e.angularVelocity *= 1f - dt * 2f;
            
            // if (CheckSphere(defTrans.position + defTrans.up * def.capsule.height, def.capsule.radius, Layers.Environment)){
            //     def.e.angularVelocity *= -1f;                
            // }
            
            float punchPower = 100f;
            
            CapsuleSphereCenters(def.capsule, out Vector3 capsulePos1, out Vector3 capsulePos2);
            if (CheckCapsule(capsulePos1, capsulePos2, def.capsule.radius * 2, Layers.PlayerHurtBox)){
                PunchPlayer(defTrans.position, punchPower);            
            }
        }        
    }
    
    private void PunchPlayer(Vector3 puncherPos, float power){
        Vector3 dirToPlayer = (_playerPosition - puncherPos).normalized;
        dirToPlayer.y *= 0.5f;
        _player.playerVelocity = dirToPlayer * power;
        PlayerCameraController.Instance.ShakeCameraLong(1f);
        PlayerCameraController.Instance.ShakeCameraBase(1f);
    }
    
    private void UpdateWindGuys(float dt){
        for (int i = 0; i < _windGuys.Count; i++){
            var windGuy = _windGuys[i];
            
            if (!windGuy.enemy.gameObject.activeSelf){
                continue;
            }
            
            MoveByVelocity(ref windGuy.enemy, dt);            
            FlyByKick(ref windGuy.enemy, dt);
            EnemyCountdowns(ref windGuy.enemy, dt);
            
            if (EnemyHit(ref windGuy.enemy)){
                continue;
            }
            
            var windGuyTransform = windGuy.enemy.transform;
            
            Vector3 windCenter = windGuyTransform.position + windGuyTransform.forward * windGuyLength;
            (Collider[], int) overlapColliders = CollidersInBoxBig(windCenter, new Vector3(windGuyWidth, windGuyWidth, windGuyLength), windGuyTransform.rotation, Layers.Player | Layers.PlayerProjectile | Layers.EnemyProjectile | Layers.EnemyHurtBox | Layers.Rope);
            
            for (int j = 0; j < overlapColliders.Item2; j++){
                if (overlapColliders.Item1[j] == null){
                    continue;
                }
                
                var target = overlapColliders.Item1[j];
                
                Enemy otherEnemy = target.GetComponentInParent<Enemy>();
                if (otherEnemy && otherEnemy.type == WindGuyType){
                    continue;
                }
                
                if (otherEnemy && otherEnemy != windGuy.enemy){
                    otherEnemy.velocity += (windGuy.windArea.PowerVector(otherEnemy.transform.position) / otherEnemy.weight) * dt;
                } else if (target.TryGetComponent<PlayerBall>(out var playerBall)){
                    playerBall.velocity += windGuy.windArea.PowerVector(playerBall.transform.position) * dt;
                } else if (target.TryGetComponent<PlayerController>(out var player)){
                    player.playerVelocity += windGuy.windArea.PowerVector(player.transform.position) * dt;
                } else if(target.TryGetComponent<EnemyProjectile>(out var enemyProjectile)){ 
                    enemyProjectile.velocity += windGuy.windArea.PowerVector(enemyProjectile.transform.position) * dt;
                } else if (target.TryGetComponent<RopeNode>(out var ropeNode)){
                    ropeNode.velocity += windGuy.windArea.PowerVector(ropeNode.transform.position) * dt * 10;
                }
            }
            
            windGuyTransform.Rotate(Vector3.forward * windGuyPower * 5 * dt); 
        }
    }
    
    private void UpdateRicoches(float dt){
        for (int i = 0; i < _ricoches.Count; i++){
            var ricoche = _ricoches[i];
            
            if (!ricoche.enemy.gameObject.activeSelf){
                continue;
            }
            
            MoveByVelocity(ref ricoche.enemy, dt);
            EnemyCountdowns(ref ricoche.enemy, dt);
            
            if (FlyByKick(ref ricoche.enemy, dt) || EnemyHit(ref ricoche.enemy)){
                continue;
            }
            
            var ricocheTransform = ricoche.enemy.transform;
            
            ricoche.pivotPosition += ricoche.enemy.velocity * dt;
            ricocheTransform.position += ricoche.enemy.velocity * dt;
            
            ricoche.orbitPosition = Quaternion.AngleAxis(ricoche.orbitingSpeed * dt, ricoche.orbitAxis) * ricoche.orbitPosition;
                        
            ricocheTransform.position = ricoche.pivotPosition + ricoche.orbitPosition;
            
            ricocheTransform.rotation = Quaternion.Slerp(ricocheTransform.rotation,
                                                         Quaternion.LookRotation((_playerPosition - ricocheTransform.position).normalized),
                                                         dt * 5);
                                                         
            Enemy closestEnemy = GetClosestEnemy(ricocheTransform.position, ricoche.enemy.gameObject);
            
            if (closestEnemy){
                ricoche.targetLine.positionCount = 2;
                Vector3 lineTargetPosition = Vector3.Lerp(ricoche.targetLine.GetPosition(1), closestEnemy.transform.position, dt * 20);
                ricoche.targetLine.SetPosition(0, ricocheTransform.position);
                ricoche.targetLine.SetPosition(1, lineTargetPosition);
            } else{
                ricoche.targetLine.positionCount = 0;
            }
                        
            KillPlayerIfNearby(ricoche.enemy);
        }
    }
    
    private void UpdateBlockers(float dt){
        for (int i = 0; i < _blockers.Count; i++){
            var blocker = _blockers[i];
            
            if (!blocker.enemy.gameObject.activeSelf){
                continue;
            }
            
            var blockerTransform = blocker.enemy.transform;
            
            blocker.pivotPosition += MoveByVelocity(ref blocker.enemy, dt);
            
            EnemyCountdowns(ref blocker.enemy, dt);
            
            if (FlyByKick(ref blocker.enemy, dt) || EnemyHit(ref blocker.enemy)){
                blocker.cycleProgress = 0.25f;
                blocker.pivotPosition = blockerTransform.position;
                continue;
            }
            
            HandleEnvCollisions(ref blocker.enemy);
            
            if (blocker.blockCooldownCountdown > 0){
                blocker.blockCooldownCountdown -= dt;
            }
            
            if (!blocker.blocking && blocker.blockTimer <= 0 && blocker.blockCooldownCountdown <= 0){
                PlayerBall ballNearby = PlayerBallNearby(blockerTransform.position, blocker.ballDetectRadius);

                if (ballNearby){
                    blocker.blocking = true;
                    blocker.startBlockPosition = blockerTransform.position;
                    var vecToBall = ballNearby.transform.position - blockerTransform.position;
                    blocker.endBlockPosition = blockerTransform.position
                                               + (vecToBall.normalized * blocker.maxBlockDistance * 0.5f)
                                               + (ballNearby.velocityNormalized * blocker.maxBlockDistance * 0.5f);
                }
            }
            
            if (blocker.blocking){
                if (MoveToPosition(ref blockerTransform, ref blocker.blockTimer, blocker.blockTime, 
                                   blocker.startBlockPosition, blocker.endBlockPosition, false, EaseOutElastic)){
                    blocker.blocking = false;                    
                    blocker.blockTimer = 0;
                    blocker.blockCooldownCountdown = blocker.blockCooldown;
                    blocker.pivotPosition = blockerTransform.position;
               }
            }
            
            if (!blocker.blocking){
                blocker.cycleProgress += dt / blocker.cycleTime;
                
                float t = blocker.cycleProgress <= 0.5f ? blocker.cycleProgress * 2 : 1f - (blocker.cycleProgress - 0.5f) * 2;
                
                var startPosition  = blocker.pivotPosition - blockerTransform.right * blocker.moveDistance * 0.5f;
                var targetPosition = blocker.pivotPosition + blockerTransform.right * blocker.moveDistance * 0.5f;
                blockerTransform.position = Vector3.Lerp(startPosition, targetPosition, EaseInOutQuad(t));
                
                if (blocker.cycleProgress >= 1){
                    blocker.cycleProgress = 0;
                }
            }
            
            
            KillPlayerIfNearby(blocker.enemy);
        }
    }
    
    private void EnemyCountdowns(ref Enemy enemy, float dt){
        if (enemy.hitImmuneCountdown > 0){
            enemy.hitImmuneCountdown -= dt;
            enemy.hitImmuneCountdown = Clamp(enemy.hitImmuneCountdown, 0, 1);
        }
        if (enemy.kickImmuneCountdown > 0){
            enemy.kickImmuneCountdown -= dt;
            enemy.kickImmuneCountdown = Clamp(enemy.kickImmuneCountdown, 0, 1);
        }
        if (enemy.effectsCooldown > 0){
            enemy.effectsCooldown -= dt;
            enemy.effectsCooldown = Clamp(enemy.effectsCooldown, 0, 1);
        }
    }
    
    private Vector3 MoveByVelocity(ref Enemy enemy, float dt){
        enemy.transform.position += enemy.velocity * dt;
        enemy.velocity *= 1f - enemy.weight * dt;
        
        if (enemy.velocity.sqrMagnitude <= EPSILON){
            enemy.velocity = Vector3.zero;
            enemy.takedKick = false;
            enemy.kickTrailParticles.Stop();
            enemy.timeInKickFlight = 0;
        }
        
        return enemy.velocity * dt;
    }
    
    private bool EnemyHit(ref Enemy enemy){
        if (enemy.justTakeHit){
            enemy.justTakeHit = false;
            
            switch (enemy.type){
                case HorizontalShooterType:
                case VerticalShooterType:
                    float radius = 20;
                    float pushPower = 60;
                    (Collider[], int) collidersInExplosionRadius = CollidersInRadius(enemy.transform.position, radius, Layers.EnemyHurtBox | Layers.PlayerHurtBox | Layers.PlayerProjectile | Layers.EnemyProjectile);
                    for (int i = 0; i < collidersInExplosionRadius.Item2; i++){
                        Collider otherCollider = collidersInExplosionRadius.Item1[i];
                        
                        Enemy otherEnemy = otherCollider.GetComponentInParent<Enemy>();
                        if (otherEnemy == enemy){
                            continue;
                        }
                        
                        Vector3 vecToOther = otherCollider.transform.position - enemy.transform.position;
                        
                        if (otherEnemy){
                            otherEnemy.TakeKick(vecToOther.normalized * Sqrt(Clamp01(1f - vecToOther.sqrMagnitude / (radius * radius))) * pushPower, enemy.transform.position);
                        }
                    }
                    PlayerCameraController.Instance.ShakeCameraBase(0.3f);
                    Particles.Instance.SpawnAndPlay(_baseDeadManParticles, enemy.transform.position);
                    break;
                default:
                    Particles.Instance.SpawnAndPlay(_baseDeadManParticles, enemy.transform.position);
                    PlayerCameraController.Instance.ShakeCameraBase(0.3f);
                    break;
            }
            
            enemy.gameObject.SetActive(false);
            return true;
        }
        
        return false;
    }
    
    private void UpdateShooters(float dt){
        for (int i = 0; i < _shooters.Count; i++){
            var shooter = _shooters[i];
            
            if (!shooter.enemy.gameObject.activeSelf){
                continue;
            }
            
            if (shooter.enemy.variation == 1 || shooter.enemy.variation == 2){
                UpdateDummy(ref shooter.dummyDodgeComponent, dt);
            }
            
            EnemyCountdowns(ref shooter.enemy, dt);
            
            MoveByVelocity(ref shooter.enemy, dt);
            
            if (FlyByKick(ref shooter.enemy, dt) || EnemyHit(ref shooter.enemy)){            
                continue;
            }
            
            Transform shooterTransform = shooter.enemy.transform;
            
            //var vectorToPlayer = (_playerPosition - shooterTransform.position).normalized;
            /*
            var horizontalVectorToPlayer = new Vector3(vectorToPlayer.x, 0, vectorToPlayer.z);
            shooter.enemy.transform.rotation = Quaternion.Slerp(shooterTransform.rotation, Quaternion.LookRotation(horizontalVectorToPlayer), dt * 3);
            */
            
            KillPlayerIfNearby(shooter.enemy);
            
            shooter.cooldownTimer -= dt;
            
            if (shooter.cooldownTimer > 0){
                continue;
            }
            
            if (shooter.delayTimer <= 0){
                EnemyProjectile projectile = SpawnEnemyProjectile(shooterTransform.position + shooterTransform.up, shooter.enemy.transform.forward * projectileStartSpeed);
                shooter.shootedCount++;
                if (shooter.shootedCount < shooter.burstShootCount){
                    shooter.delayTimer = shooter.shootDelay;
                } else{
                    shooter.delayTimer = 0;
                    shooter.cooldownTimer = shooter.shootCooldown;
                    shooter.shootedCount = 0;
                }
            } else{
                shooter.delayTimer -= dt;
            }
        }
    }
    
    private void UpdateEnemyProjectiles(float dt){
        for (int i = 0; i < _enemyProjectiles.Count; i++){
            if (!_enemyProjectiles[i].gameObject.activeSelf){
                continue;
            }
        
            EnemyProjectile projectile = _enemyProjectiles[i];
            
            if (projectile == null){
                _enemyProjectiles.RemoveAt(i);
                continue;
            }
                
            CalculateEnemyProjectileCollisions(ref projectile, dt);
            
            projectile.transform.rotation = Quaternion.LookRotation(projectile.velocity);
            projectile.transform.Translate(projectile.velocity * dt, Space.World);
            
            projectile.lifeTime += dt;
            
            if (projectile.lifeTime >= projectile.slowingLifetime){
                float lifetimeOvershoot = projectile.lifeTime - projectile.slowingLifetime;
                projectile.velocity *= Clamp01(1f - dt * (lifetimeOvershoot * lifetimeOvershoot));
                
                if (projectile.velocity.sqrMagnitude <= EPSILON){
                    DisableEnemyProjectile(ref projectile);
                }
            }
        }
    }
    
    private EnemyProjectile SpawnEnemyProjectile(Vector3 position, Vector3 velocity){
        //EnemyProjectile newProjectile = null;
        for (int i = 0; i < _enemyProjectiles.Count; i++){
            if (!_enemyProjectiles[i].gameObject.activeSelf){
                _enemyProjectiles[i].gameObject.SetActive(true);
                _enemyProjectiles[i].transform.position = position;
                _enemyProjectiles[i].velocity = velocity;
                _enemyProjectiles[i].lifeTime = 0;
                return _enemyProjectiles[i];
            }
        }
        
        EnemyProjectile newProjectile = Instantiate(_shooterProjectilePrefab, position, Quaternion.LookRotation(velocity));
        newProjectile.velocity = velocity;
        newProjectile.index = _enemyProjectiles.Count;
        newProjectile.sphere = newProjectile.GetComponent<SphereCollider>();
        _enemyProjectiles.Add(newProjectile);
        
        return newProjectile;
    }
    
    private void DisableEnemyProjectile(ref EnemyProjectile projectile){
        projectile.lifeTime = 0;
        //projectile.slowingLifetime = 1.5f;
        projectile.gameObject.SetActive(false);
    }
    
    private void CalculateEnemyProjectileCollisions(ref EnemyProjectile projectile, float dt){
        if (projectile == null){
            return;
        }
    
        //var deltaVelocity = projectile.velocity * dt;
        
        Vector3 nextPosition = projectile.transform.position + projectile.velocity * dt;
        
        //var hits = SphereCastAll(projectile.transform.position, projectile.sphere.radius, projectile.velocity.normalized, deltaVelocity.magnitude, Layers.PlayerHurtBox | Layers.Environment);
        
        ColInfo[] hits = ColInfoInRadius(nextPosition, projectile.sphere.radius, Layers.PlayerHurtBox | Layers.Environment);
        
        for (int i = 0; i < hits.Length; i++){
            var player = hits[i].col.transform.GetComponentInParent<PlayerController>();
            if (player){
                player.ResetPosition();
            }
            
            DisableEnemyProjectile(ref projectile);
        }
    }
    
    private void HandleEnvCollisions(ref Enemy enemy){
        //(Collider[], int) collidersNearby = CollidersInRadius(enemy.transform.position, enemy.sphere.radius, Layers.Environment);
        
        ColInfo[] cols = ColInfoInRadius(enemy.transform.position, enemy.sphere.radius, Layers.Environment);
        
        for (int i = 0; i < cols.Length; i++){
            if (cols[i].vecToTarget.sqrMagnitude <= EPSILON) cols[i].vecToTarget = enemy.transform.forward;
            // enemy.transform.rotation = Quaternion.LookRotation(dirToEnemy);
            // enemy.transform.position += dirToEnemy;
            enemy.velocity = cols[i].normal * (enemy.velocity.magnitude + 10);
        }
    }

    private bool FlyByKick(ref Enemy enemy, float dt){
        if (!enemy.takedKick){
            return false;
        }
        
        enemy.timeInKickFlight += dt;
        if (enemy.timeInKickFlight >= 6){
            enemy.takedKick = false;
            enemy.kickTrailParticles.Stop();
            enemy.timeInKickFlight = 0;
            return false;
        }
    
        (Collider[], int) collidersNearby = CollidersInRadius(enemy.transform.position, enemy.sphere.radius, Layers.Environment | Layers.EnemyHurtBox | Layers.Rope);
        
        for (int i = 0; i < collidersNearby.Item2; i++){
            Enemy otherEnemy = collidersNearby.Item1[i].GetComponentInParent<Enemy>();
            
            if (otherEnemy == enemy) continue;
            
            if (otherEnemy){
                enemy.TakeHit();
                otherEnemy.TakeHit();
                otherEnemy.TakeKick(enemy.velocity * 2, enemy.transform.position);
            } else if (collidersNearby.Item1[i].TryGetComponent<WinGate>(out var winGate)){
                _player.Win(enemy.transform.position); 
                enemy.TakeHit();
            } else if (collidersNearby.Item1[i].TryGetComponent<RopeNode>(out var ropeNode)){
                ropeNode.velocity += enemy.velocity * dt * 20;
            } else{
                enemy.TakeHit();
            }
        }
        
        return true;
    }
    
    private void UpdateDummy(ref Dummy dummy, float dt){
        if (!dummy.enemy.gameObject.activeSelf){
            return;
        }
        
        EnemyCountdowns(ref dummy.enemy, dt);
        
        dummy.dodgeStartPosition += MoveByVelocity(ref dummy.enemy, dt);
        
        if (FlyByKick(ref dummy.enemy, dt) || EnemyHit(ref dummy.enemy)){
            dummy.dodging = false;
            dummy.dodgeTimer = 0;
            dummy.dodgeStartPosition = dummy.enemy.transform.position;
            return;   
        }
        
        HandleEnvCollisions(ref dummy.enemy);
        
        Transform dummyTransform = dummy.enemy.transform;
        /*
        if (dummy.enemy.justTakeHit){          
            dummy.enemy.justTakeHit = false;  
            dummy.enemy.hitImmuneCountdown = 0.1f;
            
            var wishPosition = Random.onUnitSphere * 20;
            wishPosition.y = Abs(wishPosition.y) * 0.5f;
            dummyTransform.position = wishPosition;
            
            dummy.dodging = false;
            dummy.dodgeTimer = 0;
            dummy.dodgeStartPosition = dummyTransform.position;
        }
        */
        if (!dummy.dodging && dummy.dodgeTimer <= 0 && PlayerBallNearby(dummy.dodgeStartPosition, dummy.ballDetectRadius)){
            dummy.dodging = true;
        }
        
        Vector3 dodgeDirection = dummy.enemy.dodgeDirection == Horizontal ? dummyTransform.right : dummyTransform.up;
        
        if (dummy.dodging){
            if (MoveToPosition(ref dummyTransform, ref dummy.dodgeTimer, dummy.dodgeTime,
                               dummy.dodgeStartPosition,
                               dummy.dodgeStartPosition + dodgeDirection * dummy.dodgeDistance, false, EaseOutElastic)){
                dummy.dodging = false;
                dummy.dodgeTimer = dummy.dodgeTime;
            }
        } else if (dummy.dodgeTimer > 0){
            if (MoveToPosition(ref dummyTransform, ref dummy.dodgeTimer, dummy.dodgeTime,
                               dummy.dodgeStartPosition,
                               dummy.dodgeStartPosition + dodgeDirection * dummy.dodgeDistance, true, EaseOutElastic)){
                dummy.dodgeTimer = 0;
            }
        }
        
        if (!dummy.dodging && dummy.dodgeTimer <= 0){
            dummyTransform.rotation = Quaternion.Slerp(dummyTransform.rotation, Quaternion.LookRotation((_playerPosition - dummyTransform.position).normalized), dt * 30);

        }
        
        KillPlayerIfNearby(dummy.enemy);
    }
    
    private void UpdateDummies(float dt){
        for (int i = 0; i < _dummies.Count; i++){
            Dummy dummy = _dummies[i];
                        
            UpdateDummy(ref dummy, dt);
        }
    }
    
    private PlayerBall PlayerBallNearby(Vector3 checkPosition, float checkRadius){
        List<PlayerBall> balls = _player.GetBalls();
        
        for (int i = 0; i < balls.Count; i++){
            if (balls[i].speed < 10) continue;
            
            Vector3 ballToEnemy = checkPosition - balls[i].transform.position;
            
            if (ballToEnemy.sqrMagnitude <= checkRadius * checkRadius && Vector3.Dot(ballToEnemy, balls[i].velocity) > 0){
                return balls[i];
            }
        }
        
        return null;
    }
    
    private void KillPlayerIfNearby(Enemy enemy){
        var checkRadius = 1f;
        switch (enemy.type){
            case DummyType:
                checkRadius = 1f;
                break;
            case HorizontalShooterType:
            case VerticalShooterType:
                checkRadius = 1f;
                break;
        }
        if ((_playerPosition - enemy.transform.position).sqrMagnitude <= checkRadius * checkRadius){
            _player.ResetPosition();
        }
    }
    
    private void ReviveEnemy(ref Enemy enemy){
        enemy.gameObject.SetActive(true);
        enemy.hitImmuneCountdown = 0;
        enemy.velocity = Vector3.zero;
    }
    
    private void UpdateDebug(float dt){
        if (Input.GetKeyDown(KeyCode.U)){
            for (int i = 0; i < _enemies.Count; i++){
                var enemy = _enemies[i];
                ReviveEnemy(ref enemy);
            }
        }
    }
    
    [Header("EXPENSIVE BUT WILL USE TILL NO OTHER CHOICE")]
    [SerializeField, Tooltip("Expensive")] private bool drawGizmos;
    private void OnDrawGizmos(){
        if (!drawGizmos){
            return;
        }
    
        Enemy[] enemiesOnScene = FindObjectsOfType<Enemy>();
        
        for (int i = 0; i < enemiesOnScene.Length; i++){
            var enemy = enemiesOnScene[i];
    		Gizmos.matrix = enemy.transform.localToWorldMatrix;

            switch(enemy.type){
                case WindGuyType:
                    Gizmos.color = Color.blue;
                    Vector3 windCenter = Vector3.forward * windGuyLength * 0.5f;
                    Gizmos.DrawWireCube(windCenter, new Vector3(windGuyWidth, windGuyWidth, windGuyLength));
                    break;
            }
        }
    }
}
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using static UnityEngine.Mathf;
using static UnityEngine.Physics;
using static Utils;
using static EnemyType;

public enum EnemyType{
    DummyType,
    ShooterType,
    BlockerType,
    RicocheType
}

public class Dummy{
    public Enemy enemy;
    public Vector3 dodgeStartPosition;
    public float ballDetectRadius = 24f;
    public float dodgeDistance    = 7f;
    public float dodgeTime        = 8f;
    public float dodgeTimer;
    public bool  dodging;
}

public class Shooter{
    public Enemy enemy;
    public float shootCooldown   = 5f;
    public int   burstShootCount = 1;
    public float shootDelay      = 0.02f;
    public float cooldownTimer;
    public float delayTimer;
    public int   shootedCount;
}

public class Blocker{
    public Enemy enemy;
    public Vector3 pivotPosition;
    public float moveDistance = 25;
    public float cycleTime = 5;
    public float cycleProgress;
}

public class Ricoche{
    public Enemy enemy;
    public LineRenderer targetLine;
    public Vector3 pivotPosition;
    public Vector3 orbitAxis;
    public Vector3 orbitPosition;
    public float orbitRadius = 75;
    public float orbitingSpeed = 20;
}

public class EnemiesController : MonoBehaviour{
    
    [Header("Shooter")]
    [SerializeField] private float projectileStartSpeed;
    
    private Transform        _playerTransform;
    private PlayerController _player;
    private Vector3          _playerPosition;
    
    private EnemyProjectile _shooterProjectilePrefab;
    
    private List<EnemyProjectile> _enemyProjectiles = new();
    private List<Dummy>           _dummies          = new();
    private List<Shooter>         _shooters         = new();
    private List<Blocker>         _blockers         = new();
    private List<Ricoche>         _ricoches         = new();
    
    private List<Enemy> _enemies = new();
    
    private void Start(){
        _shooterProjectilePrefab = GetPrefab("ShooterProjectile").GetComponent<EnemyProjectile>();
        
        _playerTransform = FindObjectOfType<PlayerController>().transform;
        _player = _playerTransform.GetComponent<PlayerController>();
    
        var enemiesOnScene = FindObjectsOfType<Enemy>();
        
        for (int i = 0; i < enemiesOnScene.Length; i++){
            switch (enemiesOnScene[i].type){
                case DummyType:
                    _dummies.Add(new Dummy() { enemy = enemiesOnScene[i] });
                    _dummies[_dummies.Count-1].dodgeStartPosition = _dummies[_dummies.Count-1].enemy.transform.position;
                    _dummies[_dummies.Count-1].enemy.index = _dummies.Count-1;
                    break;
                case ShooterType:
                    _shooters.Add(new Shooter() {enemy = enemiesOnScene[i]});
                    _shooters[_shooters.Count-1].cooldownTimer = _shooters[_shooters.Count-1].shootCooldown;
                    _shooters[_shooters.Count-1].enemy.index = _shooters.Count-1;
                    break;
                case BlockerType:
                    _blockers.Add(new Blocker() {enemy = enemiesOnScene[i]});
                    _blockers[_blockers.Count-1].pivotPosition = _blockers[_blockers.Count-1].enemy.transform.position;
                    _blockers[_blockers.Count-1].enemy.index = _blockers.Count-1;
                    break;
                case RicocheType:
                    var ricoche = new Ricoche() {enemy = enemiesOnScene[i]};
                    ricoche.pivotPosition = ricoche.enemy.transform.position;
                    ricoche.enemy.index = _ricoches.Count;
                    
                    ricoche.orbitAxis = ricoche.enemy.transform.up;
                    ricoche.orbitPosition = ricoche.enemy.transform.right * ricoche.orbitRadius;
                    
                    ricoche.targetLine = ricoche.enemy.GetComponent<LineRenderer>();
                    
                    _ricoches.Add(ricoche);
                    break;
            }
            
            _enemies.Add(enemiesOnScene[i]);
        }
    }
    
    private void Update(){
        _playerPosition = _playerTransform.position;
        UpdateShooters();
        UpdateEnemyProjectiles();
        UpdateDummies();
        UpdateBlockers();
        UpdateRicoches();
        
        UpdateDebug();
    }
    
    private void UpdateRicoches(){
        for (int i = 0; i < _ricoches.Count; i++){
            var ricoche = _ricoches[i];
            
            if (!ricoche.enemy.gameObject.activeSelf || EnemyHit(ref ricoche.enemy)){
                continue;
            }
            
            EnemyCountdowns(ref ricoche.enemy);
            
            var ricocheTransform = ricoche.enemy.transform;
            
            ricoche.pivotPosition += ricoche.enemy.velocity * Time.deltaTime;
            ricocheTransform.position += ricoche.enemy.velocity * Time.deltaTime;
            
            ricoche.orbitPosition = Quaternion.AngleAxis(ricoche.orbitingSpeed * Time.deltaTime, ricoche.orbitAxis) * ricoche.orbitPosition;
                        
            ricocheTransform.position = ricoche.pivotPosition + ricoche.orbitPosition;
            
            ricocheTransform.rotation = Quaternion.Slerp(ricocheTransform.rotation,
                                                         Quaternion.LookRotation((_playerPosition - ricocheTransform.position).normalized),
                                                         Time.deltaTime * 5);
                                                         
            Enemy closestEnemy = GetClosestEnemy(ricocheTransform.position, ricoche.enemy.gameObject);
            
            if (closestEnemy){
                ricoche.targetLine.positionCount = 2;
                Vector3 lineTargetPosition = Vector3.Lerp(ricoche.targetLine.GetPosition(1), closestEnemy.transform.position, Time.deltaTime * 20);
                ricoche.targetLine.SetPosition(0, ricocheTransform.position);
                ricoche.targetLine.SetPosition(1, lineTargetPosition);
            } else{
                ricoche.targetLine.positionCount = 0;
            }
                        
            KillPlayerIfNearby(ricoche.enemy);
        }
    }
    
    private void UpdateBlockers(){
        for (int i = 0; i < _blockers.Count; i++){
            var blocker = _blockers[i];
            
            if (!blocker.enemy.gameObject.activeSelf || EnemyHit(ref blocker.enemy)){
                continue;
            }
            
            var blockerTransform = blocker.enemy.transform;
            
            
            EnemyCountdowns(ref blocker.enemy);
            blockerTransform.position += blocker.enemy.velocity * Time.deltaTime;
            blocker.pivotPosition += blocker.enemy.velocity * Time.deltaTime;
            
            //blockerTransform.position += blockerTransform.right * Sin(Time.time) * Time.deltaTime;
            
            blocker.cycleProgress += Time.deltaTime / blocker.cycleTime;
            
            float t = blocker.cycleProgress <= 0.5f ? blocker.cycleProgress * 2 : 1f - (blocker.cycleProgress - 0.5f) * 2;
            
            var targetPosition = blocker.pivotPosition + blockerTransform.right * blocker.moveDistance;
            blockerTransform.position = Vector3.Lerp(blocker.pivotPosition, targetPosition, EaseInOutQuad(t));
            
            if (blocker.cycleProgress >= 1){
                blocker.cycleProgress = 0;
            }
            
            KillPlayerIfNearby(blocker.enemy);
        }
    }
    
    private void EnemyCountdowns(ref Enemy enemy){
        if (enemy.hitImmuneCountdown > 0){
            enemy.hitImmuneCountdown -= Time.deltaTime;
            enemy.hitImmuneCountdown = Clamp(enemy.hitImmuneCountdown, 0, 1);
        }
    }
    
    private bool EnemyHit(ref Enemy enemy){
        if (enemy.justTakeHit){
            enemy.justTakeHit = false;
            enemy.hitImmuneCountdown = 0.1f;
            
            enemy.gameObject.SetActive(false);
            
            return true;
        }
        
        return false;
    }
    
    private void UpdateShooters(){
        for (int i = 0; i < _shooters.Count; i++){
            var shooter = _shooters[i];
            
            if (!shooter.enemy.gameObject.activeSelf){
                continue;
            }
            
            EnemyCountdowns(ref shooter.enemy);
            
            Transform shooterTransform = shooter.enemy.transform;
            
            var vectorToPlayer = (_playerPosition - shooterTransform.position).normalized;
            var horizontalVectorToPlayer = new Vector3(vectorToPlayer.x, 0, vectorToPlayer.z);
            shooter.enemy.transform.rotation = Quaternion.Slerp(shooterTransform.rotation, Quaternion.LookRotation(horizontalVectorToPlayer), Time.deltaTime * 3);
            
            if (EnemyHit(ref shooter.enemy)){            
                continue;
            }
            
            KillPlayerIfNearby(shooter.enemy);
            
            shooter.cooldownTimer -= Time.deltaTime;
            
            if (shooter.cooldownTimer > 0){
                continue;
            }
            
            if (shooter.delayTimer <= 0){
                EnemyProjectile projectile = SpawnEnemyProjectile(ref shooter.enemy, _shooterProjectilePrefab);
                projectile.velocity = vectorToPlayer * projectileStartSpeed;
                shooter.shootedCount++;
                if (shooter.shootedCount < shooter.burstShootCount){
                    shooter.delayTimer = shooter.shootDelay;
                } else{
                    shooter.delayTimer = 0;
                    shooter.cooldownTimer = shooter.shootCooldown;
                    shooter.shootedCount = 0;
                }
            } else{
                shooter.delayTimer -= Time.deltaTime;
            }
        }
    }
    
    private void UpdateEnemyProjectiles(){
        for (int i = 0; i < _enemyProjectiles.Count; i++){
            EnemyProjectile projectile = _enemyProjectiles[i];
            
            if (projectile == null){
                _enemyProjectiles.RemoveAt(i);
                continue;
            }
                
            CalculateEnemyProjectileCollisions(ref projectile);
            
            if (projectile == null){
                _enemyProjectiles.RemoveAt(i);
                continue;
            }
            
            projectile.transform.rotation = Quaternion.LookRotation(projectile.velocity);
            projectile.transform.Translate(projectile.velocity * Time.deltaTime, Space.World);
            
            projectile.lifeTime += Time.deltaTime;
            
            if (projectile.lifeTime >= 10){
                Destroy(projectile.gameObject);
                projectile = null;
                _enemyProjectiles.RemoveAt(i);
            }
        }
    }
    
    private void CalculateEnemyProjectileCollisions(ref EnemyProjectile projectile){
        if (projectile == null){
            return;
        }
    
        var deltaVelocity = projectile.velocity * Time.deltaTime;
        var hits = SphereCastAll(projectile.transform.position, projectile.sphere.radius, projectile.velocity.normalized, deltaVelocity.magnitude, Layers.PlayerHurtBox | Layers.Environment);
        
        for (int i = 0; i < hits.Length; i++){
            var player = hits[i].transform.GetComponentInParent<PlayerController>();
            if (player){
                player.ResetPosition();
            }
            
            Destroy(projectile.gameObject);
            projectile = null;
        }
    }
    
    private EnemyProjectile SpawnEnemyProjectile(ref Enemy enemy, EnemyProjectile projectilePrefab){
        var newProjectile = Instantiate(projectilePrefab, enemy.transform.position + Vector3.up, Quaternion.identity);
        newProjectile.sphere = newProjectile.GetComponent<SphereCollider>();
        _enemyProjectiles.Add(newProjectile);
        
        return newProjectile;
    }
    
    private void UpdateDummies(){
        for (int i = 0; i < _dummies.Count; i++){
            Dummy dummy = _dummies[i];
            
            if (!dummy.enemy.gameObject.activeSelf){
                continue;
            }
            
            EnemyCountdowns(ref dummy.enemy);
            
            if (EnemyHit(ref dummy.enemy)){
                continue;   
            }
            
            Transform dummyTransform = dummy.enemy.transform;
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
            
            if (!dummy.dodging && dummy.dodgeTimer <= 0 && PlayerBallNearby(dummy.dodgeStartPosition, dummy.ballDetectRadius)){
                dummy.dodging = true;
            }
            
            if (dummy.dodging){
                dummy.dodgeTimer += Time.deltaTime;
                var t = dummy.dodgeTimer / dummy.dodgeTime;
                dummyTransform.position = Vector3.LerpUnclamped(dummy.dodgeStartPosition, dummy.dodgeStartPosition + dummyTransform.right * dummy.dodgeDistance, EaseOutElastic(t));
                if (t >= 1){
                    dummy.dodging = false;
                    dummy.dodgeTimer = dummy.dodgeTime;
                }
            } else if (dummy.dodgeTimer > 0){
                dummy.dodgeTimer -= Time.deltaTime;
                var t = 1f - dummy.dodgeTimer / (dummy.dodgeTime);
                dummyTransform.position = Vector3.LerpUnclamped(dummy.dodgeStartPosition + dummyTransform.right * dummy.dodgeDistance, dummy.dodgeStartPosition, EaseInOutQuad(t));
                if (t <= 0){
                    dummy.dodgeTimer = 0;
                }
            }
            
            if (!dummy.dodging && dummy.dodgeTimer <= 0){
                dummyTransform.rotation = Quaternion.Slerp(dummyTransform.rotation, Quaternion.LookRotation((_playerPosition - dummyTransform.position).normalized), Time.deltaTime * 5);

            }
            
            KillPlayerIfNearby(dummy.enemy);
        }
    }
    
    private bool PlayerBallNearby(Vector3 checkPosition, float checkRadius){
        List<PlayerBall> balls = _player.GetBalls();
        
        for (int i = 0; i < balls.Count; i++){
            if (balls[i].speed < 10) continue;
            
            Vector3 ballToEnemy = checkPosition - balls[i].transform.position;
            
            if (ballToEnemy.sqrMagnitude <= checkRadius * checkRadius && Vector3.Dot(ballToEnemy, balls[i].velocity) > 0){
                return true;
            }
        }
        
        return false;
        //return CheckSphere(checkPosition, checkRadius, Layers.PlayerProjectile);
    }
    
    private void KillPlayerIfNearby(Enemy enemy){
        var checkRadius = 1f;
        switch (enemy.type){
            case DummyType:
                checkRadius = 1f;
                break;
            case ShooterType:
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
    
    private void UpdateDebug(){
        if (Input.GetKeyDown(KeyCode.U)){
            for (int i = 0; i < _enemies.Count; i++){
                var enemy = _enemies[i];
                ReviveEnemy(ref enemy);
            }
        }
    }
}
using UnityEngine;
using System.Collections.Generic;
using Array = System.Array;
using UnityEditor;
using static UnityEngine.Mathf;
using static UnityEngine.Physics;
using static Utils;
using static EnemyType;

public enum EnemyType{
    DummyType,
    ShooterType,
    BlockerType,
    RicocheType,
    WindGuyType
}

public class Dummy{
    public Enemy enemy;
    public Vector3 dodgeStartPosition;
    public float ballDetectRadius = 24f;
    public float dodgeDistance    = 7f;
    public float dodgeTime        = 2f;
    public float dodgeTimer;
    public bool  dodging;
}

public class Shooter{
    public Enemy enemy;
    //@HACK for testing
    public Dummy dummyDodgeComponent;
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
    public float cycleProgress = 0.25f;
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

public class WindGuy{
    public Enemy enemy;
    public WindArea windArea;
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
    
    private ParticleSystem _baseDeadManParticles;
    
    private List<EnemyProjectile> _enemyProjectiles = new();
    private List<Dummy>           _dummies          = new();
    private List<Shooter>         _shooters         = new();
    private List<Blocker>         _blockers         = new();
    private List<Ricoche>         _ricoches         = new();
    private List<WindGuy>         _windGuys         = new();
    
    private List<Enemy> _enemies = new();
    
    private void Start(){
        _shooterProjectilePrefab = GetPrefab("ShooterProjectile").GetComponent<EnemyProjectile>();
        
        _baseDeadManParticles = Particles.Instance.GetParticles("BaseDeadManParticles");
        
        _playerTransform = FindObjectOfType<PlayerController>().transform;
        _player = _playerTransform.GetComponent<PlayerController>();
        
        _targetColliders = new Collider[20];
        
    
        var enemiesOnScene = FindObjectsOfType<Enemy>();
        
        for (int i = 0; i < enemiesOnScene.Length; i++){
            enemiesOnScene[i].sphere = enemiesOnScene[i].GetComponent<SphereCollider>();
            switch (enemiesOnScene[i].type){
                case DummyType:
                    _dummies.Add(new Dummy() { enemy = enemiesOnScene[i] });
                    _dummies[_dummies.Count-1].dodgeStartPosition = _dummies[_dummies.Count-1].enemy.transform.position;
                    _dummies[_dummies.Count-1].enemy.index = _dummies.Count-1;
                    break;
                case ShooterType:
                    var shooter = new Shooter() {enemy = enemiesOnScene[i]};
                    shooter.enemy.index = _shooters.Count;
                    shooter.cooldownTimer = shooter.shootCooldown;
                    shooter.dummyDodgeComponent = new Dummy() {enemy = shooter.enemy};
                    shooter.dummyDodgeComponent.dodgeStartPosition = shooter.enemy.transform.position;
                    _shooters.Add(shooter);
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
                case WindGuyType:
                    var windGuy = new WindGuy() {enemy = enemiesOnScene[i]};
                    windGuy.enemy.index = _windGuys.Count;
                    windGuy.windArea = windGuy.enemy.GetComponentInChildren<WindArea>();
                    windGuy.windArea.windPower = windGuyPower;
                    windGuy.windArea.boxCollider.center = windGuy.enemy.transform.forward * windGuyLength * 0.5f;
                    windGuy.windArea.SetColliderSize(new Vector3(windGuyWidth, windGuyWidth, windGuyLength));
                    _windGuys.Add(windGuy);
                    break;
            }
            
            _enemies.Add(enemiesOnScene[i]);
        }
    }
    
    private void Update(){
        _playerPosition = _playerTransform.position;
        UpdateRicoches();
        UpdateShooters();
        UpdateEnemyProjectiles();
        UpdateDummies();
        UpdateBlockers();
        UpdateWindGuys();
        
        UpdateDebug();
    }
    
    private void UpdateWindGuys(){
        for (int i = 0; i < _windGuys.Count; i++){
            var windGuy = _windGuys[i];
            
            if (!windGuy.enemy.gameObject.activeSelf){
                continue;
            }
            
            MoveByVelocity(ref windGuy.enemy);            
            
            if (FlyByKick(ref windGuy.enemy) || EnemyHit(ref windGuy.enemy)){
                continue;
            }

            EnemyCountdowns(ref windGuy.enemy);
            
            var windGuyTransform = windGuy.enemy.transform;
            
            Vector3 windCenter = windGuyTransform.position + windGuyTransform.forward * windGuyLength;
            ClearArray(_targetColliders);
            int overlapCount = OverlapBoxNonAlloc(windCenter, new Vector3(windGuyWidth, windGuyWidth, windGuyLength), _targetColliders, windGuyTransform.rotation, Layers.Player | Layers.PlayerProjectile | Layers.EnemyProjectile | Layers.EnemyHurtBox);
            
            for (int j = 0; j < overlapCount; j++){
                if (_targetColliders[j] == null){
                    continue;
                }
                
                var target = _targetColliders[j];
                
                Enemy otherEnemy = target.GetComponentInParent<Enemy>();
                
                if (otherEnemy && otherEnemy != windGuy.enemy){
                    otherEnemy.velocity += (windGuy.windArea.PowerVector(otherEnemy.transform.position) / otherEnemy.weight) * Time.deltaTime;
                } else if (target.TryGetComponent<PlayerBall>(out var playerBall)){
                    playerBall.velocity += windGuy.windArea.PowerVector(playerBall.transform.position) * Time.deltaTime;
                } else if (target.TryGetComponent<PlayerController>(out var player)){
                    player.playerVelocity += windGuy.windArea.PowerVector(player.transform.position) * Time.deltaTime;
                } 
            }
            
            windGuyTransform.Rotate(Vector3.forward * windGuyPower * 5 * Time.deltaTime); 
        }
    }
    
    private void UpdateRicoches(){
        for (int i = 0; i < _ricoches.Count; i++){
            var ricoche = _ricoches[i];
            
            if (!ricoche.enemy.gameObject.activeSelf){
                continue;
            }
            
            MoveByVelocity(ref ricoche.enemy);
            
            if (FlyByKick(ref ricoche.enemy) || EnemyHit(ref ricoche.enemy)){
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
            
            if (!blocker.enemy.gameObject.activeSelf){
                continue;
            }
            
            var blockerTransform = blocker.enemy.transform;
            
            blocker.pivotPosition += MoveByVelocity(ref blocker.enemy);
            
            if (FlyByKick(ref blocker.enemy) || EnemyHit(ref blocker.enemy)){
                blocker.cycleProgress = 0.25f;
                blocker.pivotPosition = blockerTransform.position;
                continue;
            }
            
            EnemyCountdowns(ref blocker.enemy);
            
            blocker.cycleProgress += Time.deltaTime / blocker.cycleTime;
            
            float t = blocker.cycleProgress <= 0.5f ? blocker.cycleProgress * 2 : 1f - (blocker.cycleProgress - 0.5f) * 2;
            
            var startPosition  = blocker.pivotPosition - blockerTransform.right * blocker.moveDistance * 0.5f;
            var targetPosition = blocker.pivotPosition + blockerTransform.right * blocker.moveDistance * 0.5f;
            blockerTransform.position = Vector3.Lerp(startPosition, targetPosition, EaseInOutQuad(t));
            
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
    
    private Vector3 MoveByVelocity(ref Enemy enemy){
        enemy.transform.position += enemy.velocity * Time.deltaTime;
        enemy.velocity *= 1f - enemy.weight * enemy.weight * Time.deltaTime;
        
        if (enemy.velocity.sqrMagnitude <= EPSILON){
            enemy.velocity = Vector3.zero;
            enemy.takedKick = false;
        }
        
        return enemy.velocity * Time.deltaTime;
    }
    
    private bool EnemyHit(ref Enemy enemy){
        if (enemy.justTakeHit){
            enemy.justTakeHit = false;
            enemy.hitImmuneCountdown = 0.1f;
            
            switch (enemy.type){
                default:
                    Instantiate(_baseDeadManParticles, enemy.transform.position, Quaternion.identity);
                    break;
            }
            
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
            
            UpdateDummy(ref shooter.dummyDodgeComponent);
            
            EnemyCountdowns(ref shooter.enemy);
            
            MoveByVelocity(ref shooter.enemy);
            
            if (FlyByKick(ref shooter.enemy) || EnemyHit(ref shooter.enemy)){            
                continue;
            }
            
            Transform shooterTransform = shooter.enemy.transform;
            
            var vectorToPlayer = (_playerPosition - shooterTransform.position).normalized;
            /*
            var horizontalVectorToPlayer = new Vector3(vectorToPlayer.x, 0, vectorToPlayer.z);
            shooter.enemy.transform.rotation = Quaternion.Slerp(shooterTransform.rotation, Quaternion.LookRotation(horizontalVectorToPlayer), Time.deltaTime * 3);
            */
            
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
    
    private bool FlyByKick(ref Enemy enemy){
        if (!enemy.takedKick){
            return false;
        }
    
        //enemy.transform.position += enemy.velocity * Time.deltaTime;
        //enemy.velocity *= 1f - enemy.weight * enemy.weight * Time.deltaTime;
        
        ClearArray(_targetColliders);
        int collidedCount = OverlapSphereNonAlloc(enemy.transform.position, enemy.sphere.radius, _targetColliders, Layers.Environment | Layers.EnemyHurtBox);
        
        for (int i = 0; i < collidedCount; i++){
            Enemy otherEnemy = _targetColliders[i].GetComponentInParent<Enemy>();
            
            if (otherEnemy == enemy) continue;
            
            if (otherEnemy){
                enemy.TakeHit();
                otherEnemy.TakeHit();
                otherEnemy.TakeKick(enemy.velocity * 2);
            } else{
                enemy.TakeHit();
            }
        }
        
        return true;
    }
    
    private void UpdateDummy(ref Dummy dummy){
        if (!dummy.enemy.gameObject.activeSelf){
            return;
        }
        
        EnemyCountdowns(ref dummy.enemy);
        
        dummy.dodgeStartPosition += MoveByVelocity(ref dummy.enemy);
        
        if (FlyByKick(ref dummy.enemy) || EnemyHit(ref dummy.enemy)){
            dummy.dodging = false;
            dummy.dodgeTimer = 0;
            dummy.dodgeStartPosition = dummy.enemy.transform.position;
            return;   
        }
        
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
    
    private void UpdateDummies(){
        for (int i = 0; i < _dummies.Count; i++){
            Dummy dummy = _dummies[i];
                        
            UpdateDummy(ref dummy);
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
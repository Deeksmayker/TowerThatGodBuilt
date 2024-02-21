using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.Mathf;
using static UnityEngine.Physics;
using static Utils;
using static EnemyType;

public enum EnemyType{
    DummyType,
    ShooterType
}

public class Dummy{
    public Enemy enemy;
    public Vector3 dodgeStartPosition;
    public float ballDetectRadius = 12f;
    public float dodgeDistance    = 7f;
    public float dodgeTime        = 2f;
    public float dodgeTimer;
    public bool  dodging;
}

public class Shooter{
    public Enemy enemy;
    public float shootCooldown   = 3f;
    public int   burstShootCount = 10;
    public float shootDelay      = 0.1f;
    public float cooldownTimer;
    public float delayTimer;
    public int   shootedCount;
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
    
    private void Start(){
        _shooterProjectilePrefab = GetPrefab("ShooterProjectile").GetComponent<EnemyProjectile>();
    
        _playerTransform = FindObjectOfType<PlayerController>().transform;
        _player = _playerTransform.GetComponent<PlayerController>();
    
        var enemies = FindObjectsOfType<Enemy>();
        
        for (int i = 0; i < enemies.Length; i++){
            switch (enemies[i].type){
                case DummyType:
                    _dummies.Add(new Dummy() { enemy = enemies[i] });
                    _dummies[_dummies.Count-1].dodgeStartPosition = _dummies[_dummies.Count-1].enemy.transform.position;
                    break;
                case ShooterType:
                    _shooters.Add(new Shooter() {enemy = enemies[i]});
                    _shooters[_shooters.Count-1].cooldownTimer = _shooters[_shooters.Count-1].shootCooldown;
                    break;
            }
        }
    }
    
    private void Update(){
        _playerPosition = _playerTransform.position;
        UpdateShooters();
        UpdateEnemyProjectiles();
        UpdateDummies();
    }
    
    private void UpdateShooters(){
        for (int i = 0; i < _shooters.Count; i++){
            var shooter = _shooters[i];
            Transform shooterTransform = shooter.enemy.transform;
            
            var vectorToPlayer = (_playerPosition - shooterTransform.position).normalized;
            var horizontalVectorToPlayer = new Vector3(vectorToPlayer.x, 0, vectorToPlayer.z);
            shooter.enemy.transform.rotation = Quaternion.Slerp(shooterTransform.rotation, Quaternion.LookRotation(horizontalVectorToPlayer), Time.deltaTime * 3);
            
            if (shooter.enemy.justTakeHit){
                shooter.enemy.justTakeHit = false;
                Destroy(shooter.enemy.gameObject);
                _shooters.RemoveAt(i);
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
            Transform dummyTransform = dummy.enemy.transform;
            if (dummy.enemy.justTakeHit){          
                dummy.enemy.justTakeHit = false;  
                
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
        return CheckSphere(checkPosition, checkRadius, Layers.PlayerProjectile);
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
}

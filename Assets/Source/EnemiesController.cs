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
    public float shootCooldown = 3f;
    public float shootCount = 3;
    public float shootDelay = 0.3f;
    public float cooldownTimer;
    public float delayTimer;
}


public class EnemiesController : MonoBehaviour{
    
    [Header("Shooter")]
    
    private Transform        _playerTransform;
    private PlayerController _player;
    private Vector3          _playerPosition;
    
    private EnemyProjectile _shooterProjectilePrefab;

    private List<EnemyProjectile> _enemyProjectiles = new();
    private List<Dummy>           _dummies          = new();
    private List<Shooter>         _shooters         = new();
    
    private void Start(){
        //_shooterProjectilePrefab = GetPrefab("ShooterProjectile").GetComponent<EnemyProjectile>();
    
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
                    break;
            }
        }
    }
    
    private void Update(){
        _playerPosition = _playerTransform.position;
        UpdateDummies();
    }
    
    private void UpdateEnemyProjectiles(){
        for (int i = 0; i < _enemyProjectiles.Count; i++){
            EnemyProjectile projectile = _enemyProjectiles[i];
            
        }
    }
    
    private void UpdateDummies(){
        for (int i = 0; i < _dummies.Count; i++){
            Dummy dummy = _dummies[i];
            if (dummy.enemy.justTakeHit){          
                dummy.enemy.justTakeHit = false;  
                
                var wishPosition = Random.onUnitSphere * 20;
                wishPosition.y = Abs(wishPosition.y) * 0.5f;
                dummy.enemy.transform.position = wishPosition;
                
                dummy.dodging = false;
                dummy.dodgeTimer = 0;
                dummy.dodgeStartPosition = dummy.enemy.transform.position;
            }
            
            if (!dummy.dodging && dummy.dodgeTimer <= 0 && PlayerBallNearby(dummy.dodgeStartPosition, dummy.ballDetectRadius)){
                dummy.dodging = true;
            }
            
            if (dummy.dodging){
                dummy.dodgeTimer += Time.deltaTime;
                var t = dummy.dodgeTimer / dummy.dodgeTime;
                dummy.enemy.transform.position = Vector3.LerpUnclamped(dummy.dodgeStartPosition, dummy.dodgeStartPosition + dummy.enemy.transform.right * dummy.dodgeDistance, EaseOutElastic(t));
                if (t >= 1){
                    dummy.dodging = false;
                    dummy.dodgeTimer = dummy.dodgeTime;
                }
            } else if (dummy.dodgeTimer > 0){
                dummy.dodgeTimer -= Time.deltaTime;
                var t = 1f - dummy.dodgeTimer / (dummy.dodgeTime);
                dummy.enemy.transform.position = Vector3.LerpUnclamped(dummy.dodgeStartPosition + dummy.enemy.transform.right * dummy.dodgeDistance, dummy.dodgeStartPosition, EaseInOutQuad(t));
                if (t <= 0){
                    dummy.dodgeTimer = 0;
                }
            }
            
            if (!dummy.dodging && dummy.dodgeTimer <= 0){
                dummy.enemy.transform.rotation = Quaternion.Slerp(dummy.enemy.transform.rotation, Quaternion.LookRotation((_playerPosition - dummy.enemy.transform.position).normalized), Time.deltaTime * 5);

            }
        }
    }
    
    private bool PlayerBallNearby(Vector3 checkPosition, float checkRadius){
        return CheckSphere(checkPosition, checkRadius, Layers.PlayerProjectile);
    }
}

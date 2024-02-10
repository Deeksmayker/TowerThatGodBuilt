using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.Mathf;
using static UnityEngine.Physics;
using static Utils;
using static EnemyType;

public enum EnemyType{
    DummyType
}

public class Dummy{
    public Enemy enemy;
    public Vector3 startPosition;
    public float ballDetectRadius = 10f;
    public float dodgeDistance    = 10f;
    public float dodgeTime        = 1.5f;
    public float dodgeReturnDelay = 1f;
    public float dodgeTimer;
    public bool  dodging;
}


public class EnemiesController : MonoBehaviour{
    
    private Transform _playerTransform;
    private Vector3   _playerPosition;

    private List<Dummy> _dummies = new();
    
    private void Start(){
        _playerTransform = FindObjectOfType<PlayerController>().transform;
    
        var enemies = FindObjectsOfType<Enemy>();
        
        for (int i = 0; i < enemies.Length; i++){
            switch (enemies[i].type){
                case DummyType:
                    _dummies.Add(new Dummy() { enemy = enemies[i] });
                    _dummies[_dummies.Count-1].startPosition = _dummies[_dummies.Count-1].enemy.transform.position;
                    break;
            }
        }
    }
    
    private void Update(){
        _playerPosition = _playerTransform.position;
        UpdateDummies();
    }
    
    private void UpdateDummies(){
        for (int i = 0; i < _dummies.Count; i++){
            Dummy dummy = _dummies[i];
            if (dummy.enemy.justTakeHit){          
                dummy.enemy.justTakeHit = false;  
                
                var wishPosition = Random.onUnitSphere * 20;
                wishPosition.y = Abs(wishPosition.y) * 0.5f;
                dummy.enemy.transform.position = wishPosition;
                dummy.enemy.transform.rotation = Quaternion.LookRotation((_playerPosition - dummy.enemy.transform.position).normalized);
                
                dummy.dodging = false;
                dummy.dodgeTimer = 0;
                dummy.startPosition = dummy.enemy.transform.position;
            }
            
            if (!dummy.dodging && dummy.dodgeTimer <= 0 && PlayerBallNearby(dummy.startPosition, dummy.ballDetectRadius)){
                dummy.dodging = true;
            }
            
            if (dummy.dodging){
                dummy.dodgeTimer += Time.deltaTime;
                var t = dummy.dodgeTimer / dummy.dodgeTime;
                dummy.enemy.transform.position = Vector3.Lerp(dummy.startPosition, dummy.startPosition + dummy.enemy.transform.right * dummy.dodgeDistance, EaseOutQuint(t));
                if (t >= 1){
                    dummy.dodging = false;
                    dummy.dodgeTimer = dummy.dodgeTime;
                }
            } else if (dummy.dodgeTimer > 0){
                dummy.dodgeTimer -= Time.deltaTime;
                var t = 1f - dummy.dodgeTimer / (dummy.dodgeTime);
                dummy.enemy.transform.position = Vector3.Lerp(dummy.startPosition + dummy.enemy.transform.right * dummy.dodgeDistance, dummy.startPosition, EaseInOutQuad(t));
                if (t <= 0){
                    dummy.dodgeTimer = 0;
                }
            }
        }
    }
    
    private bool PlayerBallNearby(Vector3 checkPosition, float checkRadius){
        return CheckSphere(checkPosition, checkRadius, Layers.PlayerProjectile);
    }
}

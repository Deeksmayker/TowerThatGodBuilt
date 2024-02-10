using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.Mathf;
using static UnityEngine.Physics;
using static EnemyType;

public enum EnemyType{
    DummyType
}

public class Dummy{
    public Enemy enemy;
    public Vector3 startPosition;
}


public class EnemiesController : MonoBehaviour{
    private List<Dummy> _dummies = new();
    
    private void Start(){
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
        UpdateDummies();
    }
    
    private void UpdateDummies(){
        for (int i = 0; i < _dummies.Count; i++){
            if (_dummies[i].enemy.justTakeHit){          
                _dummies[i].enemy.justTakeHit = false;  
                
                var wishPosition = Random.onUnitSphere * 20;
                wishPosition.y = Abs(wishPosition.y) * 0.5f;
                _dummies[i].enemy.transform.position = _dummies[i].startPosition + wishPosition;
            }
        }
    }
}

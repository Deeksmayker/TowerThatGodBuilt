using UnityEngine;

public class EnemySpawnerAtPoint : MonoBehaviour{
    [SerializeField] private EnemyType type;
    
    private void Start(){
        FindObjectOfType<EnemiesController>().SpawnEnemy(type, transform.position, transform.rotation);
    }
}

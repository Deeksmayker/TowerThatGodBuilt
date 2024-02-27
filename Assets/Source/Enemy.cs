using UnityEngine;

public class Enemy : MonoBehaviour{
    public EnemyType type;
    public Vector3 velocity;
    public bool justTakeHit;

    public void TakeHit(Collider whoTookThatHit){
        justTakeHit = true;
    }
}

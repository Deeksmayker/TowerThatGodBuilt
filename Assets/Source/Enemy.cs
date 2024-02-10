using UnityEngine;

public class Enemy : MonoBehaviour{
    public EnemyType type;
    public bool justTakeHit;

    public void TakeHit(Collider whoTookThatHit){
        justTakeHit = true;
    }
}

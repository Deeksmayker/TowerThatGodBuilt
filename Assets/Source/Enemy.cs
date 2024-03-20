using UnityEngine;

public class Enemy : MonoBehaviour{
    public EnemyType type;
    public Vector3 velocity;
    public float hitImmuneCountdown;
    public bool justTakeHit;

    public void TakeHit(Collider whoTookThatHit){
        justTakeHit = true;
    }
}

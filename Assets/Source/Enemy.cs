using UnityEngine;

public class Enemy : MonoBehaviour{
    public EnemyType type;
    public SphereCollider sphere;
    public int index;
    public Vector3 velocity;
    public float hitImmuneCountdown;
    public float weight = 1;
    public bool takedKick;
    public bool justTakeHit;

    public void TakeHit(Collider whoTookThatHit = null){
        justTakeHit = true;
        takedKick = false;
    }
    
    public void TakeKick(Vector3 powerVector){
        takedKick = true;
        transform.rotation = Quaternion.LookRotation(powerVector);
        velocity += powerVector / weight;
    }
}

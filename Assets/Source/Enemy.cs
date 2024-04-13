using UnityEngine;

public enum DodgeDirection{
    Horizontal,
    Vertical
}

public class Enemy : MonoBehaviour{
    public EnemyType type;
    public ParticleSystem kickTrailParticles;
    public SphereCollider sphere;
    public int index;
    public Vector3 velocity;
    public float hitImmuneCountdown;
    public float kickImmuneCountdown;
    public float effectsCooldown;
    public float weight = 1;
    public bool takedKick;
    public bool justTakeHit;
    
    [Header("TEMP")]
    public DodgeDirection dodgeDirection;

    public void TakeHit(Collider whoTookThatHit = null){
        justTakeHit = true;
        takedKick = false;
        hitImmuneCountdown = 0.1f;
        effectsCooldown = 0.1f;
    }
    
    public void TakeKick(Vector3 powerVector){
        if (kickImmuneCountdown > 0){
            return;
        }
    
        if (powerVector.sqrMagnitude <= Utils.EPSILON){
            powerVector = Vector3.down;
        }
    
        effectsCooldown = 0.1f;
        kickImmuneCountdown = 0.1f;
        
        takedKick = true;
        transform.rotation = Quaternion.LookRotation(powerVector);
        velocity += powerVector / weight;
        kickTrailParticles.Play();
    }
}

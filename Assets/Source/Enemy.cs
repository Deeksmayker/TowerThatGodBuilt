using Source.Utils;
using UnityEngine;
using static UnityEngine.Mathf;

public enum DodgeDirection{
    Horizontal,
    Vertical
}

public class Enemy : MonoBehaviour{
    public Rope leftLegRope;

    public EnemyType type;
    public ParticleSystem kickTrailParticles;
    public SphereCollider sphere;
    public int index = -1;
    public bool initialized;
    public Vector3 velocity;
    public Vector3 angularVelocity;
    public float timeInKickFlight;
    public float hitImmuneCountdown;
    public float kickImmuneCountdown;
    public float effectsCooldown;
    public float weight = 1;
    public bool takedKick;
    public bool justTakeHit;
    
    public bool dead;
    
    public int variation = 1;
    
    [Header("TEMP")]
    public DodgeDirection dodgeDirection;

    public void TakeHit(Collider whoTookThatHit = null){
        justTakeHit = true;
        takedKick = false;
        hitImmuneCountdown = 0.1f;
        effectsCooldown = 0.1f;
    }
    
    public void TakeKick(Vector3 powerVector, Vector3 impactPoint){
        if (kickImmuneCountdown > 0){
            return;
        }
    
        if (powerVector.sqrMagnitude <= Utils.EPSILON){
            powerVector = Vector3.down;
        }
    
        effectsCooldown = 0.1f;
        kickImmuneCountdown = 0.1f;
        
        takedKick = true;
        timeInKickFlight = 0;
        //transform.rotation = Quaternion.LookRotation(powerVector);
        velocity += powerVector / weight;
        
        float impactPower = powerVector.magnitude;
        Vector3 vecToImpactPoint = impactPoint - (transform.position + transform.up);
        vecToImpactPoint = new Vector3(Clamp(vecToImpactPoint.x, -5f, 5f), Clamp(vecToImpactPoint.y, -5f, 5f), Clamp(vecToImpactPoint.z, -5f, 5f));
        
        angularVelocity.x -= vecToImpactPoint.y * impactPower / (weight * 2);
        angularVelocity.y += vecToImpactPoint.x * impactPower / (weight * 2);
        
        kickTrailParticles.Play();
        
        var rb =  gameObject.AddComponent<Rigidbody>();
        
        rb.velocity = powerVector * 2;
    }
}

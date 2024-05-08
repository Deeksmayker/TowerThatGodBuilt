using UnityEngine;

public class PlayerBall : MonoBehaviour{
    public ParticleSystem chargedParticles;
    public int index = -1;
    public int bounceCount;
    public int groundBounceCount;
    public float lifeTime;
    public SphereCollider sphere;
    public bool imaginary;
    public bool ricocheCharged;
    public int chargedBounceCount;
    public bool hitEnemy;
    public bool inHold;
    public bool sleeping;
    public float speed;
    public Vector3 velocity;
    public Vector3 velocityNormalized;
    public Vector3 velocityUp;
    public Vector3 velocityRight;
    public Vector3 angularVelocity;
}    

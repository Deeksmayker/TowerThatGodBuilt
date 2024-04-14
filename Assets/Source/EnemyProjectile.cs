using UnityEngine; 

public class EnemyProjectile : MonoBehaviour{
    public int index;
    public Vector3 velocity;    
    public SphereCollider sphere;
    public float lifeTime;
    public float slowingLifetime = 0f;
}

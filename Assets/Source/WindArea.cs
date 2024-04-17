using UnityEngine;
using static UnityEngine.Mathf;

public class WindArea : MonoBehaviour{
    public float windPower;
    public BoxCollider boxCollider;
    
    public ParticleSystem particles;
    
    private void Awake(){
        boxCollider = GetComponent<BoxCollider>();
        particles = GetComponentInChildren<ParticleSystem>();
    }
    
    public void SetColliderSize(Vector3 size){
        boxCollider.size = size;
        Particles.Instance.SetScale(ref particles, new Vector3(boxCollider.size.x * 2.6f, boxCollider.size.y * 2, 1));      
    }
    
    public Vector3 PowerVector(Vector3 atPosition){
        float powerProgress = 1f - Clamp01(Vector3.Distance(transform.position, atPosition) / (boxCollider.size.z * 2));
        return transform.forward * windPower * powerProgress;
    }
}

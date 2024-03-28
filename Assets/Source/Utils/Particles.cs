using UnityEngine;

public class Particles : MonoBehaviour{
    public static Particles Instance;
    
    private Transform _particlesHolder;
    
    public void Awake(){
        if (Instance && Instance != this){
            Instance = null;
            return;
        }
        
        Instance = this;
        _particlesHolder = new GameObject("ParticlesHolder").transform;
    }
    
    public ParticleSystem GetParticles(string name){
        var particles = (Resources.Load("Particles/" + name) as GameObject).GetComponent<ParticleSystem>();
        if (particles == null) Debug.LogError("Wrong particle name - " + name);
        return particles;
    }
    
    public void SpawnAndPlayParticles(ParticleSystem particles, Vector3 position){
        var newParticles = Instantiate(particles, position, Quaternion.identity);
        newParticles.transform.parent = _particlesHolder;
    }
}

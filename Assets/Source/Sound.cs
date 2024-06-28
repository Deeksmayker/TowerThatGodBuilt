using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Sound : MonoBehaviour{
    public static Sound Instance;
    
    private AudioSource _baseAudioSource, _base3DAudioSource;
    
    private List<ASource> _sources = new();
    
    private void Awake(){
        if (Instance && Instance != this){
            Destroy(Instance);
            Instance = null;
        }
        
        _baseAudioSource = (Resources.Load(ResPath.Audio + "BaseAudioSource") as GameObject).GetComponent<AudioSource>();
        _base3DAudioSource = (Resources.Load(ResPath.Audio + "Base3DAudioSource") as GameObject).GetComponent<AudioSource>();
        Instance = this;
    }
    
    private void Update(){
        for (int i = 0; i < _sources.Count; i++){
            _sources[i].lifeTime -= Time.deltaTime;
            if (_sources[i].lifeTime <= 0){
                Destroy(_sources[i].source.gameObject);
                _sources.RemoveAt(i);
            }
        }
    }
    
    public void Play(AudioClip clip, float volume = 1, float pitch = 1, float lifeTime1 = 1){
        var source1 = Instantiate(_baseAudioSource, transform);
        source1.volume = Random.Range(volume - 0.1f, volume + 0.1f);
        source1.pitch = Random.Range(pitch - 0.1f, pitch + 0.1f);
        source1.clip = clip;
        source1.Play();
        _sources.Add(new ASource(){source = source1, lifeTime = lifeTime1});
    }
    
    public void AtPos(AudioClip clip, Vector3 position, float volume = 1, float pitch = 1, float lifeTime1 = 1){
        var source1 = Instantiate(_base3DAudioSource, position, Quaternion.identity);
        source1.volume = Random.Range(volume - 0.1f, volume + 0.1f);
        source1.pitch = Random.Range(pitch - 0.1f, pitch + 0.1f);
        source1.clip = clip;
        source1.Play();
        _sources.Add(new ASource(){source = source1, lifeTime = lifeTime1});
    }
    
    public void AtPos(AudioClip[] clips, Vector3 position, float volume = 1, float pitch = 1, float lifeTime1 = 1){
        AtPos(clips[Random.Range(0, clips.Length)], position, volume, pitch, lifeTime1);
    }

    public AudioClip Clip(string name){
        var clip = Resources.Load(ResPath.Audio + name) as AudioClip;
        if (clip == null) Debug.LogError("wrong clip name");
        return clip;
    }
    
    public AudioClip[] AllClips(string name){
        var clips = Resources.LoadAll(ResPath.Audio)
        .Where(o => o.name.Contains(name))
        .Select(o => o as AudioClip)
        .ToArray();
        if (clips.Length == 0) Debug.LogError("no such clips alo");
        return clips;
    }
}

public class ASource{
    public AudioSource source;
    public float lifeTime;
}

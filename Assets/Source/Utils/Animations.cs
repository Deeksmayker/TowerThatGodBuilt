using UnityEngine;
using System.Collections.Generic;

public class Animations : MonoBehaviour{
    public static Animations Instance;
    
    private List<MaterialTask> _materialTasks = new(); 
    
    private MaterialPropertyBlock _propertyBlock;
    
    private void Awake(){
        if (Instance != null && Instance != this){
            Instance = null;
        }
        
        Instance = this;
        _propertyBlock = new MaterialPropertyBlock();
    }
    
    private void Update(){
        for (int i = 0; i < _materialTasks.Count; i++){
            var task = _materialTasks[i];
            if (task.completed) continue;
            
            task.elapsed += Time.deltaTime;
            
            if (task.elapsed >= task.duration){
                ChangeMeshRenderersColor(task.renderers, task.originalColor);
                task.completed = true;
            }
        }
    }
    
    public void ChangeMaterialColor(GameObject targetObject, Color color, float duration){
        var existedTask = MaterialTaskExist(targetObject);
        if (existedTask == null){
            existedTask = new MaterialTask();
            existedTask.targetObject  = targetObject;
            existedTask.renderers     = targetObject.GetComponentsInChildren<MeshRenderer>();
            existedTask.originalColor = existedTask.renderers[0].material.GetColor("_EmissionColor");
        } 
        existedTask.targetColor = color;
        existedTask.duration    = duration;
        existedTask.elapsed     = 0;
        existedTask.completed   = false;
        
        ChangeMeshRenderersColor(existedTask.renderers, color);
        
        _materialTasks.Add(existedTask);
    }
    
    public void ChangeMeshRenderersColor(MeshRenderer[] renderers, Color newColor){
        _propertyBlock.SetColor("_EmissionColor", newColor);
        for (int i = 0; i < renderers.Length; i++){
            renderers[i].SetPropertyBlock(_propertyBlock);
        }
    }
    
    private MaterialTask MaterialTaskExist(GameObject targetObject){
        for (int i = 0; i < _materialTasks.Count; i++){
            if (_materialTasks[i].targetObject == targetObject){
                return _materialTasks[i];
            }
        }
        
        return null;
    }
}

public class MaterialTask{
    public GameObject targetObject;
    public MeshRenderer[] renderers;
    public Color originalColor;
    public Color targetColor;
    public float duration;
    public float elapsed;
    public bool completed;
}

using UnityEngine;
using System.Collections.Generic;
using System;

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
            if (!task.targetObject){
                _materialTasks.RemoveAt(i);
                continue;
            }
            if (task.completed) continue;
            
            task.elapsed += Time.deltaTime;
            
            if (task.elapsed >= task.duration){
                ChangeMeshRenderersColor(task.renderers, task.originalColor, task.originalEmissionColor);
                task.completed = true;
            }
        }
    }
    
    public void ChangeMaterialColor(GameObject targetObject, Color color, float duration){
        var task = MaterialTaskExist(targetObject);
        bool taskIsNew = false;
        if (task == null){
            taskIsNew = true;
            task = new MaterialTask();
            task.targetObject  = targetObject;
            task.renderers     = targetObject.GetComponentsInChildren<MeshRenderer>();
            task.originalColor = task.renderers[0].material.GetColor("_BaseColor");
            task.originalEmissionColor = task.renderers[0].material.GetColor("_EmissionColor");
        } 
        task.targetColor = color;
        task.duration    = duration;
        task.elapsed     = 0;
        task.completed   = false;
        
        ChangeMeshRenderersColor(task.renderers, color, color);
        
        if (taskIsNew){
            _materialTasks.Add(task);
        }
    }
    
    public void ChangeMeshRenderersColor(MeshRenderer[] renderers, Color newColor, Color newEmissionColor){
        _propertyBlock.SetColor("_EmissionColor", newEmissionColor);
        _propertyBlock.SetColor("_BaseColor", newColor);
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

[Serializable]
public class MaterialTask{
    public GameObject targetObject;
    public MeshRenderer[] renderers;
    public Color originalColor, originalEmissionColor;
    public Color targetColor;
    public float duration;
    public float elapsed;
    public bool completed;
}

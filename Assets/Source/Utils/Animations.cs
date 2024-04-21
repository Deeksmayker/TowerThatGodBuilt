using UnityEngine;
using System.Collections.Generic;
using System;

public class Animations : MonoBehaviour{
    public static Animations Instance;
    
    private List<MaterialTask> _materialTasks = new(); 
    private List<ScaleTask> _scaleTasks = new();
    private List<MoveTask> _moveTasks = new();
    
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
            MaterialTask task = _materialTasks[i];
            
            if (!task.targetObject.activeSelf){
                continue;
            }
            
            if (!task.targetObject){
                _materialTasks.RemoveAt(i);
                continue;
            }
            if (task.completed) continue;
            
            if (task.elapsed >= task.duration){
                ChangeMeshRenderersColors(task.renderers, task.originalColors, task.originalEmissionColors);
                task.completed = true;
            }
            
            task.elapsed += Time.deltaTime;
        }
        
        for (int i = 0; i < _scaleTasks.Count; i++){
            ScaleTask task = _scaleTasks[i];
            if (!task.targetObject){
                _scaleTasks.RemoveAt(i);
                continue;
            }
            
            if (task.goingBackwards){
                task.elapsed -= Time.deltaTime;
            } else{
                task.elapsed += Time.deltaTime;
            }
            
            float t = task.goingBackwards ? task.elapsed / task.backwardsDuration : task.elapsed / task.duration;
            task.targetObject.transform.localScale = Vector3.Lerp(task.startScale, task.targetScale, task.easeFunction.Invoke(t));
            
            if (!task.goingBackwards && task.elapsed >= task.duration){
                if (task.backAfterCompleted){
                    task.elapsed *= task.backwardsDuration / task.duration;
                    task.goingBackwards = true;
                } else{
                    _scaleTasks.RemoveAt(i);
                }
            }
            
            if (task.goingBackwards && task.elapsed <= 0){
                _scaleTasks.RemoveAt(i);
            }
        }
        
        for (int i = 0; i < _moveTasks.Count; i++){
            MoveTask task = _moveTasks[i];
            if (!task.targetObject){
                _moveTasks.RemoveAt(i);
                continue;
            }
            
            if (task.goingBackwards){
                task.elapsed -= Time.deltaTime;
            } else{
                task.elapsed += Time.deltaTime;
            }
            
            float t = task.goingBackwards ? task.elapsed / task.backwardsDuration : task.elapsed / task.duration;
            task.targetObject.transform.position = Vector3.Lerp(task.startPos, task.targetPos, task.easeFunction.Invoke(t));
            
            if (!task.goingBackwards && task.elapsed >= task.duration){
                if (task.backAfterCompleted){
                    task.elapsed *= task.backwardsDuration / task.duration;
                    task.goingBackwards = true;
                } else{
                    _moveTasks.RemoveAt(i);
                }
            }
            
            if (task.goingBackwards && task.elapsed <= 0){
                _moveTasks.RemoveAt(i);
            }
        }
    }
    
    public void MoveObject(ref GameObject targetObject, Vector3 targetPos, float duration, bool backAfterCompleted, float backwardsDuration, Func<float, float> easeFunction){
        MoveTask task = MoveTaskExist(targetObject);
        bool taskIsNew = false;
        if (task == null){
            taskIsNew = true;
            task = new MoveTask();
            task.targetObject = targetObject;
            task.startPos = targetObject.transform.position;
        }
        
        task.targetPos = targetPos;
        task.duration = duration;
        task.elapsed = 0;
        task.backAfterCompleted = backAfterCompleted;
        task.backwardsDuration = backwardsDuration;
        task.goingBackwards = false;
        task.easeFunction = easeFunction;
        
        if (taskIsNew){
            _moveTasks.Add(task);
        }
    }

    
    public void ChangeScale(GameObject targetObject, Vector3 targetScale, float duration, bool backAfterCompleted, float backwardsDuration, Func<float, float> easeFunction){
        var task = ScaleTaskExist(targetObject);
        bool taskIsNew = false;
        if (task == null){
            taskIsNew = true;
            task = new ScaleTask();
            task.targetObject = targetObject;
            task.startScale = targetObject.transform.localScale;
        }
        
        task.targetScale = targetScale;
        task.duration = duration;
        task.elapsed = 0;
        task.backAfterCompleted = backAfterCompleted;
        task.backwardsDuration = backwardsDuration;
        task.goingBackwards = false;
        task.easeFunction = easeFunction;
        
        if (taskIsNew){
            _scaleTasks.Add(task);
        }
    }
    
    private ScaleTask ScaleTaskExist(GameObject targetObject){
        for (int i = 0; i < _scaleTasks.Count; i++){
            if (_scaleTasks[i].targetObject == targetObject){
                return _scaleTasks[i];
            }
        }
        
        return null;
    }
    
    private MoveTask MoveTaskExist(GameObject targetObject){
        for (int i = 0; i < _moveTasks.Count; i++){
            if (_moveTasks[i].targetObject == targetObject){
                return _moveTasks[i];
            }
        }
        
        return null;
    }
    
    public void ChangeMaterialColor(ref GameObject targetObject, Color color, float duration){
        var task = MaterialTaskExist(ref targetObject);
        bool taskIsNew = false;
        if (task == null){
            taskIsNew = true;
            task = new MaterialTask();
            task.targetObject  = targetObject;
            task.renderers     = targetObject.GetComponentsInChildren<MeshRenderer>();
            task.originalColors         = new Color[task.renderers.Length];
            task.originalEmissionColors = new Color[task.renderers.Length];
            for (int i = 0; i < task.originalColors.Length; i++){
                task.originalColors[i] = task.renderers[i].material.GetColor("_BaseColor");
            }
            for (int i = 0; i < task.originalEmissionColors.Length; i++){
                task.originalEmissionColors[i] = task.renderers[i].material.GetColor("_EmissionColor");
            }
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
    
    public void ChangeMeshRenderersColors(MeshRenderer[] renderers, Color[] newColors, Color[] newEmissionColors){
        Debug.Assert(renderers.Length == newColors.Length);
        Debug.Assert(renderers.Length == newEmissionColors.Length);
        for (int i = 0; i < renderers.Length; i++){
            _propertyBlock.SetColor("_EmissionColor", newEmissionColors[i]);
            _propertyBlock.SetColor("_BaseColor", newColors[i]);
            renderers[i].SetPropertyBlock(_propertyBlock);
        }
    }
    
    private MaterialTask MaterialTaskExist(ref GameObject targetObject){
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
    public Color[] originalColors, originalEmissionColors;
    public Color targetColor;
    public float duration;
    public float elapsed;
    public bool completed;
}

[Serializable]
public class ScaleTask{
    public GameObject targetObject;
    public Vector3 targetScale;
    public Vector3 startScale;
    public float duration;
    public float backwardsDuration;
    public bool backAfterCompleted;
    public bool goingBackwards;
    public Func<float, float> easeFunction;
    public float elapsed;
}

[Serializable]
public class MoveTask{
    public GameObject targetObject;
    public Vector3 targetPos;
    public Vector3 startPos;
    public float duration;
    public float backwardsDuration;
    public bool backAfterCompleted;
    public bool goingBackwards;
    public Func<float, float> easeFunction;
    public float elapsed;
}


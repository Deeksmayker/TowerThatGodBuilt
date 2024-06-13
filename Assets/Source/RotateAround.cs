using UnityEngine;

public class RotateAround : MonoBehaviour{
    public Vector3 axis = Vector3.right;
    public float speed = 10f;
    
    public Material skyMaterial;
    
    private float _timer;
    
    private void Update(){
        transform.Rotate(axis * speed * Time.deltaTime);
        
        if (!skyMaterial){
            return;
        }
        
        _timer += Time.deltaTime;
        
        float flashTime = 10f;
        
        if (_timer > flashTime){
            if (_timer < flashTime + 1.5f){
                float t = (_timer - flashTime) / 1.5f;
                skyMaterial.SetFloat("_SunBlendPower", Mathf.Lerp(1200, -2000f, Mathf.Sqrt(t)));
            } else{
                float t = (_timer - flashTime + 1.5f) / 1.5f;
                skyMaterial.SetFloat("_SunBlendPower", Mathf.Lerp(-2000f, 1200f, Mathf.Sqrt(t)));
                
                if (t >= 1){
                    _timer = 0;
                }
            }
        }
    }
}

using UnityEngine;

public class RotateAround : MonoBehaviour{
    public Vector3 axis = Vector3.right;
    public float speed = 10f;
    
    private void Update(){
        transform.Rotate(axis * speed * Time.deltaTime);
    }
}

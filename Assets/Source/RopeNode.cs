using UnityEngine; 

public class RopeNode : MonoBehaviour{
    public bool canMove;
    public bool stopOnCollision;
    public Vector3 oldPosition;
    public Vector3 velocity;
    public Vector3 forces;
    public float mass = 1f;
    public SphereCollider sphere;
    public int[] neighbourIndexes;
    
    private void Awake(){
        sphere = GetComponent<SphereCollider>();
    }
}

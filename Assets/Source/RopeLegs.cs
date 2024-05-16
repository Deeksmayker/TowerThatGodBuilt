using UnityEngine;
using static UnityEngine.Physics;
using static UnityEngine.Mathf;

public class RopeLegs : MonoBehaviour{
    [SerializeField] private float stepDistance = 5;
    [SerializeField] private float legLength = 20;
    [SerializeField] private Rope[] ropes;
    
    // private float _lastMoveTimer;
    // private int _lastMovedIndex;
    private Vector3[] _standingPoints;
    private float[] _lastMovedTimers;
    
    private void Awake(){
        _standingPoints = new Vector3[ropes.Length];
        _lastMovedTimers = new float[ropes.Length];
    }
    
    private void Update(){
        //_lastMoveTimer += Time.deltaTime;
        for (int i = 0; i < ropes.Length; i++){
            _lastMovedTimers[i] += Time.deltaTime;
            // if(_lastMoveTimer < 0.1f){
            //     break;
            // }
            
            // if (_lastMovedIndex == i){
            //     continue;
            // }
        
            if (Raycast(ropes[i].transform.position, ropes[i].transform.forward, out var hit, legLength, Layers.Environment)){
                if (Vector3.Distance(hit.point, _standingPoints[i]) > stepDistance
                    && _lastMovedTimers[(i + 1) % ropes.Length] > 0.1f
                    && _lastMovedTimers[(Clamp(i - 1, 0, ropes.Length)) % ropes.Length] > 0.1f){
                    ropes[i].SetEndPos(hit.point);
                    _standingPoints[i] = hit.point;
                    _lastMovedTimers[i] = 0;
                    //_lastMovedIndex = i;
                    break;
                }
            }
        }
    }
}

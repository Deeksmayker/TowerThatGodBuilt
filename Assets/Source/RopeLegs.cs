using UnityEngine;
using static UnityEngine.Physics;
using static UnityEngine.Mathf;
using static Source.Utils.Utils;

public class RopeLegs : MonoBehaviour{
    [SerializeField] private float stepDistance = 5;
    [SerializeField] private float legLength = 20;
    [SerializeField] private float moveTime = 0.5f;
    [SerializeField] private Rope[] ropes;
    
    private Leg[] _legs;
    
    private void Awake(){
        _legs = new Leg[ropes.Length];
        for (int i = 0; i < _legs.Length; i++){
            _legs[i] = new Leg();
            _legs[i].rope = ropes[i];
        }
    }
    
    private void Update(){
        for (int i = 0; i < ropes.Length; i++){
            if (_legs[i].moving){
                _legs[i].moveT += Time.deltaTime / moveTime;
                _legs[i].rope.SetEndPos(Vector3.Lerp(_legs[i].startMovePoint, _legs[i].targetPoint, EaseInOutQuad(_legs[i].moveT)));
                if (_legs[i].moveT >= 1){
                    _legs[i].moveT = 0;
                    _legs[i].moving = false;
                    _legs[i].lastMoveTimer = 0;
                } else{
                    continue;
                }
            } else{
                _legs[i].lastMoveTimer += Time.deltaTime;
            }
        
            if (Raycast(_legs[i].rope.transform.position, _legs[i].rope.transform.forward, out var hit, legLength, Layers.Environment)){
                if (_legs[i].lastMoveTimer > 0.1f
                    && Vector3.Distance(hit.point, _legs[i].targetPoint) > stepDistance
                    && !_legs[(i + 1) % ropes.Length].moving
                    && !_legs[(Clamp(i - 1, 0, ropes.Length)) % ropes.Length].moving){
                    //_legs[i].rope.SetEndPos(hit.point);
                    _legs[i].startMovePoint = _legs[i].rope.EndPos();
                    _legs[i].targetPoint = hit.point;
                    //_legs[i].targetPoint = hit.point;
                    //_legs[i].lastMoveTimer = 0;
                    _legs[i].moving = true;
                    //_lastMovedIndex = i;
                    break;
                }
            }
        }
    }
}

public class Leg{
    public Rope rope;
    public Vector3 startMovePoint;
    public Vector3 targetPoint;
    public bool moving;
    public float moveT;
    public float lastMoveTimer;
}

using UnityEngine;
using static Source.Utils.Utils;

public class IKLegs : MonoBehaviour{
    public int positionsCount = 3;
    public int iterationCount = 1;
    public float jointLength = 1f;
    public Limb limbPrefab;
    public Transform targetPoint;
    public Transform startPoint;
    public Limb startLimb;
    
    public bool threeD = true;
    
    
    private Limb[] _limbs;
    private LineRenderer _lr;
    
    private void Start(){
        //_lr = GetComponent<LineRenderer>();
    
        _limbs = new Limb[positionsCount + 1];
        _limbs[0] = startLimb;
        //_limbs[_limbs.Length-1] = targetPoint;
        for (int i = 1; i < _limbs.Length; i++){
            _limbs[i] = Instantiate(limbPrefab, transform);//, Quaternion.identity);
            _limbs[i].index = i;
        }
        //startPoint = transform.position;
        //_lr.positionCount = positionsCount + 1;
        
        if (threeD){
            jointLength = (limbPrefab.transform.position - limbPrefab.start.transform.position).magnitude;
        }
    }
    
    private void Update(){
        Vector3 startToTarget = targetPoint.position - startPoint.position;
        
        if (startToTarget.magnitude > positionsCount * jointLength){
            for (int i = 1; i < _limbs.Length; i++){
                //_lr.SetPosition(i, startPoint.position + i * startToTarget.normalized * jointLength);
                _limbs[i].transform.position = startPoint.position + (i) * startToTarget.normalized * jointLength;
                _limbs[i].transform.rotation = Quaternion.LookRotation(startToTarget);
            }
            
            return;
        }
        
        for (int iteration = 0; iteration < iterationCount; iteration++){
            Limb lastLimb = _limbs[_limbs.Length-1];
            lastLimb.transform.position = targetPoint.position;
            Vector3 previousToMe1 = lastLimb.end.position - _limbs[_limbs.Length-2].end.position;
            if (previousToMe1.sqrMagnitude > EPSILON){
                lastLimb.transform.rotation = Quaternion.LookRotation(previousToMe1);
            }
            
            for (int i = _limbs.Length - 2; i > 0; i--){
                _limbs[i].transform.position = _limbs[i+1].start.position;
                Vector3 previousToMe = _limbs[i].end.position - _limbs[i-1].end.position;
                if (previousToMe.sqrMagnitude > EPSILON){
                    _limbs[i].transform.rotation = Quaternion.LookRotation(previousToMe);
                }
            }
            
            _limbs[1].transform.position = _limbs[0].end.position + (_limbs[2].start.position - _limbs[1].start.position).normalized * jointLength;
            Vector3 vecToSecond1 = _limbs[2].start.position - _limbs[1].start.position;
            if (vecToSecond1.sqrMagnitude > EPSILON){
                _limbs[1].transform.rotation = Quaternion.LookRotation(vecToSecond1);
            }

            
            for (int i = 1; i < _limbs.Length - 1; i++){
                _limbs[i].transform.position = _limbs[i-1].end.position + _limbs[i].transform.forward * jointLength;
                Vector3 vecToNext = _limbs[i+1].start.position - _limbs[i].start.position;
                if (vecToNext.sqrMagnitude > EPSILON){
                    _limbs[i].transform.rotation = Quaternion.LookRotation(vecToNext);
                }
            }
        }
    }
    
    private void LineRendererIK(){
        Vector3 startToTarget = targetPoint.position - startPoint.position;
        
        if (startToTarget.magnitude > positionsCount * jointLength){
            for (int i = 0; i < _lr.positionCount; i++){
                _lr.SetPosition(i, startPoint.position + i * startToTarget.normalized * jointLength);
            }
            
            return;
        }
        
        _lr.SetPosition(0, startPoint.position);
        _lr.SetPosition(_lr.positionCount - 1, targetPoint.position);
        
        for (int iteration = 0; iteration < iterationCount; iteration++){
            for (int i = _lr.positionCount - 2; i > 0; i--){
                Vector3 previousToMe = _lr.GetPosition(i) - _lr.GetPosition(i + 1);
                Vector3 wishPosition = _lr.GetPosition(i + 1) + previousToMe.normalized * jointLength;
                _lr.SetPosition(i, wishPosition);
            }
            
            for (int i = 1; i < _lr.positionCount - 2; i++){
                Vector3 previousToMe = _lr.GetPosition(i) - _lr.GetPosition(i - 1);
                Vector3 wishPosition = _lr.GetPosition(i - 1) + previousToMe.normalized * jointLength;
                _lr.SetPosition(i, wishPosition);
            }
        }
    }
}

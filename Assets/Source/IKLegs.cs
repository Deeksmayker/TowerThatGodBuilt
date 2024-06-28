using UnityEngine;
using static Source.Utils.Utils;

public class IKLegs : MonoBehaviour{
    public bool updateByMyself;
    public int positionsCount = 3;
    public int iterationCount = 1;
    public float jointLength = 1f;
    public Limb limbPrefab;
    public Limb lastLimbPrefab;
    public Transform targetPoint;
    
    public bool stretchToTarget = true;
    public bool stretchToDirection;
    
    public bool threeD = true;
    public Transform startPoint;
    
    private float sumLength;
    
    private float[] _jointLengths;
    
    private static Transform s_LimbContainer;
    //public Limb startLimb;
    
    private Limb[] _limbs;
    private LineRenderer _lr;
    
    public Vector3 lastTarget;
    
    private void Start(){
        //_lr = GetComponent<LineRenderer>();
        
        startPoint = transform;
        lastTarget = startPoint.position;
        
        if (!s_LimbContainer){
            s_LimbContainer = (new GameObject("Global Limb Container")).transform;
        }
    
        _limbs = new Limb[positionsCount + 1];
        _jointLengths = new float[_limbs.Length];
        _limbs[0] = gameObject.AddComponent<Limb>();
        _limbs[0].start = transform;
        _limbs[0].end = transform;
        //_limbs[_limbs.Length-1] = targetPoint;
        for (int i = 1; i < _limbs.Length; i++){
            Limb prefabToSpawn = limbPrefab;
            if (i == _limbs.Length - 1 && lastLimbPrefab){
                prefabToSpawn = lastLimbPrefab;
            }
            _limbs[i] = Instantiate(prefabToSpawn, s_LimbContainer);//, Quaternion.identity);
            _limbs[i].index = i;
            
            _jointLengths[i] = (_limbs[i].transform.position - _limbs[i].start.transform.position).magnitude;
            sumLength += _jointLengths[i];
        }
        //startPoint = transform.position;
        //_lr.positionCount = positionsCount + 1;
        
        if (threeD){
            jointLength = (limbPrefab.transform.position - limbPrefab.start.transform.position).magnitude;
        }
    }
    
    private void Update(){
        if (!updateByMyself){
            return;
        }
              
        UpdateIK(targetPoint.position);
    }
    
    public void UpdateIK(Vector3 target){
        Vector3 startToTarget = target - _limbs[0].transform.position;
        
        // if (startToTarget.magnitude > sumLength){
        //     StretchInDirection(startToTarget);
            
        //     return;
        // }
        
        if (stretchToTarget){
            StretchInDirection(target + Vector3.up * 10 - _limbs[0].transform.position);
        } else if (stretchToDirection){
            StretchInDirection(transform.forward * 10 + Vector3.up * 10);//target + Vector3.up * 10 - _limbs[0].transform.position);
        }
        
        for (int iteration = 0; iteration < iterationCount; iteration++){
            Limb lastLimb = _limbs[_limbs.Length-1];
            lastLimb.transform.position = target;
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
            
            _limbs[1].transform.position = _limbs[0].end.position + (_limbs[2].start.position - _limbs[1].start.position).normalized * _jointLengths[1];
            Vector3 vecToSecond1 = _limbs[2].start.position - _limbs[1].start.position;
            if (vecToSecond1.sqrMagnitude > EPSILON){
                _limbs[1].transform.rotation = Quaternion.LookRotation(vecToSecond1);
            }

            
            for (int i = 1; i < _limbs.Length - 1; i++){
                _limbs[i].transform.position = _limbs[i-1].end.position + _limbs[i].transform.forward * _jointLengths[i];
                Vector3 vecToNext = _limbs[i+1].start.position - _limbs[i].start.position;
                if (vecToNext.sqrMagnitude > EPSILON){
                    _limbs[i].transform.rotation = Quaternion.LookRotation(vecToNext);
                }
            }
        }
        
//        _limbs[1].transform.position = _limbs[0].end.position + _limbs[1].transform.forward * _jointLengths[1];
        
        
        lastTarget = target;
    }
    
    public void StretchInDirection(Vector3 direction){
        for (int i = 1; i < _limbs.Length; i++){
            ////_lr.SetPosition(i, startPoint.position + i * startToTarget.normalized * jointLength);
            _limbs[i].transform.position = _limbs[0].transform.position + (i) * direction.normalized * jointLength;
            _limbs[i].transform.rotation = Quaternion.LookRotation(direction);
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

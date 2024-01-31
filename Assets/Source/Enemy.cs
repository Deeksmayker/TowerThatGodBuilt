using UnityEngine;

public class Enemy : MonoBehaviour{
    public void TakeHit(Collider whoTookThatHit){
        var wishPosition = Random.onUnitSphere * 10;
        wishPosition.y = Mathf.Abs(wishPosition.y);
        transform.position = wishPosition;
    }
}

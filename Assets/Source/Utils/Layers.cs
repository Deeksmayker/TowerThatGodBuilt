using UnityEngine;

public static class Layers{
    //Base Layers
    public static LayerMask Environment      {get;}  = LayerMask.GetMask("Environment");
    public static LayerMask Player           {get;}  = LayerMask.GetMask("Player");
    public static LayerMask PlayerProjectile {get;}  = LayerMask.GetMask("PlayerProjectile");
    public static LayerMask EnemyHurtBox     {get;}  = LayerMask.GetMask("EnemyHurtBox");
    
    //Specified usecase
    public static LayerMask PlayerBallHitable {get;} = Environment | EnemyHurtBox;
}

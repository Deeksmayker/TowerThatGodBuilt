using UnityEngine;

public static class Layers{
    //Base Layers
    public static LayerMask Environment      {get;}  = LayerMask.GetMask("Environment");
    public static LayerMask Player           {get;}  = LayerMask.GetMask("Player");
    public static LayerMask PlayerHurtBox    {get;}  = LayerMask.GetMask("PlayerHurtBox");
    public static LayerMask PlayerProjectile {get;}  = LayerMask.GetMask("PlayerProjectile");
    public static LayerMask EnemyProjectile  {get;}  = LayerMask.GetMask("EnemyProjectile");
    public static LayerMask EnemyHurtBox     {get;}  = LayerMask.GetMask("EnemyHurtBox");
    public static LayerMask Area             {get;}  = LayerMask.GetMask("Area");
    public static LayerMask Rope             {get;}  = LayerMask.GetMask("Rope");
    
    //Specified usecase
    public static LayerMask PlayerBallHitable {get;} = Environment | EnemyHurtBox | EnemyProjectile | Player;
    public static LayerMask PlayerKickHitable {get;} = /*EnemyHurtBox | EnemyProjectile | */PlayerProjectile/* | Rope*/;
}

using UnityEngine;

public static class Layers{
    public static LayerMask Environment      {get;} = LayerMask.GetMask("Environment");
    public static LayerMask Player           {get;} = LayerMask.GetMask("Player");
    public static LayerMask PlayerProjectile {get;} = LayerMask.GetMask("PlayerProjectile");
}

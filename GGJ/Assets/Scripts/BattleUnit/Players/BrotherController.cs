using UnityEngine;

public class BrotherController : PlayerController
{
    protected override void Awake()
    {
        initialHealth = 6;
        initialMaxHealth = 6;
        initialAttack = 5;
        base.Awake();
    }
}

using UnityEngine;

public class SisterController : PlayerController
{
    protected override void Awake()
    {
        initialHealth = 11;
        initialMaxHealth = 11;
        initialAttack = 2;
        base.Awake();
    }
}

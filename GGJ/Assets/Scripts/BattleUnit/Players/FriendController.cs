using UnityEngine;

public class FriendController : PlayerController
{
    protected override void Awake()
    {
        initialHealth = 9;
        initialMaxHealth = 9;
        initialAttack = 3;
        base.Awake();
    }
}

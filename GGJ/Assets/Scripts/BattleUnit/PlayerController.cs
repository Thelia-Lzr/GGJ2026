using UnityEngine;
using System;
using System.Collections;

public class PlayerController : UnitController
{
    [Header("Player Settings")]
    [SerializeField] private bool waitingForInput = false;
    
    private ActionCommand pendingAction;
    
    public event Action OnTurnStartRequested;
    
    public bool IsWaitingForInput => waitingForInput;

    //面具
    public GameObject MaskImage;

    protected override void Awake()
    {
        base.Awake();
    }
    
    public override bool HasResource(ResourceType type, int amount)
    {
        return PlayerResourceManager.Instance.HasResource(type, amount);
    }
    
    public override void SpendResource(ResourceType type, int amount)
    {
        PlayerResourceManager.Instance.SpendResource(type, amount);
    }
    
    public override void GainResource(ResourceType type, int amount)
    {
        PlayerResourceManager.Instance.GainResource(type, amount);
    }
    
    public override int GetResource(ResourceType type)
    {
        return PlayerResourceManager.Instance.GetResource(type);
    }
    
    public override bool CanPerformAction(ActionCommand command)
    {
        if (!CanAct)
            return false;
        
        if (command == null || !command.IsValid())
            return false;
        
        return HasResource(ResourceType.ActionPoint, command.ResourceCost);
    }
    
    public override void PerformAction(ActionCommand command)
    {
        if (!CanPerformAction(command))
        {
            Debug.LogWarning($"Cannot perform action: {command.ActionType}");
            return;
        }
        
        //SpendResource(ResourceType.ActionPoint, command.ResourceCost);
        
        RaiseActionPerformed(command);
    }

    public override IEnumerator Attack(BattleUnit target)
    {
        if(currentMask == null)
        {
            return base.Attack(target);
        }
        else
        {
            return currentMask.Attack(this,target);
        }
        
    }
    
    public override void TakeTurn()
    {
        if (!CanAct)
        {
            Debug.LogWarning($"Player unit {gameObject.name} cannot act this turn.");
            return;
        }
        
        OnTurnStart();
        
        waitingForInput = true;
        
        OnTurnStartRequested?.Invoke();
    }
    
    public void HandleInput(ActionCommand command)
    {
        if (!waitingForInput)
            return;
        
        if (command == null || !CanPerformAction(command))
        {
            Debug.LogWarning("Invalid action command received.");
            return;
        }
        
        pendingAction = command;
    }
    
    public override void ConfirmAction(ActionCommand command)
    {   
        base.ConfirmAction(command);
        if(attackCount > 0)
        {
            InitActionCircle();
        }
    }
    
    

    public override bool SwitchMask(Mask newMask, int cost)
    {
        bool success = base.SwitchMask(newMask, cost);
        
        if (success)
        {
            Debug.Log($"Player switched to mask: {newMask.MaskName}");
            if (MaskImage == null)
            {
                MaskImage = new GameObject("Mask");
                MaskImage.transform.SetParent(transform, false);
                MaskImage.transform.localScale = Vector3.one;
                MaskImage.transform.localPosition = new Vector3(-0.08f, 0.49f);
                MaskImage.AddComponent<SpriteRenderer>();
            }
                SpriteRenderer sprite= MaskImage.GetComponent<SpriteRenderer>();
            sprite.sortingOrder = 4;
                switch (newMask.MaskName)
                {
                    case "痛苦的面具":
                        sprite.sprite = ResourceController.Instance.MaskSprites[0];
                        break;
                    case "人体派的面具":
                        sprite.sprite = ResourceController.Instance.MaskSprites[1];
                        break;
                    case "秉烛人的面具":
                        sprite.sprite = ResourceController.Instance.MaskSprites[2];
                        break;
                    case "拉线工人的面罩":
                        sprite.sprite = ResourceController.Instance.MaskSprites[3];
                        break;
                    case "火焰少女的面具":
                        sprite.sprite = ResourceController.Instance.MaskSprites[4];
                        break;
                    case "丘丘人的面具":
                        sprite.sprite = ResourceController.Instance.MaskSprites[5];
                        break;
                    case "从不摘下的面具":
                        sprite.sprite = ResourceController.Instance.MaskSprites[6];
                        break;
                    case "难崩的假面":
                        sprite.sprite = ResourceController.Instance.MaskSprites[7];
                        break;
                }
               
        }
        
        return success;
    }
    
    public void EndTurn()
    {
        if (waitingForInput)
        {
            waitingForInput = false;
        }
        
        OnTurnEnd();
    }
    
    public void CancelAction()
    {
        pendingAction = null;
    }
    
}

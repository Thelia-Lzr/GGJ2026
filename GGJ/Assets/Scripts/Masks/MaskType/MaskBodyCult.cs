using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskBodyCult : Mask
{
<<<<<<< Updated upstream
    public MaskBodyCult() : base(maskName: "ÈËÌåÅÉµÄÃæ¾ß", switchCost: 1, maxHealth: 8, atk: 3, atkCost: 2)
    {

=======
    public MaskBodyCult() : base(maskName: "é€†å¡å·´æ‹‰è®¡æ•°å™¨", switchCost: 1, maxHealth: 8, atk: 3, atkCost: 2)
    {
        MaskIcon = Resources.Load<Sprite>("Image/CardImage/16");
        MaskObject = Resources.Load<Sprite>("Image/Mask/Mask16");
>>>>>>> Stashed changes
    }

    public override IEnumerator Attack(UnitController controller, BattleUnit target)
    {
        yield return AttackSplash(controller, target);
    }

    public override void OnEquip(BattleUnit unit)
    {
        base.OnEquip(unit);
        HealAllAllies(unit);
    }

    public override IEnumerator Activate(UnitController controller)
    {
        if (controller != null && controller.BoundUnit != null)
        {
            HealAllAllies(controller.BoundUnit);
        }
        yield return base.Activate(controller);
    }

    public override void OnUnequip(BattleUnit unit)
    {
        HealAllAllies(unit);
        base.OnUnequip(unit);
    }

    private void HealAllAllies(BattleUnit user)
    {
        if (user == null) return;

        Team allyTeam = user.UnitTeam;
        List<BattleUnit> allies = GetAllyUnits(allyTeam);

        foreach (var ally in allies)
        {
            if (ally.IsAlive())
            {
                ally.ApplyHealthChange(2);
                
                if (ally.CurrentMask != null && !ally.CurrentMask.IsBroken)
                {
                    ally.CurrentMask.RepairMask(2);
                }
            }
        }

<<<<<<< Updated upstream
        Debug.Log($"[MaskBodyCult] {user.gameObject.name} µÄÉíÌå³ç°ÝÃæ¾ßÉúÐ§£¬ËùÓÐ¼º·½½ÇÉ«»Ø¸´2µãÌåÁ¦ºÍÃæ¾ßÄÍ¾Ã£¡");
=======
        Debug.Log($"[MaskBodyCult] {user.gameObject.name} çš„èº«ä½“å´‡æ‹œé¢å…·ç”Ÿæ•ˆï¼Œæ‰€æœ‰å·±æ–¹è§’è‰²å›žå¤2ç‚¹ä½“åŠ›å’Œé¢å…·è€ä¹…ï¼");
>>>>>>> Stashed changes
    }

    private List<BattleUnit> GetAllyUnits(Team allyTeam)
    {
        List<BattleUnit> allies = new List<BattleUnit>();

        if (RoundManager.Instance != null)
        {
            foreach (var unit in RoundManager.Instance.battleUnits)
            {
                if (unit.UnitTeam == allyTeam && unit.IsAlive())
                {
                    allies.Add(unit);
                }
            }
        }

        return allies;
    }
}

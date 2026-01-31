using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskHilichurl : Mask
{
    private bool shouldDestroyAtTurnEnd = false;

    public MaskHilichurl() : base(maskName: "呀！", switchCost: 1, maxHealth: 6, atk: 2, atkCost: 1)
    {
        MaskIcon = Resources.Load<Sprite>("Image/CardImage/Card7");
        MaskObject = Resources.Load<Sprite>("Image/Mask/Mask7");
    }

    public override IEnumerator Attack(UnitController controller, BattleUnit target)
    {
        if (target == null || controller == null)
        {
            yield return AttackAOE(controller, target);
            yield break;
        }

        int targetHealthBefore = target.CurrentHealth;

        yield return AttackAOE(controller, target);

        bool targetWasKilled = !target.IsAlive() && targetHealthBefore > 0;

        if (targetWasKilled)
        {
            Debug.Log($"[MaskHilichurl] {controller.gameObject.name} 击杀了 {target.gameObject.name}，触发暴怒效果！");
            
            EnrageAllAllies(controller.BoundUnit);
            
            shouldDestroyAtTurnEnd = true;
            SubscribeToTurnEnd(controller);
        }
    }

    public override void OnUnequip(BattleUnit unit)
    {
        StunRandomEnemy(unit);
        base.OnUnequip(unit);
    }

    private void EnrageAllAllies(BattleUnit user)
    {
        if (user == null) return;

        Team allyTeam = user.UnitTeam;
        List<BattleUnit> allies = GetAllyUnits(allyTeam);

        foreach (var ally in allies)
        {
            if (ally.IsAlive())
            {
                ally.ApplyStatus(new Enraged(1));
            }
        }

        Debug.Log($"[MaskHilichurl] 所有己方角色陷入暴怒状态！");
    }

    private void StunRandomEnemy(BattleUnit user)
    {
        if (user == null) return;

        Team enemyTeam = user.UnitTeam == Team.Player ? Team.Enemy : Team.Player;
        List<BattleUnit> enemies = GetEnemyUnits(enemyTeam);

        if (enemies.Count > 0)
        {
            int randomIndex = Random.Range(0, enemies.Count);
            BattleUnit target = enemies[randomIndex];

            target.ApplyStatus(new Stunned(1));

            Debug.Log($"[MaskHilichurl] 面具毁坏，眩晕了 {target.gameObject.name}！");
        }
    }

    private void SubscribeToTurnEnd(UnitController controller)
    {
        if (controller == null || controller.BoundUnit == null) return;

        controller.BoundUnit.OnTurnEnded += HandleTurnEnd;
    }

    private void HandleTurnEnd()
    {
        if (shouldDestroyAtTurnEnd && equippedUnit != null)
        {
            Debug.Log($"[MaskHilichurl] 回合结束，销毁丘丘人面具！");
            
            equippedUnit.OnTurnEnded -= HandleTurnEnd;
            
            CurrentHealth = 0;
            OnMaskBroken();
        }
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

    private List<BattleUnit> GetEnemyUnits(Team enemyTeam)
    {
        List<BattleUnit> enemies = new List<BattleUnit>();

        if (RoundManager.Instance != null)
        {
            foreach (var unit in RoundManager.Instance.battleUnits)
            {
                if (unit.UnitTeam == enemyTeam && unit.IsAlive())
                {
                    enemies.Add(unit);
                }
            }
        }

        return enemies;
    }
}

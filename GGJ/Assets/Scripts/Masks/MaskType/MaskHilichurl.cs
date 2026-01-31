using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskHilichurl : Mask
{
    private bool shouldDestroyAtTurnEnd = false;

<<<<<<< Updated upstream
    public MaskHilichurl() : base(maskName: "ÇðÇðÈËµÄÃæ¾ß", switchCost: 1, maxHealth: 6, atk: 2, atkCost: 1)
=======
    public MaskHilichurl() : base(maskName: "å‘€ï¼", switchCost: 1, maxHealth: 6, atk: 2, atkCost: 1)
>>>>>>> Stashed changes
    {

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
<<<<<<< Updated upstream
            Debug.Log($"[MaskHilichurl] {controller.gameObject.name} »÷É±ÁË {target.gameObject.name}£¬´¥·¢±©Å­Ð§¹û£¡");
=======
            Debug.Log($"[MaskHilichurl] {controller.gameObject.name} å‡»æ€äº† {target.gameObject.name}ï¼Œè§¦å‘æš´æ€’æ•ˆæžœï¼");
>>>>>>> Stashed changes
            
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

<<<<<<< Updated upstream
        Debug.Log($"[MaskHilichurl] ËùÓÐ¼º·½½ÇÉ«ÏÝÈë±©Å­×´Ì¬£¡");
=======
        Debug.Log($"[MaskHilichurl] æ‰€æœ‰å·±æ–¹è§’è‰²é™·å…¥æš´æ€’çŠ¶æ€ï¼");
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
            Debug.Log($"[MaskHilichurl] Ãæ¾ß»Ù»µ£¬Ñ£ÔÎÁË {target.gameObject.name}£¡");
=======
            Debug.Log($"[MaskHilichurl] é¢å…·æ¯åï¼Œçœ©æ™•äº† {target.gameObject.name}ï¼");
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
            Debug.Log($"[MaskHilichurl] »ØºÏ½áÊø£¬Ïú»ÙÇðÇðÈËÃæ¾ß£¡");
=======
            Debug.Log($"[MaskHilichurl] å›žåˆç»“æŸï¼Œé”€æ¯ä¸˜ä¸˜äººé¢å…·ï¼");
>>>>>>> Stashed changes
            
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

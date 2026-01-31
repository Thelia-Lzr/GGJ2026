using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskCandleHandler : Mask
{
<<<<<<< Updated upstream
    public MaskCandleHandler() : base(maskName: "±üÖòÈËµÄÃæ¾ß", switchCost: 1, maxHealth: 11, atk: 5, atkCost: 1)
    {

=======
    public MaskCandleHandler() : base(maskName: "ç§‰çƒ›äºº", switchCost: 1, maxHealth: 11, atk: 5, atkCost: 1)
    {
        MaskIcon = Resources.Load<Sprite>("Image/CardImage/18");
        MaskObject = Resources.Load<Sprite>("Image/Mask/Mask18");
>>>>>>> Stashed changes
    }

    public override IEnumerator Activate(UnitController controller)
    {
        if (CurrentHealth <= 4)
        {
<<<<<<< Updated upstream
            Debug.Log($"[MaskCandleHandler] Ãæ¾ßÄÍ¾Ã ¡Ü 4£¬´¥·¢ÈºÌåÉËº¦Ð§¹û£¡");
=======
            Debug.Log($"[MaskCandleHandler] é¢å…·è€ä¹… â‰¤ 4ï¼Œè§¦å‘ç¾¤ä½“ä¼¤å®³æ•ˆæžœï¼");
>>>>>>> Stashed changes

            Team enemyTeam = controller.BoundUnit.UnitTeam == Team.Player ? Team.Enemy : Team.Player;
            List<BattleUnit> enemies = GetEnemyUnits(enemyTeam);

            foreach (var enemy in enemies)
            {
                if (enemy.IsAlive())
                {
                    enemy.ApplyHealthChange(-3);
<<<<<<< Updated upstream
                    Debug.Log($"[MaskCandleHandler] ¶Ô {enemy.gameObject.name} Ôì³É 3 µãÉËº¦£¡");
=======
                    Debug.Log($"[MaskCandleHandler] å¯¹ {enemy.gameObject.name} é€ æˆ 3 ç‚¹ä¼¤å®³ï¼");
>>>>>>> Stashed changes
                }
            }

            RepairMask(3);
<<<<<<< Updated upstream
            Debug.Log($"[MaskCandleHandler] Ãæ¾ß»Ø¸´ 3 µãÄÍ¾Ã£¬µ±Ç°ÄÍ¾Ã: {CurrentHealth}/{MaxHealth}");
        }
        else
        {
            Debug.Log($"[MaskCandleHandler] Ãæ¾ßÄÍ¾Ã > 4 ({CurrentHealth}/{MaxHealth})£¬Ð§¹ûÎ´´¥·¢");
=======
            Debug.Log($"[MaskCandleHandler] é¢å…·å›žå¤ 3 ç‚¹è€ä¹…ï¼Œå½“å‰è€ä¹…: {CurrentHealth}/{MaxHealth}");
        }
        else
        {
            Debug.Log($"[MaskCandleHandler] é¢å…·è€ä¹… > 4 ({CurrentHealth}/{MaxHealth})ï¼Œæ•ˆæžœæœªè§¦å‘");
>>>>>>> Stashed changes
        }

        yield return base.Activate(controller);
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


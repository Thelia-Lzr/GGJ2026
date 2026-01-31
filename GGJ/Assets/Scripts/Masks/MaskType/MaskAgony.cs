using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskAgony : Mask
{
<<<<<<< Updated upstream
    public MaskAgony() : base(maskName: "Í´¿àµÄÃæ¾ß", switchCost: 1, maxHealth: 6, atk: 3, atkCost: 1)
    {

=======
    public MaskAgony() : base(maskName: "æŠ˜ç£¨å¼€å§‹ï¼", switchCost: 1, maxHealth: 6, atk: 3, atkCost: 1)
    {
        MaskIcon = Resources.Load<Sprite>("Image/CardImage/15");
        MaskObject = Resources.Load<Sprite>("Image/Mask/Mask15");
>>>>>>> Stashed changes
    }

    public override IEnumerator Attack(UnitController controller, BattleUnit target)
    {
        yield return AttackSplash(controller, target);
    }

    public override void OnEquip(BattleUnit unit)
    {
        base.OnEquip(unit);
        
        Team enemyTeam = unit.UnitTeam == Team.Player ? Team.Enemy : Team.Player;
        List<BattleUnit> enemies = GetEnemyUnits(enemyTeam);
        
        if (enemies.Count > 0)
        {
            int randomIndex = Random.Range(0, enemies.Count);
            BattleUnit target = enemies[randomIndex];
            
            target.ApplyStatus(new Stunned(1));
            
<<<<<<< Updated upstream
            Debug.Log($"[MaskAgony] {unit.gameObject.name} ×°±¸Í´¿àÃæ¾ß£¬Ñ£ÔÎÁË {target.gameObject.name}£¡");
=======
            Debug.Log($"[MaskAgony] {unit.gameObject.name} è£…å¤‡ç—›è‹¦é¢å…·ï¼Œçœ©æ™•äº† {target.gameObject.name}ï¼");
>>>>>>> Stashed changes
        }
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

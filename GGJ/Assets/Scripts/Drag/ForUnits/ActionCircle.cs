using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCircle : DragUnit
{
    private UnitController controller;
    private BattleUnit target;
    public event Action<ActionCommand> Operation;

    private ActionCommand actionCommand;
    public void Initialize(UnitController unitController)
    {
        controller = unitController;
        startPosition = unitController.BoundUnit.transform.position;
    }
    protected override bool isMatch()
    {
        foreach(var battleUnit in RoundManager.Instance.battleUnits)
        {
            if (Vector3.Distance(transform.position, battleUnit.transform.position) < DragController.JUDGEDISTANCE && battleUnit.team == Team.Enemy)
            {
                controller = GetComponent<UnitController>();
                target = battleUnit;
                actionCommand = new ActionCommand(controller, target, ActionType.Attack);
                if (actionCommand.IsValid())
                {
                    Operation?.Invoke(actionCommand);
                    battleUnit.controller.PerformAction(actionCommand);
                    return true;
                }
            }
        }
        return false;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

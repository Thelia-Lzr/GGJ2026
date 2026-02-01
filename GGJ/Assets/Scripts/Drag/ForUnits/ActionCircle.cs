using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionCircle : DragUnit
{
    private UnitController controller;
    private BattleUnit target;
    public event Action<ActionCommand> Operation;

    private ActionCommand actionCommand;
    public GameObject Circle;
    public void Initialize(UnitController unitController)
    {
        controller = unitController;
        startPosition = unitController.BoundUnit.transform.position;
    }
    protected override bool isMatch()
    {
        foreach(var battleUnit in RoundManager.Instance.battleUnits)
        {
            //Debug.Log("Checking ActionCircle against " + battleUnit.name);
            //Debug.Log("Distance: " + Vector3.Distance(transform.position, battleUnit.transform.position));
            if (Vector3.Distance(transform.position, battleUnit.transform.position) < DragController.JUDGEDISTANCE && battleUnit.team == Team.Enemy)
            {
                //Debug.Log("ActionCircle matched with " + battleUnit.name);
                //controller = GetComponent<UnitController>();
                target = battleUnit;
                actionCommand = new ActionCommand(controller, target, ActionType.Attack);
                if (actionCommand.IsValid())
                {
                    Operation?.Invoke(actionCommand);
                    controller.ConfirmAction(actionCommand);
                    return true;
                }
            }
        }
        return false;
    }
    protected override void OnMouseDown()
    {

        Circle.GetComponent<SpriteRenderer>().sprite = ResourceController.Instance.Sprites[3];
        Circle.GetComponent<SpriteRenderer>().sortingOrder = 3;

        if (dragController.Status != 0) return;

        if (RoundManager.Instance != null && RoundManager.Instance.GetActiveTeam() != Team.Player)
        {
            return;
        }

        // 只响应左键（按钮0），忽略右键
        if (!Input.GetMouseButton(0))
        {
            return;
        }

        // 更新起始位置为当前位置（而非实例化时的位置）
        startPosition = transform.position;

        isDragging = true;
        mouseOffset = GetWorldMousePosition() - transform.position;
    }
    protected override void OnMouseUp()
    {
        if (dragController.Status != 0) return;

        if (RoundManager.Instance != null && RoundManager.Instance.GetActiveTeam() != Team.Player)
        {
            return;
        }

        // 只在左键抬起时执行
        if (!Input.GetMouseButtonUp(0))
        {
            return;
        }

        if (isDragging)
        {
            if (isMatch())
            {
                Debug.Log("?");
                afterMatch();
            }
            else
            {
                dragController.Status = 1;
        Circle.GetComponent<SpriteRenderer>().sprite = ResourceController.Instance.Sprites[4];
                Circle.GetComponent<SpriteRenderer>().sortingOrder = 1;
                StartCoroutine(ReturnBackAction());
            }
        }
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

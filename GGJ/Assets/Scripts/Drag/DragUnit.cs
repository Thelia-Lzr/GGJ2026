using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DragUnit : MonoBehaviour
{
    protected Vector3 startPosition;
    protected DragController dragController => DragController.Instance;
    protected Vector3 mouseOffset;
    protected bool isDragging;

    protected virtual void Awake()
    {
        startPosition = transform.position;
    }
    protected virtual void Start()
    {

    }
    // Update is called once per frame
    protected virtual void Update()
    {

    }
    protected virtual void OnMouseDown()
    {
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
    protected virtual void OnMouseDrag()
    {
        if (dragController.Status != 0) return;
        
        if (RoundManager.Instance != null && RoundManager.Instance.GetActiveTeam() != Team.Player)
        {
            return;
        }
        
        // 只在左键拖拽时执行
        if (!Input.GetMouseButton(0))
        {
            return;
        }

        if (isDragging)
        {
            transform.position = GetWorldMousePosition() - mouseOffset;
        }
    }
    protected virtual void OnMouseUp()
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
                StartCoroutine(ReturnBackAction());
            }
        }
    }
    protected virtual void afterMatch()
    {
        Destroy(gameObject);
    }
    protected virtual bool isMatch()
    {
        return dragController.JudgeCollider(transform.position);
    }
    protected Vector3 GetWorldMousePosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0;
        return mouseWorldPos;
    }
    protected virtual IEnumerator ReturnBackAction()
    {
        float duration = 0.3f;
        
        // 使用 DOTween 移动，这样与 HandManager 的动画系统一致
        transform.DOMove(startPosition, duration).SetEase(Ease.OutQuad);
        
        // 等待动画完成
        yield return new WaitForSeconds(duration);
        
        isDragging = false;
        dragController.Status = 0;
    }
}
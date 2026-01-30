using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragUnit : MonoBehaviour
{
    private Vector3 startPosition;
    private DragController dragController=>DragController.Instance;
    private Vector3 mouseOffset;
    private bool isDragging;

    private void Awake()
    {
        startPosition = transform.position;
    }
    private void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnMouseDown()
    {
        if (dragController. Status != 0) return;
        isDragging = true;
        mouseOffset = GetWorldMousePosition()-transform.position;
    }
    private void OnMouseDrag()
    {
        if (dragController.Status != 0) return;

        if (isDragging)
        {
            transform.position = GetWorldMousePosition() - mouseOffset;
        }
    }
    private void OnMouseUp()
    {
        if (dragController.Status != 0) return;

        if (isDragging)
        {
            if (dragController.JudgeCollider(transform.position))
            {
                Debug.Log("?");
                Destroy(gameObject);
            }
            else
            {
                dragController.Status = 1;
                StartCoroutine(ReturnBackAction());
            }

        }
    }
    private Vector3 GetWorldMousePosition()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0; 
        return mouseWorldPos;
    }
    public IEnumerator ReturnBackAction()
    {
        float speed = DragController.RETURNSPEED;
        float minDistance = DragController.RETURNPOSITION;
        Vector3 moveDirection=(startPosition-transform.position).normalized*speed;
        while (true)
        {
            transform.Translate(moveDirection,Space.World);
            if (Vector3.Distance(transform.position,startPosition)<minDistance)
            {
                transform.position = startPosition;
                break;
            }
            yield return null;
        }
        dragController.Status = 0;
        yield return null;
    }
}

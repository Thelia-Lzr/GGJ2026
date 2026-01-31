using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragUnit : MonoBehaviour
{
    protected Vector3 startPosition;
    private DragController dragController => DragController.Instance;
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
        if (dragController.Status != 0) return;
        isDragging = true;
        mouseOffset = GetWorldMousePosition() - transform.position;
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
            if (isMatch())
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
    protected virtual bool isMatch()
    {
        return dragController.JudgeCollider(transform.position);
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
        float minDistance = 0.01f;
        
        while (Vector3.Distance(transform.position, startPosition) > minDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPosition, speed * Time.deltaTime);
            yield return null;
        }
        
        transform.position = startPosition;
        isDragging = false;
        dragController.Status = 0;
    }
}
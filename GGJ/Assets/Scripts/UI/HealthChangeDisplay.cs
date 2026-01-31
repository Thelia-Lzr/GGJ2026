
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HealthChangeDisplay : MonoBehaviour
{
    public float time;
    // Start is called before the first frame update
    void Start()
    {
    }
    public void Intial(int amount)
    {
        StartCoroutine(Display(amount));
    }
    public IEnumerator Display(int amount)
    {
        time = Random.Range(0.5f, 1f);
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.anchoredPosition = new Vector2(100,200);
        rectTransform.localScale = new Vector3(1, 1, 1);
        var movePosition = new Vector2(Random.Range(-1f, 1f),Random.Range(2f, 3f));
        //Debug.Log(movePosition);
        TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();
        if (amount > 0)
        {
            text.text = "+"+amount.ToString()+"!";

        }
        else
        {
            text.text = amount.ToString()+"!";
        }
        text.color = Color.red;
        text.fontSize = Random.Range(80,150);
        while (time>0)
        {
            time-=Time.deltaTime;
            rectTransform.anchoredPosition += movePosition;
            yield return null;
        }
        Destroy(gameObject);
    }
}

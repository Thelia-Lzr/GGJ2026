using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [field:SerializeField]
    public Sprite Back1 {  get; private set; }
    [field:SerializeField]
    public GameObject BackImage { get; private set; }
    private float ScreenWidth => Screen.width;
    private float ScreenHeight=>Screen.height;
    void Start()
    {
        Image backImage= BackImage. GetComponent<Image>();
        backImage.sprite = Back1;
        float zoomW=ScreenWidth / Back1.rect.width;
        float zoomH=ScreenHeight / Back1.rect.height;
        if (zoomH > zoomW)
        {
            BackImage.GetComponent<RectTransform>().sizeDelta=new Vector2(zoomH*Back1.rect.width, zoomH*Back1.rect.height);
        }
        else
        {
            BackImage.GetComponent<RectTransform>().sizeDelta = new Vector2(zoomW * Back1.rect.width, zoomW * Back1.rect.height);

        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

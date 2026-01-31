using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourceController: MonoBehaviour
{
    public static ResourceController Instance { get; private set; }
    public TMP_FontAsset FONT { get; private set; }
    [SerializeField] public Dictionary<string, GameObject> prefabs;

    private void Awake()
    {
        FONT = Resources.Load<TMP_FontAsset>("Font/msyhSDF");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }
}

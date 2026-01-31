using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourceController: MonoBehaviour
{
    public static ResourceController Instance { get; private set; }
    public static Font FONT;
    [SerializeField] public Dictionary<string, GameObject> prefabs;

    private void Awake()
    {
        FONT = Resources.Load<Font>("FONT/simsunSDF");
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

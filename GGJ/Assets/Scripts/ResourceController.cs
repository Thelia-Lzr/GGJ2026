using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class PrefabEntry
{
    public string key;
    public GameObject prefab;
}

public class ResourceController: MonoBehaviour
{
    public static ResourceController Instance { get; private set; }
    public TMP_FontAsset FONT;
    //[SerializeField] public Dictionary<string, GameObject> prefabs;

    //public static Font FONT;
    
    [SerializeField] private List<PrefabEntry> prefabList = new List<PrefabEntry>();
    
    private Dictionary<string, GameObject> prefabs;

    private void Awake()
    {
        FONT = Resources.Load<TMP_FontAsset>("FONT/simsunSDF");
        
        // 将列表转换为字典
        prefabs = new Dictionary<string, GameObject>();
        foreach (var entry in prefabList)
        {
            if (!string.IsNullOrEmpty(entry.key) && entry.prefab != null)
            {
                prefabs[entry.key] = entry.prefab;
            }
        }
        
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
    
    public GameObject GetPrefab(string key)
    {
        return prefabs.ContainsKey(key) ? prefabs[key] : null;
    }
}

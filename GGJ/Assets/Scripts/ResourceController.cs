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
    public List<Sprite> Sprites;
    public List<Sprite> StatusSprites;
    public List<Sprite> MaskSprites;
    private void Awake()
    {
        
        // 将列表转换为字典
        prefabs = new Dictionary<string, GameObject>();
        foreach (var entry in prefabList)
        {
            if (!string.IsNullOrEmpty(entry.key) && entry.prefab != null)
            {
                prefabs[entry.key] = entry.prefab;
            }
        }
        Sprites = new List<Sprite>();
        Sprites.Add(Resources.Load<Sprite>("Image/Icon/Health"));
        Sprites.Add(Resources.Load<Sprite>("Image/Icon/Attack"));
        Sprites.Add(Resources.Load<Sprite>("Image/Icon/Shell"));
        Sprites.Add(Resources.Load<Sprite>("Image/UI/AttackShow"));
        Sprites.Add(Resources.Load<Sprite>("Image/UI/AttackDisplay"));
        StatusSprites =new List<Sprite>();
        StatusSprites.Add(Resources.Load<Sprite>("Image/Icon/Dizzy"));
        StatusSprites.Add(Resources.Load<Sprite>("Image/Icon/Ready"));
        StatusSprites.Add(Resources.Load<Sprite>("Image/Icon/Angry"));
        StatusSprites.Add(Resources.Load<Sprite>("Image/Icon/AttackUp"));
        StatusSprites.Add(Resources.Load<Sprite>("Image/Icon/AttackDown"));
        MaskSprites =new List<Sprite>();
        //痛苦面具
        MaskSprites.Add(Resources.Load<Sprite>("Image/Mask/MaskAgony"));
        //
        MaskSprites.Add(Resources.Load<Sprite>("Image/Mask/MaskBodyCult"));
        MaskSprites.Add(Resources.Load<Sprite>("Image/Mask/MaskCandleHandler"));
        MaskSprites.Add(Resources.Load<Sprite>("Image/Mask/MaskEndField"));
        MaskSprites.Add(Resources.Load<Sprite>("Image/Mask/MaskFlame"));
        MaskSprites.Add(Resources.Load<Sprite>("Image/Mask/MaskHilichurl,"));
        MaskSprites.Add(Resources.Load<Sprite>("Image/Mask/MaskNeverRemoved"));
        MaskSprites.Add(Resources.Load<Sprite>("Image/Mask/MaskOblivionis"));
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

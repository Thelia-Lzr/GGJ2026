using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 面具工厂 - 负责创建面具实例
/// </summary>
public static class MaskFactory
{
    public static Mask CreateMask(MaskType type)
    {
        switch (type)
        {
            case MaskType.Fire:
                return new FireMask();
            
            case MaskType.Ice:
                return new IceMask();
            
            case MaskType.Heal:
                return new HealMask();
            
            // TODO: 添加其他面具类型
            case MaskType.Thunder:
            case MaskType.Wind:
            case MaskType.Earth:
            case MaskType.Light:
            case MaskType.Dark:
            case MaskType.Poison:
                Debug.LogWarning($"Mask type {type} not implemented yet, returning FireMask as placeholder.");
                return new FireMask();
            
            default:
                Debug.LogError($"Unknown mask type: {type}");
                return null;
        }
    }
    
    /// <summary>
    /// 根据配置创建一个完整的牌组
    /// </summary>
    public static List<Mask> CreateDeck(List<MaskType> deckConfig)
    {
        List<Mask> deck = new List<Mask>();
        
        foreach (var type in deckConfig)
        {
            Mask mask = CreateMask(type);
            if (mask != null)
            {
                deck.Add(mask);
            }
        }
        
        return deck;
    }
    
    /// <summary>
    /// 创建一个默认的标准牌组
    /// </summary>
    public static List<Mask> CreateStandardDeck()
    {
        List<MaskType> config = new List<MaskType>
        {
            MaskType.Fire, MaskType.Fire,
            MaskType.Ice, MaskType.Ice,
            MaskType.Heal,
            MaskType.Thunder,
            MaskType.Wind,
            MaskType.Earth
        };
        
        return CreateDeck(config);
    }
}

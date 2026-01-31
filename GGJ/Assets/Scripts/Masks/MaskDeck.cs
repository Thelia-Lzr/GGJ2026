using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 面具牌组 - 管理抽牌堆和弃牌堆
/// </summary>
public class MaskDeck
{
    private List<Mask> drawPile = new List<Mask>();
    private List<Mask> discardPile = new List<Mask>();
    
    public int DrawPileCount => drawPile.Count;
    public int DiscardPileCount => discardPile.Count;
    
    /// <summary>
    /// 初始化牌组
    /// </summary>
    public void Initialize(List<Mask> masks)
    {
        drawPile = new List<Mask>(masks);
        discardPile.Clear();
        Shuffle();
        
        Debug.Log($"MaskDeck initialized with {drawPile.Count} masks.");
    }
    
    /// <summary>
    /// 抽一张面具牌
    /// </summary>
    public Mask DrawMask()
    {
        // 如果抽牌堆空了，重新洗弃牌堆
        if (drawPile.Count == 0)
        {
            ReshuffleDiscardIntoDraw();
        }
        
        // 如果还是没牌，返回null
        if (drawPile.Count == 0)
        {
            Debug.LogWarning("No masks left in deck!");
            return null;
        }
        
        Mask mask = drawPile[0];
        drawPile.RemoveAt(0);
        
        Debug.Log($"Drew mask: {mask.MaskName}. Remaining in draw pile: {drawPile.Count}");
        
        return mask;
    }
    
    /// <summary>
    /// 弃掉一张面具牌
    /// </summary>
    public void Discard(Mask mask)
    {
        if (mask == null) return;
        
        discardPile.Add(mask);
        Debug.Log($"Discarded mask: {mask.MaskName}. Discard pile: {discardPile.Count}");
    }
    
    /// <summary>
    /// 洗牌
    /// </summary>
    private void Shuffle()
    {
        for (int i = 0; i < drawPile.Count; i++)
        {
            int randomIndex = Random.Range(i, drawPile.Count);
            Mask temp = drawPile[i];
            drawPile[i] = drawPile[randomIndex];
            drawPile[randomIndex] = temp;
        }
        
        Debug.Log($"Shuffled {drawPile.Count} masks.");
    }
    
    /// <summary>
    /// 将弃牌堆洗回抽牌堆
    /// </summary>
    private void ReshuffleDiscardIntoDraw()
    {
        if (discardPile.Count == 0)
        {
            Debug.LogWarning("Discard pile is empty, cannot reshuffle.");
            return;
        }
        
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        Shuffle();
        
        Debug.Log("Reshuffled discard pile into draw pile.");
    }
}

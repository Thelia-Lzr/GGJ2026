using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class ResourceController
{
    public static Font FONT;
    static ResourceController()
    {
        FONT = Resources.Load<Font>("FONT/simsunSDF");
    }

}

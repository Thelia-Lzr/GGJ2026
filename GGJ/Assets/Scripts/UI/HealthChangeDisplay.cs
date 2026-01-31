using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthChangeDisplay : MonoBehaviour
{
    public float time;
    // Start is called before the first frame update
    void Start()
    {
        time =UnityEngine.Random.Range(0.5f,0.7f);
    }
    public IEnumerator Display()
    {
        while (true)
        {


            yield return null;

        }
    }
}

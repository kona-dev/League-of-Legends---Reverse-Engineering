using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TestDummy : MonoBehaviour
{
    
    public float trappedTimer;
    public float nettedTimer;
    [SerializeField] public bool trapped;
    [SerializeField] public bool netted;
 
    

    [SerializeField] GameObject statusObject;

    private void Start()
    {
        trappedTimer = 0;
        nettedTimer = 0;
        statusObject.SetActive(false);
    }

    private void Update()
    {
       if (trapped)
        {
            if (trappedTimer > 0) { trappedTimer -= Time.deltaTime; }
            else
            {
                trapped = false;
                if (netted == false) { statusObject.SetActive(false);  }
            }

        }

        if (netted)
        {
            if (nettedTimer > 0) { nettedTimer -= Time.deltaTime; }
            else
            {
                netted = false;
                if (trapped == false) { statusObject.SetActive(false); }
            }

        }

    }


    public void Trapped()
    {
        trappedTimer = 3;
        trapped = true;
        statusObject.SetActive(true);

        // todo root true sight
    }

    public void Netted()
    {
        nettedTimer = 3;
        netted = true;
        statusObject.SetActive(true);
        // todo slow
    }
}

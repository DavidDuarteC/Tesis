using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] int min, seg;
    [SerializeField] Text time;

    [Header("Audio")]
    [SerializeField] AudioClip timeUp;

    private float rest;
    private bool go;

    private void Awake()
    {
        rest = (min * 60) + seg;
        go = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(go)
        {
            rest -=  Time.deltaTime;
            if(rest < 1)
            {
                //Terminar
                AudioManager.i.PlaySfx(timeUp, true);
                
                go = false; 
            }
            int tempMin = Mathf.FloorToInt(rest / 60);
            int tempSeg = Mathf.FloorToInt(rest % 60);
            time.text = string.Format("{00:00}:{01:00}", tempMin, tempSeg);
        }
    }
}

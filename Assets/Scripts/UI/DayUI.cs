using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DayUI : MonoBehaviour
{
    [SerializeField] Text textDay;
    // Start is called before the first frame update
    public static DayUI i { get; private set; }

    private void Awake()
    {
        changeDay();
    }
    void Start()
    {
        i = this;
    }

    public void changeDay()
    {
        textDay.text = "Día " + PlayerController.i.Day;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DayUI : MonoBehaviour
{
    [SerializeField] Text semesterText;
    [SerializeField] Text textDay;
    // Start is called before the first frame update
    public static DayUI i { get; private set; }

    private void Awake()
    {
        ChangeSemester();
        ChangeDay();
    }
    void Start()
    {
        i = this;
    }

    public void ChangeDay()
    {
        textDay.text = "Día: " + PlayerController.i.Day;
    }
    public void ChangeSemester()
    {
        semesterText.text = "Semestre " + PlayerController.i.Semester;
    }

}

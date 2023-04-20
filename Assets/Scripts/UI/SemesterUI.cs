using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SemesterUI : MonoBehaviour
{
    [SerializeField] Text textSemester;
    // Start is called before the first frame update
    public static SemesterUI i { get; private set; }

    private void Awake()
    {
        changeSemester();
    }
    void Start()
    {
        i = this;
    }

    public void changeSemester()
    {
        textSemester.text = "Semestre " + PlayerController.i.Semester;
    }
}

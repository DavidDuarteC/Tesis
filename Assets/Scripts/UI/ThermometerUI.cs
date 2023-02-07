using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ThermometerUI : MonoBehaviour
{
    [SerializeField] GameObject stressBar;

    private void Start()
    {
        StressLevel.i.OnStressLevelChange += SetStress;
        //StressLevel.i.OnChargeStressLevel += SetStress;
    }

    public void SetStress()//Modifica la escala de la barra de salud
    {
        float newStress = StressLevel.i.Level;
        stressBar.transform.localScale = new Vector3(1f, newStress);
    }

    public IEnumerator SetStressSmooth()
    {
        //float newStress = StressLevel.i.Level;
        //float curStress = stressBar.transform.localScale.y;
        //float changeAmt = curStress + newStress;

        //while (curStress + newStress < Mathf.Epsilon)
        //{
        //    curStress -= changeAmt * Time.deltaTime;
        //    stressBar.transform.localScale = new Vector3(1f, curStress);
        //    yield return null;
        //}
        //stressBar.transform.localScale = new Vector3(1f, newStress);
        float newStress = StressLevel.i.Level;
        yield return stressBar.transform.DOScaleY(newStress, 10);
    }
}

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ThermometerUI : MonoBehaviour
{
    [SerializeField] GameObject stressBar;
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogMeditation;
    public static ThermometerUI i { get; private set; }
    private void Start()
    {
        StressLevel.i.OnStressLevelChange += SetStress;
        //StressLevel.i.OnChargeStressLevel += SetStress;
    }

    public void SetStress()//Modifica la escala de la barra de salud
    {
        float newStress;
        newStress = StressLevel.i.Level;
        if (StressLevel.i.Level < 1f && StressLevel.i.Level > 0)
        {
            StartCoroutine(SetStressSmooth(newStress));
        }
        else if(StressLevel.i.Level == 1)
        {
            if (GameController.Instance.State == GameState.Quiz || GameController.Instance.State == GameState.Battle)
            {
                StartCoroutine(SetStressSmooth(newStress));
                StressLevel.i.CountLose += 1;
                if (StressLevel.i.CountLose == 1)
                    StartCoroutine(DialogManager.Instance.ShowDialog(dialog));
                if (StressLevel.i.CountLose > 1)
                    StartCoroutine(DialogManager.Instance.ShowDialog(dialogMeditation));
            }
            if(GameController.Instance.State == GameState.FreeRoam)
            {
                StressLevel.i.ResetLevel();
                AudioManager.i.Volume(false);
                StressLevel.i.PlayVideo();
                
            }
            //newStress = StressLevel.i.Level;
            //stressBar.transform.localScale = new Vector3(1f, newStress);
        }
        else
        {
            newStress = StressLevel.i.Level;
            StartCoroutine(SetStressSmooth());
            //stressBar.transform.localScale = new Vector3(1f, newStress);
        }
    }

    public IEnumerator SetStressSmooth(float newStress)
    {
        //float curHp = stressBar.transform.localScale.y;
        //float changeAmt = newStress - curHp; // invertir la lógica

        //while (curHp < newStress - Mathf.Epsilon) // cambiar a una comparación mayor que
        //{
        //    curHp += changeAmt * Time.deltaTime; // actualizar el valor de changeAmt
        //    stressBar.transform.localScale = new Vector3(1f, curHp);
        //    yield return null;
        //}
        //stressBar.transform.localScale = new Vector3(1f, newStress);
        if (stressBar == null) yield break;

        yield return stressBar.transform.DOScaleY(newStress, 1.5f).WaitForCompletion();
        //float newStress = StressLevel.i.Level;
        //yield return stressBar.transform.DOScaleY(newStress, 10);
    }
    public IEnumerator SetStressSmooth()
    {
        //float newHP = 0;
        //float curHp = stressBar.transform.localScale.y;
        //float changeAmt = curHp - newHP;

        //while (curHp - newHP > Mathf.Epsilon)
        //{
        //    curHp -= changeAmt * Time.deltaTime;
        //    stressBar.transform.localScale = new Vector3(1f, curHp);
        //    yield return null;
        //}
        //stressBar.transform.localScale = new Vector3(1f, newHP);
        if (stressBar == null) yield break;

        yield return stressBar.transform.DOScaleY(0, 1.5f).WaitForCompletion();
    }
}

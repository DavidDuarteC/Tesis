using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Sleep : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] AudioClip audioClip;
    [SerializeField] GameObject NpcMovimientoCplusplus;
    [SerializeField] GameObject NpcMovimientoJava;
    [SerializeField] GameObject NPcMovimientoSQl;
    [SerializeField] GameObject NPcMovimientoLinux;
    [SerializeField] GameObject NPcMovimientoC;
    [SerializeField] GameObject NPcMovimientoKotlin;
    [SerializeField] GameObject NPcMovimientoAngular;
    [SerializeField] GameObject NPcMovimientoPython;
    [SerializeField] Dialog final;

    private void Update()
    {
        ActiveEvents();
    }

    public void OnPlayerTrigger(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        GameController.Instance.PauseGame(true);
        StartCoroutine(SleepeEvent(player.transform, player));
    }
    public IEnumerator SleepeEvent(Transform transform, PlayerController player) // Permite curar los approaches que tenga el personaje
    {
        
        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText("Te ves cansado, deberías dormir un poco",
            choices: new List<string>() { "Sí", "No" },
            onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            //Yes
            yield return Fader.i.FadeIn(0.6f);
            PlayerController.i.Day++;
            AudioManager.i.PlaySfx(audioClip, true);
            if (PlayerController.i.FinishQuices % 2 == 0 && PlayerController.i.FinishQuices != 0)
            {
                PlayerController.i.Semester++;
                DayUI.i.ChangeSemester();
                yield return DialogManager.Instance.ShowDialogText("Empezaste un semestre nuevo (Semestre " + player.Semester + ")");
                PlayerController.i.Day = 1;
                PlayerController.i.TotalQuices += PlayerController.i.FinishQuices;
                PlayerController.i.FinishQuices = 0;
            }
            DayUI.i.ChangeDay();
            yield return new WaitForSeconds(audioClip.length);
            var playerParty = player.GetComponent<ApproachParty>();
            playerParty.Approaches.ForEach(p => p.Heal());
            playerParty.PartyUpdate();
            ActiveEvents();

            yield return DialogManager.Instance.ShowDialogText("Hoy es un nuevo día (Día " + player.Day + ")");
            yield return Fader.i.FadeOut(0.6f);

            if (PlayerController.i.TotalQuices == 8)
            {

                StressLevel.i.Finish = true;
                yield return DialogManager.Instance.ShowDialog(final);
                AudioManager.i.Volume(false);
                StressLevel.i.PlayVideo();
                PlayerController.i.TotalQuices = 0;
            }



        }
        else if (selectedChoice == 1)
        {
            //No
            yield return DialogManager.Instance.ShowDialogText("Ok, vuelve cuando quieras");
        }
        GameController.Instance.StartFreeRoamState();
    }
    public void ActiveEvents()
    {
        if (PlayerController.i.Day >= 2 && PlayerController.i.Semester == 1)
        {
            NpcMovimientoCplusplus.SetActive(true);
        }
        else
        {
            NpcMovimientoCplusplus.SetActive(false);
        }
        if (PlayerController.i.Day >= 3 && PlayerController.i.Semester == 1)
        {
            NpcMovimientoJava.SetActive(true);
        }
        else
        {
            NpcMovimientoJava.SetActive(false);
        }
        if (PlayerController.i.Day >= 2 && PlayerController.i.Semester == 2)
        {
            NPcMovimientoSQl.SetActive(true);
        }
        else
        {
            NPcMovimientoSQl.SetActive(false);
        }
        if (PlayerController.i.Day >= 3 && PlayerController.i.Semester == 2)
        {
            NPcMovimientoLinux.SetActive(true);
        }
        else
        {
            NPcMovimientoLinux.SetActive(false);
        }
        if (PlayerController.i.Day >= 2 && PlayerController.i.Semester == 3)
        {
            NPcMovimientoC.SetActive(true);
        }
        else
        {
            NPcMovimientoC.SetActive(false);
        }
        if (PlayerController.i.Day >= 3 && PlayerController.i.Semester == 3)
        {
            NPcMovimientoKotlin.SetActive(true);
        }
        else
        {
            NPcMovimientoKotlin.SetActive(false);
        }
        if (PlayerController.i.Day >= 2 && PlayerController.i.Semester == 4)
        {
            NPcMovimientoAngular.SetActive(true);
        }
        else
        {
            NPcMovimientoAngular.SetActive(false);
        }
        if (PlayerController.i.Day >= 3 && PlayerController.i.Semester == 4)
        {
            NPcMovimientoPython.SetActive(true);
        }
        else
        {
            NPcMovimientoPython.SetActive(false);
        }
    }

    public bool TriggerRepeatedly => false;

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApproachGiver : MonoBehaviour, ISavable
{
    [SerializeField] Approach approachToGive;
    [SerializeField] Dialog dialog;

    bool used = false;

    public IEnumerator GiveApproach(PlayerController player) //Permite recibir un approach de un NPC
    {
        yield return DialogManager.Instance.ShowDialog(dialog);

        approachToGive.Init();
        player.GetComponent<ApproachParty>().AddApproach(approachToGive);

        used = true;

        AudioManager.i.PlaySfx(AudioId.ApproachObtained, pauseMusic: true);

        string dialogText = $"{player.Name} recibe {approachToGive.Base.Name}";

        yield return DialogManager.Instance.ShowDialogText(dialogText);
    }

    public bool CanBeGiven()
    {
        return approachToGive != null && !used;
    }

    public object CaptureState()
    {
        return used;
    }

    public void RestoreState(object state)
    {
        used = (bool)state;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;
    PartyMemberUI[] memberSlots;
    List<Approach> approaches;
    ApproachParty party;

    int selection = 0;

    public Approach SelectedMember => approaches[selection];

    //Party screen puede ser llamada desde diferentes estados como ActionSelection, RunningTurn, AboutToUse
    public BattleState? CalledFrom { get; set; }

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);

        party = ApproachParty.GetPlayerParty();
        SetPartyData();

        party.OnUpdated += SetPartyData;
    }

    //public void SetPartyData(List<Approach> approaches) //Muestra la informacion de cada uno de los approaches en la lista
    public void SetPartyData()
    {
        //this.approaches = approaches;
        approaches = party.Approaches;

        for(int i = 0; i < memberSlots.Length; i++)
        {
            if (i < approaches.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Init(approaches[i]);
            }
            else
                memberSlots[i].gameObject.SetActive(false);
        }

        UpdateMemberSelection(selection);

        messageText.text = "Elige tu conocimiento";

       // party.OnUpdated += SetPartyData; //Se subscribe al evento para actulizar y mostrar los approaches actuales
    }

    public void HandleUpdate(Action onSelected, Action onBack) //Permite relizar el cambio de approaches
    {
        var prevSelection = selection;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++selection;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --selection;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            selection += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            selection -= 2;

        selection = Mathf.Clamp(selection, 0, approaches.Count - 1);

        if(selection != prevSelection)
            UpdateMemberSelection(selection);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onSelected?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            onBack?.Invoke();
        }
    }

    public void UpdateMemberSelection(int selectMember) 
    {
        for(int i = 0; i < approaches.Count; i++)
        {
            if (i == selectMember)
                memberSlots[i].SetSelected(true);
            else
                memberSlots[i].SetSelected(false);
        }
    }

    public void ShowIfTmIsUsable(CtoItem tmItem)
    {
        for (int i = 0; i < approaches.Count; i++)
        {
            string message = tmItem.CanBeTaught(approaches[i]) ? "ABLE!" : "NOT ABLE!";
            memberSlots[i].SetMessage(message);
        }
    }

    public void ClearMemberSlotMessages()
    {
        for (int i = 0; i < approaches.Count; i++)
        {
            memberSlots[i].SetMessage("");
        }
    }

    public void SetMessageText(string message) //Muestra el mensaje en la patalla PartyScreen
    {
        messageText.text = message;
    }
}

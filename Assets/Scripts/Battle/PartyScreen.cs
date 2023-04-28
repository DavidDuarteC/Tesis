using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;
    PartyMemberUI[] memberSlots;
    List<Approach> pokemons;
    ApproachParty party;

    int selection = 0;

    public Approach SelectedMember => pokemons[selection];

    //Party screen puede ser llamada desde diferentes estados como ActionSelection, RunningTurn, AboutToUse
    public BattleState? CalledFrom { get; set; }

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);

        party = ApproachParty.GetPlayerParty();
        SetPartyData();

        party.OnUpdated += SetPartyData;
    }

    //public void SetPartyData(List<Approach> pokemons) //Muestra la informacion de cada uno de los pokemones en la lista
    public void SetPartyData()
    {
        //this.pokemons = pokemons;
        pokemons = party.Pokemons;

        for(int i = 0; i < memberSlots.Length; i++)
        {
            if (i < pokemons.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Init(pokemons[i]);
            }
            else
                memberSlots[i].gameObject.SetActive(false);
        }

        UpdateMemberSelection(selection);

        messageText.text = "Elige un pokemon";

       // party.OnUpdated += SetPartyData; //Se subscribe al evento para actulizar y mostrar los pokemones actuales
    }

    public void HandleUpdate(Action onSelected, Action onBack) //Permite relizar el cambio de pokemones
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

        selection = Mathf.Clamp(selection, 0, pokemons.Count - 1);

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
        for(int i = 0; i < pokemons.Count; i++)
        {
            if (i == selectMember)
                memberSlots[i].SetSelected(true);
            else
                memberSlots[i].SetSelected(false);
        }
    }

    public void ShowIfTmIsUsable(CtoItem tmItem)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            string message = tmItem.CanBeTaught(pokemons[i]) ? "ABLE!" : "NOT ABLE!";
            memberSlots[i].SetMessage(message);
        }
    }

    public void ClearMemberSlotMessages()
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            memberSlots[i].SetMessage("");
        }
    }

    public void SetMessageText(string message) //Muestra el mensaje en la patalla PartyScreen
    {
        messageText.text = message;
    }
}

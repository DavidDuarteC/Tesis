using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] Dialog dialog;

    [Header("Quest")]
    [SerializeField] QuestBase questToStart;
    [SerializeField] QuestBase questToCompleted;

    [Header("Movement")]
    [SerializeField] List<Vector2> movementPattern;
    [SerializeField] float timeBetweenPattern;

    NPCState state;
    float idleTimer = 0f;
    int currentPattern = 0;
    Quest activeQuest;

    Character character;

    ItemGiver itemGiver;
    PokemonGiver pokemonGiver;
    Healer healer;
    Merchant merchant;

    private void Awake()
    {
        character = GetComponent<Character>();
        itemGiver = GetComponent<ItemGiver>();
        pokemonGiver = GetComponent<PokemonGiver>();
        healer = GetComponent<Healer>();
        merchant = GetComponent<Merchant>();
    }

    public IEnumerator Interact(Transform initiator) // Muestra el dialogo si el jugador interactua con el NPCs
    {
        if(state == NPCState.Idle)
        {
            state = NPCState.Dialog;
            character.LookTowards(initiator.position);

            if(questToCompleted != null)
            {
                var quest = new Quest(questToCompleted);
                yield return quest.CompleteQuest(initiator);
                questToCompleted = null;

                Debug.Log($"{quest.Base.Name} completada");
            }

            if(itemGiver != null && itemGiver.CanBeGiven()) //Permite verificar si el NPC puede dar un item
            {
                yield return itemGiver.GiveItem(initiator.GetComponent<PlayerController>());
            }
            else if (pokemonGiver != null && pokemonGiver.CanBeGiven()) //Permite verificar si el NPC puede dar un pokemon
            {
                yield return pokemonGiver.GivePokemon(initiator.GetComponent<PlayerController>());
            }
            else if (questToStart != null) //Permite verificar si el NPC puede empezar un Quest
            {
                activeQuest = new Quest(questToStart);
                yield return activeQuest.StartQuest();
                questToStart = null;

                if (activeQuest.CanBeCompleted())
                {
                    yield return activeQuest.CompleteQuest(initiator);
                    activeQuest = null;
                }
            }
            else if(activeQuest != null)
            {
                if(activeQuest.CanBeCompleted())
                {
                    yield return activeQuest.CompleteQuest(initiator);
                    activeQuest = null;
                }
                else
                {
                    yield return DialogManager.Instance.ShowDialog(activeQuest.Base.InProgressDialog);
                }
            }
            else if (healer != null)
            {
                yield return healer.Heal(initiator, dialog);
            }
            else if(merchant != null)
            {
                yield return merchant.Trade();
            }
            else
            {
                yield return DialogManager.Instance.ShowDialog(dialog);
            }
            idleTimer = 0f;
            state = NPCState.Idle;
        }
    }

    public void Update()
    {
        if (state == NPCState.Idle)
        {
            idleTimer += Time.deltaTime;
            if(idleTimer > timeBetweenPattern)
            {
                idleTimer = 0f;
                if(movementPattern.Count > 0)
                    StartCoroutine(Walk());
                
            }
        }
            character.HandleUpdate();
    }

    IEnumerator Walk() //Realiza el patron de caminar de los NPCs
    {
        state = NPCState.Walking;

        var oldPos = transform.position;

        yield return character.Move(movementPattern[currentPattern]);

        if(transform.position != oldPos)
            currentPattern = (currentPattern + 1) % movementPattern.Count;

        state = NPCState.Idle;
    }

    public object CaptureState()
    {
        var saveData = new NPCQuestSaveData();
        saveData.activeQuest = activeQuest?.GetSaveData();

        if(questToStart != null)
            saveData.questToStart = (new Quest(questToStart)).GetSaveData();

        if (questToCompleted != null)
            saveData.questToComplete = (new Quest(questToCompleted)).GetSaveData();

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as NPCQuestSaveData;
        if(saveData != null)
        {
            activeQuest = (saveData.activeQuest != null)? new Quest(saveData.activeQuest) : null;

            questToStart = (saveData.questToStart != null) ? new Quest(saveData.questToStart).Base : null;
            questToCompleted = (saveData.questToComplete != null) ? new Quest(saveData.questToComplete).Base : null;


        }
    }
}

[Serializable]
public class NPCQuestSaveData
{
    public QuestSaveData activeQuest;
    public QuestSaveData questToStart;
    public QuestSaveData questToComplete;
}

public enum NPCState
{
    Idle, Walking, Dialog
}

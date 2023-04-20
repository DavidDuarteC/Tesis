using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [Header("Quiz")]
    [SerializeField] bool haveQuiz;
    //[SerializeField] List<DataQuizNpc> categories;
    [SerializeField] List<string> categories;
    [SerializeField] Dialog startQuiz;
    
    NPCState state;
    float idleTimer = 0f;
    int currentPattern = 0;
    Quest activeQuest;

    Character character;

    ItemGiver itemGiver;
    PokemonGiver pokemonGiver;
    Healer healer;
    Merchant merchant;  
    public static NPCController i;

    private void Awake()
    {
        character = GetComponent<Character>();
        itemGiver = GetComponent<ItemGiver>();
        pokemonGiver = GetComponent<PokemonGiver>();
        healer = GetComponent<Healer>();
        merchant = GetComponent<Merchant>();
    }

    private void Start()
    {
        i = this;
    }

    public IEnumerator Interact(Transform initiator) // Muestra el dialogo si el jugador interactua con el NPCs
    {
        if(state == NPCState.Idle)
        {
            state = NPCState.Dialog;
            character.LookTowards(initiator.position);
            if (haveQuiz)
                checkQuiz();
            if (questToCompleted != null)
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
            else if (haveQuiz != false && categories.Any() == true)
            {
                StartCoroutine(DialogManager.Instance.ShowDialog(startQuiz));
                yield return QuizGameUI.i.startQuiz(categories);
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

    public void checkQuiz()
    {
        int i = 0, j = 0, count = 0;
        while (i < categories.Count)
        {
            if (j < QuizManager.i.QuizData.Count)
            {
                if (categories[i] == QuizManager.i.QuizData[j].quiz.categoryName)
                    if (QuizManager.i.QuizData[j].isComplete)
                        count++;
                j++;
                continue;
            }
            j = 0;
            i++;
        }
        if (count == categories.Count)
        {
            haveQuiz = false;
        }
            
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

    //public bool HaveQuiz
    //{
    //    get { return haveQuiz; }
    //    set { haveQuiz = value; }
    //}

    public object CaptureState()
    {
        var saveData = new NPCQuestSaveData();
        saveData.activeQuest = activeQuest?.GetSaveData();

        if(questToStart != null)
            saveData.questToStart = (new Quest(questToStart)).GetSaveData();

        if (questToCompleted != null)
            saveData.questToComplete = (new Quest(questToCompleted)).GetSaveData();
        if (haveQuiz)
        {
            saveData.haveQ = haveQuiz;
            if (categories.Any())
                saveData.categories = categories;
        }
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
            haveQuiz = saveData.haveQ;
            categories = saveData.categories;
        }
    }
}

[Serializable]
public class NPCQuestSaveData
{
    public QuestSaveData activeQuest;
    public QuestSaveData questToStart;
    public QuestSaveData questToComplete;
    public bool haveQ;
    public List<string> categories;
}

public enum NPCState
{
    Idle, Walking, Dialog
}
using GDEUtils.StateMachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public enum GameState{ FreeRoam, Battle, Dialog, Menu, PartyScreen, Bag , Cutscene, Paused, Evolution, Shop, ChooseCharacter, Quiz, Info }
public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] ThermometerUI thermometerUI;
    [SerializeField] CharacterSelectorUI characterSelectorUI;
    [SerializeField] GameObject screenControl;
    [SerializeField] DayUI dayUI;

    GameState state;
    GameState prevState;
    GameState stateBeforeEvolution;

    //public StateMachine<GameController> StateMachine { get; private set; }


    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PrevScene { get; private set; }

    MenuController menuController;


    public static GameController Instance { get; private set; }
    private void Awake()
    {
        Instance = this;

        menuController = GetComponent<MenuController>();

        //Cursor.lockState = CursorLockMode.Confined; //Deshabilitar el mouse
        //Cursor.visible= true;

        PokemonDB.Init();
        MoveDB.Init();
        ConditionsDB.Init();
        ItemDB.Init();
        QuestDB.Init();
        state = GameState.ChooseCharacter;
        //characterSelectorUI.gameObject.SetActive(true);
    }

    private void Start()
    {
        //StateMachine = new StateMachine<GameController>(this);
        //StateMachine.ChangeState(FreeRoamState.i);

        battleSystem.OnBattleOver += EndBattle;

        partyScreen.Init();
        thermometerUI.SetStress();

        DialogManager.Instance.OnShowDialog += () =>
        {
            prevState = state;
            state = GameState.Dialog;
        };
        DialogManager.Instance.OnDialogFinished += () =>
        {
            if(state == GameState.Dialog)
                state = prevState;
        };

        menuController.onBack += () =>
        {
            state = GameState.FreeRoam;
        };

        menuController.onMenuSelected += OnMenuSelected;

        EvolutionManager.i.OnStartEvolution += () =>
        {
            stateBeforeEvolution = state;
            state = GameState.Evolution;
        };
        EvolutionManager.i.OnCompleteEvolution += () =>
        {
            partyScreen.SetPartyData();
            state = stateBeforeEvolution;

            AudioManager.i.PlayMusic(CurrentScene.SceneMusic, fade: true);
        };

        ShopController.i.OnStart += () => state = GameState.Shop;
        ShopController.i.OnFinish += () => state = GameState.FreeRoam;

        characterSelectorUI.OnSelectCharacter += () => state = GameState.ChooseCharacter;
        characterSelectorUI.OnFinishSelect += () => state = GameState.FreeRoam;

        QuizGameUI.i.OnStartQuiz += () => state = GameState.Quiz;
        QuizGameUI.i.OnFinishQuiz += () => state = GameState.FreeRoam;


        thermometerUI.gameObject.SetActive(true);
        dayUI.gameObject.SetActive(true);
        
    }

    public void PauseGame(bool pause)//Permite reliza una pausa para realizar el cambio de escena
    {
        if(pause)
        {
            prevState = state;
            state = GameState.Paused;
        }
        else
        {
            state = prevState;
        }
    }

    public void StartCutsceneState()
    {
        state = GameState.Cutscene;
    }

    public void StartFreeRoamState()
    {
        state = GameState.FreeRoam;
    }

    public void StartBattle(BattleTrigger trigger) //Apenas comience la batalla cambia la cama del main con la de la batalla
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        thermometerUI.gameObject.SetActive(false);
        dayUI.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokmeon = CurrentScene.GetComponent<MapArea>().GetRandonWildPokemon(trigger);

        var wilPokemonCopy = new Pokemon(wildPokmeon.Base, wildPokmeon.Level);

        battleSystem.StartBattle(playerParty, wilPokemonCopy, trigger);
    }

    TrainerController trainer;

    public void StartTrainerBattle(TrainerController trainer) //Apenas comience la batalla cambia la cama del main con la de la batalla
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        thermometerUI.gameObject.SetActive(false);
        dayUI.gameObject.SetActive(false);

        this.trainer = trainer;
        var playerParty = playerController.GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }

    public void OnEnterTrainersView(TrainerController trainer)//Perimite comprobar si entre a la vista de un entrenador
    {
        state = GameState.Cutscene;
        StartCoroutine(trainer.TriggerTrainerBattle(playerController));
    }

    void EndBattle(bool won) //Apenas termine la batalla cambia la camara a la del main
    {
        if(trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }

        partyScreen.SetPartyData();

        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
        thermometerUI.gameObject.SetActive(true);
        dayUI.gameObject.SetActive(true);

        var playerParty = playerController.GetComponent<PokemonParty>();
        bool hasEvolutions = playerParty.CheckForEvolutions();

        if (hasEvolutions)
            StartCoroutine(playerParty.RunEvolutions());
        else
            AudioManager.i.PlayMusic(CurrentScene.SceneMusic, fade: true);
    }

    private void Update() //Muestra las pantallas de juego, batalla y el dialogo con los npcs
    {
        //StateMachine.Execute();

        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                playerController.Character.moveSpeed = 10;
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                playerController.Character.moveSpeed = 5;
            }
            thermometerUI.SetStress();

            if (Input.GetKeyDown(KeyCode.Escape)) //Acceder al menu principal
            {
                menuController.OpenMenu();
                state = GameState.Menu;
            }
        }
        if (state == GameState.Cutscene)
        {
            playerController.Character.HandleUpdate();
        }
        else if(state == GameState.ChooseCharacter)
        {
            CharacterSelectorUI.i.HandleUpdate();
        }
        else if(state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if(state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
        else if(state == GameState.Menu)
        {
            menuController.HandelUpdate();
        }
        else if(state == GameState.PartyScreen)
        {
            Action onSelected = () =>
            {
                //Info del pokemon
            };

            Action onBack = () =>
            {
                partyScreen.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };

            partyScreen.HandleUpdate(onSelected, onBack);
        }
        else if(state == GameState.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };
            inventoryUI.HandleUpdate(onBack);
        }
        else if(state == GameState.Shop)
        {
            ShopController.i.HandleUpdate();
        }
        else if(state == GameState.Info)
        {
            if(Input.GetKeyDown(KeyCode.X)) 
            { 
                screenControl.SetActive(false);
                state = prevState;
            }

        }
    }

    public void SetCurrentScene(SceneDetails currScene) //agrega las escenas conectadas
    {
        PrevScene = CurrentScene;
        CurrentScene = currScene;
    }

    void OnMenuSelected(int selectedItem) //Muestra el menu de opciones en el juego
    {
        if(selectedItem == 0)
        {
            //Pokemon
            partyScreen.gameObject.SetActive(true);
            //partyScreen.SetPartyData(playerController.GetComponent<PokemonParty>().Pokemons);
            state = GameState.PartyScreen;
        }
        else if(selectedItem == 1)
        {
            //Bolsa
            inventoryUI.gameObject.SetActive(true);
            //partyScreen.SetPartyData(playerController.GetComponent<PokemonParty>().Pokemons); //Funciona
            state = GameState.Bag;
        }
        else if(selectedItem == 2)
        {
            screenControl.SetActive(true);

            state = GameState.Info;
        }
        else if(selectedItem == 3)
        {
            //Guardar
            SavingSystem.i.Save("saveSlot1");
            //QuizManager.i.SaveData();
            state = GameState.FreeRoam;
        }
        else if(selectedItem == 4)
        {
            //Guardar
            SavingSystem.i.Load("saveSlot1");
            dayUI.changeDay();
            //QuizManager.i.LoadData();
            state = GameState.FreeRoam;
        }

    }

    public IEnumerator MoveCamera(Vector2 moveOffset, bool watiForFadeOut = false) //Mueve la camara para comprar y que se muestran los personajes
    {
        yield return Fader.i.FadeIn(0.5f);

        worldCamera.transform.position += new Vector3(moveOffset.x, moveOffset.y);

        if (watiForFadeOut)
            yield return Fader.i.FadeOut(0.5f);
        else
            StartCoroutine(Fader.i.FadeOut(0.5f));

    }

    //private void OnGUI()
    //{
    //    var style = new GUIStyle();
    //    style.fontSize = 24;

    //    GUILayout.Label("STATE STACK", style);
    //    foreach (var state in StateMachine.StateStack)
    //    {
    //        GUILayout.Label(state.GetType().ToString(), style);
    //    }
    //}

    public GameState State{ 
        get => state;
        set => state = value;
    } 
}

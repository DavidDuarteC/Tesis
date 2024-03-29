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
    [SerializeField] MainMenu mainMenu;
    [SerializeField] GameObject passLevel;
    [SerializeField] GameObject blockPassLevel;
    [SerializeField] GameObject entrar67;
    [SerializeField] GameObject entrarSalonDos67;
    [SerializeField] GameObject entrarSalonTres67;
    [SerializeField] GameObject entrarSalon2Ing;
    [SerializeField] GameObject entrarSalon3Ing;
    [SerializeField] GameObject entrarSalon4Ing;

    GameState state;
    GameState prevState;

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
        
        ApproachDB.Init();
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
        
        dayUI.gameObject.SetActive(true);
        thermometerUI.gameObject.SetActive(true);
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

        //EvolutionManager.i.OnStartEvolution += () =>
        //{
        //    stateBeforeEvolution = state;
        //    state = GameState.Evolution;
        //};
        //EvolutionManager.i.OnCompleteEvolution += () =>
        //{
        //    partyScreen.SetPartyData();
        //    state = stateBeforeEvolution;

        //    AudioManager.i.PlayMusic(CurrentScene.SceneMusic, fade: true);
        //};

        ShopController.i.OnStart += () => state = GameState.Shop;
        ShopController.i.OnFinish += () => state = GameState.FreeRoam;

        characterSelectorUI.OnSelectCharacter += () => state = GameState.ChooseCharacter;
        characterSelectorUI.OnFinishSelect += () => state = GameState.FreeRoam;

        QuizGameUI.i.OnStartQuiz += () => state = GameState.Quiz;
        QuizGameUI.i.OnFinishQuiz += () => state = GameState.FreeRoam;


        
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

    MonitorController monitor;

    public void StartMonitorBattle(MonitorController monitor) //Apenas comience la batalla cambia la cama del main con la de la batalla
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        thermometerUI.gameObject.SetActive(false);
        dayUI.gameObject.SetActive(false);

        this.monitor = monitor;
        var playerParty = playerController.GetComponent<ApproachParty>();
        var monitorParty = monitor.GetComponent<ApproachParty>();

        battleSystem.StartTrainerBattle(playerParty, monitorParty);
    }

    public void OnEnterTrainersView(MonitorController monitor)//Perimite comprobar si entre a la vista de un entrenador
    {
        state = GameState.Cutscene;
        StartCoroutine(monitor.TriggerTrainerBattle(playerController));
    }

    void EndBattle(bool won) //Apenas termine la batalla cambia la camara a la del main
    {
        if(monitor != null && won == true)
        {
            monitor.BattleLost();
            monitor = null;
        }

        partyScreen.SetPartyData();

        
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
        thermometerUI.gameObject.SetActive(true);
        dayUI.gameObject.SetActive(true);
        state = GameState.FreeRoam;

        var playerParty = playerController.GetComponent<ApproachParty>();
        AudioManager.i.PlayMusic(CurrentScene.SceneMusic, fade: true);
        //bool hasEvolutions = playerParty.CheckForEvolutions();

        //if (hasEvolutions)
        //    StartCoroutine(playerParty.RunEvolutions());
        //else
        //    AudioManager.i.PlayMusic(CurrentScene.SceneMusic, fade: true);
    }

    private void Update() //Muestra las pantallas de juego, batalla y el dialogo con los npcs
    {
        //StateMachine.Execute();

        if (state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();

            SemesterEvents();
            //if (Input.GetKeyDown(KeyCode.Space))
            //{
            //    playerController.Character.moveSpeed = 10;
            //}
            //if (Input.GetKeyUp(KeyCode.Space))
            //{
            //    playerController.Character.moveSpeed = 6;
            //}
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
            characterSelectorUI.HandleUpdate();
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
                //Info del aproach
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
                state = GameState.FreeRoam;
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
            //Bolsa
            inventoryUI.gameObject.SetActive(true);
            //partyScreen.SetPartyData(playerController.GetComponent<ApproachParty>().Approaches); //Funciona
            state = GameState.Bag;
        }
        else if(selectedItem == 1)
        {
            //Controles
            screenControl.SetActive(true);
            state = GameState.Info;
        }
        else if(selectedItem == 2)
        {
            //Cargar
            SavingSystem.i.Load("saveSlot1");
            dayUI.ChangeDay();
            //QuizManager.i.LoadData();
            state = GameState.FreeRoam;
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
            Debug.Log("Salir...");
            Application.Quit();
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

    public void SemesterEvents()
    {
        if (playerController.FinishQuices % 2 == 0 && playerController.FinishQuices != 0)
        {
            passLevel.SetActive(true);
        }
        else
        {
            blockPassLevel.SetActive(false);
            passLevel.SetActive(false);
        }

        if(playerController.Semester >= 2)
        {
            entrar67.SetActive(false);
            entrarSalon2Ing.SetActive(false);
        }
        if(playerController.Semester >= 3)
        {
            entrarSalonDos67.SetActive(false);
            entrarSalon3Ing.SetActive(false);
        }
        if( playerController.Semester >= 4)
        {
            entrarSalonTres67.SetActive(false);
            entrarSalon4Ing.SetActive(false);
        }
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
    public void ActiveThermometer()
    {
        thermometerUI.gameObject.SetActive(true);
    }
}

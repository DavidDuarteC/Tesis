using DG.Tweening;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, AboutToUse, MoveToForget , BattleOver , Bag} //Estados del jugador en el juego
public enum BattleAction { Move, SwitchPokemon, UseItem, Run  }

public enum BattleTrigger { LongGrass, Water }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit; //Objeto pokemon del jugador
    [SerializeField] BattleUnit enemyUnit; //Objeto pokemon del enemigo
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject pokeballSprite;
    [SerializeField] MoveSelectionUI moveSelectionUI;

    [Header("Audio")]
    [SerializeField] AudioClip wildBattleMusic;
    [SerializeField] AudioClip trainerBattleMusic;
    [SerializeField] AudioClip battleVictoryMusic;

    [Header("Background Images")]
    [SerializeField] Image backgorundImage;
    [SerializeField] Sprite grassBackground;
    [SerializeField] Sprite waterBackground;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;
    bool aboutToUseChoice = true;

    PokemonParty playerParty;
    PokemonParty trainerParty;
    Pokemon wildPokemon;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    int escapeAttempts;
    MoveBase moveToLearn;

    BattleTrigger battleTrigger;


    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon, BattleTrigger trigger = BattleTrigger.LongGrass) // Permite empezar una batalla
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        player = playerParty.GetComponent<PlayerController>();
        isTrainerBattle = false;

        battleTrigger = trigger;

        AudioManager.i.PlayMusic(wildBattleMusic);

        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty, BattleTrigger trigger = BattleTrigger.LongGrass) // Permite empezar una batalla
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        battleTrigger = trigger;

        AudioManager.i.PlayMusic(trainerBattleMusic);

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle() //Actualiza la informacion del jugador y del enemigo
    {
        playerUnit.Clear();
        enemyUnit.Clear();

        backgorundImage.sprite = (battleTrigger == BattleTrigger.LongGrass) ? grassBackground : waterBackground;

        if(!isTrainerBattle)
        {
            //Encontrar un pokemon
            playerUnit.Setup(playerParty.GetHealthyPokemon());
            enemyUnit.Setup(wildPokemon);

            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves); //Llama a la funcion mostrar los movimientos de los pokemon
            yield return dialogBox.TypeDialog($"¡Un {enemyUnit.Pokemon.Base.Name} salvaje!"); //Muestra el texto animado
            yield return dialogBox.TypeDialog($"¡Adelante, {playerUnit.Pokemon.Base.Name}!"); //Muestra el texto animado
            dialogBox.SetDialog($"¿Qué debería hacer {playerUnit.Pokemon.Base.Name}?");

        }
        else
        {
            //Batalla con un entrenador

            //Mostrar los entrenadores al comienzo de la batalla
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);
            
            //playerImage.gameObject.SetActive(true);
            //trainerImage.gameObject.SetActive(true);
            //playerImage.sprite = player.Sprite;
            //trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} quiere una batalla");

            //Enviar el primer pokemon del entrenador
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyPokemon = trainerParty.GetHealthyPokemon();
            enemyUnit.Setup(enemyPokemon);
            yield return dialogBox.TypeDialog($"{trainer.Name} envia {enemyPokemon.Base.Name}");

            //Enviar el primer pokemon del jugador
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetHealthyPokemon();
            playerUnit.Setup(playerPokemon);
            yield return dialogBox.TypeDialog($"¡Adelante, {playerUnit.Pokemon.Base.Name}!"); //Muestra el texto animado
            dialogBox.SetMoveNames(playerUnit.Pokemon.Moves); //Llama a la funcion mostrar los movimientos de los pokemon

        }

        escapeAttempts = 0;
        partyScreen.Init();
        ActionSelection();
    }

    void BattleOver(bool won) //Verifica si la batalla termino
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        playerUnit.Hud.ClearData();
        enemyUnit.Hud.ClearData();
        OnBattleOver(won);
    }

    void ActionSelection() //Muestra las opciones para que el jugador pueda elegir si pelear o huir
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Elige una accion");
        dialogBox.EnableActionSelector(true);

    }

    void OpenPartyScreen() //Muestra la pantalla para escoger a cada uno de los pokemones en la lista
    {
        partyScreen.CalledFrom = state;
        state = BattleState.PartyScreen;
        //partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    void OpenBag()
    {
        state = BattleState.Bag;
        //partyScreen.SetPartyData();
        inventoryUI.gameObject.SetActive(true);
    }

    void MoveSelection() //Muestra los diferentes movimientos a escoger
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator AboutToUse(Pokemon newPokemon)//Permite enviar un pokemon cuando se le derroto un pokemon al enemigo
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} va a enviar a {newPokemon.Base.Name}. Quieres cambiar de pokemon?");

        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Elige un movimiento que quieras olvidar");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);//Genera una lista de Tipo Move y la convierte a una lista de MoveBase
        moveToLearn = newMove;

        state = BattleState.MoveToForget;

    }

    IEnumerator RunTurns(BattleAction playerAction) //Permite relizar un ataque al jugador y al enemigo
    {
        state = BattleState.RunningTurn;
        if(playerAction == BattleAction.Move)
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;

            //Check cual pokemon va primero
            bool playerGoesFirst = true;
            if(enemyMovePriority > playerMovePriority)
                playerGoesFirst = false;
            else if(enemyMovePriority == playerMovePriority)
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit: playerUnit;

            var secondPokemon = secondUnit.Pokemon;

            //Primer turno
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;

            if(secondPokemon.HP > 0)
            {
                //Segundo turno
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver) yield break;
            }
        }
        else
        {
            if(playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = partyScreen.SelectedMember;
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }
            else if(playerAction == BattleAction.UseItem)
            {
                dialogBox.EnableActionSelector(false);
                //state = BattleState.Bag;
                //HandleUpdate();
                yield return ThrowPokeball(null);
                //OpenBag();
            }
            else if(playerAction == BattleAction.Run)
            {
                yield return TryToEscape();
            }

            //Turno del enemigo
            var enemyMove = enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;
        }

        if (state != BattleState.BattleOver)
            ActionSelection();
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move) //permite administrar la opciones en la batalla
    {
        bool canRunMove =  sourceUnit.Pokemon.OnBeforeMove();

        if(!canRunMove) 
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.UpdateHPAsync();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Pokemon);


        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} usa {move.Base.Name}");

        if (CheckIfMoveHits(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {
            sourceUnit.PlayAttackAnimation();
            AudioManager.i.PlaySfx(move.Base.Sound);

            yield return new WaitForSeconds(1f);

            targetUnit.PlayHitAnimation();
            AudioManager.i.PlaySfx(AudioId.Hit);

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon); //Retorna la informacion del daño contra el enemigo
                yield return targetUnit.Hud.UpdateHPAsync(); //Actializa la vida del enemigo
                yield return ShowDamageDetails(damageDetails); //Muestra si fue critico o efectivo el ataque del jugador

            }

            if(move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Pokemon.HP > 0)
            {
                foreach(var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                        yield return RunMoveEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon, secondary.Target);
                }
            }


            if (targetUnit.Pokemon.HP <= 0)
            {
                yield return HandlePokemonFainted(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} fallo el ataque");
        }
    }

    IEnumerator RunMoveEffects(MoveEffects effects , Pokemon source, Pokemon target, MoveTarget moveTarget)
    {

        //Modifica estadisticas
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);
        }

        //Altera la vida o demas estaditicas segun su estado(Envenenado, paralizado, congelado, dormido, etc)
        if(effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }

        //Altera la vida o demas volatiles estadicticas como confusion
        if (effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if (state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        //Ataques como quemar or envenenar dañaran al pokemon despues de su turno
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateHPAsync();
        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }

    }

    bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
    {
        if (move.Base.AlwaysHits)
            return true;

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f}; ;
        if (accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];

        if (evasion > 0)
            moveAccuracy /= boostValues[evasion];
        else
            moveAccuracy *= boostValues[-evasion];
        return UnityEngine.Random.Range(1,101) <= moveAccuracy;
    }

    IEnumerator ShowStatusChanges(Pokemon pokemon) //Muestra si el pokemon tiene algo nuevo
    {
        while(pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    IEnumerator HandlePokemonFainted(BattleUnit fanitedUnit) //Termina la batalla si el pokemon fue derrotado
    {
        yield return dialogBox.TypeDialog($"{fanitedUnit.Pokemon.Base.Name} esta muerto");
        fanitedUnit.PlayFaintedAnimation();
        yield return new WaitForSeconds(2f);

        if (!fanitedUnit.IsPlayerUnit)
        {
            bool battleWon = true;
            if(isTrainerBattle)
                battleWon = trainerParty.GetHealthyPokemon() == null;

            if(battleWon)
                AudioManager.i.PlayMusic(battleVictoryMusic);


            //Gane exp
            int expYield = fanitedUnit.Pokemon.Base.ExpYield;
            int enemyLevel = fanitedUnit.Pokemon.Level;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            playerUnit.Pokemon.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} gano {expGain} exp");
            yield return playerUnit.Hud.SetExpSmooth();

            //Aumenta el nivel
            while (playerUnit.Pokemon.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} subio a nivel {playerUnit.Pokemon.Level}");

                //Intenta aprender un nuevo movimiento
                var newMove = playerUnit.Pokemon.GetLearnableMoveAtCurrLevel();
                if(newMove != null)
                {
                    if(playerUnit.Pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
                    {
                        playerUnit.Pokemon.LearnMove(newMove.Base);
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} aprendio {newMove.Base.Name}");
                        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);

                    }
                    else
                    {
                        //Olvidar un movimiento
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} Esta tratando de aprender {newMove.Base.Name}");
                        yield return dialogBox.TypeDialog($"Pero no puede aprender mas de {PokemonBase.MaxNumOfMoves} movimientos");
                        yield return ChooseMoveToForget(playerUnit.Pokemon, newMove.Base);
                        yield return new WaitUntil(() => state != BattleState.MoveToForget);
                        yield return new WaitForSeconds(2f);
                    }
                }


                yield return playerUnit.Hud.SetExpSmooth(true);

            }

            yield return new WaitForSeconds(1f);
        }

        checkForBattleOver(fanitedUnit);
    }

    void checkForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
                OpenPartyScreen();
            else
            {
                //StressLevel.i.AddLevel(StressLevel.i.LostMoreTimeRow ? StressLevel.i.LoseMultiply * 2 : StressLevel.i.LoseMultiply);
                StressLevel.i.AddLevel(StressLevel.i.LoseMultiply);
                BattleOver(false);
            }
        }
        else
        {
            if (!isTrainerBattle)
            {
                //StressLevel.i.LostMoreTimeRow = false;
                BattleOver(true);
            }
            else
            {
                var nextPokemon = trainerParty.GetHealthyPokemon();
                if (nextPokemon != null)
                    //Envia el siguiente pokemon
                    StartCoroutine(AboutToUse(nextPokemon));
                else
                {
                    //StressLevel.i.LostMoreTimeRow = false;
                    BattleOver(true);
                }
            }
        }
            
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails) //Muestra la informacion segun el daño del ataque
    {
        if (damageDetails.Critical > 1f)   
            yield return dialogBox.TypeDialog("Golpe critico!");
            

        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("Es super efectivo!");
        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("No es efectivo");
            
    }

    public void HandleUpdate() //Permite realiza el movimiento de escoger entre pelear o huir y lo mismo con cada uno de los movimientos del pokemon
    {
        if (state == BattleState.ActionSelection) 
        {
            HandleActionSelection(); //Seleccionar Pelear o Huir
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection(); //Seleccionar uno de los cuatro movimientos
        }
        else if(state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
        else if(state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if(state == BattleState.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = BattleState.ActionSelection;
            };

            Action<ItemBase> onItemUsed = (ItemBase usedItem) =>
            {
                StartCoroutine(OnItemUsed(usedItem));
            };

            inventoryUI.HandleUpdate(onBack, onItemUsed);
        }
        else if(state == BattleState.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
                if(moveIndex == PokemonBase.MaxNumOfMoves)
                {
                    //No va a aprender un nuevo movimiento
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} no va a prender {moveToLearn.Name}"));
                }
                else
                {
                    //Olvida el movimeinto elegido y aprende el nuevo
                    var selectedMove = playerUnit.Pokemon.Moves[moveIndex].Base;
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} olvido {selectedMove.Name} y aprendio {moveToLearn.Name}"));


                    playerUnit.Pokemon.Moves[moveIndex] = new Move(moveToLearn);
                }

                moveToLearn = null;
                state = BattleState.RunningTurn;
            };
            moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }
    }

    void HandleActionSelection() //Permite escoger entre las opciones de pelear o huir
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentAction;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentAction;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentAction += 2;
        else if(Input.GetKeyDown(KeyCode.UpArrow))
            currentAction -= 2;

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if(Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                //Pelear
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                //Bag
                //OpenBag();
                StartCoroutine(RunTurns(BattleAction.UseItem));
                //inventoryUI.gameObject.SetActive(true);
                //partyScreen.SetPartyData(); //Funciona
                //state = BattleState.Bag;
            }
            else if (currentAction == 2)
            {
                //Pokemon
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                //Huir
                StartCoroutine(RunTurns(BattleAction.Run));
            }
        }
    }

    void HandleMoveSelection() //Permite moverse libremente para escoger uno de los 4 movimientos del pokemon
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentMove;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentMove;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMove -= 2;

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if(Input.GetKeyDown(KeyCode.Z) )
        {
            var move = playerUnit.Pokemon.Moves[currentMove];
            if (move.PP == 0) return;

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    void HandlePartySelection() //Permite relizar el cambio de pokemones
    {
        Action onSelected = () =>
        {
            var selectedMember = partyScreen.SelectedMember;
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("No puedes usar un pokemon derrotado");
                return;
            }
            if (selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText("Ya estas usando ese pokemon");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.ActionSelection)
            {
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                state = BattleState.Busy;
                bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;
                StartCoroutine(SwitchPokemon(selectedMember, isTrainerAboutToUse));
            }

            partyScreen.CalledFrom = null;
        };

        Action onBack = () =>
        {
            if (playerUnit.Pokemon.HP <= 0)
            {
                partyScreen.SendMessage("Tienes que escoger un pokemon para continuar");
                return;
            }


            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.AboutToUse)
            {
                StartCoroutine(SendNextTrainerPokemon());
            }
            else
                ActionSelection();

            partyScreen.CalledFrom = null;
        };
        partyScreen.HandleUpdate(onSelected, onBack);
    }

    void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            aboutToUseChoice = !aboutToUseChoice;

        dialogBox.UpdateChoiceBox(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableChoiceBox(false);
            if(aboutToUseChoice == true)
            {
                //Opcion si
                OpenPartyScreen();
            }
            else
            {
                //Opcion no
                StartCoroutine(SendNextTrainerPokemon());
            }

        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerPokemon());
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon, bool isTrainerAboutToUse = false) //Permite el cambio de pokemones
    {
        if(playerUnit.Pokemon.HP > 0){
            yield return dialogBox.TypeDialog($"Vuele {playerUnit.Pokemon.Base.Name}");
            playerUnit.PlayFaintedAnimation();
            yield return new WaitForSeconds(2f);
        }
        

        playerUnit.Setup(newPokemon);
        dialogBox.SetMoveNames(newPokemon.Moves); //Llama a la funcion mostrar los movimientos de los pokemon
        yield return dialogBox.TypeDialog($"¡Adelante, {newPokemon.Base.Name}!"); //Muestra el texto animado

        if (isTrainerAboutToUse)
            StartCoroutine(SendNextTrainerPokemon());
        else
            state = BattleState.RunningTurn;
    }

    IEnumerator SendNextTrainerPokemon() //Envia el siguiente pokemon del entrenador cuando uno fue derrotado
    {
        state = BattleState.Busy;

        var nextPokemon = trainerParty.GetHealthyPokemon();
        enemyUnit.Setup(nextPokemon);
        yield return dialogBox.TypeDialog($"{trainer.Name} envia a {nextPokemon.Base.Name}!");
        state = BattleState.RunningTurn;

    }

    IEnumerator OnItemUsed(ItemBase usedItem)
    {
        state = BattleState.Busy;
        inventoryUI.gameObject.SetActive(false);

        if (usedItem is PokeBallItem)
        {
            yield return ThrowPokeball((PokeBallItem)usedItem);
        }

        StartCoroutine(RunTurns(BattleAction.UseItem));
    }

    IEnumerator ThrowPokeball(PokeBallItem pokeBallItem) //Lanza la pokeball para atrapar al pokemon salvaje
    {
        state = BattleState.Busy;

        bool bandera = true;
        if (pokeBallItem == null)
        {
            bandera = false;
            //pokeBallItem.CatchRateModifier = 1;
        }
            

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog("No puedes robar el pokemon de un entrenador!");
            state = BattleState.RunningTurn;
            yield break;
        }

        string poke = (bandera) ? pokeBallItem.Name.ToUpper() : "POKEBALL";
        yield return dialogBox.TypeDialog($"{player.Name} usa una {poke}!");
        var pokeballObj = Instantiate(pokeballSprite, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
        var pokeball = pokeballObj.GetComponent<SpriteRenderer>();
        
        if (pokeBallItem != null)
        {
            pokeball.sprite = pokeBallItem.Icon;
        }

        //Animaciones
        yield return pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();
        pokeball.transform.DOMoveY(enemyUnit.transform.position.y -1.3f, 0.5f).WaitForCompletion();

        int shakeCount = TryCatchPokmon(enemyUnit.Pokemon, pokeBallItem); 

        for(int i = 0; i < Mathf.Min(shakeCount, 3); i++)
        {
            yield return new WaitForSeconds(0.5f);
            yield return pokeball.transform.DOPunchRotation(new Vector3(0,0,10f), 0.8f).WaitForCompletion(); 
        }

        if(shakeCount == 4)
        {
            //El pokemon se capturo
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} fue capturado");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddPokemon(enemyUnit.Pokemon);
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} ha sido agregado a sus pokemones");


            Destroy(pokeball);
            BattleOver(true);
        }
        else
        {
            //El pokemon se escapo
            yield return new WaitForSeconds(1f);
            pokeball.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();

            if (shakeCount < 2)
                yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} se ha escapado");
            else
                yield return dialogBox.TypeDialog("Casi capturas al pokemon");

            Destroy(pokeball);
            state = BattleState.RunningTurn;
        }
    }

    int TryCatchPokmon(Pokemon pokemon, PokeBallItem pokeBallItem)
    {
        float a = 0;
        if (pokeBallItem != null)
            a = (3 * pokemon.MaxHp - 2 * pokemon.HP) * pokemon.Base.CatchRate * pokeBallItem.CatchRateModifier *ConditionsDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp);
        else
            a = (3 * pokemon.MaxHp - 2 * pokemon.HP) * pokemon.Base.CatchRate * 1 *ConditionsDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp);


        if (a >= 255)
            return 4;

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while(shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
                break;

            shakeCount++;
        }

        return shakeCount;
    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;

        if(isTrainerBattle) 
        {
            yield return dialogBox.TypeDialog("No puedes huir de batallas con entrenadores");
            state = BattleState.RunningTurn;
            yield break;
        }
        ++escapeAttempts;

        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed = enemyUnit.Pokemon.Speed;

        if(enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog("Pudiste huir de forma segura");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 255;

            if(UnityEngine.Random.Range(0, 255) < f)
            {
                yield return dialogBox.TypeDialog("Pudiste huir de forma segura");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog("No pudiste huir!");
                state = BattleState.RunningTurn;
            }
        }
    }
}

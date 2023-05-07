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
    [SerializeField] AudioClip trainerBattleMusic;
    [SerializeField] AudioClip battleVictoryMusic;

    [Header("Background Images")]
    [SerializeField] Image backgorundImage;
    [SerializeField] Sprite grassBackground;
    //[SerializeField] Sprite waterBackground;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;
    bool aboutToUseChoice = true;

    ApproachParty playerParty;
    ApproachParty trainerParty;
    Approach wildPokemon;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    int escapeAttempts;
    MoveBase moveToLearn;

    BattleTrigger battleTrigger;

    public void StartTrainerBattle(ApproachParty playerParty, ApproachParty trainerParty, BattleTrigger trigger = BattleTrigger.LongGrass) // Permite empezar una batalla
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

        //backgorundImage.sprite = (battleTrigger == BattleTrigger.LongGrass) ? grassBackground : waterBackground;

        if(!isTrainerBattle)
        {
            //Encontrar un pokemon
            playerUnit.Setup(playerParty.GetHealthyPokemon());
            enemyUnit.Setup(wildPokemon);

            dialogBox.SetMoveNames(playerUnit.approach.Moves); //Llama a la funcion mostrar los movimientos de los pokemon
            yield return dialogBox.TypeDialog($"¡Un {enemyUnit.approach.Base.Name} salvaje!"); //Muestra el texto animado
            yield return dialogBox.TypeDialog($"¡Adelante, {playerUnit.approach.Base.Name}!"); //Muestra el texto animado
            dialogBox.SetDialog($"¿Qué debería hacer {playerUnit.approach.Base.Name}?");

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
            yield return dialogBox.TypeDialog($"{trainer.Name} envía a {enemyPokemon.Base.Name}");

            //Enviar el primer pokemon del jugador
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetHealthyPokemon();
            playerUnit.Setup(playerPokemon);
            yield return dialogBox.TypeDialog($"¡Adelante, {playerUnit.approach.Base.Name}!"); //Muestra el texto animado
            dialogBox.SetMoveNames(playerUnit.approach.Moves); //Llama a la funcion mostrar los movimientos de los pokemon

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
        dialogBox.SetDialog("Elige una acción");
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

    IEnumerator AboutToUse(Approach newPokemon)//Permite enviar un pokemon cuando se le derroto un pokemon al enemigo
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} va a enviar a {newPokemon.Base.Name}. Quieres cambiar de pokemon?");

        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    IEnumerator ChooseMoveToForget(Approach pokemon, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Elige un conocimiento que quieras olvidar");
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
            playerUnit.approach.CurrentMove = playerUnit.approach.Moves[currentMove];
            enemyUnit.approach.CurrentMove = enemyUnit.approach.GetRandomMove();

            int playerMovePriority = playerUnit.approach.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.approach.CurrentMove.Base.Priority;

            //Check cual pokemon va primero
            bool playerGoesFirst = true;
            if(enemyMovePriority > playerMovePriority)
                playerGoesFirst = false;
            else if(enemyMovePriority == playerMovePriority)
                playerGoesFirst = playerUnit.approach.Speed >= enemyUnit.approach.Speed;

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit: playerUnit;

            var secondPokemon = secondUnit.approach;

            //Primer turno
            yield return RunMove(firstUnit, secondUnit, firstUnit.approach.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver) yield break;

            if(secondPokemon.HP > 0)
            {
                //Segundo turno
                yield return RunMove(secondUnit, firstUnit, secondUnit.approach.CurrentMove);
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
            else if(playerAction == BattleAction.Run)
            {
                yield return TryToEscape();
            }

            //Turno del enemigo
            var enemyMove = enemyUnit.approach.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver) yield break;
        }

        if (state != BattleState.BattleOver)
            ActionSelection();
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move) //permite administrar la opciones en la batalla
    {
        bool canRunMove =  sourceUnit.approach.OnBeforeMove();

        if(!canRunMove) 
        {
            yield return ShowStatusChanges(sourceUnit.approach);
            yield return sourceUnit.Hud.UpdateHPAsync();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.approach);


        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.approach.Base.Name} usa {move.Base.Name}");

        if (CheckIfMoveHits(move, sourceUnit.approach, targetUnit.approach))
        {
            sourceUnit.PlayAttackAnimation();
            AudioManager.i.PlaySfx(move.Base.Sound);

            yield return new WaitForSeconds(1f);

            targetUnit.PlayHitAnimation();
            AudioManager.i.PlaySfx(AudioId.Hit);

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.approach, targetUnit.approach, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.approach.TakeDamage(move, sourceUnit.approach); //Retorna la informacion del daño contra el enemigo
                yield return targetUnit.Hud.UpdateHPAsync(); //Actializa la vida del enemigo
                yield return ShowDamageDetails(damageDetails); //Muestra si fue critico o efectivo el ataque del jugador

            }

            if(move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.approach.HP > 0)
            {
                foreach(var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                        yield return RunMoveEffects(secondary, sourceUnit.approach, targetUnit.approach, secondary.Target);
                }
            }


            if (targetUnit.approach.HP <= 0)
            {
                yield return HandlePokemonFainted(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.approach.Base.Name} falló el ataque");
        }
    }

    IEnumerator RunMoveEffects(MoveEffects effects , Approach source, Approach target, MoveTarget moveTarget)
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
        sourceUnit.approach.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.approach);
        yield return sourceUnit.Hud.UpdateHPAsync();
        if (sourceUnit.approach.HP <= 0)
        {
            yield return HandlePokemonFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }

    }

    bool CheckIfMoveHits(Move move, Approach source, Approach target)
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

    IEnumerator ShowStatusChanges(Approach pokemon) //Muestra si el pokemon tiene algo nuevo
    {
        while(pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    IEnumerator HandlePokemonFainted(BattleUnit fanitedUnit) //Termina la batalla si el pokemon fue derrotado
    {
        yield return dialogBox.TypeDialog($"{fanitedUnit.approach.Base.Name} está muerto");
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
            int expYield = fanitedUnit.approach.Base.ExpYield;
            int enemyLevel = fanitedUnit.approach.Level;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            playerUnit.approach.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.approach.Base.Name} ganó {expGain} exp");
            yield return playerUnit.Hud.SetExpSmooth();

            //Aumenta el nivel
            while (playerUnit.approach.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.approach.Base.Name} subió a nivel {playerUnit.approach.Level}");

                //Intenta aprender un nuevo movimiento
                var newMove = playerUnit.approach.GetLearnableMoveAtCurrLevel();
                if(newMove != null)
                {
                    if(playerUnit.approach.Moves.Count < ApproachBase.MaxNumOfMoves)
                    {
                        playerUnit.approach.LearnMove(newMove.Base);
                        yield return dialogBox.TypeDialog($"{playerUnit.approach.Base.Name} aprendió {newMove.Base.Name}");
                        dialogBox.SetMoveNames(playerUnit.approach.Moves);

                    }
                    else
                    {
                        //Olvidar un movimiento
                        yield return dialogBox.TypeDialog($"{playerUnit.approach.Base.Name} Esta tratando de aprender {newMove.Base.Name}");
                        yield return dialogBox.TypeDialog($"Pero no puede aprender mas de {ApproachBase.MaxNumOfMoves} movimientos");
                        yield return ChooseMoveToForget(playerUnit.approach, newMove.Base);
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
                GameController.Instance.ActiveThermometer();
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
            yield return dialogBox.TypeDialog("Golpe crítico!");
            

        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("Es súper efectivo!");
        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("No es efectivo :(");
            
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
        else if(state == BattleState.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
                if(moveIndex == ApproachBase.MaxNumOfMoves)
                {
                    //No va a aprender un nuevo movimiento
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.approach.Base.Name} no va a prender {moveToLearn.Name}"));
                }
                else
                {
                    //Olvida el movimeinto elegido y aprende el nuevo
                    var selectedMove = playerUnit.approach.Moves[moveIndex].Base;
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.approach.Base.Name} olvidó {selectedMove.Name} y aprendió {moveToLearn.Name}"));


                    playerUnit.approach.Moves[moveIndex] = new Move(moveToLearn);
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
                StartCoroutine(RunTurns(BattleAction.Run));
                //inventoryUI.gameObject.SetActive(true);
                //partyScreen.SetPartyData(); //Funciona
                //state = BattleState.Bag;
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

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.approach.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.approach.Moves[currentMove]);

        if(Input.GetKeyDown(KeyCode.Z) )
        {
            var move = playerUnit.approach.Moves[currentMove];
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
                partyScreen.SetMessageText("No puedes usar un conocimiento que ya ha sido derrotado en una batalla anterior");
                return;
            }
            if (selectedMember == playerUnit.approach)
            {
                partyScreen.SetMessageText("Ya estas usando ese conocimiento");
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
            if (playerUnit.approach.HP <= 0)
            {
                partyScreen.SendMessage("Tienes que escoger un conocimiento para continuar");
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

    IEnumerator SwitchPokemon(Approach newPokemon, bool isTrainerAboutToUse = false) //Permite el cambio de pokemones
    {
        if(playerUnit.approach.HP > 0){
            yield return dialogBox.TypeDialog($"Vuelve {playerUnit.approach.Base.Name}");
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
        yield return dialogBox.TypeDialog($"{trainer.Name} enviá a {nextPokemon.Base.Name}!");
        state = BattleState.RunningTurn;

    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog("No puedes huir de las batallas contra monitores");
            state = BattleState.RunningTurn;
            yield break;
        }
        ++escapeAttempts;

        int playerSpeed = playerUnit.approach.Speed;
        int enemySpeed = enemyUnit.approach.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog("Pudiste huir de forma segura");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 255;

            if (UnityEngine.Random.Range(0, 255) < f)
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

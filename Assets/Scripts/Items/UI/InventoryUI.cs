using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection, PartySelection, MoveToForget, Busy}
public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] Text categoryText;
    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    [SerializeField] PartyScreen partyScreen;
    [SerializeField] MoveSelectionUI moveSelectionUI;

    ApproachParty party;

    List<ItemSlotUI> slotUIList;

    int selectedItem = 0;
    int selectedCategory = 0;
    MoveBase moveToLearn;

    const int itemsViewport = 8;

    InventoryUIState state;

    Inventory inventory;

    RectTransform itemlistRect;

    Action<ItemBase> onItemUsed;

    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemlistRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateItemList();

        inventory.OnUpdated += UpdateItemList;

        party = ApproachParty.GetPlayerParty();
    }

    void UpdateItemList()
    {
        //Limpiar la lista de objetos existentes
        foreach (Transform child in itemList.transform)
            Destroy(child.gameObject);

        slotUIList = new List<ItemSlotUI>();
        foreach (var itemSlot in inventory.GetSlotsByCategory(selectedCategory))
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetData(itemSlot);

            slotUIList.Add(slotUIObj);
        }


        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack, Action<ItemBase> onItemUsed =null) //Permite seleccionar los items de la bolsa
    {
        this.onItemUsed = onItemUsed;
        if (state == InventoryUIState.ItemSelection)
        {
            int prevSelection = selectedItem;
            int prevCategory = selectedCategory;

            if (Input.GetKeyUp(KeyCode.DownArrow))
                ++selectedItem;
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                --selectedItem;
            else if (Input.GetKeyUp(KeyCode.RightArrow))
                ++selectedCategory;
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                --selectedCategory;

            if (selectedCategory > Inventory.ItemCategories.Count - 1) //Rota si esta en la ultima categoria la siguinte seria la primera y asi en sentido contrario
                selectedCategory = 0;
            else if(selectedCategory < 0)
                selectedCategory = Inventory.ItemCategories.Count - 1;

            selectedItem = Mathf.Clamp(selectedItem, 0, inventory.GetSlotsByCategory(selectedCategory).Count - 1);

            if (prevCategory != selectedCategory)
            {
                ResetSelection();
                categoryText.text = Inventory.ItemCategories[selectedCategory];
                UpdateItemList();
            }
            else if (prevSelection != selectedItem)
            {
                UpdateItemSelection();
            }

            if (Input.GetKeyDown(KeyCode.Z) && selectedCategory != 0)
                StartCoroutine(ItemSelected());
            else if (Input.GetKeyDown(KeyCode.X))
                onBack?.Invoke();

        }
        else if (state == InventoryUIState.PartySelection)
        {
            //Handle Party Selection
            Action onSelected = () =>
            {
                //Usar el item en el pokemon seleccionado
                //partyScreen.gameObject.SetActive(true);
                StartCoroutine(UseItem());
            };

            Action onBackPartyScreen = () =>
            {
                //Cerrar el party screen y volver a la pantalla de items
                ClosePartyScreen();
            };
            partyScreen.HandleUpdate(onSelected, onBackPartyScreen);
        }
        else if(state == InventoryUIState.MoveToForget)
        {
            Action<int> onMoveSelected = (int moveIndex) =>
            {
                StartCoroutine(OnMoveToForgetSelected(moveIndex));
            };

            moveSelectionUI.HandleMoveSelection(onMoveSelected);
        }
    }

    IEnumerator ItemSelected() //Permite relizar la accion con la pokeball
    {
        state = InventoryUIState.Busy;

        var item = inventory.GetItem(selectedItem, selectedCategory);

        if(GameController.Instance.State == GameState.Shop)
        {
            onItemUsed?.Invoke(item);
            state = InventoryUIState.ItemSelection;
            yield break;
        }

        if(GameController.Instance.State == GameState.Battle)
        {
            //En una batalla
            if(!item.CanUseInBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"Este item no puede ser usado en batalla");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }
        else
        {
            //Fuera de una batalla
            if (!item.CanUseOutsideBattle)
            {
                yield return DialogManager.Instance.ShowDialogText($"Este item no puede ser usado fuera de una batalla");
                state = InventoryUIState.ItemSelection;
                yield break;
            }
        }

        if(selectedCategory == (int)ItemCategory.Cto)
        {
            OpenPartyScreen();
            if(item is CtoItem)//Muestra si es usable por el pokemon
                partyScreen.ShowIfTmIsUsable(item as CtoItem);
                
        }
    }

    IEnumerator UseItem() //Permite usar un item
    {
        state = InventoryUIState.Busy;

        ApproachParty playerParty = PlayerController.i.GetComponent<ApproachParty>();
        var lenguagePlayer = playerParty.GetHealthyPokemon();
        if (lenguagePlayer != null)
        {
            yield return HandleTMItems();

            var item = inventory.GetItem(selectedItem, selectedCategory);
            var pokemon = partyScreen.SelectedMember;
            var usedItem = inventory.UseItem(selectedItem, partyScreen.SelectedMember, selectedCategory);
            if (usedItem != null)
            {
                if (usedItem is RecoveyItem)
                    yield return DialogManager.Instance.ShowDialogText($"El jugador usó {usedItem.Name}");

                onItemUsed?.Invoke(usedItem);
            }
            else
            {
                if (selectedCategory == (int)ItemCategory.Items)
                    yield return DialogManager.Instance.ShowDialogText("No tendrá ningún efecto");

            }
        }
        else
        {
            StartCoroutine(DialogManager.Instance.ShowDialogText("Descansa, alguien agotado no puede seguir aprendiendo"));
        }
        ClosePartyScreen();
    }

    IEnumerator HandleTMItems()
    {
        var tmItem = inventory.GetItem(selectedItem, selectedCategory) as CtoItem;
        if (tmItem == null)
            yield break;

        var pokemon = partyScreen.SelectedMember;

        if (pokemon.HasMove(tmItem.Move))
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} ya tiene {tmItem.Move.Name}");
            yield break;

        }

        if (!tmItem.CanBeTaught(pokemon))
        {
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} no puede aprender {tmItem.Move.Name}");
            yield break;

        }

        if (pokemon.Moves.Count < ApproachBase.MaxNumOfMoves)
        {
            pokemon.LearnMove(tmItem.Move);
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} aprendió {tmItem.Move.Name}");

        }
        else
        {
            //Olvidar un movimiento
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} está intentando aprender {tmItem.Move.Name}");
            yield return DialogManager.Instance.ShowDialogText($"Pero no puede aprender más de {ApproachBase.MaxNumOfMoves} movimientos");
            yield return ChooseMoveToForget(pokemon, tmItem.Move);
            yield return new WaitUntil(() => state != InventoryUIState.MoveToForget);

        }

    }

    IEnumerator ChooseMoveToForget(Approach pokemon, MoveBase newMove) //Permite seleccionar el movimiento a olvidar
    {
        state = InventoryUIState.Busy;
        yield return DialogManager.Instance.ShowDialogText($"Elige un movimiento que quieras olvidar", true, false);
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);//Genera una lista de Tipo Move y la convierte a una lista de MoveBase
        moveToLearn = newMove;

        state = InventoryUIState.MoveToForget;

    }

    void UpdateItemSelection() //Genera la vista para ver cual opcion esta seleccionada
    {
        var slots = inventory.GetSlotsByCategory(selectedCategory);

        selectedItem = Mathf.Clamp(selectedItem, 0, slots.Count - 1);

        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i == selectedItem)
                slotUIList[i].NameText.color = GlobalSettings.i.HighlightedColor;
            else
                slotUIList[i].NameText.color = Color.black;
        }


        if (slots.Count > 0)
        {
            var item = slots[selectedItem].Item;
            itemIcon.sprite = item.Icon; //Muestra la imagen del item
            itemDescription.text = item.Description; //Muestra la info del item
        }


        HandleScrolling();
    }

    void HandleScrolling() //Permite realizar scroll con las flechas arriba y abajo
    {
        if (slotUIList.Count <= itemsViewport) return;

        float scrollPos = Mathf.Clamp(selectedItem - itemsViewport / 2, 0, selectedItem) * slotUIList[0].Hiegth;
        itemlistRect.localPosition = new Vector2(itemlistRect.localPosition.x, scrollPos);

        bool showUpArrow = selectedItem > itemsViewport / 2; //Si hay items arriba muestra la flecha arriba
        upArrow.gameObject.SetActive(showUpArrow);

        bool downUpArrow = selectedItem + itemsViewport / 2 < slotUIList.Count;// Si hay items abajo muestra la flecha abajo
        downArrow.gameObject.SetActive(downUpArrow);

    }

    void ResetSelection() //Permite resetar la seleccion
    {
        selectedItem = 0;
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);

        itemIcon.sprite = null;
        itemDescription.text = "";
    }

    void OpenPartyScreen() //Abre la pantalla de mostrar pokemones
    {
        state = InventoryUIState.PartySelection;
        //partyScreen.SetPartyData(party.Pokemons);
        partyScreen.gameObject.SetActive(true);
    }

    void ClosePartyScreen() //Cierra la pantalla de mostrar pokemones
    {
        state = InventoryUIState.ItemSelection;

        partyScreen.ClearMemberSlotMessages();
        partyScreen.gameObject.SetActive(false);
    }

    IEnumerator OnMoveToForgetSelected(int moveIndex)
    {
        var pokemon = partyScreen.SelectedMember;

        DialogManager.Instance.CloseDialog();
        moveSelectionUI.gameObject.SetActive(false);
        if (moveIndex == ApproachBase.MaxNumOfMoves)
        {
            //No va a aprender un nuevo movimiento
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} no va a aprender {moveToLearn.Name}");
        }
        else
        {
            //Olvida el movimeinto elegido y aprende el nuevo
            var selectedMove = pokemon.Moves[moveIndex].Base;
            yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} olvidó {selectedMove.Name} y aprendió {moveToLearn.Name}");


            pokemon.Moves[moveIndex] = new Move(moveToLearn);
        }

        moveToLearn = null;
        state = InventoryUIState.ItemSelection;
    }
}

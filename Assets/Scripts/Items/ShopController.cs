using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShopState { Menu, Buying, Selling, Busy }
public class ShopController : MonoBehaviour
{
    [SerializeField] Vector2 shopCameraOffset;
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] WalletUI walletUI;
    [SerializeField] CountSelectorUI countSelectorUI;
    [SerializeField] ShopUI shopUI;

    public event Action OnStart;
    public event Action OnFinish;


    ShopState state;

    Merchant merchant;

    public static ShopController i { get; private set; } //Patron singleton

    private void Awake()
    {
        i = this;
    }

    Inventory inventory;
    private void Start()
    {
        inventory = Inventory.GetInventory();
    }
    public IEnumerator StartTrading(Merchant merchant)
    {
        this.merchant = merchant;
        OnStart?.Invoke();
        yield return StartMenuState();
    }

    IEnumerator StartMenuState() //Muestra el menu de la opciones de la tienda
    {
        state = ShopState.Menu;

        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText("Como te puedo ayudar?",
            waitForInput: false,
            choices: new List<string>() { "Comprar", "Vender", "Salir" },
            onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            //Comprar
            yield return GameController.Instance.MoveCamera(shopCameraOffset);
            walletUI.Show();
            shopUI.Show(merchant.AvailableItems, (item) => StartCoroutine(BuyItem(item)),
                ()=> StartCoroutine(OnBackFromBuying()));

            state = ShopState.Buying;
        }
        else if (selectedChoice == 1)
        {
            //Vender
            state = ShopState.Selling;
            inventoryUI.gameObject.SetActive(true);
        }
        else if (selectedChoice == 2)
        {
            //Salir
            OnFinish?.Invoke();
            yield break;
        }
    }

    public void HandleUpdate()
    {
        if(state == ShopState.Selling)
        {

            inventoryUI.HandleUpdate(OnBackForSelling, (selectedItem) => StartCoroutine(SellItem(selectedItem)));
        }
        else if(state == ShopState.Buying)
        {
            shopUI.HandleUpdate();
        }
    }

    void OnBackForSelling()
    {
        inventoryUI.gameObject.SetActive(false);
        StartCoroutine(StartMenuState());
    }

    IEnumerator SellItem(ItemBase item) //Permite vender los items
    {
        state = ShopState.Busy;

        if (!item.IsSellable)
        {
            yield return DialogManager.Instance.ShowDialogText("No puede vender este item");
            state = ShopState.Selling;
            yield break;
        }

        walletUI.Show();

        float sellingPrice = Mathf.Round(item.Price / 2);
        int countToSell = 1;

        var itemCount = inventory.GetItemCount(item);
        if(itemCount > 1) //Permite escoger cuantos items vender
        {
            yield return DialogManager.Instance.ShowDialogText($"Cuantos te gustaria vender?",
                waitForInput: false, autoClose: false);

            yield return countSelectorUI.ShowSelector(itemCount, sellingPrice,
                (selectedCount) => countToSell = selectedCount);

            DialogManager.Instance.CloseDialog();
        }

        sellingPrice = sellingPrice * countToSell;

        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText($"Podria darte {sellingPrice} por eso, te parece bien?",
            waitForInput: false,
            choices: new List<string>() { "Si", "No" },
            onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

        if(selectedChoice == 0)
        {
            //Si
            inventory.RemoveItem(item, countToSell);
            //Darle dinero al jugador
            Wallet.i.AddMoney(sellingPrice);
            yield return DialogManager.Instance.ShowDialogText($"Entrego {item.Name} y recibio {sellingPrice}!");

        }

        walletUI.Close();

        state = ShopState.Selling;
    }

    IEnumerator BuyItem(ItemBase item)
    {
        state = ShopState.Busy;

        yield return DialogManager.Instance.ShowDialogText($"Cuantos te gustaria comprar?",
            waitForInput:false, autoClose: false);

        int countToBuy = 1;
        yield return countSelectorUI.ShowSelector(100, item.Price,
            (selectedCount) => countToBuy = selectedCount);

        DialogManager.Instance.CloseDialog();

        float totalPrice = item.Price * countToBuy;

        if (Wallet.i.HasMoney(totalPrice))
        {
            int selectedChoice = 0;
            yield return DialogManager.Instance.ShowDialogText($"Seria un total de {totalPrice}",
                waitForInput: false,
                choices: new List<string>() { "Si", "No" },
                onChoiceSelected: choiceIndex => selectedChoice = choiceIndex);

            if(selectedChoice == 0)
            {
                //Si va a comprar
                inventory.AddItem(item, countToBuy);
                Wallet.i.TakeMoney(totalPrice);
                yield return DialogManager.Instance.ShowDialogText($"Gracias por comprar :)");

            }
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"No tienes sufiente dinero :(");

        }

        state = ShopState.Buying;
    }

    IEnumerator OnBackFromBuying()
    {
        yield return GameController.Instance.MoveCamera(-shopCameraOffset);
        shopUI.Close();
        walletUI.Close();
        StartCoroutine(StartMenuState());
    }
}

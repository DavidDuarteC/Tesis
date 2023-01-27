using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] Image itemIcon;
    [SerializeField] Text itemDescription;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    int selectedItem;

    List<ItemBase> availableItems;
    Action<ItemBase> onItemSelected;
    Action onBack;

    List<ItemSlotUI> slotUIList;

    const int itemsViewport = 8;
    
    RectTransform itemlistRect;

    private void Awake()
    {
        itemlistRect = itemList.GetComponent<RectTransform>();
    }

    public void Show(List<ItemBase> availableItems, Action<ItemBase> onItemSelected,
        Action onBack)
    {
        this.availableItems = availableItems;
        this.onItemSelected = onItemSelected;
        this.onBack = onBack;

        gameObject.SetActive(true);
        UpdateItemList();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void HandleUpdate()
    {
        var prevSelection = selectedItem;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            selectedItem++;
        else if(Input.GetKeyDown(KeyCode.UpArrow))
            selectedItem--;

        selectedItem = Mathf.Clamp(selectedItem, 0, availableItems.Count - 1);

        if (selectedItem != prevSelection)
            UpdateItemSelection();

        if (Input.GetKeyDown(KeyCode.Z))
            onItemSelected?.Invoke(availableItems[selectedItem]);
        else if (Input.GetKeyDown(KeyCode.X))
            onBack?.Invoke();
    }

    void UpdateItemList()
    {
        //Limpiar la lista de objetos existentes
        foreach (Transform child in itemList.transform)
            Destroy(child.gameObject);

        slotUIList = new List<ItemSlotUI>();
        foreach (var item in availableItems)
        {
            var slotUIObj = Instantiate(itemSlotUI, itemList.transform);
            slotUIObj.SetNameAndPrice(item);

            slotUIList.Add(slotUIObj);
        }


        UpdateItemSelection();
    }

    void UpdateItemSelection() //Genera la vista para ver cual opcion esta seleccionada
    {
        selectedItem = Mathf.Clamp(selectedItem, 0, availableItems.Count - 1);

        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i == selectedItem)
                slotUIList[i].NameText.color = GlobalSettings.i.HighlightedColor;
            else
                slotUIList[i].NameText.color = Color.black;
        }


        if (availableItems.Count > 0)
        {
            var item = availableItems[selectedItem];
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
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject menu;

    public event Action<int> onMenuSelected;
    public event Action onBack;


    List<Text> menuItems;

    int selectedItem = 0;

    private void Awake()
    {
        menuItems = menu.GetComponentsInChildren<Text>().ToList();
    }

    public void OpenMenu() //Abre el menu
    {
        menu.SetActive(true);
        UpdateItemSelection();
    }

    public void CloseMenu() //Cierra el menu
    {
        menu.SetActive(false);
    }

    public void HandelUpdate() //accion de escoger cada una de las opciones del menu
    {
        int prevSelection = selectedItem;

        if (Input.GetKeyUp(KeyCode.DownArrow))
            ++selectedItem;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            --selectedItem;

        selectedItem = Mathf.Clamp(selectedItem, 0, menuItems.Count - 1);

        if(prevSelection != selectedItem)
            UpdateItemSelection();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onMenuSelected?.Invoke(selectedItem);
            CloseMenu();
        }
        else if(Input.GetKeyDown(KeyCode.X)) 
        {
            onBack?.Invoke();
            CloseMenu();
        }
    }

    void UpdateItemSelection() //Genera la vista para ver cual opcion esta seleccionada
    {
        for (int i = 0; i < menuItems.Count; i++)
        {
            if (i == selectedItem)
                menuItems[i].color = GlobalSettings.i.HighlightedColor;
            else
                menuItems[i].color = Color.black;
        }
    }
}

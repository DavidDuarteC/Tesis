using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new collectible item")]

public class CollectibleItem: ItemBase
{
    [SerializeField] string dialog;

    public void showDialog()
    {
        //DialogManager.Instance.ShowDialog(dialog);
    }

    public string Dialog => dialog;
}

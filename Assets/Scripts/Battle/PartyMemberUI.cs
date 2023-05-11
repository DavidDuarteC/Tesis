using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] Text messageText;

    Approach _approach;
    public void Init(Approach approach) //Asigna el nombre y el nivel para mostrarlo en pantalla
    {
        _approach = approach;
        UpdateData();
        SetMessage("");

        _approach.OnHPChanged += UpdateData;
        
    }

    void UpdateData()
    {
        nameText.text = _approach.Base.Name;
        levelText.text = "Lvl " + _approach.Level;
        hpBar.SetHP((float)_approach.HP / _approach.MaxHp);
    }
    public void SetSelected(bool selected)
    {
        if (selected) 
            nameText.color = GlobalSettings.i.HighlightedColor;
        else
            nameText.color = Color.black;
    }

    public void SetMessage(string message)
    {
        messageText.text = message; 
    }
}

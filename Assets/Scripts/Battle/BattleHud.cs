using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text statusText;
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject expBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;

    Approach _approach;

    Dictionary<ConditionID, Color> statusColors;
    public void SetData(Approach approach) //Asigna el nombre y el nivel para mostrarlo en pantalla
    {
        if (_approach != null)
        {
            _approach.OnHPChanged -= UpdateHP;
            _approach.OnStatusChanged -= SetStatusText;
        }

        _approach = approach;

        nameText.text = approach.Base.Name;
        SetLevel();
        hpBar.SetHP((float)approach.HP / approach.MaxHp);
        SetExp();

        statusColors = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn, psnColor},
            {ConditionID.brn, brnColor},
            {ConditionID.slp, slpColor},
            {ConditionID.par, parColor},
            {ConditionID.frz, frzColor},
        };

        SetStatusText();
        _approach.OnStatusChanged += SetStatusText;
        _approach.OnHPChanged += UpdateHP;
    }

    void SetStatusText()
    {
        if(_approach.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = _approach.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[_approach.Status.Id];
        }
    }

    public void SetLevel()
    {
        levelText.text = "Lvl " + _approach.Level;

    }

    public void SetExp()
    {
        if (expBar == null) return;

        float normalizedExp =  GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);

    }

    public IEnumerator SetExpSmooth(bool reset = false)
    {
        if (expBar == null) yield break;

        if (reset)
            expBar.transform.localScale = new Vector3(0, 1, 1);

        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }

    float GetNormalizedExp()
    {
        int currLevelExp = _approach.Base.GetExpForLevel(_approach.Level);
        int nextLevelExp = _approach.Base.GetExpForLevel(_approach.Level + 1);

        float normalizedExp = (float)(_approach.Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }

   

    public void UpdateHP()
    {
        if(_approach.HpChanged)
        {
            _approach.HpChanged = false;
        }
        StartCoroutine(UpdateHPAsync());
    }

    public IEnumerator UpdateHPAsync()
    {
        yield return hpBar.SetHPSmooth((float)_approach.HP / _approach.MaxHp);
    }

    public void ClearData()
    {
        if (_approach != null)
        {
            _approach.OnHPChanged -= UpdateHP;
            _approach.OnStatusChanged -= SetStatusText;
        }
    }

}

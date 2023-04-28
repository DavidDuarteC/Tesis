using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new pokeball")]
public class PokeBallItem : ItemBase
{
    [SerializeField] float catchRateModifier = 1;

    public override bool Use(Approach pokemon)
    {
        return true;
    }

    public override bool CanUseOutsideBattle => false;

    public float CatchRateModifier {
        get { return catchRateModifier; }
        set { catchRateModifier = value; }
    }

}

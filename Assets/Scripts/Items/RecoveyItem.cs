using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new recovery item")]
public class RecoveyItem : ItemBase
{
    [Header("HP")]
    [SerializeField] int hpAmount;
    [SerializeField] bool restoreMaxHp;

    [Header("PP")]
    [SerializeField] int ppAmount;
    [SerializeField] bool restoreMaxPP;

    [Header("Status Conditions")]
    [SerializeField] ConditionID status;
    [SerializeField] bool recoveryAllStatus;

    [Header("Revive")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;

    public override bool Use(Approach approach)
    {
        //Revivir
        if(revive || maxRevive)
        {
            if(approach.HP > 0)
                return false;

            if (revive)
                approach.IncreaseHP(approach.MaxHp / 2);
            else if(maxRevive)
                approach.IncreaseHP(approach.MaxHp);

            approach.CureStatus();

            return true;
        }

        //No se puede usar otros items en approaches derrotados
        if(approach.HP == 0)
            return false;

        //Restaurar HP
        if(restoreMaxHp  || hpAmount > 0)
        {
            if (approach.HP == approach.MaxHp)
                return false;

            if (restoreMaxHp)
                approach.IncreaseHP(approach.MaxHp);
            else
                approach.IncreaseHP(hpAmount);


        }

        //Recuperarse de Status
        if(recoveryAllStatus || status != ConditionID.none)
        {
            if (approach.Status == null && approach.VolatileStatus == null)
                return false;

            if (recoveryAllStatus)
            {
                approach.CureStatus();
                approach.CureVolatileStatus();
            }
            else
            {
                if (approach.Status.Id == status)
                    approach.CureStatus();
                else if (approach.VolatileStatus.Id == status)
                    approach.CureVolatileStatus();
                else
                    return false;
            }
        }

        //Restaurar el PP
        if (restoreMaxPP)
        {
            approach.Moves.ForEach(m => m.IncreasePP(m.Base.PP));
        }
        else if(ppAmount > 0)
        {
            approach.Moves.ForEach(m => m.IncreasePP(ppAmount));

        }

        return true;
    }
}

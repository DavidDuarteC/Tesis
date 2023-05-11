using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach (var kvp in Conditions)
        {
            var contidionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = contidionId;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Veneno",
                StartMessage = "ha sido envenenado",
                OnAfterTurn = (Approach approach) =>
                {
                    approach.DecreaseHP(approach.MaxHp / 8);
                    approach.StatusChanges.Enqueue($"{approach.Base.Name} se lastimo debido al veneno"); 
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Quemar",
                StartMessage = "ha sido quemado",
                OnAfterTurn = (Approach approach) =>
                {
                    approach.DecreaseHP(approach.MaxHp / 16);
                    approach.StatusChanges.Enqueue($"{approach.Base.Name} se lastimo debido a la quemadura");
                }
            }
        },
        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralizis",
                StartMessage = "ha sido paralizado",
                OnBeforeMove = (Approach approach) =>
                {
                    if(Random.Range(1,5) == 1)
                    {
                        approach.StatusChanges.Enqueue($"{approach.Base.Name} esta paralizado y no se puede mover");
                        return false;
                    }
                    return true;
                }
            }
        },
        {
            ConditionID.frz,
            new Condition()
            {
                Name = "Congelado",
                StartMessage = "ha sido congelado",
                OnBeforeMove = (Approach approach) =>
                {
                    if(Random.Range(1,5) == 1)
                    {
                        approach.CureStatus();
                        approach.StatusChanges.Enqueue($"{approach.Base.Name} no esta congelado");
                        return true;
                    }
                    return false;
                }
            }
        },
        {
            ConditionID.slp,
            new Condition()
            {
                Name = "Dormido",
                StartMessage = "se ha dormido",
                OnStart = (Approach approach) =>
                {
                    //Duerme entre 1 y 3 turnos
                    approach.StatusTime  = Random.Range(1,4);
                    Debug.Log($"Estara dormido por {approach.StatusTime} movimientos");
                },
                OnBeforeMove = (Approach approach) =>
                {
                    if (approach.StatusTime <= 0)
                    {
                        approach.CureStatus();
                        approach.StatusChanges.Enqueue($"{approach.Base.Name} desperto!");
                        return true;
                    }
                    approach.StatusTime --;
                    approach.StatusChanges.Enqueue($"{approach.Base.Name} esta durmiendo");
                    return false;
                }
            }
        },
        //Volatile Status Conditions
        {
            ConditionID.confusion,
            new Condition()
            {
                Name = "Confusion",
                StartMessage = "esta confuso",
                OnStart = (Approach approach) =>
                {
                    //confuso entre 1 y 4 turnos
                    approach.VolatileStatusTime  = Random.Range(1,5);
                    Debug.Log($"Estara confundido por {approach.VolatileStatusTime} movimientos");
                },
                OnBeforeMove = (Approach approach) =>
                {
                    if (approach.VolatileStatusTime <= 0)
                    {
                        approach.CureVolatileStatus();
                        approach.StatusChanges.Enqueue($"{approach.Base.Name} ya no esta confuso!");
                        return true;
                    }
                    approach.VolatileStatusTime --;

                    //50% de hacer un movimiento
                    if(Random.Range(1,3) == 1)
                        return true;

                    //Golpe por confusion
                    approach.StatusChanges.Enqueue($"{approach.Base.Name} esta confundido");
                    approach.DecreaseHP(approach.MaxHp /8);
                    approach.StatusChanges.Enqueue($"Se golpeo solo, por la confusion");
                    return false;
                }
            }
        },

    };

    public static float GetStatusBonus(Condition condition)
    {
        if (condition == null)
            return 1f;
        else if (condition.Id == ConditionID.slp || condition.Id == ConditionID.frz)
            return 2f;
        else if (condition.Id == ConditionID.par || condition.Id == ConditionID.psn || condition.Id == ConditionID.brn)
            return 1.5f;

        return 1f;
    }
}

public enum ConditionID
{
    none, psn, brn, slp, par, frz, confusion
}

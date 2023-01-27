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
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.DecreaseHP(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} se lastimo debido al veneno"); 
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Quemar",
                StartMessage = "ha sido quemado",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.DecreaseHP(pokemon.MaxHp / 16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} se lastimo debido a la quemadura");
                }
            }
        },
        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralizis",
                StartMessage = "ha sido paralizado",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(Random.Range(1,5) == 1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} esta paralizado y no se puede mover");
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
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(Random.Range(1,5) == 1)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} no esta congelado");
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
                OnStart = (Pokemon pokemon) =>
                {
                    //Duerme entre 1 y 3 turnos
                    pokemon.StatusTime  = Random.Range(1,4);
                    Debug.Log($"Estara dormido por {pokemon.StatusTime} movimientos");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.StatusTime <= 0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} desperto!");
                        return true;
                    }
                    pokemon.StatusTime --;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} esta durmiendo");
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
                OnStart = (Pokemon pokemon) =>
                {
                    //confuso entre 1 y 4 turnos
                    pokemon.VolatileStatusTime  = Random.Range(1,5);
                    Debug.Log($"Estara confundido por {pokemon.VolatileStatusTime} movimientos");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.VolatileStatusTime <= 0)
                    {
                        pokemon.CureVolatileStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} ya no esta confuso!");
                        return true;
                    }
                    pokemon.VolatileStatusTime --;

                    //50% de hacer un movimiento
                    if(Random.Range(1,3) == 1)
                        return true;

                    //Golpe por confusion
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} esta confundido");
                    pokemon.DecreaseHP(pokemon.MaxHp /8);
                    pokemon.StatusChanges.Enqueue($"Se golpeo solo, por la confusion");
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

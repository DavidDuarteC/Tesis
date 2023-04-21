using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create a new pokemon")] //Crea una instancia del menu para acceder desde unity y crear el objeto
public class PokemonBase : ScriptableObject //Objeto pokemon
{
    [SerializeField] new string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    //Estaditicas base
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [SerializeField] int expYield;
    [SerializeField] GrowRate growRate;

    [SerializeField] int catchRate = 255;

    [SerializeField] List<LearnableMove> learnableMoves;
    [SerializeField] List<MoveBase> learnableByItems;

    [SerializeField] List<Evolution> evolutions;

    public static int MaxNumOfMoves { get; set; } = 4;

    public int GetExpForLevel(int level)
    {
        if(growRate == GrowRate.Fast)
        {
            return 4 * (level * level * level) / 5;
        }
        else if(growRate == GrowRate.MediumFast) 
        {
            return level * level * level;
        }

        return -1;
    }
    public string Name
    {
        get { return name;}
    }

    public string Description
    {
        get { return description;}
    }

    public Sprite FrontSprite
    {
        get { return frontSprite;}
    }

    public Sprite BackSprite
    {
        get { return backSprite;}
    }

    public PokemonType Type1
    {
        get { return type1; }
    }

    public PokemonType Type2
    {
        get { return type2; }
    }

    public int MaxHp
    {
        get { return maxHp;}
    }

    public int Attack
    {
        get { return attack;}
    }

    public int Defense
    {
        get { return defense;}
    }

    public int SpAttack
    {
        get { return spAttack;}
    }
    public int SpDefense
    {
        get { return spDefense;}
    }
    public int Speed
    {
        get { return speed;}
    }

    public List<LearnableMove> LearnableMoves
    {
        get { return learnableMoves; }
    }

    public List<MoveBase> LearnableByItems
    {
        get { return learnableByItems;  } 
    }

    public List<Evolution> Evolutions => evolutions;

    public int CatchRate => catchRate;

    public int ExpYield => expYield;

    public GrowRate GrowRate => growRate;
}

[System.Serializable]
public class LearnableMove //Crea la funcion para que un pokemon pueda aprender un movimiento
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base
    {
        get { return moveBase;}
    }

    public int Level
    {
        get { return level;}
    }
}

[Serializable]
public class Evolution
{
    [SerializeField] PokemonBase evolvesInto;
    [SerializeField] int requiredLevel;
    [SerializeField] EvolutionItem requiredItem;

    public PokemonBase EvolvesInto => evolvesInto;
    public int RequiredLevel => requiredLevel;
    public ItemBase RequiredItem => requiredItem;

}

public enum PokemonType
{
    None,
    Java,
    CPlusPlus,
    C,
    Python,
    Angular,
    Linux,
    Typescript,
    SQL,
    Kotlin,
    Null
   
}

public enum GrowRate
{
    Fast, MediumFast
}

public enum Stat
{
    Attack,
    Defense,
    SpAttack, 
    SpDefense, 
    Speed, 

    
    Accuracy,
    Evasion
}

public class TypeChart
{
    static float[][] chart =
    {
        //                             JAV   CPP   C     PHY   ANG   LIN   TPS   SQL   KTL   NULL
        /*Java*/        new float [] { 1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   0f},
        /*CPlusPlus*/   new float [] { 1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   0f},
        /*C*/           new float [] { 1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   0f},
        /*Phyton*/      new float [] { 1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   0f},
        /*Angular*/     new float [] { 1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   0f},
        /*Linux*/       new float [] { 1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   0f},
        /*Typescript*/  new float [] { 1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   0f},
        /*SQL*/         new float [] { 1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   0f},        
        /*Kotlin*/      new float [] { 1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   0f},
        /*Null*/        new float [] { 0f,   0f,   0f,   0f,   0f,   0f,   0f,   0f,   0f,   0f} 
    };

    public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
    {
        if(attackType == PokemonType.None || defenseType == PokemonType.None) 
            return 1;

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}
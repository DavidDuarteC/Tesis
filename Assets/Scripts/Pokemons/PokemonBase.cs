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
    Normal,
    Fire,
    Water,
    Eletric,
    Grass,
    Ice,
    Fighting,
    Posion,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon,
    Dark,
    Steel,
    Fairy
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
        //                         NOR   FIR   WAT   ELC   GRA   ICE   FIG   POI   GRO   FLY   PSY   BUG
        /*Normal*/  new float [] { 1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f},
        /*Fire*/    new float [] { 1f,   0.5f, 0.5f, 1f,   2f,   2f,   1f,   1f,   1f,   1f,   1f,   2f},
        /*Water*/   new float [] { 1f,   2f,   0.5f, 2f,   0.5f, 1f,   1f,   1f,   2f,   1f,   1f,   1f},
        /*Electric*/new float [] { 1f,   1f,   2f,   0.5f, 0.5f, 2f,   1f,   1f,   0f,   2f,   1f,   1f},
        /*Grass*/   new float [] { 1f,   0.5f, 2f,   2f,   0.5f, 1f,   1f,   0.5f, 2f,   0.5f, 1f,   0.5f},
        /*Ice*/     new float [] { 1f,   0.5f, 0.5f, 1f,   2f,   0.5f, 1f,   1f,   2f,   2f,   1f,   1f},
        /*Fighting*/new float [] { 2f,   1f,   1f,   1f,   1f,   2f,   1f,   0.5f, 1f,   0.5f, 0.5f, 0.5f},
        /*Poison*/  new float [] { 1f,   1f,   1f,   1f,   2f,   1f,   1f,   0.5f, 0.5f, 1f,   1f,   1f},        
        /*Ground*/  new float [] { 1f,   2f,   1f,   2f,   0.5f, 1f,   1f,   2f,   1f,   0f,   1f,   0.5f},
        /*Flying*/  new float [] { 1f,   1f,   1f,   0.5f, 2f,   1f,   2f,   1f,   1f,   1f,   1f,   2f},
        /*Psychic*/ new float [] { 1f,   1f,   1f,   1f,   1f,   1f,   2f,   2f,   1f,   1f,   0.5f, 1f},
        /*Bug*/     new float [] { 1f,   0.5f, 1f,   1f,   2f,   1f,   0.5f, 0.5f, 1f,   0.5f, 2f,   1f},
        /*Rock*/    new float [] { 1f,   2f,   1f,   1f,   1f,   2f,   0.5f, 1f,   0.5f, 2f,   1f,   2f},
        /*Ghost*/   new float [] { 0f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   0.5f, 1f},
        /*Dragon*/  new float [] { 1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f},
        /*Dark*/    new float [] { 1f,   1f,   1f,   1f,   1f,   1f,   0.5f, 1f,   1f,   1f,   2f,   1f},
        /*Steel*/   new float [] { 1f,   0.5f, 0.5f, 0.5f, 1f,   2f,   1f,   1f,   1f,   1f,   1f,   2f},
        /*Fairy*/   new float [] { 1f,   0.5f, 1f,   1f,   1f,   1f,   2f,   0.5f, 1f,   1f,   1f,   1f}

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
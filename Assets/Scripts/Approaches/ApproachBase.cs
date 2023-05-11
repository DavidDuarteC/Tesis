using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Approach", menuName = "Approach/Create a new approach")] //Crea una instancia del menu para acceder desde unity y crear el objeto
public class ApproachBase : ScriptableObject //Objeto enfoque
{
    [SerializeField] new string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] ApproachType type1;
    [SerializeField] ApproachType type2;

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

    public ApproachType Type1
    {
        get { return type1; }
    }

    public ApproachType Type2
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

    public int CatchRate => catchRate;

    public int ExpYield => expYield;

    public GrowRate GrowRate => growRate;
}

[System.Serializable]
public class LearnableMove //Crea la funcion para que un approach pueda aprender un movimiento
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

public enum ApproachType
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
    Cto
   
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
        //                             JAV   CPP   C     PHY   ANG   LIN   TPS   SQL   KTL   CN
        /*Java*/        new float [] { 1f,   0f,   0f,   0f,   0f,   0f,   0f,   0f,   0f,   2f},
        /*CPlusPlus*/   new float [] { 0f,   1f,   0f,   0f,   0f,   0f,   0f,   0f,   0f,   2f},
        /*C*/           new float [] { 0f,   0f,   1f,   0f,   0f,   0f,   0f,   0f,   0f,   2f},
        /*Phyton*/      new float [] { 0f,   0f,   0f,   1f,   0f,   0f,   0f,   0f,   0f,   2f},
        /*Angular*/     new float [] { 0f,   0f,   0f,   0f,   1f,   0f,   0f,   0f,   0f,   2f},
        /*Linux*/       new float [] { 0f,   0f,   0f,   0f,   0f,   1f,   0f,   0f,   0f,   2f},
        /*Typescript*/  new float [] { 0f,   0f,   0f,   0f,   0f,   0f,   1f,   0f,   0f,   2f},
        /*SQL*/         new float [] { 0f,   0f,   0f,   0f,   0f,   0f,   0f,   1f,   0f,   2f},        
        /*Kotlin*/      new float [] { 0f,   0f,   0f,   0f,   0f,   0f,   0f,   0f,   1f,   2f},
        /*Conocimien*/  new float [] { 0f,   0f,   0f,   0f,   0f,   0f,   0f,   0f,   0f,   0f} 
    };

    public static float GetEffectiveness(ApproachType attackType, ApproachType defenseType)
    {
        if(attackType == ApproachType.None || defenseType == ApproachType.None) 
            return 1;

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}
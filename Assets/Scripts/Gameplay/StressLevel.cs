using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StressLevel : MonoBehaviour, ISavable
{
    [SerializeField] float level = 0;
    //[SerializeField] bool lostMoreTimeRow = false;
    [SerializeField] float loseMultiply = 0.1f;


    public event Action OnStressLevelChange;

    public static StressLevel i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    public void Init()
    {
        OnStressLevelChange?.Invoke();
    }

    public void AddLevel(float newLevel)
    {
        //if (lostMoreTimeRow)
        //    level += newLevel * 2;
        level += newLevel;

        //lostMoreTimeRow = lost? true: false;
        OnStressLevelChange?.Invoke();
    }


    public void ResetLevel()
    {
        level = 0;
        OnStressLevelChange?.Invoke();
    }

    public object CaptureState()
    {
        var saveData = new StressLevelData()
        {
            level = this.level,
            //lostMoreTimeRow = this.lostMoreTimeRow
        };
        return saveData;
    }

    public void RestoreState(object state)
    {
        var savedData = state as StressLevelData;
        this.level = savedData.level;
        //this.lostMoreTimeRow = savedData.lostMoreTimeRow;
        OnStressLevelChange?.Invoke();
    }

    public float Level => level;

    //public bool LostMoreTimeRow
    //{
    //    get { return lostMoreTimeRow; }
    //    set { lostMoreTimeRow = value; }
    //}

    public float LoseMultiply => loseMultiply;
}

[Serializable]
public class StressLevelData
{
    public float level = 0;
    public bool lostMoreTimeRow = false;
}

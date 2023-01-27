using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CutsceneAction
{
    [SerializeField] string name;
    [SerializeField] bool waitForCompletion = true;

    public virtual IEnumerator Play()
    {
        yield break;
    }

    public string Name
    {
        get => name;
        set => name = value;
    }

    public bool WaitForCompletion => waitForCompletion;
}

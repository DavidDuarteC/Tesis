using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Cutscene : MonoBehaviour, IPlayerTriggerable, ISavable
{
    [SerializeReference]
    [SerializeField] List<CutsceneAction> actions;
    bool active = true;
    public bool TriggerRepeatedly => false;

    public static Cutscene i;

    private void Awake()
    {
        i = this;
    }
    private void Start()
    {
        i = this;
        active = i.isActiveAndEnabled;
    }

    public IEnumerator Play()
    {
        GameController.Instance.StartCutsceneState();

        foreach (var action in actions)
        {
            if(action.WaitForCompletion)
                yield return action.Play();
            else
                StartCoroutine(action.Play());
        }

        GameController.Instance.StartFreeRoamState();
    }

    public void AddAction(CutsceneAction action)
    {
#if UNITY_EDITOR
        Undo.RegisterCompleteObjectUndo(this, "Add action to cutscene.");
#endif

        action.Name = action.GetType().ToString();
        actions.Add(action);
    }

    public void OnPlayerTrigger(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        StartCoroutine(Play());
    }

    public object CaptureState()
    {
        var save = i.isActiveAndEnabled;
        return save;
    }

    public void RestoreState(object state)
    {
        var save = (bool)state;
        active = save;
        i.gameObject.SetActive(save);
    }
}

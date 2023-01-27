using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Quest
{
    public QuestBase Base { get; private set; }
    public QuestStatus Status { get; private set; }

    public Quest(QuestBase _base)
    {
        Base = _base;
    }

    public Quest(QuestSaveData saveData)
    {
        Base = QuestDB.GetObjectByName(saveData.name);
        Status = saveData.status;
    }

    public QuestSaveData GetSaveData()
    {
        var saveData = new QuestSaveData()
        {
            name = Base.name,
            status = Status
        };
        return saveData;
    }


    public IEnumerator StartQuest() //Empieza el quest
    {
        Status = QuestStatus.Started;

        yield return DialogManager.Instance.ShowDialog(Base.StartDialog);

        var questList = QuestList.GetQuestList();
        questList.AddQuest(this); //Mete cada quest en una lista 
    }

    public IEnumerator CompleteQuest(Transform player)//Si el quest es completado
    {
        Status = QuestStatus.Completed;

        yield return DialogManager.Instance.ShowDialog(Base.CompletedDialog);

        var inventory = Inventory.GetInventory();
        if(Base.RequiredItem != null)
        {
            inventory.RemoveItem(Base.RequiredItem);
        }

        if(Base.RewardItem != null)
        {
            inventory.AddItem(Base.RewardItem);

            string playerName = player.GetComponent<PlayerController>().Name;
            yield return DialogManager.Instance.ShowDialogText($"{playerName} recibio {Base.RewardItem.Name}");
        }

        var questList = QuestList.GetQuestList();
        questList.AddQuest(this); //Mete cada quest en una lista
    }

    public bool CanBeCompleted() //Comprueba si el quest se completo
    {
        var inventory = Inventory.GetInventory();
        if(Base.RequiredItem != null)
        {
            if (!inventory.HasItem(Base.RequiredItem))
                return false;
        }

        return true;
    }
}

[Serializable]
public class QuestSaveData
{
    public string name;
    public QuestStatus status;
}

public enum QuestStatus
{
    None, Started, Completed
}

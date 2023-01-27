using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerTriggerable
{
    void OnPlayerTrigger(PlayerController player);

    bool TriggerRepeatedly { get; }
}

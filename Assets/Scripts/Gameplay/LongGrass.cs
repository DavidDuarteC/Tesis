using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class LongGrass : MonoBehaviour
{

    //public void OnPlayerTrigger(PlayerController player)
    //{
    //    if (UnityEngine.Random.Range(1, 101) <= 10)
    //    {
    //        player.Character.Animator.IsMoving = false;
    //        GameController.Instance.StartBattle(BattleTrigger.LongGrass); //Genera los encuentros con los pokemones
    //        //Debug.Log("Encontraste un pokemon");
    //    }
    //}

    public bool TriggerRepeatedly => true;

}

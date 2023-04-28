using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SurfableWater : MonoBehaviour, Interactable
{
    bool isJumpongToWater = false;

    public bool TriggerRepeatedly => true;//Cada cuento se va a repetir el trigger

    public IEnumerator Interact(Transform initiator) //Permite interactuar con el agua para surfear
    {
        var animator = initiator.GetComponent<CharacterAnimator>();
        if (animator.IsSurfing || isJumpongToWater)
            yield break;


        yield return DialogManager.Instance.ShowDialogText("El agua esta fría!");

        var pokemonWithSurf = initiator.GetComponent<ApproachParty>().Pokemons.FirstOrDefault(p => p.Moves.Any(m => m.Base.Name == "Surf"));

        if (pokemonWithSurf != null)
        {
            int selectedChoice = 0;
            yield return DialogManager.Instance.ShowDialogText($"Debería {pokemonWithSurf.Base.Name} usar surf?",
                choices: new List<string>() { "Si", "No" },
                onChoiceSelected: (selection) => selectedChoice = selection);

            if (selectedChoice == 0)
            {
                //Si
                yield return DialogManager.Instance.ShowDialogText($"{pokemonWithSurf.Base.Name} usa surf!");

                var dir = new Vector3(animator.MoveX, animator.MoveY);
                var targetPos = initiator.position + dir;

                isJumpongToWater = true;
                yield return initiator.DOJump(targetPos, 0.3f, 1, 0.5f).WaitForCompletion();
                isJumpongToWater = false;

                animator.IsSurfing = true;
            }
        }
    }

    //public void OnPlayerTrigger(PlayerController player)
    //{
    //    if (UnityEngine.Random.Range(1, 101) <= 10)
    //    {
    //        GameController.Instance.StartBattle(BattleTrigger.Water); //Genera los encuentros con los pokemones
    //        //Debug.Log("Encontraste un pokemon");
    //    }
    //}
}

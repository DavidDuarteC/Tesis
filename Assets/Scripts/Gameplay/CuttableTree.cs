using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CuttableTree : MonoBehaviour, Interactable
{
    public IEnumerator Interact(Transform initiator) //Interactura con el arbol y si un pokemon tiene cut puede cortarlo
    {
        yield return DialogManager.Instance.ShowDialogText("Este arbol parace que se puede cortar");

        var pokemonWithCut = initiator.GetComponent<ApproachParty>().Pokemons.FirstOrDefault(p => p.Moves.Any(m => m.Base.Name =="Cut"));

        if(pokemonWithCut != null)
        {
            int selectedChoice = 0;
            yield return DialogManager.Instance.ShowDialogText($"Debería {pokemonWithCut.Base.Name} usar corte?", 
                choices: new List<string>() { "Si", "No"},
                onChoiceSelected: (selection) => selectedChoice = selection);

            if(selectedChoice == 0)
            {
                //Si
                yield return DialogManager.Instance.ShowDialogText($"{pokemonWithCut.Base.Name} usa corte!");
                gameObject.SetActive(false);
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Healer : MonoBehaviour
{
    public IEnumerator Heal(Transform player, Dialog dialog) // Permite curar los pokemones que tenga el personaje
    {
        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText("Te ves cansado, deberias descansar un poco", 
            choices:  new List<string>() {"Yes", "No"},
            onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);

        if(selectedChoice == 0)
        {
            //Yes
            yield return Fader.i.FadeIn(0.5f);

            var playerParty = player.GetComponent<PokemonParty>();
            playerParty.Pokemons.ForEach(p => p.Heal());
            playerParty.ParyUpdate();

            yield return Fader.i.FadeOut(0.5f);

            yield return DialogManager.Instance.ShowDialogText("Tus pokemones estan curados");

        }
        else if(selectedChoice == 1)
        {
            //No
            yield return DialogManager.Instance.ShowDialogText("Ok, vuelve cuando quieras");
        }

        
    }
}

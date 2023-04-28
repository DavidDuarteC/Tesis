using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApproachGiver : MonoBehaviour, ISavable
{
    [SerializeField] Approach pokemonToGive;
    [SerializeField] Dialog dialog;

    bool used = false;

    public IEnumerator GivePokemon(PlayerController player) //Permite recibir un pokemon de un NPC
    {
        yield return DialogManager.Instance.ShowDialog(dialog);

        pokemonToGive.Init();
        player.GetComponent<ApproachParty>().AddPokemon(pokemonToGive);

        used = true;

        AudioManager.i.PlaySfx(AudioId.PokemonObtained, pauseMusic: true);

        string dialogText = $"{player.Name} recibe {pokemonToGive.Base.Name}";

        yield return DialogManager.Instance.ShowDialogText(dialogText);
    }

    public bool CanBeGiven()
    {
        return pokemonToGive != null && !used;
    }

    public object CaptureState()
    {
        return used;
    }

    public void RestoreState(object state)
    {
        used = (bool)state;
    }
}

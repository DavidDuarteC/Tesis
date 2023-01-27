using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionManager : MonoBehaviour
{
    [SerializeField] GameObject evolutionUI;
    [SerializeField] Image pokemonImage;

    [SerializeField] AudioClip evolutionMusic;

    public event Action OnStartEvolution;
    public event Action OnCompleteEvolution;


    public static EvolutionManager i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    public IEnumerator Evolve(Pokemon pokemon, Evolution evolution) //Verifica si un pokemon puede evolucionar
    {
        OnStartEvolution?.Invoke();
        evolutionUI.SetActive(true);

        AudioManager.i.PlayMusic(evolutionMusic);

        pokemonImage.sprite = pokemon.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} esta evolucionando");


        var oldPPokemon = pokemon.Base;
        pokemon.Evolve(evolution);

        pokemonImage.sprite = pokemon.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{oldPPokemon.Name} evoluciono a {pokemon.Base.Name}");

        evolutionUI.SetActive(false);
        OnCompleteEvolution?.Invoke();
    }
}

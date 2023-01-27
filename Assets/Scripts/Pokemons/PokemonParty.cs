using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;

    public event Action OnUpdated; //Patron observable

    public List<Pokemon> Pokemons
    {
        get { return pokemons; }
        set {
            pokemons = value;
            OnUpdated?.Invoke();
        }
    }

    private void Awake()
    {
        foreach (var pokemon in pokemons)
        {
            pokemon.Init();
        }
    }

    private void Start()
    {
        
    }

    public Pokemon GetHealthyPokemon() //Me da el primer pokemon vivo que tengo en mi lista
    {
        return pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddPokemon(Pokemon newPokemon)
    {
        if (pokemons.Count < 6)
        {
            pokemons.Add(newPokemon);
            OnUpdated?.Invoke();
        }
        else
        {
            //Hacer: Implementar Pc
        }
    }

    public bool CheckForEvolutions() //Comprueba si tengo un pokemon con posibilidad de evolucionar
    {
        return pokemons.Any(p => p.CheckForEvolution() != null);
    }

    public IEnumerator RunEvolutions() //Ejcuta la evolucion
    {
        foreach (var pokemon in pokemons)
        {
            var evolution = pokemon.CheckForEvolution();
            if(evolution != null)
            {
                yield return EvolutionManager.i.Evolve(pokemon, evolution);
            }
        }
    }

    public void ParyUpdate()
    {
        OnUpdated?.Invoke();
    }

    public static PokemonParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<PokemonParty>();
    }
}

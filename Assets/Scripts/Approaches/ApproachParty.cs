using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ApproachParty : MonoBehaviour
{
    [SerializeField] List<Approach> pokemons;

    public event Action OnUpdated; //Patron observable

    public List<Approach> Pokemons
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

    public Approach GetHealthyPokemon() //Me da el primer pokemon vivo que tengo en mi lista
    {
        return pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddPokemon(Approach newPokemon)
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

    public void PartyUpdate()
    {
        OnUpdated?.Invoke();
    }

    public static ApproachParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<ApproachParty>();
    }
}

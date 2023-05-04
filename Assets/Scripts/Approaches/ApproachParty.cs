using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ApproachParty : MonoBehaviour
{
    [SerializeField] List<Approach> approaches;

    public event Action OnUpdated; //Patron observable

    public List<Approach> Pokemons
    {
        get { return approaches; }
        set {
            approaches = value;
            OnUpdated?.Invoke();
        }
    }

    private void Awake()
    {
        foreach (var pokemon in approaches)
        {
            pokemon.Init();
        }
    }

    private void Start()
    {
        
    }

    public Approach GetHealthyPokemon() //Me da el primer pokemon vivo que tengo en mi lista
    {
        return approaches.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddPokemon(Approach newPokemon)
    {
        if (approaches.Count < 6)
        {
            approaches.Add(newPokemon);
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

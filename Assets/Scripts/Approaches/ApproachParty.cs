using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ApproachParty : MonoBehaviour
{
    [SerializeField] List<Approach> approaches;

    public event Action OnUpdated; //Patron observable

    public List<Approach> Approaches
    {
        get { return approaches; }
        set {
            approaches = value;
            OnUpdated?.Invoke();
        }
    }

    private void Awake()
    {
        foreach (var approach in approaches)
        {
            approach.Init();
        }
    }

    private void Start()
    {
        
    }

    public Approach GetHealthyApproach() //Me da el primer approach vivo que tengo en mi lista
    {
        return approaches.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddApproach(Approach newApproach)
    {
        if (approaches.Count < 6)
        {
            approaches.Add(newApproach);
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

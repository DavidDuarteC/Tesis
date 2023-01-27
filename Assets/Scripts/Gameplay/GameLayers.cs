using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] LayerMask solidObjectsLayer;//Objetos solidos
    [SerializeField] LayerMask interactableLayer;//NPC
    [SerializeField] LayerMask grassLayer;//Pasto
    [SerializeField] LayerMask playerLayer;//Personaje
    [SerializeField] LayerMask fovLayer;//Entrenador
    [SerializeField] LayerMask portalLayer;//Portal
    [SerializeField] LayerMask triggerLayer;//Story Item
    [SerializeField] LayerMask ledgeLayer;//Ledge
    [SerializeField] LayerMask waterLayer;//Water

    public static GameLayers i { get; set; }

    private void Awake()
    {
        i = this;
    }

    public LayerMask SolidLayer
    {
        get => solidObjectsLayer;
    }

    public LayerMask InteractableLayer
    {
        get => interactableLayer;
    }

    public LayerMask GrassLayer
    {
        get => grassLayer;
    }

    public LayerMask PlayerLayer
    {
        get => playerLayer;
    }

    public LayerMask FovLayer
    {
        get => fovLayer;
    }

    public LayerMask PortalLayer
    {
        get => portalLayer;
    }

    public LayerMask Ledgelayer => ledgeLayer;

    public LayerMask WaterLayer => waterLayer;

    public LayerMask TriggerLayers
    {
        get => grassLayer | fovLayer | portalLayer | triggerLayer | waterLayer;
    }
}

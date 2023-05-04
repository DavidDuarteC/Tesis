using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class Character : MonoBehaviour
{
    public float moveSpeed;//Velocidad del personaje

    public bool IsMoving { get; private set; }//Boolean si el personaje esta en movimiento

    public float OffsetY { get; private set; } = 0.3f;

    CharacterAnimator animator;

    private PlayerController player;

    public void Awake()
    {
        animator = GetComponent<CharacterAnimator>();
        SetPositionAndSnapToTile(transform.position);
    }

    private void Start()
    {
        player = PlayerController.i;
    }

    public void SetPositionAndSnapToTile(Vector2 pos) //Pone al personaje en el centro de cada cuadrado del grid
    {
        pos.x = Mathf.Floor(pos.x) + 0.5f;
        pos.y = Mathf.Floor(pos.y) + 0.5f + OffsetY;

        transform.position = pos;
    }

    public IEnumerator Move(Vector2 moveVec, Action OnMoveOver = null, bool checkCollision = true)// Crea el movimiento del personaje en la grilla
    {
        animator.MoveX = Mathf.Clamp(moveVec.x, -1f, 1f); //Genera las animaciones segun el movimiento en x
        animator.MoveY = Mathf.Clamp(moveVec.y, -1f, 1f); //Genera las animaciones segun el movimiento en y

        var targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        var ledge = CheckForLedge(targetPos); //Comprueba si hay un ledge
        if(ledge != null)
        {
            if (ledge.TryToJump(this, moveVec))
                yield break;
        }

        if (checkCollision && !IsPathClear(targetPos)) // Comprueba si esta cerca del contorno de un objeto solido
            yield break;

        //if(animator.IsSurfing && Physics2D.OverlapCircle(targetPos, 0.3f, GameLayers.i.WaterLayer) == null)
        //    animator.IsSurfing = false;

        IsMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        IsMoving = false;

        OnMoveOver?.Invoke();
    }

    public void HandleUpdate()//Permite realizar las animaciones de caminar
    {
        animator.IsMoving = IsMoving;
    }

    public bool IsPathClear(Vector3 targetPos) // Comprueba si tinen algun obstaculo que no le permite seguir el patron
    {
        var diff = targetPos - transform.position;
        var dir = diff.normalized;

        var collisionLayer = GameLayers.i.SolidLayer | GameLayers.i.InteractableLayer | GameLayers.i.PlayerLayer;
        //if (!animator.IsSurfing)
        //    collisionLayer = collisionLayer | GameLayers.i.WaterLayer;

        if (Physics2D.BoxCast(transform.position + dir, new Vector2(0.2f, 0.2f), 0f, dir, diff.magnitude - 1, collisionLayer) == true)
            return false;

        return true;
    }

    private bool IsWalkable(Vector3 targetPos)//Crea el limite entre los objetos solidos y el personaje
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, GameLayers.i.SolidLayer | GameLayers.i.InteractableLayer) != null)
        {
            return false;
        }
        return true;
    }

    Ledge CheckForLedge(Vector3 targetPos)
    {
        var collider = Physics2D.OverlapCircle(targetPos, 0.15f, GameLayers.i.Ledgelayer);
        return collider?.GetComponent<Ledge>();
    }

    public void LookTowards(Vector3 targetPos) //Permite que cuando interactue con el personaje lo pueda mirar en la direccion que es
    {
        var xdiff = Mathf.Floor(targetPos.x) - Mathf.Floor(transform.position.x);
        var ydiff = Mathf.Floor(targetPos.y) - Mathf.Floor(transform.position.y);

        if (xdiff == 0 || ydiff == 0)
        {
            animator.MoveX = Mathf.Clamp(xdiff, -1f, 1f); //Genera las animaciones segun el movimiento en x
            animator.MoveY = Mathf.Clamp(ydiff, -1f, 1f); //Genera las animaciones segun el movimiento en y

        }
        else
            Debug.Log("Error en Look Towards: no se puede preguntar al personaje mirandolo diagonalmente");
    }

    public CharacterAnimator Animator
    {
        get => animator;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MoveActorAction : CutsceneAction
{
    [SerializeField] CutsceneActor actor;
    [SerializeField] List<Vector2> movePatterns;

    public override IEnumerator Play()
    {
        var character = actor.GetCharacter();

        foreach (var moveVec in movePatterns) //Realiza el patron que se le puso al personaje o al NPC
        {
            yield return character.Move(moveVec, checkCollision: false);
        }
    }
}

[Serializable]
public class CutsceneActor
{
    [SerializeField] bool isPlayer;
    [SerializeField] Character character;

    public Character GetCharacter() => (isPlayer) ? PlayerController.i.Character : character; //Si es verdadero retornara el personaje y si no el NPC
}

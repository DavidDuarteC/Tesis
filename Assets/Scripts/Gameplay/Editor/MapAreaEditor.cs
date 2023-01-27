using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapArea))]
public class MapAreaEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        int totalChanceInGrass = serializedObject.FindProperty("totalChance").intValue;
        int totalChanceInWater = serializedObject.FindProperty("totalChanceWater").intValue;


        //var style = new GUIStyle();
        //style.fontStyle = FontStyle.Bold;

        //GUILayout.Label($"Total Chance = {totalChanceInGrass}", style);

        if(totalChanceInGrass != 100 && totalChanceInGrass != -1)
            EditorGUILayout.HelpBox($"El porcentaje total de los pokemones de la hierba es de {totalChanceInGrass} y no de 100", MessageType.Error);

        if (totalChanceInWater != 100 && totalChanceInWater != -1)
            EditorGUILayout.HelpBox($"El porcentaje total de los pokemones del agua es de {totalChanceInWater} y no de 100", MessageType.Error);
    }
}

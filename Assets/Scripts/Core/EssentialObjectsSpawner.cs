using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssentialObjectsSpawner : MonoBehaviour
{
    [SerializeField] GameObject essentialObjetcsPrefab;

    private void Awake()
    {
        var existingObjects = FindObjectsOfType<EssentialObjects>();
        if (existingObjects.Length == 0)
        {
            //Si ya hay un cuadricula, entonces se genera en el centro de esta
            var spawnPos = new Vector3 (0, 0, 0);

            var grid = FindObjectOfType<Grid>();
            if(grid != null)
                spawnPos = grid.transform.position;

            Instantiate(essentialObjetcsPrefab, spawnPos, Quaternion.identity);

        }
    }
}

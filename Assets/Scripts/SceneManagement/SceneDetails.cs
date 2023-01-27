using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDetails : MonoBehaviour
{
    [SerializeField] List<SceneDetails> connctedScenes;
    [SerializeField] AudioClip sceneMusic;
    public bool IsLoaded { get; private set; }

    List<SavableEntity> savableEntities;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            Debug.Log($"Entro en {gameObject.name}");

            LoadScene();
            GameController.Instance.SetCurrentScene(this);

            if(sceneMusic != null )
                AudioManager.i.PlayMusic(sceneMusic, fade: true);

            //Conecta todas las escenas
            foreach(var scene in connctedScenes)
            {
                scene.LoadScene();
            }

            //Desconectar las escenas que no esta conectadas
            var prevScene = GameController.Instance.PrevScene;
            if(GameController.Instance.PrevScene != null)
            {
                var previoslyLoadedScenes = GameController.Instance.PrevScene.connctedScenes;
                foreach(var scene in previoslyLoadedScenes)
                {
                    if (!connctedScenes.Contains(scene) && scene != this)
                        scene.UnloadScene();
                }

                if(!connctedScenes.Contains(prevScene)) //Permite guardar esceneas que no estan conecatdas con otras y que esta al guadar no se conecten a una escena en la cual no estan conectadas
                    prevScene.UnloadScene();
            }
        }
    }

    public void LoadScene()
    {
        if (!IsLoaded)
        {
            var operation = SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            IsLoaded = true;

            operation.completed += (AsyncOperation op) =>
            {
                savableEntities = GetSavableEntitiesInScene();
                SavingSystem.i.RestoreEntityStates(savableEntities);
            };
        }
    }

    public void UnloadScene()
    {
        if (IsLoaded)
        {
            SavingSystem.i.CaptureEntityStates(savableEntities);
    
            SceneManager.UnloadSceneAsync(gameObject.name);
            IsLoaded = false;
        }
    } 

    List<SavableEntity> GetSavableEntitiesInScene() //Permite guardar los datos de una escena
    {
        var currScene = SceneManager.GetSceneByName(gameObject.name);
        var savableEntities = FindObjectsOfType<SavableEntity>().Where(x => x.gameObject.scene == currScene).ToList();
        return savableEntities;
    }

    public AudioClip SceneMusic => sceneMusic;
}

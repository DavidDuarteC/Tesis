using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void Play()
    {
        gameObject.SetActive(false);
        CharacterSelectorUI.i.Show();
    }
    public void Load()
    {
        SavingSystem.i.Load("saveSlot1");
        gameObject.SetActive(false);
        CharacterSelectorUI.i.Play();
    }

    public void Quit()
    {
        Debug.Log("Salir...");
        Application.Quit();
    }
}

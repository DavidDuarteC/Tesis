using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class CharacterSelectorUI : MonoBehaviour
{
    [SerializeField] int index;
    [SerializeField] Image img;
    [SerializeField] new TextMeshProUGUI name;
    [SerializeField] Image leftArrow;
    [SerializeField] Image rightArrow;

    private Moving how;

    private PlayerController playerController;

    public event Action OnSelectCharacter;
    public event Action OnFinishSelect;
    public static CharacterSelectorUI i { get; private set; } //Patron singleton

    private void Start()
    {
        i = this;
        playerController = PlayerController.i;

        index = PlayerPrefs.GetInt("PlayerIndex");

        if (index > Enum.GetValues(typeof(Moving)).Length - 1)
        {
            index = 0;
        }

        ChangeScreen();
    }

    public void Show()
    {
        OnSelectCharacter?.Invoke();
        gameObject.SetActive(true);
    }

    public void ChangeScreen()
    {
        //PlayerPrefs.SetInt("PlayerIndex", index);
        if (index == 0)
        {
            img.sprite = playerController.GetComponent<Character>().GetComponent<CharacterAnimator>().SpriteBoy;
            name.text = "Chico";
            how = Moving.Move;
            leftArrow.gameObject.SetActive(false);
            rightArrow.gameObject.SetActive(true);
        }
        else if (index == 1)
        {
            img.sprite = playerController.GetComponent<Character>().GetComponent<CharacterAnimator>().SpriteGirl;
            name.text = "Chica";
            how = Moving.None;
            rightArrow.gameObject.SetActive(false);
            leftArrow.gameObject.SetActive(true);
        }


        if (Input.GetKeyDown(KeyCode.Z))
        {
            playerController.IsHow = how;
            Play();
        }
    }

    public void HandleUpdate()
    {
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            if (index == Enum.GetValues(typeof(Moving)).Length - 1)
            {
                index = 1;
            }
            else
            {
                index += 1;
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (index == 0)
            {
                index = 0;
            }
            else
            {
                index -= 1;

            }
        }
        ChangeScreen();
    }

    public void Play()
    {
        gameObject.SetActive(false);
        OnFinishSelect?.Invoke();
    }
}

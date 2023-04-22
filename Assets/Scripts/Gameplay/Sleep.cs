using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Sleep : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] AudioClip audioClip;
    [SerializeField] GameObject NpcMovimeinto1;

    private void Update()
    {
        if (PlayerController.i.Day == 2 && PlayerController.i.Semester == 1)
        {
            NpcMovimeinto1.SetActive(true);
        }
    }
    public void OnPlayerTrigger(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        StartCoroutine(SleepeEvent(player.transform, player));
    }
    public IEnumerator SleepeEvent(Transform transform, PlayerController player) // Permite curar los pokemones que tenga el personaje
    {
        int selectedChoice = 0;
        yield return DialogManager.Instance.ShowDialogText("Te ves cansado, deberías dormir un poco",
            choices: new List<string>() { "Sí", "No" },
            onChoiceSelected: (choiceIndex) => selectedChoice = choiceIndex);

        if (selectedChoice == 0)
        {
            //Yes
            yield return Fader.i.FadeIn(0.6f);
            AudioManager.i.PlaySfx(audioClip, true);
            PlayerController.i.Day++;
            yield return new WaitForSeconds(audioClip.length);
            var playerParty = player.GetComponent<PokemonParty>();
            playerParty.Pokemons.ForEach(p => p.Heal());
            playerParty.PartyUpdate();
            if (PlayerController.i.Day == 2 && PlayerController.i.Semester == 1)
            {
                NpcMovimeinto1.SetActive(true);
            }
            DayUI.i.changeDay();
            yield return DialogManager.Instance.ShowDialogText("Hoy es un nuevo día (Día " + player.Day + ")");
            yield return Fader.i.FadeOut(0.6f);
            

        }
        else if (selectedChoice == 1)
        {
            //No
            yield return DialogManager.Instance.ShowDialogText("Ok, vuelve cuando quieras");
        }


    }

    public bool TriggerRepeatedly => false;

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonitorController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] new string name;
    //[SerializeField] Sprite sprite;
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterBattle;
    [SerializeField] Dialog dialogLoseBattle;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;

    [SerializeField] AudioClip monitorAppearsClip;

    ApproachParty playerParty;

    //Estado
    bool battleLost = false;

    Character character;
    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }

    private void Update()
    {
        character.HandleUpdate();
    }

    public IEnumerator Interact(Transform initiator)
    {
        character.LookTowards(initiator.position);
        playerParty = PlayerController.i.GetComponent<ApproachParty>();
        var lenguagePlayer = playerParty.GetHealthyApproach();

        if (lenguagePlayer == null)
        {
            yield return DialogManager.Instance.ShowDialog(dialogLoseBattle);
            GameController.Instance.StartFreeRoamState();
            AudioManager.i.PrevPlayMusic();
            yield break;
        }else if (!battleLost )
        {
            AudioManager.i.PlayMusic(monitorAppearsClip);
            yield return DialogManager.Instance.ShowDialog(dialog);
            GameController.Instance.StartMonitorBattle(this);
            Debug.Log("Empezo la batalla");
        }
        else
        {
            yield return DialogManager.Instance.ShowDialog(dialogAfterBattle);
        }
    }

    public void BattleLost()
    {
        battleLost = true;
        fov.gameObject.SetActive(false);
    }

    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        playerParty = PlayerController.i.GetComponent<ApproachParty>();
        var lenguagePlayer = playerParty.GetHealthyApproach();
        if (lenguagePlayer != null)
            AudioManager.i.PlayMusic(monitorAppearsClip);

        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;
        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

        yield return character.Move(moveVec);

        //Muestra el dialogo
        

        if (lenguagePlayer == null)
        {
            yield return DialogManager.Instance.ShowDialog(dialogLoseBattle);
            GameController.Instance.StartFreeRoamState();
            AudioManager.i.PrevPlayMusic();
            yield break;
        }else
        {
            yield return DialogManager.Instance.ShowDialog(dialog);
            GameController.Instance.StartMonitorBattle(this);
            Debug.Log("Empezo la batalla");
        }
        
    }

    public void SetFovRotation(FacingDirection dir)
    {
        float angle = 0f;
        if (dir == FacingDirection.Right)
            angle = 90f;
        else if (dir == FacingDirection.Up)
            angle = 180f;
        else if (dir == FacingDirection.Left)
            angle = 270f;

        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }

    public object CaptureState()
    {
        return battleLost;
    }

    public void RestoreState(object state)
    {
        battleLost = (bool)state;
        if(battleLost)
            fov.gameObject.SetActive(false);
    }

    public string Name
    {
        get => name;
    }

    //public Sprite Sprite
    //{
    //    get => sprite;
    //}
}
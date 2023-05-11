using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] int lettersPerSecond;

    [SerializeField] Text dialogText;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;
    [SerializeField] GameObject choiceBox;

    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> moveTexts;

    [SerializeField] Text ppText;
    [SerializeField] Text typeText;

    [SerializeField] Text yesText;
    [SerializeField] Text noText;

    Color highlightedColor;

    private void Start()
    {
        highlightedColor = GlobalSettings.i.HighlightedColor;
    }


    public void SetDialog(string dialog) //Actualiza el texto del dialogo
    {
        dialogText.text = dialog;
    }
    
    public IEnumerator TypeDialog(string dialog) //Genera la animacion del texto cuando se muestra
    {
        dialogText.text = "";
        foreach(var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f/lettersPerSecond);
        }

        yield return new WaitForSeconds(1f);
    }

    public void EnableDialogText(bool enabled) //Muestra o no muestra el texto del dialogo
    {
        dialogText.enabled = enabled;
    }

    public void EnableActionSelector(bool enabled) //Muestra o no muestra las opciones de pelear o huir
    {
        actionSelector.SetActive(enabled);
    }

    public void EnableMoveSelector(bool enabled) //Muestra o no muestra los movimientos y su info
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }
    public void EnableChoiceBox(bool enabled) //Muestra las opciones de cambiar Approach o seguir peleando con el que esta
    {
        choiceBox.SetActive(enabled);
    }

    public void UpdateActionSelection(int selectedAction) //Permite evidenciar por el cambio de color entre pelear o huir
    {
        for(int i = 0; i < actionTexts.Count; i++)
        {
            if (i == selectedAction)
                actionTexts[i].color = highlightedColor;
            else
                actionTexts[i].color = Color.black;
        }
    }

    public void UpdateMoveSelection(int selectMove, Move move) //Permite evidenciar por el cambio de color y el de la informacion del tipo y el pp de los movimientos
    {
        for(int i = 0; i < moveTexts.Count; i++)
        {
            if( i == selectMove)
                moveTexts[i].color = highlightedColor;
            else
                moveTexts[i].color = Color.black;
        }

        ppText.text = $"PP {move.PP}/{move.Base.PP}";
        typeText.text = move.Base.Type.ToString();

        if (move.PP == 0)
            ppText.color = Color.red;
        else
            ppText.color = Color.black;
    }

    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < moveTexts.Count; i++)
        {
            if (i < moves.Count)
                moveTexts[i].text = moves[i].Base.Name;
            else
                moveTexts[i].text = "-";
        }
    }

    public void UpdateChoiceBox(bool yesSelected) //Permite evidenciar por el cambio de color entre pelear o huir
    {
        if(yesSelected)
        {
            yesText.color = highlightedColor;
            noText.color = Color.black;
        }
        else
        {
            yesText.color = Color.black;
            noText.color = highlightedColor;
        }
    }
}

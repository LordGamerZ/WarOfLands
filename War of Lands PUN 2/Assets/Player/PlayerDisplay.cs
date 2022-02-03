using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class PlayerDisplay : MonoBehaviour
{
    public Image Icon;
    public TextMeshProUGUI NameText;
    public TMP_Dropdown MyTeamNum;
    public TextMeshProUGUI TeamNum;
    public bool IsMine;

    private void Awake()
    {
        transform.parent = LobbyController.Instance.PlayersPanel;
        LayoutRebuilder.MarkLayoutForRebuild(LobbyController.Instance.PlayersPanel.GetComponent<RectTransform>());
        IsMine = false;
    }

    private void Start()
    {
        if(IsMine)
        {
            TeamNum.transform.parent.gameObject.SetActive(false); 
        }
        else
        {
            MyTeamNum.gameObject.SetActive(false);
        }
    }

    public void ChangeMyDisplay()
    {
        if(IsMine)
        {
            LobbyController.Instance.ChangeMyTeamNum();
        }
    }
}

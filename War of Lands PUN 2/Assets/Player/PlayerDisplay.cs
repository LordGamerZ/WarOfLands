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
    public LobbyController LobbyControl;

    private void Awake()
    {
        IsMine = false;
    }

    private void Start()
    {
        if (IsMine)
        {
            TeamNum.transform.parent.gameObject.SetActive(false); 
        }
        else
        {
            MyTeamNum.gameObject.SetActive(false);
        }
    }

    [PunRPC]
    public void SetUsername(string username, int lobbyControllerID)
    {
        NameText.text = username;
        LobbyControl = PhotonNetwork.GetPhotonView(lobbyControllerID).GetComponent<LobbyController>();
        transform.parent = LobbyControl.PlayersPanel;
        LayoutRebuilder.MarkLayoutForRebuild(LobbyControl.PlayersPanel.GetComponent<RectTransform>());
    }

    public void ChangeMyDisplay()
    {
        if(IsMine)
        { 
            LobbyControl.ChangeMyTeamNum();
        }
    }
}

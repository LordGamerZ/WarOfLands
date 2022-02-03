using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class LobbyController : MonoBehaviourPunCallbacks
{
    public static LobbyController Instance;

    public List<Color> Colors;

    public GameObject HexForest;
    public GameObject HexPlains;
    public GameObject HexHills;

    //Player Display Variables
    public Transform PlayersPanel;
    public GameObject PlayerDisplayPrefab;
    public PlayerDisplay MyDisplay;

    //Chat Variables
    public GameObject ChatBox;
    public GameObject ChatPrefab;

    public InputField ChatInput;

    //Launch Match Variables
    public Button LaunchButton;

    public TMP_Dropdown BoardSizes;
    public int[] Sizes;

    public TMP_Dropdown TurnTimes;
    public int[] Times;

    public int[] BasePositions;

    private void Awake()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            LaunchButton.gameObject.SetActive(false);
            BoardSizes.gameObject.SetActive(false);
            TurnTimes.gameObject.SetActive(false);
        }
        else
        {
            Instance = this;

            Sizes = new int[4] { 25, 50, 75, 100 };
            List<TMP_Dropdown.OptionData> boardOptions = new()
            {
                new TMP_Dropdown.OptionData("Small (25 x 25)"),
                new TMP_Dropdown.OptionData("Medium (50 x 50)"),
                new TMP_Dropdown.OptionData("Large (75 x 75"),
                new TMP_Dropdown.OptionData("Too Big (100 x 100")
            };
            BoardSizes.AddOptions(boardOptions);
            BoardSizes.value = 1;

            Times = new int[4] { 30, 60, 90, 120 };
            List<TMP_Dropdown.OptionData> timeOptions = new()
            {
                new TMP_Dropdown.OptionData("30 seconds"),
                new TMP_Dropdown.OptionData("60 seconds"),
                new TMP_Dropdown.OptionData("90 seconds"),
                new TMP_Dropdown.OptionData("120 seconds"),
            };
            TurnTimes.AddOptions(timeOptions);
            BoardSizes.value = 1;
        }

        MyDisplay = PhotonNetwork.Instantiate("PlayerDisplayPrefab", Vector3.zero, Quaternion.identity).GetComponent<PlayerDisplay>();
        MyDisplay.IsMine = true;
    }

    public void SendChat()
    {
        gameObject.GetPhotonView().RPC("DisplayChat", RpcTarget.All, PhotonNetwork.NickName, ChatInput.text);
        ChatInput.text = "";
    }

    public void ChangeMyTeamNum()
    {
        gameObject.GetPhotonView().RPC("SetTeamNum", RpcTarget.All, MyDisplay.gameObject.GetPhotonView().ViewID, int.Parse(MyDisplay.MyTeamNum.options[MyDisplay.MyTeamNum.value].text));
    }

    [PunRPC]
    public void SetTeamNum(int photonID, int newTeamNum)
    {
        PhotonNetwork.GetPhotonView(photonID).GetComponent<PlayerDisplay>().TeamNum.text = newTeamNum.ToString();
    }

    [PunRPC]
    public void DisplayChat(string sender, string message)
    {
        GameObject chat = Instantiate(ChatPrefab, ChatBox.transform);
        chat.GetComponent<TextMeshProUGUI>().text = sender + ": " + message;
    }

    public void BeginGame()
    {
        int[] playerIDs = new int[PhotonNetwork.CurrentRoom.Players.Keys.Count];
        BasePositions = new int[PhotonNetwork.PlayerList.Length];
        int[] teamNums = new int[5];
        int[] boardTypes = new int[Sizes[BoardSizes.value] * Sizes[BoardSizes.value]];

        int[] chances = new int[20] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2 };

        for (int i = 0; i < boardTypes.Length; i++)
        {
            boardTypes[i] = chances[Random.Range(0, 20)];
        }

        for (int i = 0; i < PhotonNetwork.CurrentRoom.Players.Keys.Count; i++)
        {
            playerIDs[i] = new List<int>(PhotonNetwork.CurrentRoom.Players.Keys)[i];
        }

        gameObject.GetPhotonView().RPC("BuildGrid", RpcTarget.All, boardTypes, playerIDs);

        for (int i = 0; i < BasePositions.Length; i++)
        {
            HexPos baseHexPos = GameManager.Instance.Board[BasePositions[i]];
            BasicHeadQuarterCommands headQuarters = PhotonNetwork.Instantiate("HeadQuartersPrefab", baseHexPos.transform.position, Quaternion.identity).GetComponent<BasicHeadQuarterCommands>();
            GameManager.Instance.gameObject.GetPhotonView().RPC("SetupBase", RpcTarget.All,
                headQuarters.gameObject.GetPhotonView().ViewID, playerIDs[i], teamNums[i], BasePositions[i], GameManager.ColorToArray(Colors[i]));

            int[] players = new int[PhotonNetwork.CurrentRoom.PlayerCount];

            for (int j = 0; j < players.Length; j++)
            {
                players[i] = new List<int>(PhotonNetwork.CurrentRoom.Players.Keys)[i];
            }

            GameManager.Instance.gameObject.GetPhotonView().RPC("SetInfo", RpcTarget.All, Times[TurnTimes.value], players);
        }

        PlayerInteraction.Instance.IsTurn = true;
        PhotonNetwork.Destroy(gameObject.GetPhotonView());
    }

    public void LeaveMatch()
    {
        PhotonNetwork.Destroy(MyDisplay.gameObject.GetPhotonView());
        PhotonNetwork.LeaveRoom();

        SceneManager.LoadScene("CreateJoinLobby");
    }

    [PunRPC]
    public void BuildGrid(int[] boardTypes, int[] playerIDs)
    {
        UIControl.Instance.gameObject.SetActive(true);
        PlayerInteraction.Instance.gameObject.SetActive(true);

        int num = 0;
        int numCastles = 0;

        for (int i = 0; i < Sizes[BoardSizes.value]; i++)
        {
            float zPos;

            if (i % 2 == 0)
            {
                zPos = -0.01f;
            }
            else
            {
                zPos = 2.59f;
            }

            for (int j = 0; j < Sizes[BoardSizes.value]; j++)
            {
                int hexViewID;

                if (boardTypes[num] == 1)
                {
                    hexViewID = num;
                    GameManager.Instance.Board[num] = Instantiate(HexForest, new Vector3(i * 4.49f, 0, zPos + (j * 5)), Quaternion.identity).GetComponent<HexPos>();
                    GameManager.Instance.Board[num].ID = num;
                }
                else if (boardTypes[num] == 2)
                {
                    hexViewID = num;
                    GameManager.Instance.Board[num] = Instantiate(HexHills, new Vector3(i * 4.49f, 0, zPos + (j * 5)), Quaternion.identity).GetComponent<HexPos>();
                    GameManager.Instance.Board[num].ID = num;
                }
                else
                {
                    hexViewID = num;
                    GameManager.Instance.Board[num] = Instantiate(HexPlains, new Vector3(i * 4.49f, 0, zPos + (j * 5)), Quaternion.identity).GetComponent<HexPos>();
                    GameManager.Instance.Board[num].ID = num;
                }

                if (numCastles < playerIDs.Length)
                {
                    if (i == 4 || i == boardTypes.Length - 5)
                    {
                        if (j == 4 || j == boardTypes.Length - 5)
                        {
                            BasePositions[numCastles] = hexViewID;
                            numCastles += 1;
                        }
                    }
                }

                num += 1;
            }
        }
    }
}
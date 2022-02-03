using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int[] PlayerIDs;

    public int TeamNum;

    public Color MyColor;

    public Dictionary<int, HexPos> Board;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
        }
        else 
        {
            Instance = this;
            Board = new();
        }
    }

    [PunRPC]
    public void MyTurn(int playerID)
    {
        if(PhotonNetwork.MasterClient.ActorNumber == playerID)
        {
            UIControl.Instance.IncrementTurns();
        }

        if (playerID == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            PlayerInteraction.Instance.IsTurn = true;
            UIControl.Instance.ChangeArrowSprite();
        }
    }

    [PunRPC]
    public void SetInfo(int turnTime, int[] players)
    {
        UIControl.Instance.TurnTime = turnTime;
        PlayerIDs = players;
    }

    public float[] MyColorToArray()
    {
        return ColorToArray(MyColor);
    }

    public static float[] ColorToArray(Color color)
    {
        return new float[3] { color.r, color.g, color.b };
    }

    [PunRPC]
    public void SetupBase(int photonViewID, int ownerID, int teamNum, int hexID, float[] color)
    {
        BasicHeadQuarterCommands headQuarters = PhotonNetwork.GetPhotonView(photonViewID).GetComponent<BasicHeadQuarterCommands>();

        headQuarters.CurrentPos = Board[hexID];
        headQuarters.OwnerID = ownerID;
        headQuarters.TeamNum = teamNum;

        Board[hexID].Building = headQuarters;
        Board[hexID].Select(3);

        if (headQuarters.OwnerID == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            MyColor = new Color(color[0], color[1], color[2], 255);
            TeamNum = teamNum;
        }

        foreach (Transform tf in headQuarters.Model.transform.Find("Colours"))
        {
            tf.GetComponent<Renderer>().material.color = new Color(color[0], color[1], color[2], 255);
        }
    }

    public void Lose()
    {

    }

    [PunRPC]
    public void SetupBuilding(int buildingNum, int ownerID, int teamNum, int currentPosID, float[] color, int builderID)
    {
        UnitSelectable unit = PhotonNetwork.GetPhotonView(buildingNum).GetComponent<UnitSelectable>();

        unit.OwnerID = ownerID;
        unit.TeamNum = teamNum;
        unit.CurrentPos = Board[currentPosID];

        if (unit.Model.transform.Find("Colours"))
        {
            foreach (Transform tf in unit.Model.transform.Find("Colours"))
            {
                tf.GetComponent<Renderer>().material.color = new Color(color[0], color[1], color[2], 255);
            }
        }

        Board[currentPosID].Building = unit;

        if(ownerID == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            PhotonNetwork.GetPhotonView(builderID).GetComponent<BuilderCommands>().Build();
        }
    }

    [PunRPC]
    public void SetupUnit(int photonViewID, int ownerID, int teamNum, int hexID, float[] color)
    {
        UnitSelectable unit = PhotonNetwork.GetPhotonView(photonViewID).GetComponent<UnitSelectable>();

        unit.OwnerID = ownerID;
        unit.TeamNum = teamNum;
        unit.CurrentPos = Board[hexID];

        if (unit.SelectableType == SelectableTypes.Melee)
        {
            Board[hexID].MeleeUnit = unit as MeleeCommands;
            Board[hexID].Select(0);
            unit.Model.GetComponent<Renderer>().material.color = new Color(color[0], color[1], color[2], 255);
        }
        else if (unit.SelectableType == SelectableTypes.Ranged)
        {
            Board[hexID].RangedUnit = unit as RangedCommands;
            Board[hexID].Select(1);
            unit.Model.GetComponent<Renderer>().material.color = new Color(color[0], color[1], color[2], 255);
        }
        if (unit.SelectableType == SelectableTypes.Builder)
        {
            Board[hexID].Builder = unit as BuilderCommands;
            Board[hexID].Select(2);
        }
    }
}

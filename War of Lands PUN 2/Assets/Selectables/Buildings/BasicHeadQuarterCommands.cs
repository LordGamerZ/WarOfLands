using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BasicHeadQuarterCommands : BasicFactoryCommands
{
    public float WoodIncome;
    public float OreIncome;
    public float GoldIncome;

    public float Effectiveness;

    private new void Awake()
    {
        SelectedSprite.enabled = false;
        BuildQueue = new List<int>();

        Effectiveness = 1;
    }

    private void Start()
    {
        if (OwnerID == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            PlayerInteraction.Instance.MyBase = this;
            PlayerInteraction.Instance.transform.position = new Vector3(transform.position.x, 30, transform.position.z);

            List<HexPos> hexPositions = new();

            if (Physics.Raycast(new Vector3(transform.position.x, 10, transform.position.z - 5), Vector3.down * 20, out RaycastHit hitFour, 20))
            {
                hexPositions.Add(hitFour.transform.GetComponent<HexPos>());
            }

            if (Physics.Raycast(new Vector3(transform.position.x - 5, 10, transform.position.z + 2.5f), Vector3.down * 20, out RaycastHit hitFive, 20))
            {
                hexPositions.Add(hitFive.transform.GetComponent<HexPos>());
            }

            if (Physics.Raycast(new Vector3(transform.position.x + 5, 10, transform.position.z + 2.5f), Vector3.down * 20, out RaycastHit hitSix, 20))
            {
                hexPositions.Add(hitSix.transform.GetComponent<HexPos>());
            }

            for (int i = 0; i < hexPositions.Count; i++)
            {
                int builder = PhotonNetwork.Instantiate(Profiles[0].Prefab.name, hexPositions[i].transform.position, Quaternion.identity)
                    .GetPhotonView().ViewID;
                GameManager.Instance.gameObject.GetPhotonView().RPC("SetupUnit", RpcTarget.All, builder, PhotonNetwork.LocalPlayer.ActorNumber, TeamNum, hexPositions[i].ID, GameManager.Instance.MyColorToArray());
            }
        }
    }

    [PunRPC]
    public override void NextTurn()
    {
        if (OwnerID == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            if (BuildQueue.Count > 0)
            {
                Timer += 1;

                if (Timer >= Profiles[BuildQueue[0]].ProductionTime)
                {
                    int unitID = PhotonNetwork.Instantiate(Profiles[BuildQueue[0]].Prefab.name, CurrentPos.transform.position, Quaternion.identity).GetPhotonView().ViewID;
                    Color myColor = GameManager.Instance.MyColor;
                    GameManager.Instance.gameObject.GetPhotonView().RPC("SetupUnit", RpcTarget.All, unitID, PhotonNetwork.LocalPlayer.UserId,
                        GameManager.Instance.TeamNum, CurrentPos.gameObject.GetPhotonView().ViewID, new int[3] { Mathf.FloorToInt(myColor.r), Mathf.FloorToInt(myColor.g), Mathf.FloorToInt(myColor.b) });
                    Timer = 0;
                    BuildQueue.RemoveAt(0);
                }
            }

            UIControl.Instance.UpdateResources(WoodIncome * Effectiveness, OreIncome * Effectiveness, GoldIncome * Effectiveness);
        }
    }

    public override void Death()
    {
        CurrentPos.Remove(3);
        CurrentPos.Building = null;

        if (OwnerID == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            GameManager.Instance.Lose();
        }
    }
}
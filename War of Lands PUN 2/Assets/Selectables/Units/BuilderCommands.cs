using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BuilderCommands : UnitSelectable
{
    public int MaxBuilds;
    public int CurrentBuilds;

    public bool IsBuilding;
    public bool IsDeforesting;

    public float MaxMoves;
    public float CurrentMoves;

    public List<BuildingProfile> Profiles = new List<BuildingProfile>();

    public void Awake()
    {
        SelectedSprite.enabled = false;
        IsBuilding = false;
        IsDeforesting = false;
        CurrentBuilds = 0;
        CurrentMoves = 0;
    }

    [PunRPC]
    public void RemoveForest()
    {
        if (CurrentPos.BiomeType == Biomes.Forest && CurrentBuilds < MaxBuilds)
        {
            CurrentBuilds += 1;

            //ClientSend.RemoveForest(CurrentPos.ViewID);
        }
    }

    [PunRPC]
    public void SetPath(int hexID)
    {
        if (CurrentPos.BiomeType == Biomes.Plains)
        {

        }
    }

    public void AttemptBuild(int buildingNum)
    {
        if (!IsBuilding)
        {
            if (!CurrentPos.Building)
            {
                if (CurrentPos.TerrainType == Profiles[buildingNum].TerrainType && CurrentPos.BiomeType == Profiles[buildingNum].BiomeType)
                {
                    if (UIControl.Instance.CanAfford(-Profiles[buildingNum].WoodCost, -Profiles[buildingNum].StoneCost, -Profiles[buildingNum].GoldCost))
                    {
                        StartBuild(buildingNum);
                        UIControl.Instance.UpdateResources(-Profiles[buildingNum].WoodCost, -Profiles[buildingNum].StoneCost, -Profiles[buildingNum].GoldCost);
                        IsBuilding = true;
                    }
                }
            }
        }
    }

    public void StartBuild(int profileNum)
    {
        UnitSelectable unitSelectable = PhotonNetwork.Instantiate(Profiles[profileNum].Prefab.name, CurrentPos.transform.position, Quaternion.identity).GetComponent<UnitSelectable>();
        PlayerInteraction.Instance.MyUnits.Add(unitSelectable.gameObject.GetPhotonView());

        GameManager.Instance.gameObject.GetPhotonView().RPC("SetupBuilding", RpcTarget.All, unitSelectable.gameObject.GetPhotonView().ViewID, OwnerID, TeamNum, CurrentPos.ID, GameManager.Instance.MyColorToArray(), gameObject.GetPhotonView().ViewID);

        Building building = unitSelectable.GetComponent<Building>();
        building.SetBuilding(Profiles[profileNum].ProductionTime, this, GameManager.Instance.MyColor);
    }

    public void Build()
    {
        if (IsBuilding)
        {
            while (MaxBuilds > CurrentBuilds)
            {
                if (CurrentPos.Building.GetComponent<Building>())
                {
                    if (CurrentPos.Building.GetComponent<Building>().Build())
                    {
                        Destroy(CurrentPos.Building.GetComponent<Building>());
                        CurrentPos.Select(3);
                        IsBuilding = false;
                    }

                    CurrentBuilds += 1;
                }
                else
                {
                    IsBuilding = false;
                }
            }
        }
    }

    public bool CanMove(HexPos hexPos)
    {
        if (!IsBuilding && hexPos.TerrainType == MovementType)
        {
            if (Vector3.Distance(hexPos.transform.position, CurrentPos.transform.position) < 8 && Vector3.Distance(hexPos.transform.position, CurrentPos.transform.position) > 1)
            {
                if (CurrentMoves + 1 > MaxMoves)
                {
                    return false;
                }

                if (hexPos.Builder)
                {
                    return false;
                }

                if (hexPos.MeleeUnit)
                {
                    if (hexPos.MeleeUnit.TeamNum != TeamNum)
                    {
                        return false;
                    }
                }

                if (hexPos.RangedUnit)
                {
                    if (hexPos.RangedUnit.TeamNum != TeamNum)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        return false;
    }

    [PunRPC]
    public void Move(int hexPosID)
    {
        HexPos hexPos = GameManager.Instance.Board[hexPosID];

        transform.rotation = Quaternion.Euler(0, Quaternion.LookRotation(hexPos.transform.position - transform.position).eulerAngles.y, 0);
        CurrentMoves += 1;


        CurrentPos.Builder = null;
        CurrentPos.Remove(2);

        CurrentPos = hexPos;
        transform.position = CurrentPos.transform.position;
        CurrentPos.Builder = this;

        CurrentPos.Select(2);

        if (OwnerID == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            UIControl.Instance.ShowBuildMenu();
        }
    }

    public override void NextTurn()
    {
        if (OwnerID == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            CurrentBuilds = 0;
            CurrentMoves = 0;

            Build();
        }
    }

    public override void Death()
    {
        CurrentPos.Remove(2);
        CurrentPos.Builder = null;
        Destroy(gameObject);
    }
}
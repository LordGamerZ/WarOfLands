using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BasicFactoryCommands : UnitSelectable
{
    public List<Profile> Profiles;
    public List<int> BuildQueue; 

    public float Timer;

    public void Awake()
    {
        SelectedSprite.enabled = false;
        BuildQueue = new List<int>();

        Timer = 0;
    }

    public void CreateUnit(int profileID)
    {
        int photonViewID = PhotonNetwork.Instantiate(Profiles[profileID].Prefab.name, CurrentPos.transform.position, Quaternion.identity).GetPhotonView().ViewID;
        GameManager.Instance.gameObject.GetPhotonView().RPC("SetupUnit", RpcTarget.All, photonViewID, OwnerID,
            TeamNum, CurrentPos.gameObject.GetPhotonView().ViewID, GameManager.Instance.MyColorToArray());
    }

    public void AddToQueue(int unitNum)
    {
        if (UIControl.Instance.CanAfford(-Profiles[unitNum].WoodCost, -Profiles[unitNum].StoneCost, -Profiles[unitNum].GoldCost)) 
        {
            BuildQueue.Add(unitNum);
            UIControl.Instance.UpdateResources(-Profiles[unitNum].WoodCost, -Profiles[unitNum].StoneCost, -Profiles[unitNum].GoldCost); 
        }
    }

    public override void NextTurn()
    {
        if (OwnerID == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            if (BuildQueue.Count > 0)
            {
                Timer += 1;

                if (Timer >= Profiles[BuildQueue[0]].ProductionTime)
                {
                    UnitSelectable unitSelectable = PhotonNetwork.Instantiate(Profiles[ BuildQueue[0]].Prefab.name, CurrentPos.transform.position, Quaternion.identity).GetComponent<UnitSelectable>();
                    PlayerInteraction.Instance.MyUnits.Add(unitSelectable.gameObject.GetPhotonView());

                    GameManager.Instance.gameObject.GetPhotonView().RPC("SetupUnit", RpcTarget.All, unitSelectable.gameObject.GetPhotonView().ViewID, OwnerID, TeamNum, CurrentPos.ID, GameManager.Instance.MyColorToArray());
                    Timer = 0;
                    BuildQueue.RemoveAt(0);
                }
            }
        }
    }

    public override void Death()
    {
        CurrentPos.Remove(3);
        CurrentPos.Building = null;
        Destroy(gameObject);
    }
}

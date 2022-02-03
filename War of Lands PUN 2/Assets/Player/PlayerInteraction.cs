using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction Instance;

    public Camera MainCamera;
    public BasicHeadQuarterCommands MyBase;

    public bool IsTurn;

    public UnitSelectable Selected;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(Instance);
        }
        else
        {
            Instance = this;
            Selected = null;
            IsTurn = false;

            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        HexPos hexPos = ReturnHexPos();

        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData PED = new(UIControl.Instance.EV)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> Results = new();
            UIControl.Instance.GR.Raycast(PED, Results);

            if (UIControl.Instance.IsBuilding)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    Selected.GetComponent<BuilderCommands>().RemoveForest();
                }
            }

            if (Results.Count > 0)
            {
                if (UIControl.Instance.IsBuilding || UIControl.Instance.IsRecruiting)
                {
                    foreach (RaycastResult RR in Results)
                    {
                        if (RR.gameObject.GetComponent<ProfileIcon>())
                        {
                            if (UIControl.Instance.IsBuilding)
                            {
                                Selected.GetComponent<BuilderCommands>().AttemptBuild(RR.gameObject.transform.GetSiblingIndex());
                            }
                            else if (UIControl.Instance.IsRecruiting)
                            {
                                Selected.GetComponent<BasicFactoryCommands>().AddToQueue(RR.gameObject.transform.GetSiblingIndex());
                            }

                            break;
                        }
                    }
                }
            }

            if (hexPos && Results.Count == 0)
            {
                int whichUnit = -1;

                if (Selected)
                {
                    if (hexPos == Selected.CurrentPos)
                    {
                        if (Selected.SelectableType == SelectableTypes.Melee)
                        {
                            if (hexPos.RangedUnit)
                            {
                                whichUnit = 1;
                            }
                            else if (hexPos.Builder)
                            {
                                whichUnit = 2;
                            }
                            else if (hexPos.Building)
                            {
                                whichUnit = 3;
                            }
                        }
                        else if (Selected.SelectableType == SelectableTypes.Ranged)
                        {
                            if (hexPos.Builder)
                            {
                                whichUnit = 2;
                            }
                            else if (hexPos.Building)
                            {
                                whichUnit = 3;
                            }
                        }
                        else if (Selected.SelectableType == SelectableTypes.Builder)
                        {
                            if (hexPos.Building)
                            {
                                whichUnit = 3;
                            }
                        }
                    }
                }

                if(whichUnit == -1)
                {
                    if (hexPos.MeleeUnit)
                    {
                        whichUnit = 0;
                    }
                    else if (hexPos.RangedUnit)
                    {
                        whichUnit = 1;
                    }
                    else if (hexPos.Builder)
                    {
                        whichUnit = 2;
                    }
                    else if (hexPos.Building)
                    {
                        whichUnit = 3;
                    }
                }

                if (whichUnit == 0)
                {
                    if (hexPos.MeleeUnit.TeamNum == GameManager.Instance.TeamNum)
                    {
                        SelectUnit(hexPos.MeleeUnit);
                    }
                    else
                    {
                        DeselectUnit();
                    }
                }
                else if (whichUnit == 1)
                {
                    if (hexPos.RangedUnit.TeamNum == GameManager.Instance.TeamNum)
                    {
                        SelectUnit(hexPos.RangedUnit);
                    }
                    else
                    {
                        DeselectUnit();
                    }
                }
                else if (whichUnit == 2)
                {
                    if (hexPos.Builder.TeamNum == GameManager.Instance.TeamNum)
                    {
                        SelectUnit(hexPos.Builder);
                    }
                    else
                    {
                        DeselectUnit();
                    }
                }
                else if (whichUnit == 3)
                {
                    if (hexPos.Building.TeamNum == GameManager.Instance.TeamNum)
                    {
                        SelectUnit(hexPos.Building);
                    }
                    else
                    {
                        DeselectUnit();
                    }
                }
                else
                {
                    DeselectUnit();
                }
            }
            else if (!hexPos && Results.Count == 0)
            {
                DeselectUnit();
            }
        }
        else if (Input.GetMouseButtonDown(1) && IsTurn && Selected)
        {
            if (hexPos)
            {
                if (Selected.SelectableType == SelectableTypes.Melee)
                {
                    int canAttack = Selected.GetComponent<MeleeCommands>().CanAttack(hexPos);

                    if (canAttack == 2)
                    {
                        Selected.gameObject.GetPhotonView().RPC("Attack", RpcTarget.All, hexPos.ID);
                    }
                    else if (canAttack == 1)
                    {
                        Selected.gameObject.GetPhotonView().RPC("Move", RpcTarget.All, hexPos.ID);
                    }
                    else
                    {
                        //Cant do that
                    }
                }
                else if (Selected.SelectableType == SelectableTypes.Ranged)
                {
                    int canAttack = Selected.GetComponent<RangedCommands>().CanAttack(hexPos);

                    if (canAttack == 2)
                    {
                        Selected.gameObject.GetPhotonView().RPC("Attack", RpcTarget.All, hexPos.ID);
                    }
                    else if (canAttack == 1)
                    {
                        Selected.gameObject.GetPhotonView().RPC("Move", RpcTarget.All, hexPos.ID);
                    }
                    else
                    {
                        //Cant do that
                    }
                }
                else if (Selected.SelectableType == SelectableTypes.Builder)
                {
                    if (Selected.GetComponent<BuilderCommands>().CanMove(hexPos))
                    {
                        Selected.gameObject.GetPhotonView().RPC("Move", RpcTarget.All, hexPos.ID);
                    }
                    else
                    {
                        //Cant do that
                    }
                }
            }
        }
    }

    public void RemoveForest(int hexViewID)
    {

    }

    public void RemovePlayer(int clientID)
    {

    }

    public void NextTurn()
    {

    }

    public void UnitAttack(int unitViewID, int hexPosID)
    {

    }

    public void MoveUnit(int unitViewID, int hexPosID)
    {

    }

    public HexPos ReturnHexPos()
    {
        HexPos hexPos = null;

        if (Physics.Raycast(MainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        {
            hexPos = hit.transform.GetComponent<HexPos>();
        }

        return hexPos;
    }

    public void SelectUnit(UnitSelectable unit)
    {
        DeselectUnit();

        if(unit.GetComponent<MeleeCommands>())
        {
            unit.CurrentPos.Select(0);
        }
        else if (unit.GetComponent<RangedCommands>())
        {
            unit.CurrentPos.Select(1);
        }
        else if (unit.GetComponent<BuilderCommands>())
        {
            unit.CurrentPos.Select(2);
        }
        else
        {
            unit.CurrentPos.Select(3);
        }

        unit.SelectedSprite.enabled = true;
        Selected = unit;

        if (unit.SelectableType == SelectableTypes.Builder)
        {
            UIControl.Instance.ShowBuildMenu();
        }
        else if (unit.SelectableType == SelectableTypes.HeadQuarters || unit.SelectableType == SelectableTypes.Barracks)
        {
            UIControl.Instance.ShowRecruitMenu();
        }
    }

    public void DeselectUnit()
    {
        if (Selected)
        {
            Selected.SelectedSprite.enabled = false;
            UIControl.Instance.CloseProfileMenu();
            Selected = null;
        }
    }
}

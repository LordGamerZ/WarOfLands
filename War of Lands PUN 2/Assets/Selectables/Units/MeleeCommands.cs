using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;

public enum MeleeTypes
{
    Infantry,
    Cavalry
}


public class MeleeCommands : UnitSelectable
{
    public float MaxMoves;
    public float CurrentMoves;

    public int MaxAttacks;
    public int NumAttacks;

    public float Damage;

    public MeleeTypes MeleeType;

    public void Awake()
    {
        SelectedSprite.enabled = false;
        CurrentMoves = 0;
        NumAttacks = 0;

        if (Damage > 0)
        {
            Damage *= -1;
        }
    }

    public override void Death()
    {
        CurrentPos.Remove(0);
        CurrentPos.MeleeUnit = null;
        Destroy(gameObject);
    }

    [PunRPC]
    public void Attack(int hexID)
    {
        HexPos hexPos = GameManager.Instance.Board[hexID];
        NumAttacks += 1;

        if (hexPos.MeleeUnit)
        {
            if (MeleeType == MeleeTypes.Cavalry && hexPos.MeleeUnit.MeleeType == MeleeTypes.Infantry)
            {
                hexPos.MeleeUnit.ChangeHealth(Damage * 2);
                ChangeHealth(hexPos.MeleeUnit.Damage);
            }
            else if (MeleeType == MeleeTypes.Infantry && hexPos.MeleeUnit.MeleeType == MeleeTypes.Cavalry)
            {
                hexPos.MeleeUnit.ChangeHealth(Damage);
                ChangeHealth(hexPos.MeleeUnit.Damage * 2);
            }
        }
        else if (hexPos.RangedUnit)
        {
            if (hexPos.RangedUnit.RangedType == RangedTypes.Archer)
            {
                hexPos.RangedUnit.ChangeHealth(Damage * 2);
            }
            else
            {
                hexPos.RangedUnit.ChangeHealth(Damage);
            }
        }
        else if (hexPos.Builder)
        {
            hexPos.Builder.ChangeHealth(Damage);
        }
        else if (hexPos.Building)
        {
            hexPos.Building.ChangeHealth(Damage * 0.1f);
        }

        if (CanAttack(hexPos) == 1)
        {
            Move(hexPos.ID);
        }
    }

    public int CanAttack(HexPos hexPos)
    {
        if (Vector3.Distance(hexPos.transform.position, CurrentPos.transform.position) < 8 && Vector3.Distance(hexPos.transform.position, CurrentPos.transform.position) > 1)
        {
            if (CurrentMoves + 1 > MaxMoves)
            {
                return 0;
            }

            if (hexPos.MeleeUnit)
            {
                if (NumAttacks + 1 <= MaxAttacks && hexPos.MeleeUnit.TeamNum != TeamNum)
                {
                    return 2;
                }
                else
                {
                    return 0;
                }
            }

            if (hexPos.RangedUnit)
            {
                if (hexPos.RangedUnit.TeamNum == TeamNum)
                {
                    return 0;
                }
                else if (NumAttacks + 1 <= MaxAttacks)
                {
                    return 2;
                }
                else
                {
                    return 0;
                }
            }

            if (hexPos.Builder)
            {
                if (hexPos.Builder.TeamNum == TeamNum)
                {
                    return 1;
                }
                else if (NumAttacks + 1 <= MaxAttacks)
                {
                    return 2;
                }
                else
                {
                    return 0;
                }
            }

            if (hexPos.Building)
            {
                if (hexPos.Building.TeamNum == TeamNum)
                {
                    return 1;
                }
                else if (NumAttacks + 1 <= MaxAttacks)
                {
                    return 2;
                }
                else
                {
                    return 0;
                }
            }

            return 1;
        }

        return 0;
    }

    [PunRPC]
    public void Move(int hexPosID)
    {
        HexPos hexPos = GameManager.Instance.Board[hexPosID];

        transform.rotation = Quaternion.Euler(0, Quaternion.LookRotation(hexPos.transform.position - transform.position).eulerAngles.y, 0);
        CurrentMoves += 1;

        CurrentPos.MeleeUnit = null;
        CurrentPos.Remove(0);
        CurrentPos = hexPos;
        transform.position = hexPos.transform.position;
        hexPos.MeleeUnit = this;

        CurrentPos.Select(0);
    }

    public override void NextTurn()
    {
        if (OwnerID == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            CurrentMoves = 0;
            NumAttacks = 0;
        }
    }
}
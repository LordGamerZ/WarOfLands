using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Currently has no use
public enum Terrains
{
    Land,
    Sea,
    Amphibious
}

//Decides what buildings can be built on a tile
public enum Biomes
{
    Plains,
    Hills,
    Forest
}

/*Unlike other objects within the game, hex positions on the map aren't managed by photon, mainly because
 they don't need to and also because at time there can be a large number of them, they also arent ever destroyed during the game*/
public class HexPos : MonoBehaviour
{
    public int ID;

    public MeleeCommands MeleeUnit;
    public RangedCommands RangedUnit;
    public BuilderCommands Builder;
    public UnitSelectable Building;

    public BillBoard Board;

    public int Showing;

    public Terrains TerrainType;
    public Biomes BiomeType;

    //Initialisation
    private void Awake()
    {
        Building = null;
        MeleeUnit = null;
        RangedUnit = null;
        Builder = null;
        Showing = -1;
    }

    //Stops showing unit icon above tile
    public void Remove(int whichUnit)
    {
        if (whichUnit == 0)
        {
            Board.MeleeIcon.SetActive(false);
        }
        if (whichUnit == 1)
        {
            Board.RangedIcon.SetActive(false);
        }
        if (whichUnit == 2)
        {
            Board.BuilderIcon.SetActive(false);
        }
        if (whichUnit == 3)
        {
            Board.BuildingIcon.SetActive(false);
        }

        if (MeleeUnit && whichUnit != 0)
        {
            MeleeUnit.Model.SetActive(true);
            Showing = 0;
        }
        else if (RangedUnit && whichUnit != 1)
        {
            RangedUnit.Model.SetActive(true);
            Showing = 1;
        }
        else if (Builder && whichUnit != 2)
        {
            Builder.Model.SetActive(true);
            Showing = 2;
        }
        else if (Building && whichUnit != 3)
        {
            Building.Model.SetActive(true);
            Showing = 3;
        }
        else
        {
            Showing = -1;
            Board.gameObject.SetActive(false);
        }
    }

    //Show unit on a tile and hide other units
    public void Select(int whichUnit)
    {
        if (Showing != -1)
        {
            if (Showing == 0)
            {
                MeleeUnit.Model.SetActive(false);
            }
            else if (Showing == 1)
            {
                RangedUnit.Model.SetActive(false);
            }
            else if (Showing == 2)
            {
                Builder.Model.SetActive(false);
            }
            else if (Showing == 3)
            {
                Building.Model.SetActive(false);
            }
        }

        Board.gameObject.SetActive(true);

        if (whichUnit == 0)
        {
            MeleeUnit.Model.SetActive(true);
            Board.MeleeIcon.SetActive(true);
        }
        else if (whichUnit == 1)
        {
            RangedUnit.Model.SetActive(true);
            Board.RangedIcon.SetActive(true);
        }
        else if (whichUnit == 2)
        {
            Builder.Model.SetActive(true);
            Board.BuilderIcon.SetActive(true);
        }
        else if (whichUnit == 3)
        {
            Building.Model.SetActive(true);
            Board.BuildingIcon.SetActive(true);
        }

        Showing = whichUnit;
    }
}
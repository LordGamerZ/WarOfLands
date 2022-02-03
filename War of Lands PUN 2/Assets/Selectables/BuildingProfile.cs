using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Profile", menuName = "New Building Profile")]
public class BuildingProfile : Profile
{
    public Terrains TerrainType;
    public Biomes BiomeType;
}

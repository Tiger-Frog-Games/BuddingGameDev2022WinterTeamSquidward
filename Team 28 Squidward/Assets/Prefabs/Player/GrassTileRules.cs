using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

[CreateAssetMenu(menuName ="TileSet/Custom Grass Tiles Rules")]
public class GrassTileRules : RuleTile<GrassTileRules.Neighbor> {
    public bool alwaysConnect;
    public TileBase[] tilesToConnect;
    public TileBase waterTile;
    public TileBase mudTile;

    public class Neighbor : RuleTile.TilingRule.Neighbor {
        //public const int This = 1;// Dont need these?
        //public const int NotThis = 2;
        public const int Any = 3;
        public const int Water = 4;
        public const int Mud = 5;
        public const int Nothing = 6;
    }

    public override bool RuleMatch(int neighbor, TileBase tile) {
        switch (neighbor) {
            case Neighbor.This: return checkThisTile(tile);
            case Neighbor.NotThis: return checkNotThis(tile);
            case Neighbor.Any: return checkAny(tile);
            case Neighbor.Water: return checkWater(tile);
            case Neighbor.Mud: return checkMud(tile);
            case Neighbor.Nothing: return checkNothing(tile);
        }
        return base.RuleMatch(neighbor, tile);
    }

    private bool checkThisTile(TileBase tile)
    {
        if (!alwaysConnect)
        {
            return tile == this;
        }
        return tilesToConnect.Contains(this) || tile == this ;
    }

    private bool checkNotThis(TileBase tile)
    {
        return tile != this;
    }

    private bool checkAny(TileBase tile)
    {
        return tile != null;
    }

    private bool checkSpecific(TileBase tile)
    {
        return false;
    }

    private bool checkWater(TileBase tile)
    {
        return tile = waterTile;
    }

    private bool checkMud(TileBase tile)
    {
        return tile = mudTile ;
    }

    private bool checkNothing(TileBase tile)
    {
        return tile == null;
    }

}
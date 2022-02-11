using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GrassTileRules))]
[CanEditMultipleObjects]
public class GraseTileEditor : RuleTileEditor
{
    public Texture2D Any;
    public Texture2D Water;
    public Texture2D Mud;
    public Texture2D Nothing;

    public override void RuleOnGUI(Rect rect, Vector3Int position, int neighbor)
    {
            //public const int Any = 3;
            //public const int Water = 4;
            //public const int Mud = 5;
            //public const int Nothing = 6;
        switch (neighbor)
        {
            case 3:
                GUI.DrawTexture(rect, Any);
                return;
            case 4:
                GUI.DrawTexture(rect, Water);
                return;
            case 5:
                GUI.DrawTexture(rect, Mud);
                return;
            case 6:
                GUI.DrawTexture(rect, Nothing);
                return;
        }

        base.RuleOnGUI(rect, position, neighbor);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CurtainTile))]
public class CurtainTileEditor : Editor
{
    void OnEnable()
    {
        CurtainTile curtain = (CurtainTile) target;

        Transform curtainHolder = curtain.gameObject.transform.parent;
        if (curtainHolder.childCount == 1)
        {
            curtain.extention.Deactivate();
            return;
        }

        DestroyImmediate(curtain.gameObject);
    }
}

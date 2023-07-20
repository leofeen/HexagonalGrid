using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridLevel))]
public class TwoSpritesGridLevelExtention : MonoBehaviour, IGridLevelExtention
{
    GridLevel level = null;

    public void Execute()
    {
        if (level is null) level = GetComponent<GridLevel>();

        foreach (PlacedObject placedObject in level.placedObjects)
        {
            placedObject.GetComponent<TwoSpritesPlacedObject>().ChangeSprite();
        }
    }

    public string GetFunctionName()
    {
        return "Change Sprites";
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridLevel))]
public class HideMapExtention : MonoBehaviour, IGridLevelExtention
{
    public bool isHidingActive = false;

    GridLevel level;
    List<GameObject> curtainTiles = new List<GameObject>();
    GameObject curtainHolder;

    public void Execute()
    {
        if (level == null) level = GetComponent<GridLevel>();

        if (isHidingActive)
        {
            Deactivate();
        }
        else
        {
            Activate();
        }
    }

    public string GetFunctionName()
    {
        return "Toogle Active Hiding";
    }

    public void Activate()
    {
        curtainHolder = new GameObject();
        curtainHolder.name = "Curtain Holder";
        curtainHolder.transform.Translate(Vector3.back * 0.5f);

        foreach (PlacedObject placedObject in level.placedObjects)
        {
            GameObject plasedGO = placedObject.gameObject;

            GameObject copy = (GameObject) Instantiate(plasedGO, plasedGO.transform.position, placedObject.transform.rotation, curtainHolder.transform);
            copy.name = "Curtain Tile";
            copy.GetComponent<SpriteRenderer>().color = Color.black;
            copy.AddComponent<CurtainTile>().extention = this;
            curtainTiles.Add(copy);
        }

        isHidingActive = true;
    }

    public void Deactivate()
    {
        foreach (GameObject curtain in curtainTiles)
        {
            DestroyImmediate(curtain);
        }

        DestroyImmediate(curtainHolder);

        curtainTiles = new List<GameObject>();
        
        isHidingActive = false;
    }
}

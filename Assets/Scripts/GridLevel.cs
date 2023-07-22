using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class GridLevel : MonoBehaviour
{
    public string mapName = "New Map";
    public float gridPlaneHeight = 0;
    public int numberOfHexesOnSide = 10;
    public float hexSize = 1f;
    public bool showChosenElement = false;
    public List<GridObject> avaibleObjects = new List<GridObject>();
    [HideInInspector]
    public int selectedIndex = 0;
    public Color gridLinesColor = new Color(1, 1, 1, 0.5f);
    public Color mouseOverOutlineColor = new Color(1, 0, 0);
    [HideInInspector]
    public List<PlacedObject> placedObjects = new List<PlacedObject>();

    void OnEnable()
    {
        RefreshPlacedObjectsList();
    }

    public void RefreshPlacedObjectsList()
    {
        placedObjects = transform.GetComponentsInChildren<PlacedObject>().ToList<PlacedObject>();
    }

    public GameObject PlaceObject(Vector2Int indicies, GridObject objectToPlace)
    {
        List<PlacedObject> toRemove = new List<PlacedObject>();
        foreach (PlacedObject placedObject in placedObjects)
        {
            if (placedObject.indiciesOnGrid == indicies)
            {
                if (objectToPlace.overrideExisting)
                {
                    UnityEditor.Undo.DestroyObjectImmediate(placedObject.gameObject);
                    toRemove.Add(placedObject);
                }
                else if (placedObject.id == objectToPlace.id)
                {
                    return placedObject.gameObject;
                }      
            }
        }

        foreach (PlacedObject obj in toRemove)
        {
            placedObjects.Remove(obj);
        }

        Vector3 positionOnGrid = IndiciesToWorldPoint(indicies) + objectToPlace.offset.ToXY();
        GameObject instanceGO = Instantiate<GameObject>(objectToPlace.prefab, positionOnGrid, Quaternion.identity, transform);

        PlacedObject newPlacedObject = instanceGO.AddComponent<PlacedObject>() as PlacedObject;
        newPlacedObject.id = objectToPlace.id;
        newPlacedObject.indiciesOnGrid = indicies;
        newPlacedObject.objectName = objectToPlace.objectName;
        placedObjects.Add(newPlacedObject);

        return instanceGO;
    }

    public GameObject PlaceObject(Vector3 position, GridObject objectToPlace)
    {
        Vector2Int indicies = WorldPointToIndicies(position);
        return PlaceObject(indicies, objectToPlace);
    }

    public GameObject PlaceSelectedObject(Vector3 position)
    {
        Vector2Int indicies = WorldPointToIndicies(position);
        return PlaceSelectedObject(indicies);
    }

    public GameObject PlaceSelectedObject(Vector2Int indicies)
    {     
        return PlaceObject(indicies, avaibleObjects[selectedIndex]);
    }

    public void ClearGrid()
    {
        foreach (PlacedObject placedObject in placedObjects)
        {
            UnityEditor.Undo.DestroyObjectImmediate(placedObject.gameObject);
        }

        placedObjects = new List<PlacedObject>();
    }

    public void DeleteObject(Vector3 position)
    {
        Vector2Int indicies = WorldPointToIndicies(position);
        DeleteObject(indicies);
    }

    public void DeleteObject(Vector2Int indicies)
    {
        for (int i = 0; i < placedObjects.Count; i++)
        {
            if (placedObjects[i].indiciesOnGrid == indicies)
            {
                UnityEditor.Undo.DestroyObjectImmediate(placedObjects[i].gameObject);
                placedObjects.Remove(placedObjects[i]);
                break;
            }
        }
    }

    public void RecalculatePositions()
    {
        foreach (PlacedObject placedObject in placedObjects)
        {
            GridObject associatedGridObject = avaibleObjects.First(x => x.objectName == placedObject.objectName);

            Vector3 newPosition = IndiciesToWorldPoint(placedObject.indiciesOnGrid) + associatedGridObject.offset.ToXY();
            placedObject.position = newPosition;
        }
    }

    public Vector2Int WorldPointToIndicies(Vector3 point)
    {
        float q = (Mathf.Sqrt(3) / 3f * point.x - 1f / 3f * point.y) / hexSize;
        float r = (2f / 3f * point.y) / hexSize;
        return RoundToIntIndicies(q, r);
    }

    public Vector2Int RoundToIntIndicies(float q, float r)
    {
        return RoundToIntIndicies(new Vector2(q, r));
    }

    public Vector2Int RoundToIntIndicies(Vector2 frac)
    {
        int q = Mathf.RoundToInt(frac.x);
        int r = Mathf.RoundToInt(frac.y);
        int s = Mathf.RoundToInt(-frac.x - frac.y);

        float qDiff = Mathf.Abs(q - frac.x);
        float rDiff = Mathf.Abs(r - frac.y);
        float sDiff = Mathf.Abs(s + frac.x + frac.y);

        if ((qDiff > rDiff) && (qDiff > sDiff))
        {
            q = -r - s;
        }
        else if (rDiff > sDiff)
        {
            r = -q - s;
        }

        return new Vector2Int(q, r);
    }

    public Vector3 IndiciesToWorldPoint(Vector2Int indicies)
    {
        float x = hexSize * (Mathf.Sqrt(3) * indicies.x + Mathf.Sqrt(3) / 2f * indicies.y);
        float y = hexSize * 3f / 2f * indicies.y;
        return new Vector3(x, y, gridPlaneHeight);
    }

    public Vector3 IndiciesToWorldPoint(int q, int r)
    {
        return IndiciesToWorldPoint(new Vector2Int(q, r));
    }


    [System.Serializable]
    public class GridObject
    {
        static int _id;
        public string objectName;
        public GameObject prefab;
        public Vector2 offset;
        public bool overrideExisting = true;
        public int id 
        {
            get;
            private set;
        }

        public GridObject()
        {
            id = System.Threading.Interlocked.Increment(ref _id);
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class GridLevel : MonoBehaviour
{
    public float gridPlaneHeight = 0;
    public int maxCoordinateLinesDrawn = 20;
    public GameObject standartGridObject;
    public List<GridObject> avaibleObjects = new List<GridObject>();
    [HideInInspector]
    public int selectedIndex = 0;
    public Vector2 gridSize;
    public Color gridLinesColor = new Color(1, 1, 1, 0.5f);
    public Color mouseOverFillColor = new Color(1, 1, 1, 0.2f);
    public Color mouseOverOutlineColor = new Color(0, 0, 0);
    
    List<PlacedObject> placedObjects = new List<PlacedObject>();

    public GridObject selectedObject {
        get {
            return avaibleObjects[selectedIndex];
        }
    }

    void OnEnable()
    {
        RefreshPlacedObjectsList();
    }

    public void RefreshPlacedObjectsList()
    {
        placedObjects = transform.GetComponentsInChildren<PlacedObject>().ToList<PlacedObject>();
    }

    public void RecalculateGridSize()
    {
        gridSize = new Vector2(
            Mathf.Abs(standartGridObject.transform.localScale.x), 
            Mathf.Abs(standartGridObject.transform.localScale.y)
            );
    }

    public GameObject PlaceSelectedObject(Vector3 position)
    {
        Vector2Int indicies = WorldPointToIndicies(position);
        return PlaceSelectedObject(indicies);
    }
    public GameObject PlaceSelectedObject(Vector2Int indicies)
    {     
        List<PlacedObject> toRemove = new List<PlacedObject>();
        foreach (PlacedObject placedObject in placedObjects)
        {
            if (placedObject.indiciesOnGrid == indicies)
            {
                if (selectedObject.overrideExisting)
                {
                    UnityEditor.Undo.DestroyObjectImmediate(placedObject.gameObject);
                    toRemove.Add(placedObject);
                }
                else if (placedObject.id == selectedObject.id)
                {
                    return placedObject.gameObject;
                }      
            }
        }

        foreach (PlacedObject obj in toRemove)
        {
            placedObjects.Remove(obj);
        }

        Vector3 positionOnGrid = IndiciesToWorldPoint(indicies) + selectedObject.offset.ToXY();
        GameObject instanceGO = Instantiate<GameObject>(selectedObject.prefab, positionOnGrid, Quaternion.identity, transform);

        PlacedObject newPlacedObject = instanceGO.AddComponent<PlacedObject>() as PlacedObject;
        newPlacedObject.id = selectedObject.id;
        newPlacedObject.indiciesOnGrid = indicies;
        newPlacedObject.associatedGridObjectIndex = selectedIndex;
        placedObjects.Add(newPlacedObject);

        return instanceGO;
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
            GridObject associatedGridObject = avaibleObjects[placedObject.associatedGridObjectIndex];

            Vector3 newPosition = IndiciesToWorldPoint(placedObject.indiciesOnGrid) + associatedGridObject.offset.ToXY();
            placedObject.position = newPosition;
        }
    }

    public Vector2Int WorldPointToIndicies(Vector3 point)
    {
        Vector3 shiftedPoint = point + 0.5f * gridSize.ToXY();
        return new Vector2Int((int) Mathf.Floor(shiftedPoint.x / gridSize.x), (int) Mathf.Floor(shiftedPoint.y / gridSize.y));
    }

    public Vector3 IndiciesToWorldPoint(Vector2Int indicies)
    {
        return new Vector3(indicies.x * gridSize.x, indicies.y * gridSize.y, gridPlaneHeight);
    }

    public Vector3 IndiciesToWorldPoint(int x, int y)
    {
        return IndiciesToWorldPoint(new Vector2Int(x, y));
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


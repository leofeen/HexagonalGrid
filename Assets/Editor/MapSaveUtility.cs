using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class MapSaveUtility
{
    GridLevel target;

    public static MapSaveUtility GetInstance(GridLevel level)
    {
        return new MapSaveUtility{target = level};
    }

    public void SaveMap(string mapName)
    {
        if (target.avaibleObjects.Count == 0) return;

        MapData mapData = ScriptableObject.CreateInstance<MapData>();

        mapData.avaibleObjects = new List<GridLevel.GridObject>(target.avaibleObjects);

        foreach (PlacedObject placedObject in target.placedObjects)
        {
            mapData.placedObjects[placedObject.indiciesOnGrid] = placedObject.id;
        }

        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");

        AssetDatabase.CreateAsset(mapData, $"Assets/Resources/{mapName}.asset");
        AssetDatabase.SaveAssets();
    }

    public void LoadMap(string mapName)
    {
        MapData mapData = Resources.Load<MapData>(mapName);

        if (mapData == null)
        {
            EditorUtility.DisplayDialog("FIle does not exist", "Please, enter valid map name", "OK");
            return;
        }

        target.ClearGrid();
        target.avaibleObjects = mapData.avaibleObjects;
        PlaceObjects(mapData);
    }

    void PlaceObjects(MapData mapData)
    {
        foreach (var (objectIndicies, objectId) in mapData.placedObjects)
        {
            GridLevel.GridObject objectToPlace = mapData.avaibleObjects.First(x => x.id == objectId);
            target.PlaceObject(objectIndicies, objectToPlace);
        }
    }
}

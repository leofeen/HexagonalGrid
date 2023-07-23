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
            mapData.indicies.Add(placedObject.indiciesOnGrid);
            mapData.placedObjects.Add(placedObject.objectName);
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
        for (int i = 0; i < mapData.indicies.Count; i++)
        {
            GridLevel.GridObject objectToPlace = mapData.avaibleObjects.First(x => x.objectName == mapData.placedObjects[i]);
            target.PlaceObject(mapData.indicies[i], objectToPlace);
        }
    }
}

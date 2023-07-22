using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapData : ScriptableObject
{
    public Dictionary<Vector2Int, int> placedObjects = new Dictionary<Vector2Int, int>();
    public List<GridLevel.GridObject> avaibleObjects = new List<GridLevel.GridObject>();
}

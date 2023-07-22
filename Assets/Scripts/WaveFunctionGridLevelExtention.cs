using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(GridLevel))]
public class WaveFunctionGridLevelExtention : MonoBehaviour, IGridLevelExtention
{
    GridLevel level = null;
    public WaveFunctionCollapse waveFunction;

    public void Execute()
    {
        if (level == null) level = GetComponent<GridLevel>();
        if (waveFunction == null) return;

        level.ClearGrid();

        waveFunction.Collapse();
        Dictionary<Vector3Int, string> generatedMap = waveFunction.InterpretWaveSpace();

        foreach (var (indicies3D, stateName) in generatedMap)
        {
            Vector2Int indicies = new Vector2Int(indicies3D.x, indicies3D.y);
            GridLevel.GridObject objectToPlace = level.avaibleObjects.First(x => x.objectName == stateName);
            level.PlaceObject(indicies, objectToPlace);
        }
    }

    public string GetFunctionName()
    {
        return "Generate New Map";
    }
}

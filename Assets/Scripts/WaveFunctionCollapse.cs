using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
public class WaveFunctionCollapse : MonoBehaviour
{
    public int spaceSize = 10;
    public SupportedSpaceConfigurations chosenConfiguration = SupportedSpaceConfigurations.Square2D;
    public int numberOfSides => GetNumberOfSides(chosenConfiguration);
    public List<WaveFunctionState> avaibleStates = new List<WaveFunctionState>();
    public WaveFunctionState UndeterminedState;

    // public string perloadPresetName = "New Preset";

    WaveFunctionSpace space;

    void ClearNeighbourStates()
    {
        foreach (WaveFunctionState state in avaibleStates)
        {
            for (int i = 0; i < numberOfSides; i++)
            {
                state.allowedNeighborStates[i] = new List<WaveFunctionState>();
            }
        }
    }

    void DeduplicateNeighbourStates()
    {
        foreach (WaveFunctionState state in avaibleStates)
        {
            for (int i = 0; i < numberOfSides; i++)
            {
                state.allowedNeighborStates[i] = state.allowedNeighborStates[i].Distinct().ToList();
            }
        }
    }

    public void ApplyPreset(WavePreset preset)
    {  
        ClearNeighbourStates();

        for (int i = 0; i < preset.nodeDatas.Count; i++)
        {
            WaveFunctionState currentState = avaibleStates[i];

            List<NodeLinkData> connections = preset.nodeLinkDatas.Where(x => x.baseNodeGUID == currentState.GUID).ToList();
            for (int j = 0; j < connections.Count; j++)
            {
                string targetGuid = connections[j].targetNodeGUID;
                WaveFunctionState targetState = avaibleStates.First(x => x.GUID == targetGuid);

                int currentSide = connections[j].baseNodePortSide;
                int targetSide = connections[j].targetNodePortSide;

                currentState.allowedNeighborStates[currentSide].Add(targetState);
                targetState.allowedNeighborStates[targetSide].Add(currentState);
            }
        }

        DeduplicateNeighbourStates();
    }

    public void Collapse()
    {
        InitializeSpace();
        
        CollapseAtPoint(space.zero, avaibleStates);

        List<List<int>> previouslyCollapsed = new List<List<int>>();
        previouslyCollapsed.Add(space.zero);

        while (true)
        {
            List<List<int>> allNeighbours = GetAllNeighbours(previouslyCollapsed);
            // Debug.Log(allNeighbours.Count);
            List<List<int>> undeterminedStateNeighbours = allNeighbours.FindAll(x => space[x] == UndeterminedState);

            if (undeterminedStateNeighbours.Count == 0) break;

            CollapseAtPoints(undeterminedStateNeighbours);
            previouslyCollapsed = new List<List<int>>(undeterminedStateNeighbours);
        }
    }

    void InitializeSpace()
    {
        space = new WaveFunctionSpace(spaceSize, numberOfSides);
        space.FillWith(UndeterminedState);
    }

    void CollapseAtPoint(List<int> coordinates, List<WaveFunctionState> avaibleStatesForPoint)
    {
        // Debug.Log(avaibleStatesForPoint.Count);
        int numberOfState = Random.Range(0, avaibleStatesForPoint.Count);
        // Debug.Log(numberOfState);
        WaveFunctionState collapsedState = avaibleStatesForPoint[numberOfState];

        space[coordinates] = collapsedState;
    }

    void CollapseAtPoints(List<List<int>> points)
    {
        List<List<WaveFunctionState>> avaibleStatesForPoints = new List<List<WaveFunctionState>>();
        List<int> entropy = new List<int>();

        points.ForEach(x => avaibleStatesForPoints.Add(GetAvaibleStatesAtPoint(x)));
        avaibleStatesForPoints.ForEach(x => entropy.Add(x.Count));

        points = points.OrderBy(x => entropy[points.IndexOf(x)]).ToList();
        avaibleStatesForPoints = avaibleStatesForPoints.OrderBy(x => entropy[avaibleStatesForPoints.IndexOf(x)]).ToList();

        for (int i = 0; i < points.Count; i++)
        {
            CollapseAtPoint(points[i], avaibleStatesForPoints[i]);
        }        
    }

    List<List<int>> GetAllNeighbours(List<List<int>> points)
    {
        List<List<int>> result = new List<List<int>>();

        foreach (List<int> coordinates in points)
        {
            List<List<int>> neighbours = space.GetNeighbours(coordinates).Values.ToList();
            result = result.Union(neighbours).ToList();
        }

        return DeleteDuplicates(result);
    }

    List<WaveFunctionState> GetAvaibleStatesAtPoint(List<int> coordinates)
    {
        List<WaveFunctionState> result = new List<WaveFunctionState>(avaibleStates);

        Dictionary<int, List<int>> neighbours = space.GetNeighbours(coordinates);
        foreach (int i in neighbours.Keys)
        {
            WaveFunctionState neighbourState = space[neighbours[i]];
            List<WaveFunctionState> allowedNeighboursForNeighbourState = (neighbourState == UndeterminedState) 
                ? avaibleStates 
                : neighbourState.allowedNeighborStates.GetValueOrDefault((i + space.numberOfCoordinates) % numberOfSides, avaibleStates);

            result = result.Intersect(allowedNeighboursForNeighbourState).ToList();
        }

        return result;
    }

    List<List<int>> DeleteDuplicates(List<List<int>> points)
    {
        List<List<int>> result = new List<List<int>>();

        foreach (List<int> coordinates in points)
        {
            bool skip = false;
            foreach (List<int> x in result) if (Enumerable.SequenceEqual(coordinates, x)) skip = true;
            if (skip) continue;
            result.Add(coordinates);
        }

        return result;
    }

    public Dictionary<Vector3Int, string> InterpretWaveSpace()
    {
        Dictionary<Vector3Int, string> result = new Dictionary<Vector3Int, string>();

        result[Vector3Int.zero] = space[space.zero].stateName;

        List<List<int>> previouslyChecked = new List<List<int>>();
        previouslyChecked.Add(space.zero);

        while (true)
        {
            List<List<int>> allNeighbours = GetAllNeighbours(previouslyChecked);
            // Debug.Log(allNeighbours.Count);
            List<List<int>> uncheckedCoordinates = SubtractListCoordinates(allNeighbours, previouslyChecked);

            if (uncheckedCoordinates.Count == 0) break;

            foreach (List<int> coordinates in uncheckedCoordinates)
            {
                Vector3Int indicies;
                if (coordinates.Count == 2)
                {
                    indicies = new Vector3Int(coordinates[0], coordinates[1], 0);
                }
                else
                {
                    indicies = new Vector3Int(coordinates[0], coordinates[1], coordinates[2]);
                }

                result[indicies] = space[coordinates].stateName;
            }

            previouslyChecked.AddRange(uncheckedCoordinates);
        }

        return CleanseInvalidCoordinates(result);
    }

    public Dictionary<Vector3Int, string> CleanseInvalidCoordinates(Dictionary<Vector3Int, string> uncleansed)
    {
        if (!(chosenConfiguration == SupportedSpaceConfigurations.Hex2D)) return uncleansed;

        Dictionary<Vector3Int, string> cleansed = new Dictionary<Vector3Int, string>();
        foreach (var (indicies, stateName) in uncleansed)
        {
            if (indicies.x + indicies.y + indicies.z == 0) cleansed[indicies] = stateName;
        }

        return cleansed;
    }

    public static List<List<int>> SubtractListCoordinates(List<List<int>> first, List<List<int>> second)
    {
        List<List<int>> result = new List<List<int>>();

        foreach (List<int> coordinates in first)
        {
            bool skip = false;
            foreach (List<int> x in second) if (Enumerable.SequenceEqual(coordinates, x)) skip = true;
            if (skip) continue;
            result.Add(coordinates);
        }

        return result;
    }

    public static int GetNumberOfSides(SupportedSpaceConfigurations configuration)
    {
        if (configuration == SupportedSpaceConfigurations.Square2D) return 4;
        if (configuration == SupportedSpaceConfigurations.Square3D
            || configuration == SupportedSpaceConfigurations.Hex2D) return 6;

        return 0;
    }


    public enum SupportedSpaceConfigurations
    {
        Square2D,
        Square3D,
        Hex2D,
    }
}

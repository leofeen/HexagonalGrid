using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveFunctionSpace
{
    public int spaceSize;
    public int numberOfSides => WaveFunctionCollapse.GetNumberOfSides(configuration);

    public int numberOfCoordinates => GetNumberOfCoordinates(configuration);
    public List<int> zero {
        get {
            if (_zero.Count != 0) return _zero;

            for (int i = 0; i < numberOfCoordinates; i++) _zero.Add(0);
            return _zero;
        }
    }
    List<int> _zero = new List<int>();
    
    List<WaveFunctionState> space;
    int spaceListSize;
    bool isHexagonal;
    SupportedSpaceConfigurations configuration;

    public WaveFunctionSpace(int spaceSize, SupportedSpaceConfigurations configuration)
    {
        this.spaceSize = spaceSize;
        this.configuration = configuration;
        this.isHexagonal = configuration == SupportedSpaceConfigurations.Hex2D;

        spaceListSize = Mathf.RoundToInt(Mathf.Pow(2*spaceSize + 1, numberOfCoordinates));
        space = new List<WaveFunctionState>();
    }

    int CalculateIndexInList(List<int> coordinates)
    {
        int indexInList = 0;
        for (int i = 0; i < coordinates.Count; i++)
        {
            indexInList += (coordinates[i] + spaceSize) * Mathf.RoundToInt(Mathf.Pow(2*spaceSize + 1, i));
        }

        return indexInList;
    } 

    public WaveFunctionState this[List<int> coordinates]
    {
        get { return space[CalculateIndexInList(coordinates)]; }
        set { space[CalculateIndexInList(coordinates)] = value; }
    }

    public Dictionary<int, List<int>> GetNeighbours(List<int> coordinates)
    {
        Dictionary<int, List<int>> result = new Dictionary<int, List<int>>();

        if (!isHexagonal)
        {
            for (int i = 0; i < numberOfSides; i++)
            {
                int coordinateNumber = i % numberOfCoordinates;
                bool isNegative = i / numberOfCoordinates == 1;

                int coordinateDelta = isNegative ? -1 : 1;
                int newCoordinateValue = coordinates[coordinateNumber] + coordinateDelta;

                if (Mathf.Abs(newCoordinateValue) > spaceSize) continue;

                List<int> copy = new List<int>(coordinates);
                copy[coordinateNumber] = newCoordinateValue;
                result[i] = copy;
            }
        }
        else
        {
            List<int>[] hexDirections = new List<int>[]{
                new List<int>(new int[]{1, 0}),
                new List<int>(new int[]{0, 1}),
                new List<int>(new int[]{-1, 1}),
                new List<int>(new int[]{-1, 0}),
                new List<int>(new int[]{0, -1}),
                new List<int>(new int[]{1, -1}),
            };
            
            for (int i = 0; i < 6; i++)
            {
                List<int> delta = hexDirections[i];
                
                int newQ = coordinates[0] + delta[0];
                int newR = coordinates[1] + delta[1];

                if (Mathf.Abs(newQ) > spaceSize || Mathf.Abs(newR) > spaceSize) continue;

                List<int> copy = new List<int>(coordinates);
                copy[0] = newQ;
                copy[1] = newR;
                result[i] = copy;
            }
        }

        return result;
    }

    public void FillWith(WaveFunctionState filler)
    {
        for (int i = 0; i < spaceListSize; i++)
        {
            space.Add(filler);
        }
    }

    static int GetNumberOfCoordinates(SupportedSpaceConfigurations configuration)
    {
        if (configuration == SupportedSpaceConfigurations.Square2D) return 2;
        if (configuration == SupportedSpaceConfigurations.Square3D) return 3;
        if (configuration == SupportedSpaceConfigurations.Hex2D) return 2;

        return 0;
    }
}

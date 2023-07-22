using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveFunctionSpace
{
    public int spaceSize;
    public int numberOfSides;

    public int numberOfCoordinates => numberOfSides / 2;
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

    public WaveFunctionSpace(int spaceSize, int numberOfSides)
    {
        this.spaceSize = spaceSize;
        this.numberOfSides = numberOfSides;

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

        return result;
    }

    public void FillWith(WaveFunctionState filler)
    {
        for (int i = 0; i < spaceListSize; i++)
        {
            space.Add(filler);
        }
    }
}

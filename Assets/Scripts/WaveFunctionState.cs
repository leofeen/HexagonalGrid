using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu]
public class WaveFunctionState : ScriptableObject
{
    [HideInInspector]
    public string GUID;
    public string stateName;
    public Dictionary<int, List<WaveFunctionState>> allowedNeighborStates = new Dictionary<int, List<WaveFunctionState>>(); 
}

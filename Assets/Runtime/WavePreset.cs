using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WavePreset : ScriptableObject
{
    public List<NodeLinkData> nodeLinkDatas = new List<NodeLinkData>();
    public List<WaveFunctionStateNodeData> nodeDatas = new List<WaveFunctionStateNodeData>();
}

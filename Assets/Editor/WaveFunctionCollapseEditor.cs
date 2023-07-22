using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WaveFunctionCollapse))]
public class WaveFunctionCollapseEditor : Editor
{
    WaveFunctionCollapse waveFunction;

    void OnEnable()
    {
        waveFunction = (WaveFunctionCollapse) target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Redact Wave Fuction States"))
        {
            WaveFunctionStatesRedactor.RedactWaveFuction(waveFunction);
        }

        if (GUILayout.Button("Collapse"))
        {
            waveFunction.Collapse();
        }
    }
}

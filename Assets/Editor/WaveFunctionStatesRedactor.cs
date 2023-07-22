using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class WaveFunctionStatesRedactor : EditorWindow
{
    WaveFunctionView waveView;
    string presetName = "New Preset";

    public static void RedactWaveFuction(WaveFunctionCollapse waveFunction = null)
    {
        WaveFunctionStatesRedactor wnd = GetWindow<WaveFunctionStatesRedactor>();
        wnd.titleContent = new GUIContent("WaveFunctionStatesRedactor");

        WaveFunctionView waveView = wnd.waveView;
        waveView.target = waveFunction;
        waveView.GenerateStateNodes();
        waveView.ConnectNodes();
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        GenerateGraphView(root);
        GenerateToolbar(root);
    }

    void GenerateGraphView(VisualElement root)
    {
        waveView = new WaveFunctionView();
        waveView.StretchToParentSize();
        root.Add(waveView);
    }

    void GenerateToolbar(VisualElement root)
    {
        Toolbar toolbar = new Toolbar();

        TextField presetNameField = new TextField("Preset Name: ");
        presetNameField.SetValueWithoutNotify(presetName);
        presetNameField.MarkDirtyRepaint();
        presetNameField.RegisterValueChangedCallback((evt) => {
            presetName = evt.newValue;
        });
        toolbar.Add(presetNameField);

        toolbar.Add(new Button(() => RequestDataOperation(true)){text = "Save Preset"});
        toolbar.Add(new Button(() => RequestDataOperation(false)){text = "Load Preset"});

        root.Add(toolbar);
    }

    void RequestDataOperation(bool save)
    {
        if (string.IsNullOrEmpty(presetName))
        {
            EditorUtility.DisplayDialog("Incorrect preset name", "Please, enter valid preset name", "OK");
            return;
        }

        GraphSaveUtility saveUtility = GraphSaveUtility.GetInstance(waveView);
        WavePreset constructedPreset;

        if (save)
        {
            constructedPreset = saveUtility.SaveGraph(presetName);
        }
        else
        {
            constructedPreset = saveUtility.LoadGraph(presetName);
        }

        if (constructedPreset == null) return;

        waveView.target.ApplyPreset(constructedPreset);
    }
}
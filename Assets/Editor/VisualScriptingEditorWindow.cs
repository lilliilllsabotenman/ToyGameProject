        // Editor/VisualScriptingEditorWindow.cs
using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

public class VisualScriptingEditorWindow : EditorWindow
{
    private VisualScriptingGraphView _graphView;
    private VariablesPanel _variablesPanel;

    [MenuItem("Window/Animation Visual Scripting Editor")]
    public static void Open()
    {
        VisualScriptingEditorWindow window = GetWindow<VisualScriptingEditorWindow>("AVS Editor");
        window.minSize = new Vector2(800, 600);
    }

    private void OnEnable()
    {
        ConstructGraphView();
        AddToolbar();
    }

    private void ConstructGraphView()
    {
        _graphView = new VisualScriptingGraphView
        {
            name = "Visual Scripting Graph"
        };
        // ウィンドウ全体に広げる
        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);

        _variablesPanel = new VariablesPanel();
        _variablesPanel.style.position = Position.Absolute;
        _variablesPanel.style.right = 0;
        _variablesPanel.style.top = 20;
        _variablesPanel.style.bottom = 0;
        rootVisualElement.Add(_variablesPanel);
    }

    private void AddToolbar()
    {
        var toolbar = new UnityEditor.UIElements.Toolbar();

        var saveBtn = new Button(() => GraphSerializer.Save(_graphView.nodes, _graphView.edges)) { text = "Save" };
        var loadBtn = new Button(() => GraphSerializer.Load(_graphView)) { text = "Load" };

        Button targetTypeBtn = new Button { text = "Target: " + GetCurrentTargetTypeLabel() };
        targetTypeBtn.clicked += () => OnTargetTypeClicked(targetTypeBtn);

        toolbar.Add(saveBtn);
        toolbar.Add(loadBtn);
        toolbar.Add(targetTypeBtn);
        rootVisualElement.Add(toolbar);
    }

    private void OnTargetTypeClicked(Button anchor)
    {
        AdvancedDropdownState state = new AdvancedDropdownState();
        TargetTypeDropdown dropdown = new TargetTypeDropdown(state, type => SetTargetType(anchor, type));
        dropdown.Show(anchor.worldBound);
    }

    private void SetTargetType(Button anchor, Type type)
    {
        VisualScriptingGraphData data = LoadOrCreateGraphData();
        data.TargetTypeName = type.AssemblyQualifiedName;
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();

        anchor.text = "Target: " + type.Name;
    }

    private string GetCurrentTargetTypeLabel()
    {
        VisualScriptingGraphData data = AssetDatabase.LoadAssetAtPath<VisualScriptingGraphData>(VisualScriptingGraphView.GraphDataPath);
        if (data == null || string.IsNullOrEmpty(data.TargetTypeName)) return "(未選択)";

        Type type = Type.GetType(data.TargetTypeName);
        return type != null ? type.Name : "(未選択)";
    }

    private static VisualScriptingGraphData LoadOrCreateGraphData()
    {
        VisualScriptingGraphData data = AssetDatabase.LoadAssetAtPath<VisualScriptingGraphData>(VisualScriptingGraphView.GraphDataPath);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<VisualScriptingGraphData>();
            AssetDatabase.CreateAsset(data, VisualScriptingGraphView.GraphDataPath);
        }
        return data;
    }

    private void OnDisable()
    {
        if (_graphView != null) rootVisualElement.Remove(_graphView);
        if (_variablesPanel != null) rootVisualElement.Remove(_variablesPanel);
    }
}
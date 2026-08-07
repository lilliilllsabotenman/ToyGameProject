// Editor/VisualScriptingEditorWindow.cs
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

public class VisualScriptingEditorWindow : EditorWindow
{
    private VisualScriptingGraphView _graphView;

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
    }

    private void AddToolbar()
    {
        var toolbar = new UnityEditor.UIElements.Toolbar();

        var saveBtn = new Button(() => _graphView.SaveGraph()) { text = "Save" };
        var loadBtn = new Button(() => _graphView.LoadGraph()) { text = "Load" };

        toolbar.Add(saveBtn);
        toolbar.Add(loadBtn);
        rootVisualElement.Add(toolbar);
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(_graphView);
    }
}
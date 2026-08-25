        // Editor/VisualScriptingEditorWindow.cs
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class VisualScriptingEditorWindow : EditorWindow
{
    private VisualScriptingGraphView _graphView;
    private VariablesPanel _variablesPanel;

    // EditorWindow自体がScriptableObjectなので、ここに持たせればドメインリロードを跨いで残る。
    [SerializeField] private EditHistoryData _history = new();

    public static VisualScriptingEditorWindow Open()
    {
        VisualScriptingEditorWindow window = GetWindow<VisualScriptingEditorWindow>("AVS Editor");
        window.minSize = new Vector2(800, 600);
        return window;
    }

    // 編集対象(dataBase内のdata)を直接渡してこのウィンドウを開くための入口。
    // 呼び出し側(ParameterPresetEditorなど)が既に持っているVisualScriptingGraphData参照をそのまま渡す。
    public void Initialize(VisualScriptingGraphDataBase dataBase, VisualScriptingGraphData data)
    {
        _graphView.CurrentDataBase = dataBase;
        _graphView.CurrentData = data;
        GraphConverter.RebuildView(_graphView, data);
    }

    private void OnEnable()
    {
        ConstructGraphView();

        rootVisualElement.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.actionKey && evt.keyCode == KeyCode.S)
            {
                _graphView.TriggerAutoSave();
                evt.StopPropagation();
            }
            else if (evt.actionKey && evt.keyCode == KeyCode.Z)
            {
                _graphView.Undo();
                // _variablesPanel.Refresh(); // Member管理UIをAVSEditorから一時的に切り離し中(新システムへの一括差し替え待ち)
                evt.StopPropagation();
            }
            else if (evt.actionKey && evt.keyCode == KeyCode.Y)
            {
                _graphView.Redo();
                // _variablesPanel.Refresh(); // Member管理UIをAVSEditorから一時的に切り離し中(新システムへの一括差し替え待ち)
                evt.StopPropagation();
            }
        });
    }

    private void ConstructGraphView()
    {
        _graphView = new VisualScriptingGraphView();
        _graphView.History = _history;
        // ウィンドウ全体に広げる
        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);

        // Member管理UIをAVSEditorから一時的に切り離し中(新システムへの一括差し替え待ち)。
        // _variablesPanel = new VariablesPanel(_graphView);
        // _variablesPanel.style.position = Position.Absolute;
        // _variablesPanel.style.right = 0;
        // _variablesPanel.style.top = 20;
        // _variablesPanel.style.bottom = 0;
        // rootVisualElement.Add(_variablesPanel);
    }

    private void OnDisable()
    {
        if (_graphView != null) rootVisualElement.Remove(_graphView);
        if (_variablesPanel != null) rootVisualElement.Remove(_variablesPanel);
    }
}

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
    private AnimationParameterPanel _animationParameterPanel;
    private Button _targetTypeBtn;

    // EditorWindow自体がScriptableObjectなので、ここに持たせればドメインリロードを跨いで残る。
    [SerializeField] private EditHistoryData _history = new();

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
        Undo.undoRedoPerformed += OnUndoRedoPerformed;

        // Ctrl+S(MacはCmd+S)で手動Save
        rootVisualElement.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.actionKey && evt.keyCode == KeyCode.S)
            {
                GraphSerializer.Save(_graphView);
                evt.StopPropagation();
            }
        });
    }

    private void ConstructGraphView()
    {
        _graphView = new VisualScriptingGraphView
        {
            name = "Visual Scripting Graph"
        };
        _graphView.History = _history;
        // ウィンドウ全体に広げる
        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);

        _variablesPanel = new VariablesPanel(_graphView);
        _variablesPanel.style.position = Position.Absolute;
        _variablesPanel.style.right = 0;
        _variablesPanel.style.top = 20;
        _variablesPanel.style.bottom = 0;
        rootVisualElement.Add(_variablesPanel);

        _animationParameterPanel = new AnimationParameterPanel(OnParameterSelected);
        _animationParameterPanel.style.position = Position.Absolute;
        _animationParameterPanel.style.left = 0;
        _animationParameterPanel.style.top = 20;
        _animationParameterPanel.style.bottom = 0;
        rootVisualElement.Add(_animationParameterPanel);
    }

    private void AddToolbar()
    {
        var toolbar = new UnityEditor.UIElements.Toolbar();

        var saveBtn = new Button(() => GraphSerializer.Save(_graphView)) { text = "Save" };

        _targetTypeBtn = new Button { text = "Target: " + GetCurrentTargetTypeLabel() };
        _targetTypeBtn.clicked += () => OnTargetTypeClicked(_targetTypeBtn);

        toolbar.Add(saveBtn);
        toolbar.Add(_targetTypeBtn);
        rootVisualElement.Add(toolbar);
    }

    // AnimationParameterパネルでパラメーターボタンが押されたときの入口。旧Loadボタンの責務を引き継ぐ。
    private void OnParameterSelected(AnimatorParameterInfo info)
    {
        GraphSerializer.Load(_graphView, info);
        RefreshAfterLoad();
    }

    // プロジェクト全体のUndo/Redoで発火(このグラフと無関係でも発火する)。今開いているパラメーターを読み直して表示をデータに追従させる。
    private void OnUndoRedoPerformed()
    {
        if (string.IsNullOrEmpty(_graphView.CurrentParameterName)) return;

        VisualScriptingGraphData data = GraphSerializer.GetCurrent(_graphView);
        if (data == null) return;

        GraphSerializer.Load(_graphView, new AnimatorParameterInfo(_graphView.CurrentParameterName, data.ParameterType));
        RefreshAfterLoad();
    }

    private void RefreshAfterLoad()
    {
        _targetTypeBtn.text = "Target: " + GetCurrentTargetTypeLabel();
        _variablesPanel.Refresh();
    }

    private void OnTargetTypeClicked(Button anchor)
    {
        AdvancedDropdownState state = new AdvancedDropdownState();
        TargetTypeDropdown dropdown = new TargetTypeDropdown(state, type => SetTargetType(anchor, type));
        dropdown.Show(anchor.worldBound);
    }

    private void SetTargetType(Button anchor, Type type)
    {
        VisualScriptingGraphData data = GraphSerializer.GetCurrent(_graphView);
        if (data == null) return;

        data.TargetTypeName = type.AssemblyQualifiedName;
        GraphSerializer.PersistCurrent();

        anchor.text = "Target: " + type.Name;
    }

    private string GetCurrentTargetTypeLabel()
    {
        VisualScriptingGraphData data = GraphSerializer.GetCurrent(_graphView);
        if (data == null || string.IsNullOrEmpty(data.TargetTypeName)) return "(未選択)";

        Type type = Type.GetType(data.TargetTypeName);
        return type != null ? type.Name : "(未選択)";
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        if (_graphView != null) rootVisualElement.Remove(_graphView);
        if (_variablesPanel != null) rootVisualElement.Remove(_variablesPanel);
        if (_animationParameterPanel != null) rootVisualElement.Remove(_animationParameterPanel);
    }
}
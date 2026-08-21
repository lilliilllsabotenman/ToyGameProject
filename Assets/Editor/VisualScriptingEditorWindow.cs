        // Editor/VisualScriptingEditorWindow.cs
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class VisualScriptingEditorWindow : EditorWindow
{
    private VisualScriptingGraphView _graphView;
    private VariablesPanel _variablesPanel;
    private AnimationParameterPanel _animationParameterPanel;

    // EditorWindow自体がScriptableObjectなので、ここに持たせればドメインリロードを跨いで残る。
    [SerializeField] private EditHistoryData _history = new();

    public static VisualScriptingEditorWindow Open()
    {
        VisualScriptingEditorWindow window = GetWindow<VisualScriptingEditorWindow>("AVS Editor");
        window.minSize = new Vector2(800, 600);
        return window;
    }

    public void Initialize(Animator animator)
    {
        _animationParameterPanel.SetAnimator(animator);
    }

    private void OnEnable()
    {
        ConstructGraphView();

        rootVisualElement.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.actionKey && evt.keyCode == KeyCode.S)
            {
                GraphSerializer.Save(_graphView);
                evt.StopPropagation();
            }
            else if (evt.actionKey && evt.keyCode == KeyCode.Z)
            {
                GraphSerializer.Undo(_graphView);
                _variablesPanel.Refresh();
                evt.StopPropagation();
            }
            else if (evt.actionKey && evt.keyCode == KeyCode.Y)
            {
                GraphSerializer.Redo(_graphView);
                _variablesPanel.Refresh();
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

    // AnimationParameterパネルでパラメーターボタンが押されたときの入口。旧Loadボタンの責務を引き継ぐ。
    private void OnParameterSelected(AnimatorParameterInfo info)
    {
        GraphSerializer.Load(_graphView, info);
        _variablesPanel.Refresh();
    }

    private void OnDisable()
    {
        if (_graphView != null) rootVisualElement.Remove(_graphView);
        if (_variablesPanel != null) rootVisualElement.Remove(_variablesPanel);
        if (_animationParameterPanel != null) rootVisualElement.Remove(_animationParameterPanel);
    }
}
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ParameterPrisetEditor : EditorWindow
{
    private VisualScriptingGraphDataBase prisetData;
    private List<VisualScriptingGraphData> previewPrisets = new();

    private List<AnimatorParameterInfo> _parameters;
    private VisualElement _presetListContainer;
    private AnimationParameterPanel _animationParameterPanel;

    public static ParameterPrisetEditor Open()
    {
        ParameterPrisetEditor window = GetWindow<ParameterPrisetEditor>("プログラマーをちょっと楽にするエディター");
        window.minSize = new Vector2(300, 200);
        return window;
    }

    public void Initialized(List<AnimatorParameterInfo> Parameters)
    {
        _parameters = Parameters;
        RefreshPresetList();
    }

    private void CreateGUI()
    {
        Toolbar toolbar = new Toolbar();

        ToolbarButton savePresetButton = new ToolbarButton { text = "遷移条件を新規作成" };
        savePresetButton.clicked += () =>
        {
            VisualScriptingEditorWindow.Open();
        };

        toolbar.Add(savePresetButton);
        rootVisualElement.Add(toolbar);

        _animationParameterPanel = new AnimationParameterPanel(_ => { });
        _animationParameterPanel.style.position = Position.Absolute;
        _animationParameterPanel.style.left = 0;
        _animationParameterPanel.style.top = 20;
        _animationParameterPanel.style.bottom = 0;
        rootVisualElement.Add(_animationParameterPanel);

        _presetListContainer = new VisualElement();
        _presetListContainer.style.marginLeft = 220;
        rootVisualElement.Add(_presetListContainer);

        RefreshPresetList();
    }

    // _parametersを元にpreviewPrisetsを取得し直し、プリセット一覧のボタンを作り直す。
    private void RefreshPresetList()
    {
        if (_presetListContainer == null) return;

        if (_parameters != null)
        {
            previewPrisets = GetCompatibleParameter(_parameters);
            _animationParameterPanel?.SetParameters(_parameters);
        }

        _presetListContainer.Clear();

        foreach (VisualScriptingGraphData preset in previewPrisets)
        {
            if(preset is IPresetDisplayInfo p)
            {
                Color color = p.GetColor();
                Color borderColor = color * 0.7f;
                borderColor.a = 1f;

                // メインボタンと小ボタンを横に並べるための行コンテナ
                VisualElement row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.marginBottom = 6;

                Button button = new Button { text = p.GetDisplayName() };
                button.style.height = 40;
                button.style.flexGrow = 1;
                button.style.backgroundColor = color;

                // 文字を見やすく
                button.style.fontSize = 14;
                button.style.unityFontStyleAndWeight = FontStyle.Bold;
                button.style.color = Color.white;

                // 角を丸く
                button.style.borderTopLeftRadius = 6;
                button.style.borderTopRightRadius = 6;
                button.style.borderBottomLeftRadius = 6;
                button.style.borderBottomRightRadius = 6;

                // 下だけ太い枠線にして、押せそうな厚み(立体感)を出す
                button.style.borderTopWidth = 1;
                button.style.borderLeftWidth = 1;
                button.style.borderRightWidth = 1;
                button.style.borderBottomWidth = 3;
                button.style.borderTopColor = borderColor;
                button.style.borderLeftColor = borderColor;
                button.style.borderRightColor = borderColor;
                button.style.borderBottomColor = borderColor;

                // メインボタンの横に置く小ボタン(動作は未バインド)
                Button smallButton = new Button { text = "編集" };
                smallButton.style.width = 60;
                smallButton.style.height = 40;
                smallButton.style.marginLeft = 4;

                row.Add(button);
                row.Add(smallButton);
                _presetListContainer.Add(row);
            }
        }
    }

    private void OnFocus()
    {
        Debug.Log("IsForcus");
        RefreshPresetList();
    }

    private List<VisualScriptingGraphData> GetCompatibleParameter(List<AnimatorParameterInfo> Parameters)
    {
        List<VisualScriptingGraphData> previewPrisets = new();

        prisetData = Resources.Load<VisualScriptingGraphDataBase>("AnimationTransitionData/PresetData");
        if (prisetData == null) return previewPrisets;

        foreach (AnimatorParameterInfo info in Parameters)
        {
            foreach (VisualScriptingGraphData data in prisetData.data)
            {
                if (info.Type == data.ParameterType)
                {
                    previewPrisets.Add(data);
                }
            }
        }

        return previewPrisets;
    }

    private void OnDestroy()
    {
        previewPrisets.Clear();
        _parameters.Clear();
    }
}

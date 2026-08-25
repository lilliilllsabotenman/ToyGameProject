using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ParameterPresetEditor : EditorWindow
{
    private List<VisualScriptingGraphDataBase> presetBases = new();
    private Dictionary<VisualScriptingGraphData, VisualScriptingGraphDataBase> _presetOwners = new();
    private VisualScriptingGraphDataBase graphData;
    private List<VisualScriptingGraphData> previewPresets = new();

    private AnimatorParameterInfo _selectedParameter;
    private bool _hasSelectedParameter;

    private static readonly Color SelectedPresetBorderColor = new Color(1f, 0.9f, 0.2f);

    private List<AnimatorParameterInfo> _parameters;
    private VisualElement _presetListContainer;
    private AnimationParameterPanel _animationParameterPanel;

    public static ParameterPresetEditor Open()
    {
        ParameterPresetEditor window = GetWindow<ParameterPresetEditor>("プログラマーをちょっと楽にするエディター");
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
        _animationParameterPanel = new AnimationParameterPanel(info =>
        {
            _selectedParameter = info;
            _hasSelectedParameter = true;
            RefreshPresetList();
        });
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

    // _parametersを元にpreviewPresetsを取得し直し、プリセット一覧のボタンを作り直す。
    private void RefreshPresetList()
    {
        if (_presetListContainer == null) return;

        graphData = PresetSerializer.LoadOrCreateExecuteDataBase();

        if (_parameters != null)
        {
            previewPresets = GetCompatibleParameter();
            _animationParameterPanel?.SetParameters(_parameters);
        }

        SyncAppliedPresets();

        _presetListContainer.Clear();


        Button savePresetButton = new Button { text = "遷移条件を新規作成" };
        savePresetButton.clicked += () =>
        {
            PresetNameDialog.Show(displayInfo =>
            {
                if (!string.IsNullOrEmpty(displayInfo.DisplayName))
                {
                    VisualScriptingGraphDataBase dataBase = PresetSerializer.LoadOrCreateUserPresetBase();
                    VisualScriptingGraphData data = PresetSerializer.CreatePreset(_selectedParameter, displayInfo);

                    VisualScriptingEditorWindow window = VisualScriptingEditorWindow.Open();
                    window.Initialize(dataBase, data);
                }
            });
        };

        //ここで改行
        if (_hasSelectedParameter)
        {
            _presetListContainer.Add(savePresetButton);
        }

        foreach (VisualScriptingGraphData preset in previewPresets)
        {
            if(preset is IPresetDisplayInfo p)
            {
                PresetDisplayInfo displayInfo = p.GetDisplayInfo();
                Color color = displayInfo.Color;
                Color borderColor = color * 0.7f;
                borderColor.a = 1f;

                // メインボタンと小ボタンを横に並べるための行コンテナ
                VisualElement row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.marginBottom = 6;

                Button button = CreatePresetButton(preset, displayInfo, color, borderColor);

                // メインボタンの横に置く小ボタン(動作は未バインド)
                Button smallButton = new Button { text = "編集" };
                smallButton.style.width = 60;
                smallButton.style.height = 40;
                smallButton.style.marginLeft = 4;

                smallButton.clicked += () =>
                {
                    PresetEditorWindow window = PresetEditorWindow.Open();
                    _presetOwners.TryGetValue(preset, out VisualScriptingGraphDataBase owner);
                    window.Initialize(owner, preset);
                };

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

    private List<VisualScriptingGraphData> GetCompatibleParameter()
    {
        List<VisualScriptingGraphData> previewPresets = new();
        _presetOwners.Clear();

        presetBases = PresetSerializer.FindAllPresetBases();

        foreach (VisualScriptingGraphDataBase presetBase in presetBases)
        {
            foreach (VisualScriptingGraphData data in presetBase.data)
            {
                if (_selectedParameter.Type == data.ParameterType)
                {
                    previewPresets.Add(data);
                    _presetOwners[data] = presetBase;
                }
            }
        }

        return previewPresets;
    }

    // presetBases(UserPreset/DefaultPresetの全アセット)を横断して、PresetIdが一致するテンプレートを探す。
    private VisualScriptingGraphData FindTemplateByPresetId(string presetId)
    {
        foreach (VisualScriptingGraphDataBase presetBase in presetBases)
        {
            VisualScriptingGraphData found = presetBase.data.Find(d => d.PresetId == presetId);
            if (found != null) return found;
        }

        return null;
    }

    // graphData内の適用済みインスタンスを、対応するテンプレート(presetBases、PresetId一致)の最新内容で作り直す。
    // 適用時にバインドした実パラメータ名(name)だけは引き継ぐ。テンプレート・適用済みのどちらも未読み込みなら何もしない。
    private void SyncAppliedPresets()
    {
        if (graphData == null || presetBases == null || presetBases.Count == 0) return;

        List<VisualScriptingGraphData> appliedSnapshot = new List<VisualScriptingGraphData>(graphData.data);
        bool changed = false;

        foreach (VisualScriptingGraphData applied in appliedSnapshot)
        {
            if (string.IsNullOrEmpty(applied.PresetId)) continue;

            VisualScriptingGraphData template = FindTemplateByPresetId(applied.PresetId);
            if (template == null) continue;

            string boundName = PresetParameterBinder.GetBoundName(applied, template.ParameterType);

            VisualScriptingGraphData refreshed = template.Clone();
            PresetParameterBinder.BindParameterName(refreshed, template.ParameterType, boundName);

            graphData.RemoveData(applied.Name);
            graphData.SetData(refreshed);
            changed = true;
        }

        if (changed)
        {
            EditorUtility.SetDirty(graphData);
            AssetDatabase.SaveAssets();
        }
    }

    private void OnDestroy()
    {
        previewPresets.Clear();
        _parameters.Clear();
    }


    // プリセット一覧のメインボタンを1個作る。見た目(色分け・立体感・選択中ハイライト)と、
    // クリック時の適用処理(Clone→パラメータ名バインド→graphDataへ反映→保存)をまとめて持つ。
    private Button CreatePresetButton(VisualScriptingGraphData preset, PresetDisplayInfo displayInfo, Color color, Color borderColor)
    {
        Button button = new Button { text = displayInfo.DisplayName };
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

        // 選択中のプリセットだけ、目立つ枠線で上書きする(背景色は型ごとの色分けとして残す)。
        // graphDataに実際に適用されているかで判定するので、ウィンドウを開き直してもハイライトは復元される。
        // Nameは適用先パラメータ名なので同名プリセットが複数ありうる。PresetId(プリセット自身の識別子)で判定する。
        if (graphData.HasPresetId(preset.PresetId))
        {
            button.style.borderTopWidth = 4;
            button.style.borderLeftWidth = 4;
            button.style.borderRightWidth = 4;
            button.style.borderBottomWidth = 4;
            button.style.borderTopColor = SelectedPresetBorderColor;
            button.style.borderLeftColor = SelectedPresetBorderColor;
            button.style.borderRightColor = SelectedPresetBorderColor;
            button.style.borderBottomColor = SelectedPresetBorderColor;
        }

        button.clicked += () =>
        {
            if (graphData.HasDuplicate(preset.Name))
            {
                graphData.RemoveData(preset.Name);
            }

            VisualScriptingGraphData clone = preset.Clone();
            PresetParameterBinder.BindParameterName(clone, _selectedParameter.Type, _selectedParameter.Name);
            graphData.SetData(clone);

            EditorUtility.SetDirty(graphData);
            AssetDatabase.SaveAssets();

            RefreshPresetList();
        };

        return button;
    }
}

    // Editor/AVSEditor/PresetEditorWindow.cs
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// PPEの「編集」からAVSEditor(ノードグラフ)へ行く前に挟む中間ウィンドウ。
// デザイナーがノードを直接触らずに、プリセットのValue種別Membersだけを簡易編集できるようにする。
// ノード自体をいじりたい場合は「ノードエディターを開く」から従来通りAVSEditorへ進める。
public class PresetEditorWindow : EditorWindow
{
    private VisualScriptingGraphDataBase _dataBase;
    private VisualScriptingGraphData _data;
    private VisualElement _listContainer;

    public static PresetEditorWindow Open()
    {
        PresetEditorWindow window = GetWindow<PresetEditorWindow>("プリセット編集");
        window.minSize = new Vector2(320, 240);
        return window;
    }

    public void Initialize(VisualScriptingGraphDataBase dataBase, VisualScriptingGraphData data)
    {
        _dataBase = dataBase;
        _data = data;
        Refresh();
    }

    private void CreateGUI()
    {
        Refresh();
    }

    private void Refresh()
    {
        rootVisualElement.Clear();
        if (_data == null) return;

        PresetDisplayInfo displayInfo = ((IPresetDisplayInfo)_data).GetDisplayInfo();

        // まだDataへの書き込みには繋いでいない(onChangedは渡さない)。
        PresetDisplayInfoField displayInfoField = new PresetDisplayInfoField(
                                                            displayInfo,
                                                            UpdateGraphData,
                                                            44,
                                                            200);
        rootVisualElement.Add(displayInfoField);

        _listContainer = new VisualElement();
        rootVisualElement.Add(_listContainer);
        RefreshMemberList();

        Button openEditorButton = new Button(OnOpenEditorClicked) { text = "ノードエディターを開く" };
        openEditorButton.style.marginTop = 12;
        rootVisualElement.Add(openEditorButton);
    }

    private void RefreshMemberList()
    {
        _listContainer.Clear();

        foreach (MemberVariableData member in _data.Members)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 4;

            Label label = new Label(member.Name);
            label.style.flexGrow = 1;
            row.Add(label);

            if (member.Kind == MemberKind.Value)
            {
                string memberName = member.Name;
                System.Type type = string.IsNullOrEmpty(member.TypeName) ? null : System.Type.GetType(member.TypeName);

                if (type == typeof(bool))
                {
                    bool.TryParse(member.DefaultValue, out bool boolValue);
                    Toggle toggle = new Toggle { value = boolValue };
                    toggle.RegisterValueChangedCallback(evt => SetDefaultValue(memberName, evt.newValue.ToString()));
                    row.Add(toggle);
                }
                else
                {
                    TextField field = new TextField { value = member.DefaultValue ?? string.Empty, isDelayed = true };
                    field.RegisterValueChangedCallback(evt => SetDefaultValue(memberName, evt.newValue));
                    row.Add(field);
                }
            }
            else
            {
                // Component種別は参照解決が必要になるため、この簡易ウィンドウでは表示のみ(編集はノードエディター側で行う)。
                Label readonlyLabel = new Label("(Component)");
                row.Add(readonlyLabel);
            }

            _listContainer.Add(row);
        }
    }

    private void SetDefaultValue(string memberName, string value)
    {
        MemberVariableData member = _data.Members.Find(m => m.Name == memberName);
        if (member == null) return;

        member.DefaultValue = value;

        if (_dataBase != null)
        {
            EditorUtility.SetDirty(_dataBase);
            AssetDatabase.SaveAssets();
        }
    }

    private void OnOpenEditorClicked()
    {
        VisualScriptingEditorWindow window = VisualScriptingEditorWindow.Open();
        window.Initialize(_dataBase, _data);
    }

    private void UpdateGraphData(PresetDisplayInfo data)
    {
        ((IPresetDisplayInfoWriter)_data).SetDisplayInfo(data);

        if (_dataBase != null)
        {
            EditorUtility.SetDirty(_dataBase);
            AssetDatabase.SaveAssets();
        }
    }
}

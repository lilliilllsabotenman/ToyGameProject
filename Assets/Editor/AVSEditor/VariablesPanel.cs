// Editor/VariablesPanel.cs
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

// グラフのメンバ変数(Component参照)の一覧・追加・削除を行うパネル。
// 操作のたびにGraphDataアセットへ即時保存する(ノード/エッジのSaveボタンとは非同期)。
public class VariablesPanel : VisualElement
{
    private static readonly (string Label, Type Type)[] ValueTypes =
    {
        ("Int", typeof(int)),
        ("Float", typeof(float)),
        ("String", typeof(string)),
        ("Bool", typeof(bool)),
    };

    private VisualElement _listContainer;
    private VisualScriptingGraphView _graphView;

    public VariablesPanel(VisualScriptingGraphView graphView)
    {
        _graphView = graphView;

        style.width = 220;
        style.paddingLeft = 4;
        style.paddingRight = 4;
        style.paddingTop = 4;
        style.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        Label title = new Label("Variables");
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        Add(title);

        Button addComponentButton = new Button { text = "+ Component" };
        addComponentButton.clicked += () => OnAddComponentClicked(addComponentButton);
        Add(addComponentButton);

        Button addValueButton = new Button { text = "+ Value" };
        addValueButton.clicked += OnAddValueClicked;
        Add(addValueButton);

        _listContainer = new VisualElement();
        Add(_listContainer);

        Refresh();
    }

    private void OnAddComponentClicked(VisualElement anchor)
    {
        AdvancedDropdownState state = new AdvancedDropdownState();
        ComponentTypeDropdown dropdown = new ComponentTypeDropdown(state, type => AddMember(type, MemberKind.Component));
        dropdown.Show(anchor.worldBound);
    }

    private void OnAddValueClicked()
    {
        GenericMenu menu = new GenericMenu();
        foreach ((string label, Type type) in ValueTypes)
        {
            menu.AddItem(new GUIContent(label), false, () => AddMember(type, MemberKind.Value));
        }
        menu.ShowAsContext();
    }

    // CurrentDataBaseをSetDirty+SaveAssetsするだけの小さいヘルパー。
    private void Persist()
    {
        if (_graphView.CurrentDataBase == null) return;
        EditorUtility.SetDirty(_graphView.CurrentDataBase);
        AssetDatabase.SaveAssets();
    }

    private void AddMember(Type type, MemberKind kind)
    {
        VisualScriptingGraphData data = _graphView.CurrentData;
        if (data == null) return;

        string baseName = type.Name;
        string name = baseName;
        int suffix = 1;
        while (data.Members.Any(m => m.Name == name))
        {
            suffix++;
            name = baseName + suffix;
        }

        data.Members.Add(new MemberVariableData { Name = name, Kind = kind, TypeName = type.AssemblyQualifiedName });
        Persist();
        Refresh();
    }

    private void RemoveMember(string memberName)
    {
        VisualScriptingGraphData data = _graphView.CurrentData;
        if (data == null) return;

        data.Members.RemoveAll(m => m.Name == memberName);
        Persist();
        Refresh();
    }

    private void SetDefaultValue(string memberName, string value)
    {
        VisualScriptingGraphData data = _graphView.CurrentData;
        if (data == null) return;

        MemberVariableData member = data.Members.Find(m => m.Name == memberName);
        if (member == null) return;

        member.DefaultValue = value;
        Persist();
    }

    public void Refresh()
    {
        _listContainer.Clear();
        VisualScriptingGraphData data = _graphView.CurrentData;
        if (data == null) return;

        foreach (MemberVariableData member in data.Members)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;

            Type type = string.IsNullOrEmpty(member.TypeName) ? null : Type.GetType(member.TypeName);
            Label label = new Label($"{member.Name} : {(type != null ? type.Name : "?")}");
            label.style.flexGrow = 1;
            row.Add(label);

            if (member.Kind == MemberKind.Value)
            {
                string memberName = member.Name;

                if (type == typeof(bool))
                {
                    bool.TryParse(member.DefaultValue, out bool boolValue);
                    Toggle valueToggle = new Toggle { value = boolValue };
                    valueToggle.RegisterValueChangedCallback(evt => SetDefaultValue(memberName, evt.newValue.ToString()));
                    row.Add(valueToggle);
                }
                else
                {
                    TextField valueField = new TextField { value = member.DefaultValue ?? string.Empty };
                    valueField.RegisterValueChangedCallback(evt => SetDefaultValue(memberName, evt.newValue));
                    row.Add(valueField);
                }
            }

            string removeName = member.Name;
            Button removeButton = new Button(() => RemoveMember(removeName)) { text = "x" };
            row.Add(removeButton);

            _listContainer.Add(row);
        }
    }
}

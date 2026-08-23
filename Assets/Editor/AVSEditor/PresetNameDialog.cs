// Editor/PresetNameDialog.cs
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// プリセット名を1つだけ入力させるための簡易モーダルウィンドウ。
// Show()はウィンドウが閉じるまでブロックし、OKで入力した名前を、キャンセル(閉じるだけ)ならnullを返す。
public class PresetNameDialog : EditorWindow
{
    private string _presetName = "";
    private bool _confirmed;

    public static string Show()
    {
        PresetNameDialog window = CreateInstance<PresetNameDialog>();
        window.titleContent = new GUIContent("プリセット名");
        window.minSize = window.maxSize = new Vector2(300, 80);
        window.ShowModal();
        return window._confirmed ? window._presetName : null;
    }

    private void CreateGUI()
    {
        TextField nameField = new TextField("名前") { value = _presetName };
        nameField.RegisterValueChangedCallback(evt => _presetName = evt.newValue);
        rootVisualElement.Add(nameField);

        Button okButton = new Button(() =>
        {
            _confirmed = true;
            Close();
        })
        { text = "OK" };
        rootVisualElement.Add(okButton);
    }
}

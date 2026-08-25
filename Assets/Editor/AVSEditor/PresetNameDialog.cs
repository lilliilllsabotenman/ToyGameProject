// Editor/PresetNameDialog.cs
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// プリセット名と表示色を入力させるための簡易フローティングウィンドウ。
// ShowModal()だとOS側のモーダル開閉タイミングでメインウィンドウがちらつくため、非モーダル(ShowUtility)+コールバックにしている。
// OKで入力した名前・選択した色を渡してonConfirmedを呼ぶ。キャンセル(閉じるだけ)ならonConfirmedは呼ばれない。
public class PresetNameDialog : EditorWindow
{
    private PresetDisplayInfoField _field;
    private Action<PresetDisplayInfo> _onConfirmed;

    public static void Show(Action<PresetDisplayInfo> onConfirmed)
    {
        PresetNameDialog window = CreateInstance<PresetNameDialog>();
        window._onConfirmed = onConfirmed;
        window.titleContent = new GUIContent("プリセット名");
        window.minSize = window.maxSize = new Vector2(300, 140);
        window.ShowUtility();
    }

    private void CreateGUI()
    {
        _field = new PresetDisplayInfoField(new PresetDisplayInfo("", Color.white));
        rootVisualElement.Add(_field);

        Button okButton = new Button(() =>
        {
            Action<PresetDisplayInfo> onConfirmed = _onConfirmed;
            PresetDisplayInfo displayInfo = _field.Value;
            _onConfirmed = null;
            Close();
            onConfirmed?.Invoke(displayInfo);
        })
        { text = "OK" };
        rootVisualElement.Add(okButton);
    }
}

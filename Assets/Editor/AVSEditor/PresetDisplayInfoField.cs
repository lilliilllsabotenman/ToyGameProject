// Editor/AVSEditor/PresetDisplayInfoField.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// プリセットの名前+色をまとめて編集するための共通UI部品。
// PresetNameDialog(新規作成)とPresetEditorWindow(既存編集)の両方から、見た目・処理ともにこれ1つを使う。
public class PresetDisplayInfoField : VisualElement
{
    // 仮置きの色セット(目に痛くない彩度に抑えたもの)。実際の色味は調整予定。
    private static readonly Color[] _palette =
    {
        new Color(0.85f, 0.55f, 0.55f),
        new Color(0.85f, 0.70f, 0.55f),
        new Color(0.85f, 0.85f, 0.55f),
        new Color(0.65f, 0.80f, 0.55f),
        new Color(0.55f, 0.80f, 0.65f),
        new Color(0.55f, 0.80f, 0.80f),
        new Color(0.55f, 0.65f, 0.85f),
        new Color(0.65f, 0.55f, 0.85f),
        new Color(0.80f, 0.55f, 0.80f),
        new Color(0.75f, 0.75f, 0.75f),
    };

    private static readonly Color SelectedSwatchBorderColor = new Color(1f, 0.9f, 0.2f);

    private readonly List<Button> _swatches = new List<Button>();
    private readonly Action<PresetDisplayInfo> _onChanged;

    private string _name;
    private Color _color;

    // 呼び出し側が「今の値」を随時読み出すための入口(OKボタン押下時などに使う)。
    public PresetDisplayInfo Value => new PresetDisplayInfo(_name, _color);

    // initial: 表示する初期値。onChanged: 値が変わるたび(名前入力/色選択)に呼ばれる。
    // 即時反映が要らない使い方(PresetNameDialogのようにOKボタンでまとめて取得する形)ならonChangedはnullでよい。
    // swatchSize: 色スウォッチ1個の一辺のサイズ(px)。nameFieldWidth: 名前欄の幅(px)、0以下なら親に合わせて伸びるデフォルト挙動。
    public PresetDisplayInfoField(
        PresetDisplayInfo initial,
        Action<PresetDisplayInfo> onChanged = null,
        float swatchSize = 22f,
        float nameFieldWidth = 0f)
    {
        _name = initial.DisplayName;
        _color = initial.Color;
        _onChanged = onChanged;

        TextField nameField = new TextField("名前") { value = _name };
        // TextFieldはラベル+入力欄の複合コントロールなので、外側ではなく入力欄自体に幅を指定する。
        // 入力欄はデフォルトでflexGrowが効いており、widthを指定してもflexの伸縮計算で上書きされるため、先に0にしておく。
        if (nameFieldWidth > 0f)
        {
            VisualElement input = nameField.Q(className: TextField.inputUssClassName);
            input.style.flexGrow = 0;
            input.style.width = nameFieldWidth;
        }
        nameField.RegisterValueChangedCallback(evt =>
        {
            _name = evt.newValue;
            _onChanged?.Invoke(Value);
        });
        Add(nameField);

        VisualElement colorContainer = new VisualElement();
        colorContainer.style.flexDirection = FlexDirection.Row;
        colorContainer.style.flexWrap = Wrap.Wrap;
        colorContainer.style.marginTop = 6;
        colorContainer.style.marginBottom = 8;
        Add(colorContainer);

        foreach (Color color in _palette)
        {
            Button swatch = new Button(() =>
            {
                _color = color;
                RefreshSwatches();
                _onChanged?.Invoke(Value);
            });
            swatch.userData = color;
            swatch.style.width = swatchSize;
            swatch.style.height = swatchSize;
            swatch.style.marginRight = 2;
            swatch.style.marginBottom = 2;
            swatch.style.backgroundColor = color;
            _swatches.Add(swatch);
            colorContainer.Add(swatch);
        }

        RefreshSwatches();
    }

    private void RefreshSwatches()
    {
        foreach (Button swatch in _swatches)
        {
            bool isSelected = (Color)swatch.userData == _color;
            swatch.style.borderTopWidth = isSelected ? 3 : 1;
            swatch.style.borderLeftWidth = isSelected ? 3 : 1;
            swatch.style.borderRightWidth = isSelected ? 3 : 1;
            swatch.style.borderBottomWidth = isSelected ? 3 : 1;
            Color borderColor = isSelected ? SelectedSwatchBorderColor : Color.black;
            swatch.style.borderTopColor = borderColor;
            swatch.style.borderLeftColor = borderColor;
            swatch.style.borderRightColor = borderColor;
            swatch.style.borderBottomColor = borderColor;
        }
    }
}

// Editor/AnimationParameterPanel.cs
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UIElements;

// Animatorのパラメーター1件(名前+型)をまとめて運ぶための入れ物。
public readonly struct AnimatorParameterInfo
{
    public readonly string Name;
    public readonly AnimatorControllerParameterType Type;

    public AnimatorParameterInfo(string name, AnimatorControllerParameterType type)
    {
        Name = name;
        Type = type;
    }
}

// 指定したAnimatorが持つAnimatorControllerのパラメーター一覧をボタンとして表示するパネル。
// ボタンを押すと、そのパラメーター情報でonParameterSelectedを呼ぶ(実際に何をするかは呼び出し側が決める)。
public class AnimationParameterPanel : VisualElement
{
    private static readonly Color SelectedColor = new Color(0.6f, 0.85f, 1f);

    private readonly Action<AnimatorParameterInfo> _onParameterSelected;
    private Animator _animator;
    private List<AnimatorParameterInfo> _parameters;
    private VisualElement _listContainer;
    private AnimatorParameterInfo? _selected;

    public AnimationParameterPanel(Action<AnimatorParameterInfo> onParameterSelected)
    {
        _onParameterSelected = onParameterSelected;

        style.width = 220;
        style.paddingLeft = 4;
        style.paddingRight = 4;
        style.paddingTop = 4;
        style.backgroundColor = new Color(0, 0, 0, 0.5f);

        Label title = new Label("Animation Parameters");
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        Add(title);

        _listContainer = new VisualElement();
        Add(_listContainer);

        Refresh();
    }

    // Editor側(VisualScriptingEditorWindow.Initialize)から対象のAnimatorを渡すための入口。
    public void SetAnimator(Animator animator)
    {
        _animator = animator;
        _parameters = null;
        _selected = null;
        Refresh();
    }

    // Animatorコンポーネントを介さず、既に解決済みのパラメータ一覧を直接渡すための入口(ParameterPresetEditorなど)。
    // 同じ一覧が再度渡されただけ(フォーカス復帰など)なら選択状態は保持し、別の一覧に切り替わったときだけ選択を解除する。
    public void SetParameters(List<AnimatorParameterInfo> parameters)
    {
        if (_parameters != parameters) _selected = null;

        _parameters = parameters;
        _animator = null;
        Refresh();
    }

    private void Refresh()
    {
        _listContainer.Clear();

        foreach (AnimatorParameterInfo parameter in ResolveParameters())
        {
            Button button = new Button(() =>
            {
                _selected = parameter;
                _onParameterSelected?.Invoke(parameter);
                Refresh();
            })
            { text = $"{parameter.Name} : {parameter.Type}" };

            if (_selected != null && _selected.Value.Equals(parameter))
            {
                button.style.backgroundColor = SelectedColor;
            }

            _listContainer.Add(button);
        }
    }

    private List<AnimatorParameterInfo> ResolveParameters()
    {
        if (_parameters != null) return _parameters;

        List<AnimatorParameterInfo> result = new();

        AnimatorController controller = _animator != null ? _animator.runtimeAnimatorController as AnimatorController : null;
        if (controller == null) return result;

        foreach (AnimatorControllerParameter parameter in controller.parameters)
        {
            result.Add(new AnimatorParameterInfo(parameter.name, parameter.type));
        }

        return result;
    }
}

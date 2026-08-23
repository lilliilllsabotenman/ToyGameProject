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
    private readonly Action<AnimatorParameterInfo> _onParameterSelected;
    private Animator _animator;
    private List<AnimatorParameterInfo> _parameters;
    private VisualElement _listContainer;

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
        Refresh();
    }

    // Animatorコンポーネントを介さず、既に解決済みのパラメータ一覧を直接渡すための入口(ParameterPrisetEditorなど)。
    public void SetParameters(List<AnimatorParameterInfo> parameters)
    {
        _parameters = parameters;
        _animator = null;
        Refresh();
    }

    private void Refresh()
    {
        _listContainer.Clear();

        foreach (AnimatorParameterInfo parameter in ResolveParameters())
        {
            Button button = new Button(() => _onParameterSelected?.Invoke(parameter)) { text = $"{parameter.Name} : {parameter.Type}" };
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

// Editor/AnimationParameterPanel.cs
using System;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.UIElements;
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

        ObjectField animatorField = new ObjectField("Animator")
        {
            objectType = typeof(Animator),
            allowSceneObjects = true
        };
        animatorField.RegisterValueChangedCallback(evt =>
        {
            _animator = evt.newValue as Animator;
            Refresh();
        });
        Add(animatorField);

        _listContainer = new VisualElement();
        Add(_listContainer);

        Refresh();
    }

    private void Refresh()
    {
        _listContainer.Clear();

        AnimatorController controller = _animator != null ? _animator.runtimeAnimatorController as AnimatorController : null;
        if (controller == null) return;

        foreach (AnimatorControllerParameter parameter in controller.parameters)
        {
            Button button = new Button(() => _onParameterSelected?.Invoke(new AnimatorParameterInfo(parameter.name, parameter.type))) { text = $"{parameter.name} : {parameter.type}" };
            _listContainer.Add(button);
        }
    }
}

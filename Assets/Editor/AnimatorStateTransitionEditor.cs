// Editor/AnimatorStateTransitionEditor.cs
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.Collections.Generic;

// Unity組み込みのTransition Inspector(Exit Time/Conditionsなど)は書き換えず、
// ヘッダー領域にだけプリセット関連UIを追加する。CustomEditorで丸ごと差し替えると
// 組み込みInspectorの作り込まれた描画(条件ドロップダウン等)が汎用描画で壊れるため、この方式にしている。
[InitializeOnLoad]
public static class AnimatorStateTransitionEditor
{
    static AnimatorStateTransitionEditor()
    {
        Editor.finishedDefaultHeaderGUI += OnHeaderGUI;
    }

    private static readonly HashSet<string> _loggedParameters = new HashSet<string>();
    private static readonly HashSet<AnimatorParameterInfo> parameterInfos = new();
    private static Object _lastTarget;

    private static void OnHeaderGUI(Editor editor)
    {
        if (editor.target is not AnimatorState state) return;

        // 選択対象が変わったら、前のステートの分を持ち越さないようリセットする。
        if (editor.target != _lastTarget)
        {
            _lastTarget = editor.target;
            _loggedParameters.Clear();
            parameterInfos.Clear();
        }

        // ステートが持つ全遷移(出ていく分+入ってくる分)を横断して、条件に使われてるパラメータを集める。
        List<AnimatorStateTransition> transitions = new List<AnimatorStateTransition>(state.transitions);
        transitions.AddRange(AnimatorParameterResolver.FindIncomingTransitions(state));

        foreach (AnimatorStateTransition transition in transitions)
        {
            foreach (AnimatorCondition condition in transition.conditions)
            {
                if (_loggedParameters.Add(condition.parameter))
                {
                    AnimatorControllerParameterType? type = AnimatorParameterResolver.FindParameterType(transition, condition.parameter);
                    if (type != null)
                    {
                        parameterInfos.Add(new AnimatorParameterInfo(condition.parameter, type.Value));
                    }
                }
            }
        }

        if(GUILayout.Button("プログラマーに楽をさせる"))
        {
            ParameterPresetEditor window = ParameterPresetEditor.Open();
            window.Initialized(new List<AnimatorParameterInfo>(parameterInfos));
        }
    }
}

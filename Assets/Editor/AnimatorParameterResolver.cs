// Editor/AnimatorParameterResolver.cs
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.Collections.Generic;

// AnimatorStateTransition側からAnimatorのパラメータ情報を逆引きするための静的API。
// AnimatorStateTransitionから所属コントローラーへ直接辿るAPIが無いため、プロジェクト内の全AnimatorControllerを走査する。
public static class AnimatorParameterResolver
{
    // transitionが属するAnimatorControllerを力技で探し、parameterNameと一致するパラメータの型を返す。見つからなければnull。
    public static AnimatorControllerParameterType? FindParameterType(AnimatorStateTransition transition, string parameterName)
    {
        AnimatorController controller = FindOwningController(transition);
        if (controller == null) return null;

        return FindParameter(controller, parameterName)?.type;
    }

    // controller内でparameterNameと一致するパラメータを探す。見つからなければnull。
    public static AnimatorControllerParameter? FindParameter(AnimatorController controller, string parameterName)
    {
        foreach (AnimatorControllerParameter parameter in controller.parameters)
        {
            if (parameter.name == parameterName) return parameter;
        }

        return null;
    }

    private static AnimatorController FindOwningController(AnimatorStateTransition transition)
    {
        foreach (string guid in AssetDatabase.FindAssets("t:AnimatorController"))
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(guid));
            if (controller == null) continue;

            foreach (AnimatorControllerLayer layer in controller.layers)
            {
                if (ContainsTransition(layer.stateMachine, transition)) return controller;
            }
        }

        return null;
    }

    // ステートマシンを再帰的に辿り、AnyStateからの遷移・各ステートの遷移・子ステートマシンの中まで探す。
    private static bool ContainsTransition(AnimatorStateMachine stateMachine, AnimatorStateTransition transition)
    {
        foreach (AnimatorStateTransition candidate in stateMachine.anyStateTransitions)
        {
            if (candidate == transition) return true;
        }

        foreach (ChildAnimatorState child in stateMachine.states)
        {
            foreach (AnimatorStateTransition candidate in child.state.transitions)
            {
                if (candidate == transition) return true;
            }
        }

        foreach (ChildAnimatorStateMachine child in stateMachine.stateMachines)
        {
            if (ContainsTransition(child.stateMachine, transition)) return true;
        }

        return false;
    }

    // stateへ向かう遷移(他ステート/AnyStateからの入遷移)を力技で全部集める。
    // AnimatorStateから所属コントローラー/ステートマシンへ直接辿るAPIが無いため、プロジェクト内の全AnimatorControllerを走査する。
    public static List<AnimatorStateTransition> FindIncomingTransitions(AnimatorState state)
    {
        List<AnimatorStateTransition> result = new List<AnimatorStateTransition>();

        foreach (string guid in AssetDatabase.FindAssets("t:AnimatorController"))
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(guid));
            if (controller == null) continue;

            foreach (AnimatorControllerLayer layer in controller.layers)
            {
                CollectTransitionsTo(layer.stateMachine, state, result);
            }
        }

        return result;
    }

    // ステートマシンを再帰的に辿り、destinationStateがstateと一致する遷移(AnyStateからのもの含む)を全部集める。
    private static void CollectTransitionsTo(AnimatorStateMachine stateMachine, AnimatorState state, List<AnimatorStateTransition> result)
    {
        foreach (AnimatorStateTransition candidate in stateMachine.anyStateTransitions)
        {
            if (candidate.destinationState == state) result.Add(candidate);
        }

        foreach (ChildAnimatorState child in stateMachine.states)
        {
            foreach (AnimatorStateTransition candidate in child.state.transitions)
            {
                if (candidate.destinationState == state) result.Add(candidate);
            }
        }

        foreach (ChildAnimatorStateMachine child in stateMachine.stateMachines)
        {
            CollectTransitionsTo(child.stateMachine, state, result);
        }
    }
}

// Editor/PresetParameterBinder.cs
using UnityEngine;
using System.Collections.Generic;

// プリセット内の「SetOwnerX」系ノード(DefaultNodeのSetOwnerFloat/Integer/Bool/Trigger)を探し、
// そのnameパラメータを実際に適用する対象のAnimatorパラメータ名へ書き換える。
public static class PresetParameterBinder
{
    // パラメータ型ごとに、対応するSetOwnerXアクションのMethodKey。
    private static readonly Dictionary<AnimatorControllerParameterType, string> _methodKeys = new()
    {
        [AnimatorControllerParameterType.Float] = "SetOwnerFloat",
        [AnimatorControllerParameterType.Int] = "SetOwnerInteger",
        [AnimatorControllerParameterType.Bool] = "SetOwnerBool",
        [AnimatorControllerParameterType.Trigger] = "SetOwnerTrigger"
    };

    // parameterTypeに対応するSetOwnerXアクションのMethodKeyを引く。対応が無ければnull。
    public static string GetMethodKey(AnimatorControllerParameterType parameterType)
    {
        return _methodKeys.TryGetValue(parameterType, out string methodKey) ? methodKey : null;
    }

    // dataの中から、parameterTypeに対応するSetOwnerXノードだけを探し、nameパラメータをparameterNameへ書き換える。
    public static void BindParameterName(VisualScriptingGraphData data, AnimatorControllerParameterType parameterType, string parameterName)
    {
        if (!_methodKeys.TryGetValue(parameterType, out string methodKey)) return;

        foreach (BaseNodeData nodeData in data.Nodes)
        {
            if (nodeData is ActionNodeData actionData && actionData.MethodKey == methodKey)
            {
                actionData.SetParam("name", parameterName);
            }
        }
    }

    // dataの中から、parameterTypeに対応するSetOwnerXノードのnameパラメータを読み取る。見つからなければnull。
    public static string GetBoundName(VisualScriptingGraphData data, AnimatorControllerParameterType parameterType)
    {
        if (!_methodKeys.TryGetValue(parameterType, out string methodKey)) return null;

        foreach (BaseNodeData nodeData in data.Nodes)
        {
            if (nodeData is ActionNodeData actionData && actionData.MethodKey == methodKey)
            {
                return actionData.GetParam("name");
            }
        }

        return null;
    }
}

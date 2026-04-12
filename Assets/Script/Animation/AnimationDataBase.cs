using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum ParametorType
{
    Bool,
    Float,
    Int,
    Trigger
}

/// <summary>
/// 条件付きフィールド
/// use = true の場合のみこの条件が有効になる
/// </summary>
[System.Serializable]
public class ConditionField<T> where T : Enum
{
    public bool use;
    public T value;
}

/// <summary>
/// アニメーション1件分の定義
/// 「この状態のとき、このパラメータをONにする」
/// </summary>                          
[System.Serializable]
public class AnimationData
{
    public ConditionField<MovementState> moveState;
    public ConditionField<PositioningState> positState;
    public ConditionField<PostureState> postState;

    [Header("アニメーションパラメーター名")]
    public string AnimationParameter;

    [Header("パラメーターの型")]
    public ParametorType parametorType;
}

/// <summary>
/// ScriptableObjectとしてデータを持つ
/// AnimatorとAnimationData群をまとめる
/// </summary>
[CreateAssetMenu(menuName = "AnimationDataBase")]
public class AnimationDataBase : ScriptableObject
{
    public AnimationData[] animData;
}

/// <summary>
/// Dictionaryキー
/// 「どのState型の」「どの値か」を一意に識別する
/// </summary>
public struct StateKey : IEquatable<StateKey>
{
    public Type type;
    public int value;

    public StateKey(Type type, int value)
    {
        this.type = type;
        this.value = value;
    }

    public bool Equals(StateKey other)
        => type == other.type && value == other.value;

    public override int GetHashCode()
        => HashCode.Combine(type, value);
}
using System.Collections.Generic;
using UnityEngine;

public enum ParametorID
{
    //拡張前提、増える度に増やす
    Speed,
    ColSize
}

[System.Serializable]
public class AnimationSettings<T>
{
    [Header("パラメーターID")]
    public T ID;
    [Header("アニメーション優先度")]
    public AnimationData data;
}

[CreateAssetMenu(menuName = "Animation/AnimationMappng")]
public class AnimationMapings : ScriptableObject
{
    [Header("Float用")]
    public List<AnimationDataFloat> FloatParametorList;
    [Header("MovementState用")]
    public List<AnimationSettings<MovementState>> MovementState;
    [Header("PostureState用")]
    public List<AnimationSettings<PostureState>> PostureState;
    [Header("PositioningState用")]
    public List<AnimationSettings<PositioningState>> PositioningState;
}
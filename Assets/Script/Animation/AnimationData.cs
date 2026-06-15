using UnityEngine;

[CreateAssetMenu(menuName = "Animation/AnimationData")]
public class AnimationData : ScriptableObject
{
    public int priolity;
    public AnimationClip animClip;
}

[CreateAssetMenu(menuName = "Animation/AnimationDataFloat")]
public class AnimationDataFloat : ScriptableObject
{
    public ParametorID ID;
    public int priolity;
    public string AnimParamName;
}

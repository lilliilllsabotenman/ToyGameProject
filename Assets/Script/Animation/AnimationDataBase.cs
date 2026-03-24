using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class ConditionField<T> where T : Enum
{
    public bool use;   // ← この条件を使うか
    public T value;    // ← 値
}

[System.Serializable]
public class AnimationData
{
    public ConditionField<MovementState> moveState;
    public ConditionField<PositioningState> positState;
    public ConditionField<PostureState> postState;

    [Header("アニメーションパラメーター名")]
    public string AnimationParameter;
}

[CreateAssetMenu(menuName = "AnimationDataBase")]
public class AnimationDataBase : ScriptableObject
{
    public Animator animator;
    public AnimationData[] animData;
}

public class AnimatonSorting
{
    private AnimationDataBase animDataBase;

    public AnimatonSorting(AnimationDataBase animDataBase)
    {
        this.animDataBase = animDataBase;
    }

    public void GetAnimationParameter(PlayerStateData state)
    {
        if(animDataBase.animData == null) return;

        foreach (AnimationData data in animDataBase.animData)
        {
            if (data.moveState.use && data.moveState.value != state.movementState) 
            {
                StopAnimation(data.AnimationParameter);
                continue;
            }
            if (data.positState.use && data.positState.value != state.positioningState)
            {
                StopAnimation(data.AnimationParameter);
                continue;
            }
            if (data.postState.use && data.postState.value != state.postureState)
            {
                StopAnimation(data.AnimationParameter);
                continue;
            }
            ViewAnimation(data.AnimationParameter);
        }
        return;
    }

    private void ViewAnimation(string param)
    {
        if(animDataBase.animator == null) return;

        animDataBase.animator.SetBool(param, true);
    }

    private void StopAnimation(string param)
    {
        if(animDataBase.animator == null) return;

        animDataBase.animator.SetBool(param, false);
    }
}
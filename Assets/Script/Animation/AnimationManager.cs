using UnityEngine;
using System.Collections.Generic;
using System;

public class AnimationData
{
    public string Parametor;
    public bool useFloat;
    public AnimationParametor fParam;
}

public class AnimationDataBase
{
    private Dictionary<MovementState, AnimationData> moveData = new Dictionary<MovementState, AnimationData>();
    private Dictionary<PositioningState, AnimationData> positData = new Dictionary<PositioningState, AnimationData>();
    private Dictionary<PostureState, AnimationData> postData = new Dictionary<PostureState, AnimationData>();

    public AnimationDataBase()
    {
    }

    public string GetBollParametor<T>(T state) where T : Enum
    {
        if (state is MovementState m && moveData.TryGetValue(m, out var mData))
            return mData.Parametor;

        if (state is PositioningState p && positData.TryGetValue(p, out var pData))
            return pData.Parametor;

        if (state is PostureState po && postData.TryGetValue(po, out var poData))
            return poData.Parametor;

        return null;
    }

    public bool GetFloatRefarence<T>(T state, out AnimationParametor value) where T : Enum
    {
        value = null;

        if (state is MovementState m && moveData.TryGetValue(m, out var mData) && mData.useFloat)
        {
            value = mData.fParam;
            return true;
        }

        if (state is PositioningState p && positData.TryGetValue(p, out var pData) && pData.useFloat)
        {
            value = pData.fParam;
            return true;
        }

        if (state is PostureState po && postData.TryGetValue(po, out var poData) && poData.useFloat)
        {
            value = poData.fParam;
            return true;
        }

        return false;
    }
}

public class AnimationManager
{
    private List<Enum> RemoveAnimationBuffer = new List<Enum>();
    private List<Enum> RunningAnimation = new List<Enum>();
    
    public AnimationManager()
    {}

    public void InvokeAnimationMove(MovementState state)
    {
        AnimationDuplicationChecker(state);
        RunningAnimation.Add(state);
    }

    public void InvokeAnimationPosit(PositioningState state)
    {
        AnimationDuplicationChecker(state);
        RunningAnimation.Add(state);
    }

    public void InvokeAnimationPost(PostureState state)
    {
        AnimationDuplicationChecker(state);
        RunningAnimation.Add(state);
    }

    public void AnimationDuplicationChecker(Enum state)
    {
        Type type = state.GetType();

        for (int i = RunningAnimation.Count - 1; i >= 0; i--)
        {
            if (RunningAnimation[i].GetType() == type)
            {
                RemoveAnimationBuffer.Add(RunningAnimation[i]);
            }
        }

        foreach (var r in RemoveAnimationBuffer)
        {
            RunningAnimation.Remove(r);
        }

        RemoveAnimationBuffer.Clear();
    }
}

public class AniationExecutor
{
    private List<AnimationParametor> RemoveAnimationBuffer = new List<AnimationParametor>();
    private List<AnimationParametor> RunningAnimation = new List<AnimationParametor>();

    public void Execute(string param)
    {
        if (string.IsNullOrEmpty(param)) return;

        var animator = UnityEngine.Object.FindFirstObjectByType<Animator>();
        if (animator == null) return;

        animator.SetBool(param, true);
    }

    public void OnUpDateExeCute(string param, AnimationParametor fParam)
    {
        if (string.IsNullOrEmpty(param) || fParam == null) return;

        var animator = UnityEngine.Object.FindFirstObjectByType<Animator>();
        if (animator == null) return;

        animator.SetFloat(param, fParam.GetFloatParametor());
    }

    public void onUpdate()
    {
    }
}
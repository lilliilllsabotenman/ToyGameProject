using UnityEngine;
using System.Collections.Generic;
using System;

public class AnimationDataBase
{
    private Dictionary<MovementState, string> moveData = new();
    private Dictionary<PositioningState, string> positData = new();
    private Dictionary<PostureState, string> postData = new();

    public AnimationDataBase(AnimationParametorDataBase source, Animator animator)
    {
        foreach (pData d in source.data)
        {
            // if (d == null || string.IsNullOrWhiteSpace(d.parametorName)) continue;

            if (d.useMovement && !moveData.ContainsKey(d.move))
                moveData.Add(d.move, d.parametorName);

            if (d.usePositioning && !positData.ContainsKey(d.posit))
                positData.Add(d.posit, d.parametorName);

            if (d.usePosture && !postData.ContainsKey(d.post))
                postData.Add(d.post, d.parametorName);
        }

        Validate(animator);
    }

    private void Validate(Animator animator)
    {
        if (animator == null)
        {
            Debug.LogError("[Animation] Animator is null");
            return;
        }

        foreach (var kv in moveData)
            Check(animator, kv.Value, $"Movement:{kv.Key}");

        foreach (var kv in positData)
            Check(animator, kv.Value, $"Positioning:{kv.Key}");

        foreach (var kv in postData)
            Check(animator, kv.Value, $"Posture:{kv.Key}");
    }

    private void Check(Animator animator, string param, string context)
    {
        // if (string.IsNullOrWhiteSpace(param))
        // {
        //     Debug.LogWarning($"[Animation] Empty param ({context})");
        //     return;
        // }

        // bool found = false;
        // foreach (var p in animator.parameters)
        // {
        //     if (p.name == param)
        //     {
        //         found = true;
        //         break;
        //     }
        // }
    }

    public string GetParam<T>(T state) where T : Enum
    {
        if (state is MovementState m && moveData.TryGetValue(m, out var mData))
            return mData;

        if (state is PositioningState p && positData.TryGetValue(p, out var pData))
            return pData;

        if (state is PostureState po && postData.TryGetValue(po, out var poData))
            return poData;

        return null;
    }
}

public class AnimationManager
{
    private Dictionary<Type, string> RunningAnimation = new Dictionary<Type, string>();

    private AnimationDataBase animationDataBase;
    private AnimationExecutor animationExecutor;
    
    public AnimationManager(
        StateWatcher stateWatcher,
        AnimationDataBase animationDataBase,
        AnimationExecutor animationExecutor
    )
    {
        this.animationDataBase = animationDataBase;
        this.animationExecutor = animationExecutor;

        stateWatcher.Subscribe<MovementState>(InvokeAnimationMove);
        stateWatcher.Subscribe<PositioningState>(InvokeAnimationPosit);
        stateWatcher.Subscribe<PostureState>(InvokeAnimationPost);
    }

    public void InvokeAnimationMove(MovementState state)
    {
        string param = animationDataBase.GetParam<MovementState>(state);

        if(string.IsNullOrEmpty(param))
        {
            BreakAnimation(state.GetType());
            return;
        }
        
        AnimationDuplicationChecker(state.GetType(), param);
    }

    public void InvokeAnimationPosit(PositioningState state)
    {
        string param = animationDataBase.GetParam<PositioningState>(state);

        if(string.IsNullOrEmpty(param))
        {
            BreakAnimation(state.GetType());
            return;
        }
        
        AnimationDuplicationChecker(state.GetType(), param);
    }

    public void InvokeAnimationPost(PostureState state)
    {
        string param = animationDataBase.GetParam<PostureState>(state);

        // Debug.Log(state);

        if(string.IsNullOrEmpty(param))
        {
            BreakAnimation(state.GetType());
            return;
        }
        
        AnimationDuplicationChecker(state.GetType(), param);
    }

    public void AnimationDuplicationChecker(Type state, string param)
    {
        if(RunningAnimation.TryGetValue(state, out string p))
            animationExecutor.setFalse(p);

        animationExecutor.Execute(param);

        RunningAnimation[state] = param;
    }

    public void BreakAnimation(Type t)
    {
        if(RunningAnimation.TryGetValue(t, out string param))
            animationExecutor.setFalse(param);

        RunningAnimation.Remove(t);
    }
}

public class AnimationExecutor
{
    private Animator animator;
    private Dictionary<string, AnimationParametorModify> paramModify;

    public AnimationExecutor(
        Animator animator,
        Dictionary<string, AnimationParametorModify> paramModify)
    {
        this.animator = animator;
        this.paramModify = paramModify;
    }

    public void Execute(string param)
    {
        if (animator == null) return;

        animator.SetBool(param, true);
    }

    public void setFalse(string param)
    {

        animator.SetBool(param, false);
    }

    public void onUpdate()
    {
        foreach(var p in paramModify)
        {
            animator.SetFloat(p.Key, p.Value.GetFloatParametor());
        }
    }
}
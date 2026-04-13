using UnityEngine;
using System;
using System.Collections.Generic;

public class AnimationData<T> where T : Enum
{
    public T Type;
    public string Parametor;
    public bool isBlend;
    public float BlendParametor;

    public bool GetBlendParametor(out float value)
    {
        value = BlendParametor;
        return isBlend;
    }
}

public class AnimationDataBase
{
    public readonly Dictionary<MovementState, AnimationData<MovementState>> movementAnimationParametor = new();
    public readonly Dictionary<PostureState, AnimationData<PostureState>> postureAnimationParametor = new();
    public readonly Dictionary<PositioningState, AnimationData<PositioningState>> positioningAnimationParametor = new();
}

public class AnimationManager
{
    private readonly Animator animator;

    // 内部状態（とりあえず保持）
    private float speed;

    public AnimationManager(Animator animator, StateWatcher watcher)
    {
        this.animator = animator;

        // StateWatcherと接続
        watcher.Subscribe<MovementState>(OnMovementChanged);
        watcher.Subscribe<PostureState>(OnPostureChanged);
        watcher.Subscribe<PositioningState>(OnPositioningChanged);
    }

    private void OnMovementChanged(MovementState state)
    {
    }

    private void OnPostureChanged(PostureState state)
    {
    }

    private void OnPositioningChanged(PositioningState state)
    {
    }
}

public class AnimationExecutor
{
}
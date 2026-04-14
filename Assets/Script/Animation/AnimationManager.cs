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
    
    public void SetAnimationParamFloat(float param)
    {
    }
}		

public class AnimationDataBase
{
    public readonly Dictionary<MovementState, AnimationData<MovementState>> movementAnimationParametor = new();
    public readonly Dictionary<PostureState, AnimationData<PostureState>> postureAnimationParametor = new();
    public readonly Dictionary<PositioningState, AnimationData<PositioningState>> positioningAnimationParametor = new();
    
    //Todo:
}

public class AnimationManager
{
    private readonly Animator animator;
    private readonly Dictionary<string, float> AnimationBlend = new();

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
    
    private void JudgeExecutorType<T>(AnimationData<T> data)
    {
    	
    }
}

public class AnimationExecutor //Todo:どこで渡す？
{	
	private Animator anim;

	public void OnUpdate(Dictionary<string, float> data)
	{
		foreach(var d in data)
		{
			anim.SetFloat(d.Key, d.Value);
		}
	}
	
	public void Execute

}
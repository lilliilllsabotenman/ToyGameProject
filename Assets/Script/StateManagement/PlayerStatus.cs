using UnityEngine;
using System;

public enum MovementState
{
    Stand,
    Move,
    Dash
}

public enum PostureState
{
    Upright,
    Crouch
}

public enum PositioningState
{
    Ground,
    Jump,
    Gliding,
    Floating,
    Clip
}

public interface IStateJudge
{
    public bool StateJudgment(PlayerStateData state);
}

public class PlayerStateData
{
    public MovementState movementState { get; private set;}
    public PositioningState positioningState { get; private set;}
    public PostureState postureState { get; private set;}

    public void SetMovementState(MovementState state)
    {
        movementState = state;
    }

    public void SetPostioningState(PositioningState state)
    {  
        positioningState = state;
    }

    public void SetPostureState(PostureState state)
    {
        postureState = state;
    }
}

public class PlayerStateManager
{
    public PlayerStateData stateData {get; private set;}
    public event Action<PlayerStateData> AnimationChangeJudge;

    public PlayerStateManager(PlayerStateData stateData)
    {
        this.stateData = stateData;
    }

    public void SetEvent(Action<PlayerStateData> AnimationChangeJudge)
    {
        // this.AnimationChangeJudge += AnimationChangeJudge;
    }

    public bool TryMovementStateChange(MovementState state, IStateJudge judgment)
    {
        if(judgment.StateJudgment(stateData))
        {
            if(stateData.movementState == state) return false;

            stateData.SetMovementState(state);

            AnimationChangeJudge?.Invoke(stateData);
            return true;
        }

        return false;
    }

    public bool TryPositioningStateChange(PositioningState state, IStateJudge judgment)
    {
        if(judgment.StateJudgment(stateData))
        {
            if(stateData.positioningState == state) return false;

            stateData.SetPostioningState(state);  

            AnimationChangeJudge?.Invoke(stateData);  
            return true;
        }

        return false;
    }

    public bool TryPostureStateChange(PostureState state, IStateJudge judgment)
    {
        if(judgment.StateJudgment(stateData))
        {
            if(stateData.postureState == state) return false;

            stateData.SetPostureState(state);

            AnimationChangeJudge?.Invoke(stateData);
            return true;
        }

        return false;
    }
}
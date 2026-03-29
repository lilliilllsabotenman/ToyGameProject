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
    public PlayerStateData stateData { get; private set; }

    // 中立イベント（意味を持たない）
    public event Action<PlayerStateData> OnStateChanged;

    public PlayerStateManager(PlayerStateData stateData)
    {
        this.stateData = stateData;
    }

    public bool TryMovementStateChange(MovementState state, IStateJudge judgment)
    {
        if (!judgment.StateJudgment(stateData)) return false;
        if (stateData.movementState == state) return false;

        stateData.SetMovementState(state);
        OnStateChanged?.Invoke(stateData);
        return true;
    }

    public bool TryPositioningStateChange(PositioningState state, IStateJudge judgment)
    {
        if (!judgment.StateJudgment(stateData)) return false;
        if (stateData.positioningState == state) return false;

        stateData.SetPostioningState(state);
        OnStateChanged?.Invoke(stateData);
        return true;
    }

    public bool TryPostureStateChange(PostureState state, IStateJudge judgment)
    {
        if (!judgment.StateJudgment(stateData)) return false;
        if (stateData.postureState == state) return false;

        stateData.SetPostureState(state);
        OnStateChanged?.Invoke(stateData);
        return true;
    }
}
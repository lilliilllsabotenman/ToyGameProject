using UnityEngine;
using System.Collections.Generic;
using System;
using StateJudgment;

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
    public MovementState movementState { get; private set; }
    public PositioningState positioningState { get; private set; }
    public PostureState postureState { get; private set; }

    public void SetMovementState(MovementState state)
    {
        // Debug.Log(state);
        movementState = state;
    }

    public void SetPostioningState(PositioningState state)
    {
        // Debug.Log(state);
        positioningState = state;
    }

    public void SetPostureState(PostureState state)
    {
        // Debug.Log(state);
        postureState = state;
    }
}

public class PlayerStateManager
{
    public PlayerStateData stateData { get; private set; }
    public event Action<PlayerStateData> OnStateChanged;

    private Dictionary<Type, Func<Enum, bool>> stateChanged;
    private StateChangeJudgment stateJudge = new();

    public PlayerStateManager(PlayerStateData stateData)
    {
        this.stateData = stateData;

        // FIX: Explicitly initialize defaults instead of relying on enum order.
        this.stateData.SetMovementState(MovementState.Stand);
        this.stateData.SetPostioningState(PositioningState.Ground);
        this.stateData.SetPostureState(PostureState.Upright);

        stateChanged = new Dictionary<Type, Func<Enum, bool>>
        {
            { typeof(MovementState), e => movementChanged((MovementState)e) },
            { typeof(PositioningState), e => positioningChanged((PositioningState)e) },
            { typeof(PostureState), e => postureChanged((PostureState)e) },
        };
    }

    public bool TryChangeState(Enum newState)
    {
        Type type = newState.GetType();

        if (!stateChanged.TryGetValue(type, out var handler))
        {
            Debug.LogError($"Unsupported state type: {type}");
            return false;
        }

        bool result = handler(newState);

        if (result)
            OnStateChanged?.Invoke(stateData);

        return result;
    }

    public bool movementChanged(MovementState state)
    {
        // FIX: State value was never written before; movement stayed Stand and blocked PlayerDefaultAction.
        if (stateJudge.GetStateModify<MovementState>(state, stateData)) 
        {
            stateData.SetMovementState(state);
            return true;
        }
        else return false;
    }

    public bool positioningChanged(PositioningState state)
    {
        if (stateJudge.GetStateModify<PositioningState>(state, stateData)) 
        {
            stateData.SetPostioningState(state);
            return true;
        }
        else return false;
    }

    public bool postureChanged(PostureState state)
    {
        if (stateJudge.GetStateModify<PostureState>(state, stateData)) 
        {
            stateData.SetPostureState(state);
            return true;
        }
        else return false;
    }
}

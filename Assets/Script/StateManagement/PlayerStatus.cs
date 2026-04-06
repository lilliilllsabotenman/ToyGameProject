using UnityEngine;
using System.Collections.Generic;
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

    private Dictionary<Type, Func<Enum, bool>> stateChanged;

    public event Action<PlayerStateData> OnStateChanged;

    public PlayerStateManager(PlayerStateData stateData)
    {
        this.stateData = stateData;

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
        // 処理
        return true;
    }

    public bool positioningChanged(PositioningState state)
    {
        // 処理
        return true;
    }

    public bool postureChanged(PostureState state)
    {
        // 処理
        return true;
    }
}
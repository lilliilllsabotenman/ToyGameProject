using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

///<summary>
/// 状態監視用のクラス
/// イベントで受け取った変化を抽象化してほかクラスに通知する。
/// </summary>
public class StateWatcher 
{
    private readonly Dictionary<Type, Delegate> _events;

    private MovementState moveState;
    private PostureState postState;
    private PositioningState positState;

    private bool initialized = false;

    public StateWatcher(PlayerStateManager manager)
    {
        manager.OnStateChanged += StateChanged;
        _events = new Dictionary<Type, Delegate>();
    }

    public void Subscribe<T>(Action<T> action) where T : Enum // 登録
    {
        Type t = typeof(T);

        if (t != typeof(MovementState) &&
            t != typeof(PostureState) &&
            t != typeof(PositioningState))
        {
            Debug.LogError("StateWatcherに変なの入った");
            return;
        }

        if (_events.TryGetValue(t, out var existing))
        {
            _events[t] = Delegate.Combine(existing, action);
        }
        else
        {
            _events[t] = action;
        }
    }

    private void Invoke<T>(T state) where T : Enum
    {
        if (_events.TryGetValue(typeof(T), out var del))
        {
            ((Action<T>)del)?.Invoke(state);
        }
    }

    public void StateChanged(PlayerStateData state)
    {
        // 初回は基準値セットだけ
        if (!initialized)
        {
            moveState = state.movementState;
            postState = state.postureState;
            positState = state.positioningState;
            initialized = true;
            return;
        }

        // Movement
        if (!EqualityComparer<MovementState>.Default.Equals(moveState, state.movementState))
        {
            moveState = state.movementState;
            Invoke(moveState);
        }

        // Posture
        if (!EqualityComparer<PostureState>.Default.Equals(postState, state.postureState))
        {
            postState = state.postureState;
            Invoke(postState);
        }

        // Positioning
        if (!EqualityComparer<PositioningState>.Default.Equals(positState, state.positioningState))
        {
            positState = state.positioningState;
            Invoke(positState);
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

///<summary>
/// 状態監視用のクラス
/// イベントで受け取った変化を抽象化してほかクラスに通知する。
/// </summary>
public class StateWatcher
{
    // 型ごとのイベントを保持
    private readonly Dictionary<Type, Delegate> _events;

    private MovementState moveState;
    private PostureState postState;
    private PositioningState positState;

    private bool initialized = false;

    public StateWatcher(PlayerStateManager manager)
    {
        manager.OnStateChanged += StateChanged;
    }

    /// <summary>
    /// ジェネリックでイベント登録
    /// </summary>
    public void ChangeStateEvent<T>(Action<T> action) where T : Enum
    {
        Type t = typeof(T);

        if (t != typeof(MovementState) &&
            t != typeof(PostureState) &&
            t != typeof(PositioningState))
        {
            Debug.LogError("だから型安全を確保しろって言ったんだ");
        }

        _events[typeof(T)] = action;
    }

    /// <summary>
    /// イベント発火（内部用）
    /// </summary>
    private void Invoke<T>(T state) where T : Enum
    {
        if (_events.TryGetValue(typeof(T), out var del))
        {
            ((Action<T>)del)?.Invoke(state);
        }
    }

    public void StateChanged(PlayerStateData state)
    {
        Debug.Log("State Changed");

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
using UnityEngine;
using System.Collections.Generic;
using System;

public enum BehaviourType
{
    Event,
    Update,
    Pasiv
}

#region Data

public class BehaviourData
{
    public readonly Dictionary<MovementState, AbilityBehaviour> movementBehaviour = new();
    public readonly Dictionary<PositioningState, AbilityBehaviour> positioningBehaviour = new();
    public readonly Dictionary<PostureState, AbilityBehaviour> postureBehaviour = new();

    public bool AddAbilityData(AbilityBehaviour behaviour)
    {
        var state = behaviour.IGetMyAbilityState();

        switch (state)
        {
            case MovementState m:
            {
                if (movementBehaviour.ContainsKey(m))
                {
                    Debug.LogError($"MovementState {m} は既に登録済み");
                    return false;
                }

                movementBehaviour.Add(m, behaviour);
                return true;
            }

            case PositioningState p:
            {
                if (positioningBehaviour.ContainsKey(p))
                {
                    Debug.LogError($"PositioningState {p} は既に登録済み");
                    return false;
                }

                positioningBehaviour.Add(p, behaviour);
                return true;
            }

            case PostureState p:
            {
                if (postureBehaviour.ContainsKey(p))
                {
                    Debug.LogError($"PostureState {p} は既に登録済み");
                    return false;
                }

                postureBehaviour.Add(p, behaviour);
                return true;
            }

            default:
                Debug.LogError("未対応のState型");
                return false;
        }
    }

    public bool RemoveAbilityData(AbilityBehaviour behaviour)
    {
        var state = behaviour.IGetMyAbilityState();

        switch (state)
        {
            case MovementState m:
            {
                if (!movementBehaviour.ContainsKey(m))
                {
                    Debug.LogError($"MovementState {m} は未登録");
                    return false;
                }

                movementBehaviour.Remove(m);
                return true;
            }

            case PositioningState p:
            {
                if (!positioningBehaviour.ContainsKey(p))
                {
                    Debug.LogError($"PositioningState {p} は未登録");
                    return false;
                }

                positioningBehaviour.Remove(p);
                return true;
            }

            case PostureState p:
            {
                if (!postureBehaviour.ContainsKey(p))
                {
                    Debug.LogError($"PostureState {p} は未登録");
                    return false;
                }

                postureBehaviour.Remove(p);
                return true;
            }

            default:
                Debug.LogError("未対応のState型");
                return false;
        }
    }
}

#endregion

#region Manager

public class BehaviourManager
{
    private BehaviourData data;
    private BehaviourExecutor executor;

    public BehaviourManager(
        BehaviourData data,
        BehaviourExecutor executor,
        StateWatcher watcher)
    {
        this.data = data;
        this.executor = executor;

        // Stateの変更に対してEvent登録
        watcher.Subscribe<MovementState>(MovementStateChanged);
        watcher.Subscribe<PostureState>(PostureStateChanged);
        watcher.Subscribe<PositioningState>(PositioningStateChanged);
    }

    public void MovementStateChanged(MovementState state)
    {
        if (data.movementBehaviour.TryGetValue(state, out var behaviour))
        {
            executor.BehaviourLoopChecker(behaviour);
        }
    }

    public void PositioningStateChanged(PositioningState state)
    {
        if (data.positioningBehaviour.TryGetValue(state, out var behaviour))
        {
            executor.BehaviourLoopChecker(behaviour);
        }
    }

    public void PostureStateChanged(PostureState state)
    {
        if (data.postureBehaviour.TryGetValue(state, out var behaviour))
        {
            executor.BehaviourLoopChecker(behaviour);
        }
    }
}

#endregion

#region Executor

public class BehaviourExecutor
{
    private List<AbilityBehaviour> removeBuffer = new();
    private List<AbilityBehaviour> onFinishedBehaviour = new();
    private List<AbilityBehaviour> CancelBehaviour = new(); //スケールする可能性が無いのでBuffer系はリスト管理

    private Dictionary<AbilityBehaviour, AbilityTimeChecker> onLoopBehaviour = new();

    public void BehaviourLoopChecker(AbilityBehaviour behaviour)
    {
        if (behaviour.IEventDriven())
        {
            behaviour.Behaviour();
        }
        else
        {
            if(GetRunningBehaviourType(behaviour.IGetMyAbilityState().GetType(), out var value)) 
                onLoopBehaviour.Remove(value);
            var timer = new AbilityTimeChecker(behaviour.GetValidityTime());
            onLoopBehaviour[behaviour] = timer;
        }
    }

    public bool GetRunningBehaviourType(Type type, out AbilityBehaviour value)
    {
        foreach(var behaviour in onLoopBehaviour)
        {
            if(behaviour.Key.IGetMyAbilityState().GetType()== type) 
            {
                value = behaviour.Key;
                return true; 
            }
        }
        value = null;//falseの場合は絶対使われないからOK！！！
        return false;
    }

    private void LoopBehaviourRemover()
    {
        foreach (var behaviour in removeBuffer)
        {
            onLoopBehaviour.Remove(behaviour);
        }

        removeBuffer.Clear();
    }

    private void FinishedBehaviourRemover()
    {
        foreach(var finish in onFinishedBehaviour)
        {
            CancelBehaviour.Remove(finish);
        }

        onFinishedBehaviour.Clear();
    }

    public void OnUpdate()
    {
        if(onLoopBehaviour.Count <= 0) return;

        foreach (var kvp in onLoopBehaviour)
        {
            var behaviour = kvp.Key;
            var timer = kvp.Value;

            if (timer.TimeCount())
            {
                if (behaviour.Cancel())
                {
                    if (!CancelBehaviour.Contains(behaviour)) 
                    {
                        CancelBehaviour.Add(behaviour);
                    }

                    removeBuffer.Add(behaviour);
                    continue;
                }
            }

            behaviour.Behaviour();
        }

        LoopBehaviourRemover();

        foreach (var cancel in CancelBehaviour)
        {
            if(!cancel.Cancel()) onFinishedBehaviour.Add(cancel);
        }

        FinishedBehaviourRemover();
    }
}

#endregion

#region Timer

public class AbilityTimeChecker
{
    private float time = 0;
    private float timeLimit;

    public AbilityTimeChecker(float timeLimit)
    {
        this.timeLimit = timeLimit;
    }

    public bool TimeCount()
    {
        time += Time.deltaTime;
        return time >= timeLimit;
    }
}

#endregion
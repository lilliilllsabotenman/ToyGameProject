using System;
using System.Collections.Generic;
using UnityEngine;

#region =========================
/*      AnimationMapBuilder     */
#endregion

public class AnimationMapBuilder
{
    public Dictionary<StateKey, List<AnimationData>> Build(AnimationDataBase db)
    {
        if (db == null)
            throw new Exception("AnimationDataBaseがnull");

        if (db.animData == null)
            throw new Exception("AnimationData配列がnull");

        var map = new Dictionary<StateKey, List<AnimationData>>();

        foreach (var data in db.animData)
        {
            if (data == null)
                throw new Exception("AnimationDataにnullが含まれている");

            if (string.IsNullOrEmpty(data.AnimationParameter))
                throw new Exception("AnimationParameterが未設定");

            if (data.moveState.use)
                Add(map, typeof(MovementState), Convert.ToInt32(data.moveState.value), data);

            if (data.positState.use)
                Add(map, typeof(PositioningState), Convert.ToInt32(data.positState.value), data);

            if (data.postState.use)
                Add(map, typeof(PostureState), Convert.ToInt32(data.postState.value), data);
        }

        return map;
    }

    private void Add(
        Dictionary<StateKey, List<AnimationData>> map,
        Type type,
        int value,
        AnimationData data)
    {
        var key = new StateKey(type, value);

        if (!map.TryGetValue(key, out var list))
        {
            list = new List<AnimationData>();
            map[key] = list;
        }

        list.Add(data);
    }
}

#region =========================
/*      AnimationExecutor       */
#endregion

public class AnimationExecutor
{
    private readonly Animator animator;
    private readonly Dictionary<StateKey, List<AnimationData>> map;

    // 全パラメータ管理（排他制御用）
    Dictionary<Type, HashSet<string>> paramsByType = new();

    public AnimationExecutor(
        Animator animator,
        Dictionary<StateKey, List<AnimationData>> map)
    {
        if (animator == null)
            throw new Exception("Animatorがnull");

        if (map == null)
            throw new Exception("Mapがnull");

        this.animator = animator;
        this.map = map;

        CollectAllParams();
    }

    // -------------------------
    // 全パラメータ収集
    // -------------------------
    private void CollectAllParams()
    {
        foreach (var pair in map)
        {
            var type = pair.Key.type;

            if (!paramsByType.ContainsKey(type))
                paramsByType[type] = new HashSet<string>();

            foreach (var data in pair.Value)
            {
                paramsByType[type].Add(data.AnimationParameter);
            }
        }
    }

    // -------------------------
    // 実行（排他制御あり）
    // -------------------------
    public void Execute(Type type, int value)
    {
        // ① そのTypeだけOFF
        if (paramsByType.TryGetValue(type, out var paramsSet))
        {
            foreach (var param in paramsSet)
            {
                animator.SetBool(param, false);
            }
        }

        // ② 該当だけON
        var key = new StateKey(type, value);

        if (!map.TryGetValue(key, out var list))
            return;

        foreach (var data in list)
        {
            if (!Check(data)) continue;

            animator.SetBool(data.AnimationParameter, true);
        }
    }

    // -------------------------
    // 条件チェック（拡張ポイント）
    // -------------------------
    private bool Check(AnimationData data)
    {
        // 将来ここに複合条件を書く
        return true;
    }
}

#region =========================
/*        AnimationSystem       */
#endregion

public class AnimationSystem
{
    private readonly AnimationExecutor executor;

    public AnimationSystem(AnimationDataBase db, Animator animator)
    {
        if (db == null)
            throw new Exception("AnimationDataBaseがnull");

        if (animator == null)
            throw new Exception("Animatorがnull");

        var builder = new AnimationMapBuilder();
        var map = builder.Build(db);

        executor = new AnimationExecutor(animator, map);
    }

    public void Bind(StateWatcher watcher)
    {
        if (watcher == null)
            throw new Exception("StateWatcherがnull");

        watcher.Subscribe<MovementState>(OnMovement);
        watcher.Subscribe<PositioningState>(OnPosition);
        watcher.Subscribe<PostureState>(OnPosture);
    }

    #region Execution
    private void OnMovement(MovementState state)
        => executor.Execute(typeof(MovementState), Convert.ToInt32(state));

    private void OnPosition(PositioningState state)
        => executor.Execute(typeof(PositioningState), Convert.ToInt32(state));

    private void OnPosture(PostureState state)
        => executor.Execute(typeof(PostureState), Convert.ToInt32(state));
    #endregion
}

// //選別クラス
// {
//     //主体関数(AniamtionParametor)
//     {
//         if(GetType(AnimationParametor) == typeof(float))
//             //モディファイア管理ぶち込み
//         else if(/*Bool の場合*/)
//             //今まで通りExcuteぶち込み
//     }
// }


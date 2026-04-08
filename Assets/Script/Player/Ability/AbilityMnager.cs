using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// アイテム種別（キーとして使用）
/// </summary>
public enum ItemType
{
    None,
    Circle,
    Square,
    Triangle,
    Star
}

/// <summary>
/// 「1アイテム = 1能力」の束
/// データの最小単位
/// </summary>
public class AbilityItemSlot
{
    public ItemObjectBehaviour ItemObject;   // 見た目・ワールド挙動
    public IAbility Ability;                 // ロジック本体
    public ItemType itemType;                // 識別キー
    public AbilityBehaviour Behaviour;       // 追加の振る舞い（補助）

    public AbilityItemSlot(
            ItemObjectBehaviour itemObject,
            AbilityBehaviour behaviour,
            IAbility ability,
            ItemType itemType)
    {
        // 不正データをここで完全排除（データ保証）
        if (itemObject == null) throw new ArgumentNullException();
        if (ability == null) throw new ArgumentNullException();
        if (behaviour == null) throw new ArgumentNullException();
        if (itemType == ItemType.None) throw new ArgumentException();

        this.ItemObject = itemObject;
        this.Behaviour = behaviour;
        this.Ability = ability;
        this.itemType = itemType;
    }
}

/// <summary>
/// データ管理専用クラス
/// ・Dictionaryの所有者
/// ・データの整合性を保証する層
/// </summary>
public class AbilityDataBase
{
    // 全能力のマスター（唯一のソース）
    private Dictionary<ItemType, AbilityItemSlot> MasterAbilities = new();

    /// <summary>
    /// キー存在チェック（軽量）
    /// </summary>
    public bool CheckData(ItemType type)
    {
        return MasterAbilities.ContainsKey(type);
    }

    /// <summary>
    /// 追加 or 上書き
    /// </summary>
    public void AddAbility(ItemType type, AbilityItemSlot slot)
    {
        MasterAbilities[type] = slot;
    }

    /// <summary>
    /// 削除
    /// </summary>
    public void RemoveAbility(ItemType type)
    {
        MasterAbilities.Remove(type);
    }

    /// <summary>
    /// Slot取得（最重要API）
    /// → 外部はDictionaryに触らない
    /// </summary>
    public bool TryGetSlot(ItemType type, out AbilityItemSlot slot)
    {
        return MasterAbilities.TryGetValue(type, out slot);
    }

    /// <summary>
    /// 全列挙（デバッグ・再構築用）
    /// </summary>
    public IEnumerable<AbilityItemSlot> GetAllSlots()
    {
        return MasterAbilities.Values;
    }
}

/// <summary>
/// アビリティ管理（オーケストレーター）
/// ・データと実行の橋渡し
/// ・ロジックは持たない（重要）
/// </summary>
public class AbilityManager
{
    private AbilityDataBase abilityDataBase;

    private ItemRemover removeItem;
    private AbilityActivator abilityActivator;

    public AbilityManager(
        AbilityDataBase dataBase,
        ItemRemover removeItem,
        InputResolver resolver)
    {
        this.abilityDataBase = dataBase;
        this.removeItem = removeItem;

        // 実行系は別責務として分離
        this.abilityActivator = new AbilityActivator(resolver);
    }

    /// <summary>
    /// 能力追加
    /// ・既存があれば置き換え
    /// ・Data → Activator の順で反映
    /// </summary>
    public bool TryAddAbility(AbilityItemSlot slot)
    {
        var type = slot.itemType;

        // 既存があれば削除（整合性維持）
        if (abilityDataBase.CheckData(type))
        {
            RemoveItem(type);
        }

        // データ登録
        abilityDataBase.AddAbility(type, slot);

        // 実行登録
        abilityActivator.SetLevel(slot);

        return true;
    }

    /// <summary>
    /// 削除処理
    /// ・Activator → 見た目 → Data の順で破棄
    /// （依存の逆順）
    /// </summary>
    public void RemoveItem(ItemType type)
    {
        if (abilityDataBase.TryGetSlot(type, out var slot))
        {
            // 実行解除
            abilityActivator.DeleteItemLevel(slot);

            // 見た目復元
            removeItem.Remove(slot);

            // データ削除
            abilityDataBase.RemoveAbility(type);
        }
    }
}

/// <summary>
/// 実行管理クラス
/// ・Input接続
/// ・イベント更新対象の管理
/// ・実行リストの構築
/// </summary>
public class AbilityActivator
{
    private readonly InputResolver resolver;

    // 実行対象リスト（用途別に分離）
    private readonly List<OnEventAbility> eventAbilities = new();
    private readonly List<OnFixedUpdateAbility> fixedUpdateAbilities = new();

    // 現在有効なスロット（唯一のソース）
    private readonly List<AbilityItemSlot> activeSlots = new();

    // 型ごとの管理（スタック・レベル概念用）
    private readonly Dictionary<Type, List<AbilityItemSlot>> abilitiesByType = new();

    public AbilityActivator(InputResolver resolver)
    {
        this.resolver = resolver;
    }

    public IReadOnlyList<OnEventAbility> EventAbilities => eventAbilities;
    public IReadOnlyList<OnFixedUpdateAbility> FixedUpdateAbilities => fixedUpdateAbilities;

    /// <summary>
    /// 能力追加（有効化）
    /// </summary>
    public void SetLevel(AbilityItemSlot slot)
    {
        activeSlots.Add(slot);

        var type = slot.Ability.GetType();

        // 型ごとのグルーピング（重複・レベル管理用）
        if (!abilitiesByType.ContainsKey(type))
            abilitiesByType[type] = new List<AbilityItemSlot>();

        abilitiesByType[type].Add(slot);

        // Input登録
        Register(slot.Ability);

        // 実行リスト再構築
        RebuildExecutionLists();
    }

    /// <summary>
    /// 能力削除（無効化）
    /// </summary>
    public void DeleteItemLevel(AbilityItemSlot slot)
    {
        activeSlots.Remove(slot);

        var type = slot.Ability.GetType();

        if (abilitiesByType.ContainsKey(type))
        {
            abilitiesByType[type].Remove(slot);

            if (abilitiesByType[type].Count == 0)
                abilitiesByType.Remove(type);
        }

        // Input解除
        Unregister(slot.Ability);

        RebuildExecutionLists();
    }

    /// <summary>
    /// Input登録
    /// </summary>
    private void Register(IAbility ability)
    {
        var actionType = ability.GetActionType();

        resolver.BindPressed(actionType, ability);
        resolver.BindReleased(actionType, ability);
        resolver.BindHeld(actionType, ability);
    }

    /// <summary>
    /// Input解除
    /// </summary>
    private void Unregister(IAbility ability)
    {
        var actionType = ability.GetActionType();

        resolver.UnbindPressed(actionType);
        resolver.UnbindReleased(actionType);
        resolver.UnbindHeld(actionType);
    }

    /// <summary>
    /// 実行対象リスト再構築
    /// ・毎回フルリビルド（シンプル＆安全）
    /// </summary>
    private void RebuildExecutionLists()
    {
        eventAbilities.Clear();
        fixedUpdateAbilities.Clear();

        foreach (var slot in activeSlots)
        {
            var ability = slot.Ability;

            if (ability is OnEventAbility e)
                eventAbilities.Add(e);

            if (ability is OnFixedUpdateAbility f)
                fixedUpdateAbilities.Add(f);
        }
    }
}

/// <summary>
/// アイテム削除（見た目・ワールド復元）
/// </summary>
public class ItemRemover
{
    private Transform tf;

    public ItemRemover(Transform tf)
    {
        this.tf = tf;
    }

    public void Remove(AbilityItemSlot slot)
    {
        // ワールドに戻す（ドロップなど）
        slot.ItemObject.Restoration(tf.position);
    }
}
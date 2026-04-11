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
public class AbilityItemData
{
    public ItemObjectBehaviour ItemObject;   // 見た目・ワールド挙動
    public IAbility Ability;                 // ロジック本体
    public ItemType itemType;                // 識別キー
    public AbilityBehaviour Behaviour;       // 追加の振る舞い（補助）

    public AbilityItemData(
        ItemObjectBehaviour itemObject,
        AbilityBehaviour behaviour,
        IAbility ability,
        ItemType itemType)
    {
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
/// </summary>
public class AbilityDataBase
{
    private Dictionary<ItemType, AbilityItemData> masterAbilities = new();

    public bool CheckData(ItemType type)
    {
        return masterAbilities.ContainsKey(type);
    }

    public void AddAbility(ItemType type, AbilityItemData itemData)
    {
        masterAbilities[type] = itemData;
    }

    public void RemoveAbility(ItemType type)
    {
        masterAbilities.Remove(type);
    }

    public bool TryGetSlot(ItemType type, out AbilityItemData itemData)
    {
        return masterAbilities.TryGetValue(type, out itemData);
    }

    public IEnumerable<AbilityItemData> GetAllSlots()
    {
        return masterAbilities.Values;
    }
}

#region  AbilityManager

/// <summary>
/// アビリティ管理（オーケストレーター）
/// </summary>
public class AbilityManager
{
    private AbilityDataBase abilityDataBase;

    private ItemRemover removeItem;
    private AbilityActivator abilityActivator;

    private BehaviourData behaviourData; // ★追加

    public AbilityManager(
        AbilityDataBase dataBase,
        Transform tf,
        InputResolver resolver)
    {
        this.abilityDataBase = dataBase;

        // ★ここで共通インスタンス作る
        behaviourData = new BehaviourData();

        this.removeItem = new ItemRemover(tf, behaviourData);
        this.abilityActivator = new AbilityActivator(resolver, behaviourData);
    }

    public bool TryAddAbility(AbilityItemData itemData)
    {
        var type = itemData.itemType;

        if (abilityDataBase.CheckData(type))
        {
            RemoveItem(type);
        }

        abilityDataBase.AddAbility(type, itemData);
        abilityActivator.SetLevel(itemData);

        return true;
    }

    public void RemoveItem(ItemType type)
    {
        if (abilityDataBase.TryGetSlot(type, out var itemData))
        {
            abilityActivator.DeleteItemLevel(itemData);
            removeItem.Remove(itemData);
            abilityDataBase.RemoveAbility(type);
        }
    }
}

#endregion
#region AbilityActivetor

/// <summary>
/// 実行管理クラス
/// </summary>
public class AbilityActivator
{
    private readonly InputResolver resolver;
    private readonly BehaviourData behaviourData; // ★追加

    private readonly List<AbilityItemData> activeItems = new();
    private readonly Dictionary<Type, List<AbilityItemData>> abilitiesByType = new();

    public AbilityActivator(
            InputResolver resolver, 
            BehaviourData behaviourData) // ★変更
    {
        this.resolver = resolver;
        this.behaviourData = behaviourData;
    }

    public void SetLevel(AbilityItemData itemData)
    {
        activeItems.Add(itemData);

        var type = itemData.Ability.GetType();

        if (!abilitiesByType.ContainsKey(type))
            abilitiesByType[type] = new List<AbilityItemData>();

        abilitiesByType[type].Add(itemData);

        Register(itemData.Ability);

        // ★ Behaviour登録
        behaviourData.AddAbilityData(itemData.Behaviour);
    }

    public void DeleteItemLevel(AbilityItemData itemData)
    {
        activeItems.Remove(itemData);

        var type = itemData.Ability.GetType();

        if (abilitiesByType.ContainsKey(type))
        {
            abilitiesByType[type].Remove(itemData);

            if (abilitiesByType[type].Count == 0)
                abilitiesByType.Remove(type);
        }

        Unregister(itemData.Ability);

        // ★ Behaviour削除
        behaviourData.RemoveAbilityData(itemData.Behaviour);
    }

    private void Register(IAbility ability)
    {
        var actionType = ability.GetActionType();

        resolver.BindPressed(actionType, ability);
        resolver.BindReleased(actionType, ability);
        resolver.BindHeld(actionType, ability);
    }

    private void Unregister(IAbility ability)
    {
        var actionType = ability.GetActionType();

        resolver.UnbindPressed(actionType);
        resolver.UnbindReleased(actionType);
        resolver.UnbindHeld(actionType);
    }
}

#endregion
#region  ItemRemover

/// <summary>
/// アイテム削除（見た目・ワールド復元）
/// </summary>
public class ItemRemover
{
    private Transform tf;
    private BehaviourData behaviourData; // ★追加

    public ItemRemover(
            Transform tf, 
            BehaviourData behaviourData) // ★変更
    {
        this.tf = tf;
        this.behaviourData = behaviourData;
    }

    public void Remove(AbilityItemData itemData)
    {
        // ★ Behaviour削除（安全のためここでも）
        behaviourData.RemoveAbilityData(itemData.Behaviour);

        itemData.ItemObject.Restoration(tf.position);
    }
}
#endregion
using UnityEngine;
using System;
using System.Collections.Generic;

public enum ItemType
{
    None,
    Circle,
    Square,
    Triangle,
    Star
}

public class AbilityItemSlot
{
    public ItemObjectBehaviour ItemObject;
    public IAbility Ability;
    public ItemType itemType;

    public AbilityItemSlot(ItemObjectBehaviour itemObject, IAbility ability, ItemType itemType)
    {
        this.ItemObject = itemObject;
        this.Ability = ability;
        this.itemType = itemType;
    }

    public bool IsComplete()
    {
        return ItemObject != null
            && Ability != null
            && itemType != ItemType.None;
    }
}

/// <summary>
/// アビリティ管理クラス（所持・構成管理のみ）
/// </summary>
public class AbilityManager
{
    public Dictionary<ItemType, AbilityItemSlot> MasterAbilities = new();

    private ItemRemover removeItem;
    private AbilityActivator abilityActivator;

    public AbilityManager(
        ItemRemover removeItem,
        InputResolver resolver)
    {
        this.removeItem = removeItem;

        this.abilityActivator = new AbilityActivator(
            MasterAbilities,
            resolver
        );
    }

    public bool TryAddAbility(AbilityItemSlot slot)
    {
        if(!slot.IsComplete()) return false;

        if (MasterAbilities.TryGetValue(slot.itemType, out var old))
        {
            RemoveItem(old);
        }

        MasterAbilities[slot.itemType] = slot;

        abilityActivator.SetLevel(slot);

        return true;
    }

    public void RemoveItem(AbilityItemSlot slot)
    {
        MasterAbilities.Remove(slot.itemType);

        abilityActivator.DeleteItemLevel(slot);

        removeItem.Remove(slot);
    }
}

/// <summary>
/// アビリティの有効化・接続ハブ（責務集約）
/// — Input / Event / Update を一括管理
/// </summary>
public class AbilityActivator
{
    private readonly Dictionary<ItemType, AbilityItemSlot> masterAbilities;
    private readonly InputResolver resolver;

    private readonly List<OnEventAbility> eventAbilities = new();
    private readonly List<OnFixedUpdateAbility> fixedUpdateAbilities = new();

    private Dictionary<Type, List<AbilityItemSlot>> abilitiesByType = new();

    public AbilityActivator(
        Dictionary<ItemType, AbilityItemSlot> masterAbilities,
        InputResolver resolver
    )
    {
        this.masterAbilities = masterAbilities;
        this.resolver = resolver;
    }

    // ===== 公開API =====

    public IReadOnlyList<OnEventAbility> EventAbilities => eventAbilities;
    public IReadOnlyList<OnFixedUpdateAbility> FixedUpdateAbilities => fixedUpdateAbilities;

    public void SetLevel(AbilityItemSlot slot)
    {
        Type type = slot.Ability.GetType();

        if (!abilitiesByType.ContainsKey(type))
            abilitiesByType[type] = new List<AbilityItemSlot>();

        abilitiesByType[type].Add(slot);

        // 新規追加分のみ登録（重要）
        Register(slot.Ability);

        RebuildExecutionLists();
    }

    public void DeleteItemLevel(AbilityItemSlot slot)
    {
        Type type = slot.Ability.GetType();

        if (!abilitiesByType.ContainsKey(type))
            return;

        abilitiesByType[type].Remove(slot);

        Unregister(slot.Ability);

        if (abilitiesByType[type].Count == 0)
            abilitiesByType.Remove(type);

        RebuildExecutionLists();
    }

    // ===== 内部処理 =====

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

    private void RebuildExecutionLists()
    {
        eventAbilities.Clear();
        fixedUpdateAbilities.Clear();

        foreach (var slot in masterAbilities.Values)
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
/// アイテム削除処理
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
        slot.ItemObject.Restoration(tf.position);
    }
}
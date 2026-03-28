using UnityEngine;
using System;
using System.Collections.Generic;

public enum ItemType
{
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

    public AbilityItemSlot(ItemObjectBehaviour ItemObject, IAbility Ability, ItemType itemType)
    {
        this.ItemObject = ItemObject;
        this.Ability = Ability;
        this.itemType = itemType;
    }
}

/// <summary>
/// アビリティ管理クラス（接続ハブ）
/// </summary>
public class AbilityManager
{
    public Dictionary<ItemType, AbilityItemSlot> MasterAbilities = new();

    private ItemRemover removeItem;
    private AbilityActivator abilityActivator;

    private InputWatcher inputWatcher;

    public List<OnEventAbility> EventAbilities = new();
    public List<OnFixedUpdateAbility> FixedUpdateAbilities = new();

    public AbilityManager(
        ItemRemover removeItem,
        InputWatcher inputWatcher)
    {
        this.removeItem = removeItem;
        this.inputWatcher = inputWatcher;

        this.abilityActivator = new AbilityActivator(
            MasterAbilities,
            EventAbilities,
            FixedUpdateAbilities,
            RegisterPlayerAction,
            UnregisterPlayerAction
        );
    }

    public bool TryAddAbility(AbilityItemSlot slot)
    {
        if (MasterAbilities.TryGetValue(slot.itemType, out var old))
        {
            RemoveItem(old);
        }

        slot.Ability.SetActive();

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

    // ===== Input接続 =====

    private void RegisterPlayerAction(OnPlayerAction a)
    {
        var action = a.GetAction();
        inputWatcher.BindPressed(a.ActionType, action);
    }

    private void UnregisterPlayerAction(OnPlayerAction a)
    {
        var action = a.GetAction();
        inputWatcher.UnbindPressed(a.ActionType, action);
    }

}

public class AbilityActivator
{
    private readonly Dictionary<ItemType, AbilityItemSlot> masterAbilities;
    private readonly List<OnEventAbility> eventAbilities;
    private readonly List<OnFixedUpdateAbility> fixedUpdateAbilities;

    private readonly Action<OnPlayerAction> registerAction;
    private readonly Action<OnPlayerAction> unregisterAction;

    private Dictionary<Type, List<AbilityItemSlot>> abilitiesByType = new();

    public AbilityActivator(
        Dictionary<ItemType, AbilityItemSlot> masterAbilities,
        List<OnEventAbility> eventAbilities,
        List<OnFixedUpdateAbility> fixedUpdateAbilities,
        Action<OnPlayerAction> registerAction,
        Action<OnPlayerAction> unregisterAction
    )
    {
        this.masterAbilities = masterAbilities;
        this.eventAbilities = eventAbilities;
        this.fixedUpdateAbilities = fixedUpdateAbilities;
        this.registerAction = registerAction;
        this.unregisterAction = unregisterAction;
    }

    public void AbilityAssignment()
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

    public void SetLevel(AbilityItemSlot slot)
    {
        Type type = slot.Ability.GetType();

        if (!abilitiesByType.ContainsKey(type))
        {
            abilitiesByType[type] = new List<AbilityItemSlot>();
        }

        abilitiesByType[type].Add(slot);

        int count = abilitiesByType[type].Count;

        foreach (var s in abilitiesByType[type])
        {
            s.Ability.SetLevel(count);

            if (s.Ability is OnPlayerAction a)
                registerAction(a);
        }

        AbilityAssignment();
    }

    public void DeleteItemLevel(AbilityItemSlot slot)
    {
        Type type = slot.Ability.GetType();

        if (!abilitiesByType.ContainsKey(type))
            return;

        abilitiesByType[type].Remove(slot);

        if (slot.Ability is OnPlayerAction a)
            unregisterAction(a);

        int count = abilitiesByType[type].Count;

        if (count == 0)
        {
            abilitiesByType.Remove(type);
        }
        else
        {
            foreach (var s in abilitiesByType[type])
            {
                s.Ability.SetLevel(count);
            }
        }

        AbilityAssignment();
    }
}

public class ItemRemover
{
    private Transform tf;

    public void Remove(AbilityItemSlot iAbilitySlot)
    {
        iAbilitySlot.ItemObject.Restoration(tf.position);
    }

    public ItemRemover(Transform tf)
    {
        this.tf = tf;
    }
}
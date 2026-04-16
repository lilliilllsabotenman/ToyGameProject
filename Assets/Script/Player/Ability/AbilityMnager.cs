using UnityEngine;
using System;
using System.Collections.Generic;

public enum ItemType//文字通りアイテムの種類
{
    None,
    Circle,
    Square,
    Triangle,
    Star,
    Default
}

public class AbilityItemData
{
    public ItemObjectBehaviour ItemObject; 
    public IAbility Ability;                 
    public ItemType itemType;                
    public AbilityBehaviour Behaviour;       

    public AbilityItemData( //安全保障
        ItemObjectBehaviour itemObject,
        IAbility ability,
        AbilityBehaviour behaviour,
        ItemType itemType)
    {
        if (itemObject == null) throw new ArgumentNullException();
        if (ability == null) throw new ArgumentNullException();
        if (behaviour == null) throw new ArgumentNullException();
        if (itemType == ItemType.None) throw new ArgumentException();

        this.ItemObject = itemObject;
        this.Ability = ability;
        this.Behaviour = behaviour;
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

public class AbilityManager
{
    private AbilityDataBase abilityDataBase;

    private ItemRemover removeItem;
    private AbilityActivator abilityActivator;

    private BehaviourData behaviourData;
    private BehaviourManager behaviourManager;

    public AbilityManager(
        AbilityDataBase dataBase,
        BehaviourExecutor behaviourExecutor,
        Transform tf,
        StateWatcher stateWatcher,
        InputResolver resolver)
    {
        this.abilityDataBase = dataBase;

        behaviourData = new BehaviourData();
        behaviourManager = new BehaviourManager(
                    behaviourData, 
                    behaviourExecutor,
                    stateWatcher);

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

public class AbilityActivator
{
    private readonly InputResolver resolver;
    private readonly BehaviourData behaviourData;

    private readonly List<AbilityItemData> activeItems = new();
    private readonly Dictionary<Type, List<AbilityItemData>> abilitiesByType = new();

    public AbilityActivator(
            InputResolver resolver, 
            BehaviourData behaviourData) 
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

        behaviourData.AddAbilityData(itemData.Behaviour);//Behaviour側の管理に登録
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

        behaviourData.RemoveAbilityData(itemData.Behaviour);//Behaviourから削除
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

public class ItemRemover
{
    private Transform tf;
    private BehaviourData behaviourData;

    public ItemRemover(
            Transform tf, 
            BehaviourData behaviourData)
    {
        this.tf = tf;
        this.behaviourData = behaviourData;
    }

    public void Remove(AbilityItemData itemData)
    {
        behaviourData.RemoveAbilityData(itemData.Behaviour);

        itemData.ItemObject.Restoration(tf.position);
    }
}
#endregion
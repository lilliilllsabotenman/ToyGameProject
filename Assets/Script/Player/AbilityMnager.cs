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
/// アビリティ管理を管轄するデータクラス。
/// MonoBehaviour実行タイミングによって継承されるinterfaceが異なるためそれを利用してリストを選別。実行タイミングごとに整理する。
/// </summary>
public class AbilityManager
{
    public Dictionary<ItemType, AbilityItemSlot> MasterAbilities = new Dictionary<ItemType, AbilityItemSlot>();//アイテムスロット
    
    private ItemRemover removeItem;
    private AbilityActivater abilityActivater;
    
    //適応されるUnityライフサイクルでリストを分ける。アイテムスロットが更新されたら振り分けるって形式だからあんま重くないはず
    public List<OnEventAbility> EventAbilities = new List<OnEventAbility>();
    public List<OnUpdateAbility> UpdateAbilities = new List<OnUpdateAbility>();
    public List<OnFixedUpdateAbility> FixedUpdateAbilities = new List<OnFixedUpdateAbility>();

    public bool TryAddAbility(AbilityItemSlot iAbility)
    {
        // すでに同じタイプの Ability があるかチェック
        if (MasterAbilities.TryGetValue(iAbility.itemType, out var ability)) 
        {
            RemoveItem(ability);

            iAbility.Ability.SetActive();

            MasterAbilities.Remove(iAbility.itemType);
            MasterAbilities[iAbility.itemType] = iAbility;

            abilityActivater.SetLevel(iAbility);

            return true;
        }

        iAbility.Ability.SetActive();

        MasterAbilities[iAbility.itemType] = iAbility;

        abilityActivater.SetLevel(iAbility);

        
        return true;
    }

    public void RemoveItem(AbilityItemSlot a)
    {
        MasterAbilities.Remove(a.itemType);
        removeItem.Remove(a);
        abilityActivater.DeletItemLevel(a);
    }

    public AbilityManager(ItemRemover removeItem)
    {
        this.removeItem = removeItem;
        this.abilityActivater = new AbilityActivater(
            MasterAbilities,
            EventAbilities,
            UpdateAbilities,
            FixedUpdateAbilities
        );
    }
}

public class AbilityActivater
{
    private readonly Dictionary<ItemType, AbilityItemSlot> masterAbilities;
    private readonly List<OnEventAbility> eventAbilities;
    private readonly List<OnUpdateAbility> updateAbilities;
    private readonly List<OnFixedUpdateAbility> fixedUpdateAbilities;
    private Dictionary<Type, List<AbilityItemSlot>> abilitiesByType = new Dictionary<Type, List<AbilityItemSlot>>();

    public AbilityActivater(
        Dictionary<ItemType, AbilityItemSlot> masterAbilities,
        List<OnEventAbility> eventAbilities,
        List<OnUpdateAbility> updateAbilities,
        List<OnFixedUpdateAbility> fixedUpdateAbilities
    )
    {
        this.masterAbilities = masterAbilities;
        this.eventAbilities = eventAbilities;
        this.updateAbilities = updateAbilities;
        this.fixedUpdateAbilities = fixedUpdateAbilities;
    }

    public void AbilityAssignment()
    {
        eventAbilities.Clear();
        updateAbilities.Clear();
        fixedUpdateAbilities.Clear();

        foreach (AbilityItemSlot iAbility in masterAbilities.Values)
        {
            if(iAbility.Ability is OnEventAbility e) eventAbilities.Add(e); 
            if(iAbility.Ability is OnUpdateAbility u) updateAbilities.Add(u);
            if(iAbility.Ability is OnFixedUpdateAbility f) fixedUpdateAbilities.Add(f);
        }
    }

    public void SetLevel(AbilityItemSlot iAbility)
    {
        Type type = iAbility.Ability.GetType();

        if (!abilitiesByType.ContainsKey(type))
        {
            abilitiesByType[type] = new List<AbilityItemSlot>();
        }

        abilitiesByType[type].Add(iAbility);

        int count = abilitiesByType[type].Count;

        foreach (var slot in abilitiesByType[type])
        {
            slot.Ability.SetLevel(count);
        }
        
        AbilityAssignment();
    }

    public void DeletItemLevel(AbilityItemSlot iAbility)
    {
        Type type = iAbility.Ability.GetType();

        abilitiesByType[type].Remove(iAbility);

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

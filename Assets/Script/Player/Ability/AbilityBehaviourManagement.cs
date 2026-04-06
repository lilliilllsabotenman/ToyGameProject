using UnityEngine;
using System.Collections.Generic;

public enum BehaviourType
{
    Event,
    Update,
    Pasiv
}

public class BehaviourData
{
    public BehaviourType behaviourType;
    public AbilityBehaviour abilityBehaviour;
}

public class BehaviourManager
{
    private Dictionary<ItemType, BehaviourData> BehaviourMasterData = new();
}
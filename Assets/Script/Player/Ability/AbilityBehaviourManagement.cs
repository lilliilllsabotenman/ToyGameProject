using UnityEngine;
using System.Collections.Generic;

public enum BehaviourType
{
    Event,
    Update,
    Pasiv
}

public class BehaviourManager
{
    private AbilityDataBase abilityDataBase;

    private List<AbilityBehaviour> behaviourList = new();
    private Dictionary<MovementState, AbilityBehaviour> movementBehaviour = new();
    private Dictionary<PositioningState, AbilityBehaviour> positioningBehaviour = new();
    private Dictionary<PostureState, AbilityBehaviour> postureBehaviour = new();

    public BehaviourManager(AbilityDataBase abilityDataBase)
    {
        this.abilityDataBase = abilityDataBase;
    }

    public void onUpdate()
    {
    }
}
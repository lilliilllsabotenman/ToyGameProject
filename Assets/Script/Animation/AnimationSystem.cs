using UnityEngine;
using System.Collections.Generic;
using System;

public class AniamtionDataBase
{
    public Dictionary<MovementState, AnimationData> movementTable;
    public Dictionary<PostureState, AnimationData> postureTable;
    public Dictionary<PositioningState, AnimationData> positioningTable;
    public Dictionary<ParametorID, AnimationDataFloat> floatTable;

    public AniamtionDataBase(AnimationMapings mappings)
    {
        movementTable = CreateTable(mappings.MovementState);

        postureTable = CreateTable(mappings.PostureState);

        positioningTable = CreateTable(mappings.PositioningState);

        floatTable = CreateTableFloat(mappings.FloatParametorList);
    }

    private Dictionary<T, AnimationData> CreateTable<T> (List<AnimationSettings<T>> settings)
    {
        Dictionary<T, AnimationData> table = new();

        foreach (var setting in settings)
        {
            if (setting.data == null)
            {
                continue;
            }

            if (table.ContainsKey(setting.ID))
            {
                Debug.LogWarning($"Duplicate Key : {setting.ID}");

                continue;
            }

            table.Add(setting.ID, setting.data);
        }

        return table;
    }

    private Dictionary<ParametorID, AnimationDataFloat> CreateTableFloat (List<AnimationDataFloat> settings)
    {
        Dictionary<ParametorID, AnimationDataFloat> table = new();

        foreach (var setting in settings)
        {
            if (setting == null)
            {
                continue;
            }

            if (table.ContainsKey(setting.ID))
            {
                Debug.LogWarning($"Duplicate Key : {setting.ID}");

                continue;
            }

            table.Add(setting.ID, setting);
        }

        return table;
    }
}

public class AnimationSystem : MonoBehaviour
{ 
    [SerializeField] private GameObject Player;
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerObserver observer;
    [SerializeField] private AnimationMapings animationMappings;

    private AniamtionDataBase animationDataBase;
    // private List<>

    private bool isEventAnimation = false;

    public void Start()
    {   
        StateWatcher stateWatcher = Player.GetComponent<PlayerController>().GetStateWatcher();

        observer = new PlayerObserver(Player);
        animationDataBase = new AniamtionDataBase(animationMappings);

        stateWatcher.Subscribe<MovementState>(OnStateChanged);
        stateWatcher.Subscribe<PostureState>(OnStateChanged);
        stateWatcher.Subscribe<PositioningState>(OnStateChanged);
    }

    private void OnStateChanged<T>(T state) where T : Enum
    {
    }

    private void Update()
    {
        // OnUpdate();
        if(isEventAnimation) return;
    }
}

public class AnimationExecutor
{
    private Animator animator;

    public void OnUpdate()
    {
        // foreach (ParametorID ID in Enum.GetValues(typeof(ParametorID)))
        // {    
        //     if (animationDataBase.floatTable.TryGetValue(ID, out var data) && data != null)
        //     {
        //         if(string.IsNullOrEmpty(data.AnimParamName)) return;
        //         animator.SetFloat(data.AnimParamName, observer.GetParametor(ID));
        //     }
        // }
    }
}
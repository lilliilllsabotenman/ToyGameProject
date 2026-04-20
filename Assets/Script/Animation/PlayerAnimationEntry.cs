using UnityEngine;
using System;
using System.Collections.Generic;
using AnimationParametorObserver;

public class PlayerAnimationEntry : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationParametorDataBase dataBase;

    private AnimationDataBase animationDataBase;
    private AnimationManager animationManager;
    private AnimationExecutor animationExecutor;

    void Start()
    {
        PlayerController playerController = player.GetComponent<PlayerController>();

        animationDataBase = new AnimationDataBase(dataBase, animator);

        animationExecutor = new AnimationExecutor(
            animator,
            fParamInit()
        );

        animationManager = new AnimationManager(
            playerController.stateWatcher,
            animationDataBase,
            animationExecutor
        );
    }

    private Dictionary<string, AnimationParametorModify> fParamInit()
    {
        BoxCollider col = player.GetComponent<BoxCollider>();

        SpeedModify speed = new (player.GetComponent<Rigidbody>());
        CrochModify croch = new (
            col,
            col.bounds.size.y,
            col.bounds.size.y / 10
            );

        return new Dictionary<string, AnimationParametorModify>{
            {"Speed", speed},
            {"CrouchLevel", croch}
        };
    }

    private void Update()
    {
        if(animationExecutor == null) return;
        animationExecutor.onUpdate();
    }
}

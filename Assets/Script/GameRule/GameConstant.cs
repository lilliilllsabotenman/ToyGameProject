using UnityEngine;
using System;

[System.Serializable]
public class GameConstant 
{
    [Header("歩く速さ")]
    public float moveSpeed;

    [Header("ジャンプの高さ")]
    public float jumpForce;
}

public class ConstansModify
{
    public float DashModify {get; private set;} = 2;
    public float JumpForceModify {get; private set;} = 2;

    public void SetDashModify(float modify)
    {
        DashModify = modify;
    }

    public void SetJumpModify(float modify)
    {
        JumpForceModify = modify;
    }
}

public class GameConstantParametor
{
    private GameConstant gameConstant;
    private ConstansModify constansModify;
    private PlayerStateManager playerStateManager;

    public GameConstantParametor(
        GameConstant gameConstant,
        ConstansModify constansModify,
        PlayerStateManager playerStateManager)
    {
        this.gameConstant = gameConstant;
        this.constansModify = constansModify;
        this.playerStateManager = playerStateManager;
    }

    public float GetMoveSpeed()
    {
        if(playerStateManager.stateData.movementState == MovementState.Dash) return gameConstant.moveSpeed * constansModify.DashModify; 
        else return gameConstant.moveSpeed;
    }

    public float GetJumpForce()
    {
        return gameConstant.jumpForce;
    }
}

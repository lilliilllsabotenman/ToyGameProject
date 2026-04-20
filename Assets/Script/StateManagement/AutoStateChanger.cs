using UnityEngine;

#region 
public class AutoStateChanged
{
    private LayerMask groundLayer;
    private PlayerStateManager playerState;

    public AutoStateChanged(PlayerStateManager playerState, LayerMask groundLayer)
    {
        this.groundLayer = groundLayer;
        this.playerState = playerState;
    }

    public bool CheckLanding(Collision collision)
    {
        Debug.Log("Collision");
        // レイヤー判定
        if((groundLayer.value & (1 << collision.gameObject.layer)) == 0)
            return false;

        // 接触面の法線チェック（上向き面のみ）
        foreach(ContactPoint contact in collision.contacts)
        {
            if(contact.normal.y > 0.5f)
                return playerState.positioningChanged(PositioningState.Ground);
        }

        return false;
    }
}
#endregion
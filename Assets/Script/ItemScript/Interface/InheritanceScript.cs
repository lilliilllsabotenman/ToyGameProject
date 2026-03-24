using UnityEngine;

public class ItemObjectBehaviour : MonoBehaviour
{
    protected Rigidbody rb;
    public ActionType iAction;

    public virtual AbilityItemSlot GetAbility(GameObject obj)
    {
        return null;
    }

    public void SuccessJoinItem()
    {
        this.gameObject.SetActive(false);
    }

    public void Restoration(Vector3 pos)
    {
        this.transform.position = new Vector3(pos.x, pos.y + 2, pos.z);
        this.gameObject.SetActive(true);

        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            7,
            rb.linearVelocity.z
        );
    }
}

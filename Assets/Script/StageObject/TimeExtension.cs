using UnityEngine;

public class TimeExtension : MonoBehaviour
{
    [SerializeField] private string objectTag;

    private bool isFirest = true; //時間延長一回だけならこれでいいや

    public void Init()
    {
        //初期化。イベント登録
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag(objectTag));
            Debug.Log("時間延長");
    }
}

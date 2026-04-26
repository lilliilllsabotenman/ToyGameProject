using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSEController : MonoBehaviour
{
    public AudioClip jumpClip;
    public AudioClip landClip;
    public AudioClip dashClip;
    public AudioClip crouchClip;
    public AudioClip itemGetClip;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // ジャンプ
    public void PlayJumpSE()
    {
        PlaySE(jumpClip);
    }

    //着地
    public void PlayLandSE()
    {
        PlaySE(landClip);
    }

    // ダッシュ
    public void PlayDashSE()
    {
        PlaySE(dashClip);
    }

    // しゃがみ
    public void PlayCrouchSE()
    {
        PlaySE(crouchClip);
    }

    // アイテム取得
    public void PlayItemGetSE()
    {
        PlaySE(itemGetClip);
    }

    private void PlaySE(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}

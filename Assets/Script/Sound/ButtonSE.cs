using UnityEngine;

public class ButtonSE : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip decideSE;
    public AudioClip cancelSE;

    public void PlayDecide()
    {
        audioSource.PlayOneShot(decideSE);
    }

    public void PlayCancel()
    {
        audioSource.PlayOneShot(cancelSE);
    }
}

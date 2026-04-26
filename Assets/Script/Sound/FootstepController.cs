using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class FootstepController : MonoBehaviour
{
    [Header("Footstep Sounds")]
    public AudioClip woodClip;
    public AudioClip clothClip;
    public AudioClip defaultClip;

    [Header("Movement")]
    public float moveThreshold = 0.1f;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.3f;
    public LayerMask groundLayer;
    public Vector3 groundCheckOffset = new Vector3(0, 0.1f, 0);

    [Header("Ground Angle")]
    [Range(0f, 1f)]
    public float groundNormalThreshold = 0.6f; // ←追加（重要）

    [Header("Ground Layers")]
    public LayerMask woodLayer;
    public LayerMask clothLayer;

    private AudioSource audioSource;
    private Rigidbody rb;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();

        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        Vector3 origin = transform.position + groundCheckOffset;

        RaycastHit hit;
        bool isGrounded =
            Physics.Raycast(origin, Vector3.down, out hit, groundCheckDistance, groundLayer)
            && hit.normal.y > groundNormalThreshold;

        // 水平速度
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float speed = horizontalVelocity.magnitude;

        if (isGrounded && speed > moveThreshold)
        {
            AudioClip targetClip = GetFootstepClip(hit.collider.gameObject.layer);

            if (audioSource.clip != targetClip)
            {
                audioSource.clip = targetClip;
                audioSource.Play();
            }
            else if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    AudioClip GetFootstepClip(int layer)
    {
        if (((1 << layer) & woodLayer) != 0)
            return woodClip;

        if (((1 << layer) & clothLayer) != 0)
            return clothClip;

        return defaultClip;
    }
}

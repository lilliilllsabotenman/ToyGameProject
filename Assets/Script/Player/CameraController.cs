using UnityEngine;

public class CameraControlller : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Camera Settings")]
    [SerializeField] private float distance = 4f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 4f;
    [SerializeField] private float height = 1.5f;
    [SerializeField] private float crouchHeightOffset = -1f;

    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float smoothSpeed = 10f;

    [Header("Collision")]
    [SerializeField] private float sphereRadius = 0.25f;
    [SerializeField] private float wallOffset = 0.1f;
    [SerializeField] private LayerMask collisionMask;

    [Header("Clamp Settings")]
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;

    [Header("Easing")]
    [SerializeField] private float heightSmooth = 12f;

    private float yaw;
    private float pitch;

    private float currentDistance;
    private float currentHeightOffset;

    private PlayerStateManager stateManager;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        stateManager = target.GetComponent<PlayerController>().playerStateManager;

        currentDistance = distance;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleCameraRotation();
        HandleCameraPosition();
    }

    private void HandleCameraRotation()
    {
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
    }

    private void HandleCameraPosition()
    {
        Vector3 targetPos = target.position + Vector3.up * height;

        targetPos.y -= 0.5f;

        float targetOffset = 0f;
        if (stateManager.stateData.postureState == PostureState.Crouch)
            targetOffset = crouchHeightOffset;

        currentHeightOffset = Mathf.Lerp(
            currentHeightOffset,
            targetOffset,
            Time.deltaTime * heightSmooth);

        targetPos.y += currentHeightOffset;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 direction = rotation * Vector3.back;

        float targetDistance = maxDistance;

        // ① Pivot → Camera
        if (Physics.SphereCast(
            targetPos,
            sphereRadius,
            direction,
            out RaycastHit hit,
            maxDistance,
            collisionMask,
            QueryTriggerInteraction.Ignore))
        {
            targetDistance = hit.distance - wallOffset;
        }

        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

        Vector3 desiredPos = targetPos + direction * targetDistance;

        // ② Camera → Pivot (壁内部対策)
        if (Physics.Raycast(
            desiredPos,
            -direction,
            out RaycastHit backHit,
            targetDistance,
            collisionMask,
            QueryTriggerInteraction.Ignore))
        {
            targetDistance = backHit.distance - wallOffset;
        }

        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

        // ③ 距離を滑らかに補間
        currentDistance = Mathf.Lerp(
            currentDistance,
            targetDistance,
            Time.deltaTime * smoothSpeed);

        transform.position = targetPos + direction * currentDistance;
        transform.LookAt(targetPos);
    }
    }

/// <summary>
/// 画面中央付近に映っているオブジェクトを、SphereCast を使って検出するクラス。
/// 「画面中央から一定範囲内の複数点」へレイを飛ばし、
/// 最も近いヒット対象を返す。
/// </summary>
[System.Serializable]
public class ScreenCenterDetector
{
    [Header("レイを飛ばす最大距離（これ以上先は検出しない）")]
    [SerializeField] private float maxDistance;

    [Header("SphereCast の半径（レイを太くして、多少ズレても当たりやすくする）")]
    [SerializeField] private float sphereRadius;

    [Header("画面中央からどれだけ離れた点までチェックするか（ピクセル単位）")]
    [SerializeField] private float screenRange;

    [Header("検出対象とする LayerMask")]
    [SerializeField] private LayerMask targetLayer;
    
    [Header("メインカメラ")]
    [SerializeField] private Camera cam;
    /// <summary>
    /// 画面中央の一定範囲に映っているオブジェクトを取得する。
    /// ・画面中央 + 上下左右の計5点から SphereCast を飛ばす
    /// ・最も近くでヒットしたオブジェサイクルクトを返す
    /// </summary>cycle
    public GameObject GetTarget(Camera cam)
    {
        // 未設定なら MainCamera をフォールバックで使う
        if (this.cam == null) this.cam = cam;
        if (this.cam == null) return null;

        // 画面中央の座標（ピクセル）
        float cx = Screen.width * 0.5f;
        float cy = Screen.height * 0.5f;

        // チェックするスクリーン座標のセット
        // 中央 + 上下左右の5点
        Vector2[] points =
        {
            new(cx, cy),                    // 中央
            new(cx + screenRange, cy),      // 右
            new(cx - screenRange, cy),      // 左
            new(cx, cy + screenRange),      // 上
            new(cx, cy - screenRange)       // 下
        };

        GameObject nearest = null;
        float minDist = float.MaxValue;

        foreach (var p in points)
        {
            // スクリーン座標からワールド空間へレイを生成
            Ray ray = cam.ScreenPointToRay(p);

            // SphereCast で「太いレイ」を飛ばす
            // → 細い Raycast よりも狙いやすく、多少ズレても当たる
            if (Physics.SphereCast(ray, sphereRadius, out RaycastHit hit, maxDistance, targetLayer))
            {
                float dist = hit.distance;

                // 最も近いヒット対象を記録
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = hit.collider.gameObject;
                }
            }
        }

        // 最も近かったオブジェクト（または null）
        return nearest;
    }
}

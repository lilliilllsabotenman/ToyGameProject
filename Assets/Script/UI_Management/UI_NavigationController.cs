using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ワールド上のターゲットをスクリーンUIとして表示する
/// ・対象Transformをインスペクターで指定
/// ・表示するUI(Image)もインスペクターで指定
/// ・画面内はその位置に表示、画面外は端にクランプ
/// ・カメラ背面にも対応（反転）
/// </summary>
public class UI_NavigationController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;   // 追従対象（ワールド座標）

    [Header("UI")]
    [SerializeField] private RectTransform ui;   // 表示するUI（ImageのRectTransform）
    [SerializeField] private Image uiImage;      // 見た目（任意）

    [Header("Settings")]
    [SerializeField] private Camera targetCamera; // 基準カメラ（未指定ならMainCamera）
    [SerializeField] private bool clampToScreen = true; // 画面外なら端に固定
    [SerializeField] private float screenMargin = 32f;  // 端に寄せすぎない余白(px)

    private Camera cam;

    void Awake()
    {
        // カメラ決定（未指定ならMainCamera）
        cam = targetCamera != null ? targetCamera : Camera.main;
    }

    void LateUpdate()
    {
        // 安全チェック
        if (target == null || ui == null || cam == null) return;

        Debug.Log("this is not Null");

        // --- 1) ワールド → スクリーン座標に変換 ---
        Vector3 sp = cam.WorldToScreenPoint(target.position);

        // --- 2) 背面補正 ---
        // カメラの後ろにある場合、単純に使うとUIが暴れるため反転させる
        if (sp.z < 0f)
        {
            sp *= -1f;
        }

        // --- 3) 画面内/外の判定（Viewportで行うと扱いやすい） ---
        Vector3 vp = cam.WorldToViewportPoint(target.position);
        bool isOffscreen = vp.x < 0f || vp.x > 1f || vp.y < 0f || vp.y > 1f || vp.z < 0f;

        Vector3 finalScreenPos;

        if (clampToScreen && isOffscreen)
        {
            // --- 4) 画面端にクランプ ---
            // 0〜1に収めてからピクセルへ
            vp.x = Mathf.Clamp01(vp.x);
            vp.y = Mathf.Clamp01(vp.y);

            float x = vp.x * Screen.width;
            float y = vp.y * Screen.height;

            // 端に寄りすぎないよう余白を入れる
            x = Mathf.Clamp(x, screenMargin, Screen.width - screenMargin);
            y = Mathf.Clamp(y, screenMargin, Screen.height - screenMargin);

            finalScreenPos = new Vector3(x, y, 0f);
        }
        else
        {
            // 画面内ならそのまま
            finalScreenPos = new Vector3(sp.x, sp.y, 0f);
        }

        // --- 5) UIに適用 ---
        // Overlay Canvas前提：そのままpositionに入れてOK
        ui.position = finalScreenPos;

        // --- 6) 回転（任意：方向を示したい場合） ---
        // 画面中心からの方向で矢印を回す
        Vector2 centerOffset = new Vector2(
            (finalScreenPos.x / Screen.width) - 0.5f,
            (finalScreenPos.y / Screen.height) - 0.5f
        );

        float angle = Mathf.Atan2(centerOffset.y, centerOffset.x) * Mathf.Rad2Deg;
        ui.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
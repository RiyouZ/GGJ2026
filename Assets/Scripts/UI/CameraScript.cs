using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 引用 UI 命名空间

public class CameraScript : MonoBehaviour
{
    [Header("UI 设置")]
    public Button startButton;        // 拖入你的开始按钮

    [Header("核心目标设置")]
    public float finalSize = 5.0f;    // 最终大小 (Size = 5)
    public float startZoomScale = 1.5f; // 起始放大倍数 (150%)

    [Header("动画手感")]
    public float revealSpeed = 0.8f;  // 拉远速度
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("呼吸摇摆")]
    public float swayAmount = 0.15f;
    public float swaySpeed = 0.4f;
    [Range(0, 1)]
    public float verticalBias = 0.8f;

    // 内部状态
    private Camera cam;
    private Vector3 originalPos;
    private float progress = 0f;
    private bool isStarted = false; // 【关键】一开始是 false，不动

    void Start()
    {
        cam = GetComponent<Camera>();
        originalPos = transform.position;

        // --- 1. 初始定格 ---
        // 强制先把摄像机按在 150% 的特写状态
        float startSize = finalSize / startZoomScale;
        cam.orthographicSize = startSize;

        // --- 2. 绑定按钮 ---
        if (startButton != null)
        {
            startButton.onClick.AddListener(PlayIntro);
        }
    }

    // 点击按钮执行这个
    public void PlayIntro()
    {
        isStarted = true; // 打开开关，Update 里才开始动

        // 可选：点击后隐藏按钮
        // startButton.gameObject.SetActive(false); 
    }

    void Update()
    {
        // 如果还没点按钮，直接跳过，保持静止
        if (!isStarted) return;

        // --- 下面是动画逻辑 ---

        // A. 呼吸摇摆 (Perlin Noise)
        float noiseX = Mathf.PerlinNoise(Time.time * swaySpeed, 0f) - 0.5f;
        float noiseY = Mathf.PerlinNoise(0f, Time.time * swaySpeed) - 0.5f;

        Vector3 swayOffset = new Vector3(
            noiseX * swayAmount * (1 - verticalBias),
            noiseY * swayAmount * verticalBias,
            0
        );

        // B. 缩放动画 (Zoom Out)
        // 只有当进度小于 1 时才计算缩放，播完了就只保留摇摆
        if (progress < 1.0f)
        {
            progress += Time.deltaTime * revealSpeed;
            float t = easeCurve.Evaluate(progress);

            // 从 (3.33) -> (5.0)
            float startSize = finalSize / startZoomScale;
            cam.orthographicSize = Mathf.Lerp(startSize, finalSize, t);
        }
        else
        {
            // 确保最后数值精确
            cam.orthographicSize = finalSize;
        }

        // C. 应用位置
        transform.position = originalPos + swayOffset;
    }
}
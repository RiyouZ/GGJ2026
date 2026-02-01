using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(LineRenderer))]
public class LineRenderScript : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;

    [Header("虚线动画")]
    public float dashScale = 2.0f;   // 虚线密度
    public float scrollSpeed = 2.0f; // 流动速度 (确保这个不是0)

    [Header("基本设置")]
    public int segmentCount = 50;
    public float curveHeight = 1.0f;
    public float lineWidth = 0.1f;
    public Color lineColor = Color.white;

    [Header("防遮挡")]
    public float zOffset = -1.0f;
    public int sortOrder = 32767;

    private LineRenderer lineRenderer;
    private Material dashMaterial;

    public void DrawLine(Transform start, Transform end)
    {
        this.gameObject.SetActive(true);
        startPoint = start;
        endPoint = end;
    }

    public void ClearLine()
    {
        this.gameObject.SetActive(false);
    }

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // --- 1. Shader 强力修复 ---
        // Sprites/Default 有时候不听话，我们优先找粒子 Shader，它们对 UV 动画支持最好
        Shader shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Mobile/Particles/Alpha Blended");
        if (shader == null) shader = Shader.Find("Sprites/Default"); // 最后的保底

        // 生成纹理
        Texture2D dashTexture = GenerateDashTexture();

        dashMaterial = new Material(shader);
        dashMaterial.mainTexture = dashTexture;

        // 确保材质渲染队列是最高的，防止被遮挡
        dashMaterial.renderQueue = 4000;

        lineRenderer.material = dashMaterial;

        // --- 2. 必须设置为 Tile (平铺) ---
        lineRenderer.textureMode = LineTextureMode.Tile;

        // 基础设置
        lineRenderer.positionCount = segmentCount + 1;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.useWorldSpace = true;
        lineRenderer.numCapVertices = 5;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
    }

    void Update()
    {
        if (startPoint == null || endPoint == null) return;

        DrawCurve();
        AnimateDash();
    }

    Texture2D GenerateDashTexture()
    {
        // 增加宽度到 64，让虚线更清晰
        int width = 64;
        Texture2D texture = new Texture2D(width, 1);
        texture.wrapMode = TextureWrapMode.Repeat; // 关键：必须是 Repeat 才能滚动
        texture.filterMode = FilterMode.Bilinear;

        for (int x = 0; x < width; x++)
        {
            // 70% 实心，30% 透明，调节这里的比例可以改变虚线的长短
            bool isDash = x < (width * 0.7f);
            Color color = isDash ? Color.white : Color.clear;
            texture.SetPixel(x, 0, color);
        }
        texture.Apply();
        return texture;
    }

    void AnimateDash()
    {
        if (lineRenderer == null || dashMaterial == null) return;

        // --- 3. 强制滚动逻辑 ---
        // 使用 Time.time 驱动偏移量
        // 负号表示向目标点流动，正号表示向起点流动
        float offset = (Time.time * -scrollSpeed) % 1000f;

        // 使用底层 ID 设置偏移，这比直接访问属性更稳定
        dashMaterial.SetTextureOffset("_MainTex", new Vector2(offset, 0));
    }

    void DrawCurve()
    {
        Vector3 p0 = startPoint.position;
        Vector3 p2 = endPoint.position + new Vector3(0f, 0.5f);

        p0.z = zOffset;
        p2.z = zOffset;

        float distance = Vector3.Distance(p0, p2);

        // 动态调整纹理缩放，确保虚线不会变形
        // 注意：这里也使用底层 ID 设置
        dashMaterial.SetTextureScale("_MainTex", new Vector2(distance * dashScale, 1));

        float dynamicHeight = Mathf.Clamp(distance * 0.3f, 0.5f, curveHeight);
        Vector3 mid = (p0 + p2) / 2f;
        Vector3 p1 = mid + Vector3.up * dynamicHeight;
        p1.z = zOffset;

        for (int i = 0; i <= segmentCount; i++)
        {
            float t = i / (float)segmentCount;
            Vector3 point = (1 - t) * (1 - t) * p0 +
                            2 * (1 - t) * t * p1 +
                            t * t * p2;

            point.z = zOffset;
            lineRenderer.SetPosition(i, point);
        }
    }
}
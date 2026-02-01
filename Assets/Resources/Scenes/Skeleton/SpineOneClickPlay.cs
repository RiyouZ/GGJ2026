using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 引用 UI 命名空间

public class a : MonoBehaviour
{
    [Header("设置")]
    public string introAnimationName = "intro"; // 你的开场动画名字
    public Button startButton;                  // 点击播放的按钮

    // 兼容两种 Spine 组件：世界物体的 Animation 或 UI 的 Graphic
    private SkeletonAnimation skeletonAnimation;
    private SkeletonGraphic skeletonGraphic;

    void Start()
    {
        // 自动查找组件（无论你是用 SkeletonAnimation 还是 SkeletonGraphic）
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        skeletonGraphic = GetComponent<SkeletonGraphic>();

        if (startButton != null)
        {
            startButton.onClick.AddListener(PlayIntro);
        }
    }

    public void PlayIntro()
    {
        // 核心逻辑：设置动画，loop = false (不循环)

        if (skeletonAnimation != null)
        {
            // 针对游戏世界里的物体
            skeletonAnimation.AnimationState.SetAnimation(0, introAnimationName, false);
        }
        else if (skeletonGraphic != null)
        {
            // 针对 UI 面板里的物体
            skeletonGraphic.AnimationState.SetAnimation(0, introAnimationName, false);
        }

        // 可选：点击后把按钮藏起来，防止重复点
        // startButton.gameObject.SetActive(false);
    }
}
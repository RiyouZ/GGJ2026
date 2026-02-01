using Frame.Audio;
using RuGameFramework.Event;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity; // 必须引用

public class UIScript : MonoBehaviour
{
    public GameObject SkillButton;
    public GameObject NextRoundButton;
    public GameObject StartButton;
    public GameObject BeforeGame;
    public GameObject InGame;

    [Header("拖入场景里的 Spine 物体")]
    public SkeletonAnimation spineObject; // 【变化点】类型变成了 SkeletonAnimation

    [Header("把你的按钮拖进来")]
    public Button playButton;

    [Header("填入动画名字")]
    public string animationName = "GameStart"; // 比如 "start", "animation" 等
    public string animationName2 = "GameStart"; // 比如 "start", "animation" 等

    private const string EVENT_NAME = "SceneGameEnd";
    // Start is called before the first frame update
    void Start()
    {
        // 游戏一开始，给按钮绑定点击事件
        if (playButton != null)
        {
            playButton.onClick.AddListener(StartGame);
        }
    }

    // Update is called once per frame
    void Update()
    {
        EventManager.AddListener(EVENT_NAME, (args) => EndGame());
    }

    public void Skill()
    {
        EventManager.InvokeEvent("ScenePlayerSkill", null);
        WwiseAudio.PlayEvent("Play_Doll_Skill_Cast_SFX", SkillButton);
    }
    public void NextRound()
    {
        EventManager.InvokeEvent("TurnPlayerActionComplete", null);
        WwiseAudio.PlayEvent("Play_Doll_Move_Prepare_Quick_SFX", NextRoundButton);
    }

    public void StartGame()
    {
        EventManager.InvokeEvent("SceneGameStart", null);
        WwiseAudio.PlayEvent("Play_SFX_Scene_Transition_Open", StartButton);
        BeforeGame.SetActive(false);
        Invoke("Waitit", 1.5f);


        if (spineObject == null) return;

        // --- 核心播放逻辑 ---
        // 参数1 (0): 轨道索引，通常用0
        // 参数2 (animationName): 你的动画名字
        // 参数3 (false): loop，是否循环？开场动画只播一次，所以是 false
        spineObject.AnimationState.SetAnimation(0, animationName, false);
    }
    public void Waitit() 
    { 
        InGame.SetActive(true);
    }

    public void EndGame()
    {
        // --- 核心播放逻辑 ---
        // 参数1 (0): 轨道索引，通常用0
        // 参数2 (animationName): 你的动画名字
        // 参数3 (false): loop，是否循环？开场动画只播一次，所以是 false
        spineObject.AnimationState.SetAnimation(0, animationName2, false);
        InGame.SetActive(false);
        Invoke("QuitGame", 1.5f);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
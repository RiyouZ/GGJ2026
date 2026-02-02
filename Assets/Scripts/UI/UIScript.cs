using Frame.Audio;
using RuGameFramework.Event;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Game;
using Game.Scene; // 必须引用

public class UIScript : MonoBehaviour
{
    public Button SkillButton;
    public Button NextRoundButton;
    public Button StartButton;
    public GameObject BeforeGame;
    public GameObject InGame;

    [Header("拖入场景里的 Spine 物体")]
    public SkeletonAnimation spineObject; // 【变化点】类型变成了 SkeletonAnimation

    [Header("把你的按钮拖进来")]
    public Button playButton;


    public const string ANIME_START = "start"; // 比如 "start", "animation" 等
    public const string ANIME_END = "end"; // 比如 "start", "animation" 等

    // Start is called before the first frame update
    void Start()
    {
        // 游戏一开始，给按钮绑定点击事件
        if (playButton != null)
        {
            playButton.onClick.AddListener(StartGame);
        }

        if( NextRoundButton != null)
        {
            NextRoundButton.onClick.AddListener(NextRound);
        }

        EventManager.AddListener(GameScene.EVENT_ROUND_COMP, (args) => HideNextRoundButton());
        EventManager.AddListener(GameScene.EVENT_GAME_END, (args) => EndGame());

        EventManager.AddListener(MouseInteractSystem.EVENT_SKILL_CANCEL, (args) => SkillOn());
        // 监听技能真正应用的事件，避免与技能切换事件混淆
        EventManager.AddListener(MouseInteractSystem.EVENT_SKILL_APPLIED, (args) => SkillOff());

        WwiseAudio.PlayEvent("Play_Menu_Level_Music", this.gameObject);

    }

    public void SkillOn()
    {
        SkillButton.transform.GetChild(0).gameObject.SetActive(value: false);
    }

    public void SkillOff()
    {
        SkillButton.transform.GetChild(0).gameObject.SetActive(true);
    }

    public void Skill()
    {
        EventManager.InvokeEvent(GameScene.EVENT_SCENE_PLAYER_SKILL_USED, null);
        WwiseAudio.PlayEvent("Play_Crown_Glow_SFX", this.gameObject);
    }
    public void NextRound()
    {
        EventManager.InvokeEvent(TurnSystem.EVENT_PLAYER_ACTION_COMPLETE, null);
        NextRoundButton.transform.GetChild(1).gameObject.SetActive(true);
    }

    public void HideNextRoundButton()
    {
        NextRoundButton.transform.GetChild(1).gameObject.SetActive(false);
    }

    public void StartGame()
    {
        EventManager.InvokeEvent("SceneGameStart", null);
        WwiseAudio.PlayEvent("Stop_SFX_Scene_Transition_Close", this.gameObject);
        WwiseAudio.PlayEvent("Play_Level_1", this.gameObject);
        BeforeGame.gameObject.SetActive(false);
        Invoke("Wait", 1.5f);


        if (spineObject == null) return;

        // --- 核心播放逻辑 ---
        // 参数1 (0): 轨道索引，通常用0
        // 参数2 (animationName): 你的动画名字
        // 参数3 (false): loop，是否循环？开场动画只播一次，所以是 false
        spineObject.AnimationState.SetAnimation(0, ANIME_START, false);
    }
    public void Wait() 
    {   
        InGame.gameObject.SetActive(true);
    }

    public void EndGame()
    {
        // --- 核心播放逻辑 ---
        // 参数1 (0): 轨道索引，通常用0
        // 参数2 (animationName): 你的动画名字
        // 参数3 (false): loop，是否循环？开场动画只播一次，所以是 false
        spineObject.AnimationState.SetAnimation(0, ANIME_END, false);
        InGame.gameObject.SetActive(false);
        WwiseAudio.PlayEvent("Play_SFX_Scene_Transition_Close", this.gameObject);
        Invoke("QuitGame", 6f);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
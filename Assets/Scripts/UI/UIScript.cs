using RuGameFramework.Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Skill()
    {
        EventManager.InvokeEvent("ScenePlayerSkill", null);
    }
    public void NextRound()
    {
        EventManager.InvokeEvent("TurnPlayerActionComplete", null);
    }

    public void StartGame()
    {
        EventManager.InvokeEvent("SceneGameStart", null);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

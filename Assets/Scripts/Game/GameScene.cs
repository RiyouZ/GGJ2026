using System;
using System.Collections;
using System.Collections.Generic;
using Frame.Audio;
using Frame.Core;
using UnityEngine;

namespace Game
{
    public class GameScene : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            WwiseAudio.LoadBank("Main");
        }

        void Update()
        {
           SystemOrderServer.UpdateSystemServer(Time.deltaTime);
        }

        void FixedUpdate()
        {
            SystemOrderServer.FixUpdateSystemServer(Time.fixedDeltaTime);
        }

        void LateUpdate()
        {
            SystemOrderServer.LateUpdateSystemServer(Time.deltaTime);
        }

        void OnDestroy()
        {
            SystemOrderServer.Dispose();
        }
    }

}

﻿using AE_SkillEditor_Plus.RunTime;
using UnityEngine;

namespace AE_SkillEditor_Plus.RunTime.Driver
{
    public class AEPlayableBehavior
    {
        public AEPlayableStateEnum State { get; protected set; }

        public AEPlayableBehavior(StandardClip clip)
        {
        }

        public void OnEnter()
        {
            State = AEPlayableStateEnum.Running;
            // Debug.LogWarning("OnEnter");
        }

        public void Tick(int currentFrameID, int fps)
        {
            if (State != AEPlayableStateEnum.Running) return;
            // Debug.Log("OnUpdate  "  + currentFrameID);
        }

        public void OnExit()
        {
            State = AEPlayableStateEnum.Exit;
            // Debug.LogWarning("OnExit");
        }
    }
}
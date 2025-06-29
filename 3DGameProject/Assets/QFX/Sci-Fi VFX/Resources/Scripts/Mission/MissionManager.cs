using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public enum MissionState { NotStarted, InProgress, Completed}

    public MissionState state = MissionState.NotStarted;

    public void StartMission()
    {
        state = MissionState.InProgress;
        //미션 시작 로직
    }

    public void CompleteMission()
    {
        state = MissionState.Completed;
        // 보상 지급, 다음 미션 등
    }
}

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
        //�̼� ���� ����
    }

    public void CompleteMission()
    {
        state = MissionState.Completed;
        // ���� ����, ���� �̼� ��
    }
}

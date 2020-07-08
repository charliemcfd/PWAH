using UnityEngine;
using UnityEngine.Events;

using System.Collections;
using System.Collections.Generic;

public class s_EventManager : MonoBehaviour {


    private Dictionary<string, UnityEvent> m_eventDictionary;
    private static s_EventManager g_EventManager;

    //Event members

    //Player Respawn Events
    public static evt_SpawnTubeResetEvent SpawnTubeResetEvent = new evt_SpawnTubeResetEvent();
    public static evt_SpawnTubeOpenEvent SpawnTubeOpenEvent = new evt_SpawnTubeOpenEvent();

    //Level complete events
    public static evt_LevelCompleteEvent LevelCompleteEvent = new evt_LevelCompleteEvent();

    //Scene Load events
    public static evt_SceneLoadedEvent SceneLoadedEvent = new evt_SceneLoadedEvent();

    //Camera Events
    public static evt_CameraSetPosEvent CameraSetPosEvent = new evt_CameraSetPosEvent();
	public static evt_CameraSetTargetObjectEvent CameraSetTargetObjectEvent = new evt_CameraSetTargetObjectEvent();
	public static evt_CameraResetToSpawnPosition CameraResetToSpawnPositionEvent = new evt_CameraResetToSpawnPosition();



	public static s_EventManager instance
    {
        get
        {
            if (!g_EventManager)
            {
                g_EventManager = FindObjectOfType(typeof(s_EventManager)) as s_EventManager;

                if (!g_EventManager)
                {
                    Debug.LogError("There needs to be one active EventManger script on a GameObject in your scene.");
                }
                else
                {
                    g_EventManager.Init();
                }
            }

            return g_EventManager;
        }
    }

    void Init()
    {
        DontDestroyOnLoad(this);
    }

}

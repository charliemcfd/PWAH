using UnityEngine;
using UnityEngine.Events;

using System.Collections;

public class TestEvent : UnityEvent<string> { }

/// <summary>
/// Player Respawn Events
/// </summary>
/// 

public class evt_SpawnTubeResetEvent : UnityEvent<float> { } // float AnimationTimer
public class evt_SpawnTubeOpenEvent : UnityEvent { }

/// <summary>
/// Level Completion events
/// </summary>
public class evt_LevelCompleteEvent : UnityEvent { }

/// <summary>
/// Scene Load events
/// </summary>

public class evt_SceneLoadedEvent : UnityEvent { }

/// <summary>
/// Camera Events
/// </summary>
/// 
public class evt_CameraSetPosEvent : UnityEvent<Vector2> { } //Vector2 Position

public class evt_CameraSetTargetObjectEvent : UnityEvent<GameObject> { }

public class evt_CameraResetToSpawnPosition : UnityEvent { }
/// <summary>
/// Collision Trigger events
/// </summary>
/// 



public class s_EventDefinitions{}

using UnityEngine;
using UnityEngine.Events;

using System.Collections;

public class TestEvent : UnityEvent<string> { }

#region Player Respawn Events
/// <summary>
/// Player Respawn Events
/// </summary>
/// 
public class evt_SpawnTubeResetEvent : UnityEvent<float> { } // float AnimationTimer
public class evt_SpawnTubeOpenEvent : UnityEvent { }
#endregion

#region Level Completion Events
/// <summary>
/// Level Completion events
/// </summary>
public class evt_LevelCompleteEvent : UnityEvent { }
#endregion

#region Scene Load Events
/// <summary>
/// Scene Load events
/// </summary>

public class evt_SceneLoadedEvent : UnityEvent { }
#endregion

#region Camera Events
/// <summary>
/// Camera Events
/// </summary>
/// 
public class evt_CameraSetPosEvent : UnityEvent<Vector2> { } //Vector2 Position

public class evt_CameraSetTargetObjectEvent : UnityEvent<GameObject> { }

public class evt_CameraResetToSpawnPosition : UnityEvent { }
#endregion

#region Collision Trigger Events
/// <summary>
/// Collision Trigger events
/// </summary>
/// 
#endregion



public class s_EventDefinitions{}

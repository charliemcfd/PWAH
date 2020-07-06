using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AnimationCommandsNameSpace;

public enum RecordStatus 
{ 	
	stopped = 1,
	paused, 
	recording 
}
public enum RecordActions 
{ 
	PlayerMovement, 
	TerrainCollision,
	AnimationChange,
	PlayerStateChange,
	PlayerVisibilityChange,
	InstantiateSmallExplosion,
}

public class s_GameplayRecorder : MonoBehaviour {
    
	public static s_GameplayRecorder instance;
	private RecordStatus recordStatus;
	private List<RecordedEvent> replayData = new List<RecordedEvent>();
    private List<List<RecordedEvent>> storedReplays = new List<List<RecordedEvent>>();

	private bool m_logDebugInfo;
	void Awake(){
		instance = this;
	}

    void Start()
    {
		m_logDebugInfo = false;
#if DEBUG_ENABLED
		m_logDebugInfo = true;
#endif

	}

	public void PauseRecording()
	{
		if (m_logDebugInfo && recordStatus != RecordStatus.recording)
		{
			Debug.LogError("Cant pause! " + recordStatus);
		}
		recordStatus = RecordStatus.paused;
	}
	public void StartRecording(UInt64 startTime){
		//Delete all actions after STARTTIME. Continue Recording from this point

		//If there are entries
		if (replayData.Count > 0)
		{
			//Iterate backwards over the list
			//Doing so means that any index shift due to "Remove()" will happen on elements that have already been checked, so no accidental skipping of elements
			for (int i = replayData.Count - 1; i >= 0; i--)
			{
				//If the recorded time on the action is greater than our desired start time, remove it
				if (replayData[i].time >= startTime)
				{
					replayData.Remove(replayData[i]);
				}
			}
		}
		StartRecording();
	}
	public void StartRecording()
	{

		if (m_logDebugInfo)
		{
			Debug.Log("Starting new recording");
		}

		//If we are resuming from a paused recording
		switch(recordStatus)
		{
			case RecordStatus.paused:
				{
					if (m_logDebugInfo)
					{
						Debug.Log("Record state is Paused - Not making new object");
					}
					break;
				}
				//Intended fallthrough
			case RecordStatus.stopped:
			case RecordStatus.recording:
				{
					if (m_logDebugInfo)
					{
						Debug.Log("Record state is Not Paused - Making new object");
						replayData = new List<RecordedEvent>();

					}
					break;
				}
			default:
				break;
		}

		recordStatus = RecordStatus.recording;
	}
	public bool IsRecording(){
		return recordStatus == RecordStatus.recording;
	}
	public bool IsPaused(){
		return recordStatus == RecordStatus.paused;
	}

	//Add Animation Command Action
	public void AddAction(UInt64 uTimeStamp, RecordActions action, sAnimationCommand animationcommand)
	{
		if (!IsRecording())
		{
			return;
		}

		RecordedEvent newAction = new RecordedEvent();
		newAction.recordedAction = action;
		newAction.time = uTimeStamp;
		newAction.animationcommand = animationcommand;
		replayData.Add(newAction);

	}

	//Add enum state change action
	public void AddAction(UInt64 uTimeStamp, RecordActions action, int enumState)
	{
		if (!IsRecording())
		{
			return;
		}

		RecordedEvent newAction = new RecordedEvent();
		newAction.recordedAction = action;
		newAction.time = uTimeStamp;
		newAction.enumState = enumState;
		replayData.Add(newAction);
	}

	//Add Visibility change action
	public void AddAction(UInt64 uTimeStamp, RecordActions action, bool _bVisible)
	{
		if (!IsRecording())
		{
			return;
		}
		RecordedEvent newAction = new RecordedEvent();
		newAction.recordedAction = action;
		newAction.time = uTimeStamp;
		newAction.visible = _bVisible;
		replayData.Add(newAction);
	}

	//Add Generic Action
	public void AddAction(UInt64 uTimeStamp, RecordActions action)
	{
		if (!IsRecording())
		{
			return;
		}
		AddAction(uTimeStamp, action, Vector3.zero);
	}
	
	//Add Generic action with positional, rotational and scale data.
	//Note: This is typically used for transform updates
	public void AddAction(UInt64 uTimeStamp, RecordActions action, Vector3 position = default(Vector3), Vector3 rotation = default(Vector3), float scaleX = 1.0f, float scaleY = 1.0f)
	{
		if (!IsRecording())
		{
			return;
		}

		RecordedEvent newAction = new RecordedEvent();
		newAction.recordedAction = action;
		newAction.position = position;
		newAction.rotation = rotation;
        newAction.scaleX = scaleX;
        newAction.scaleY = scaleY;
		newAction.time = uTimeStamp;
		replayData.Add(newAction);
	}
	public void StopRecording()
	{
		if (recordStatus != RecordStatus.recording)
		{
			Debug.LogError("s_GameplayRecorder::StopRecording - Atempting to stop recording on a recording that is not currently in the recording state!");
		}
		recordStatus = RecordStatus.stopped;

		//Create a new list of replay data and populate it with the data we collected from this recording
		List<RecordedEvent> _ThisRecording = new List<RecordedEvent>(replayData);

		//Store the replay data
		storedReplays.Add(_ThisRecording);
	}
	public void ClearPreviousRecording()
	{
		replayData.Clear();
	}

	public int GetNumReplays()
	{
		return storedReplays.Count;
	}


	//Returns a replay for a given index in the form of a list
	public List<RecordedEvent> GetEventsList(int _iIndex){

        if (_iIndex < storedReplays.Count)
        {
            return storedReplays[_iIndex];
        }
        else
        {
            Debug.LogError("GAMEPLAY RECORDER: Attemping to get replay with invalid index");
            return null;
        }
	}

}
public class RecordedEvent {
	public RecordActions recordedAction;
	public UInt64 time;
	public Vector3 position;
	public Vector3 rotation;

    /*
        Note: ScaleX+Y are two floats as opposed to a Vector3. This is because the default scale should be (1,1,1) (Z not used)
        however this is no way to construct the vector like Vector3.zero.

        To avoid unnecessary instantiation (and garbage), we will use floats.
    */
    public float scaleX;
    public float scaleY;

    public sAnimationCommand animationcommand;
	public int enumState;
	public bool visible;

    void Start()
    {
        position = Vector3.zero;
        rotation = Vector3.zero;
        scaleX = 1;
        scaleY = 1;
    }
}
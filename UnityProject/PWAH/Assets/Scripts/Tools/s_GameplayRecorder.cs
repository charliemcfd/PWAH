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
	playerMovement, 
	TerrainCollision,
	AnimationChange,
	PlayerStateChange,
	PlayerVisibilityChange,
	InstantiateSmallExplosion,
}

public class s_GameplayRecorder : MonoBehaviour {
    
	public static s_GameplayRecorder SP;
	private RecordStatus recordStatus;
	private float startedRecording =0;
	private List<RecordedEvent> replayData = new List<RecordedEvent>();
    private List<List<RecordedEvent>> storedReplays = new List<List<RecordedEvent>>();
	private float pausedAt = 0;
	void Awake(){
		SP =this;
        //REgister with GSP
		//StartRecording();
	}

    void Start()
    {
        GameSystemPointers.instance.m_GameplayRecorder = this;
    }

    public float RecordTime()
	{
		if (recordStatus != RecordStatus.recording)
			Debug.LogError("Cant get time!");
		return Time.realtimeSinceStartup - startedRecording;
	}
	public void PauseRecording()
	{
		if (recordStatus != RecordStatus.recording)
			Debug.LogError("Cant pause! " + recordStatus);
		pausedAt = RecordTime();
		recordStatus = RecordStatus.paused;
	}
	public void StartRecording(float startTime){
		//Delete all actions after STARTTIME. Continue Recording from this point
		if (replayData.Count >= 0)
		{
			for (int i = replayData.Count - 1; i >= 0; i--)
			{
				RecordedEvent action = replayData[i];
				if (action.time >= startTime)
					replayData.Remove(action);
			}
		}
		pausedAt = startTime;
		StartRecording();
	}
	public void StartRecording()
	{
        Debug.Log("Starting new recording");
		if(recordStatus == RecordStatus.paused){
            Debug.Log("Not making new object");

            startedRecording = Time.realtimeSinceStartup - pausedAt;
		}else{
            Debug.Log("making new object");

            startedRecording = Time.realtimeSinceStartup;
			replayData = new List<RecordedEvent>();
		}
		recordStatus = RecordStatus.recording;
	}
	public bool IsRecording(){
		return recordStatus == RecordStatus.recording;
	}
	public bool IsPaused(){
		return recordStatus == RecordStatus.paused;
	}
	//Hereby multiple defenitions so that you can add as much data as you want.
	public void AddAction(UInt64 uTimeStamp, RecordActions action, sAnimationCommand animationcommand)
	{
		if (!IsRecording())
		{
			//Debug.LogError("Record didn't start!");
			return;
		}
		RecordedEvent newAction = new RecordedEvent();

		newAction.recordedAction = action;
		newAction.time = uTimeStamp;
		newAction.animationcommand = animationcommand;

		newAction.position = Vector3.zero;
		newAction.rotation = Vector3.zero;
		replayData.Add(newAction);


	}

	public void AddAction(UInt64 uTimeStamp, RecordActions action, int enumState)
	{
		if (!IsRecording())
		{
			//Debug.LogError("Record didn't start!");
			return;
		}

		RecordedEvent newAction = new RecordedEvent();
		newAction.recordedAction = action;
		newAction.position = Vector3.zero;
		newAction.rotation = Vector3.zero;
		newAction.time = uTimeStamp;
		newAction.enumState = enumState;
		replayData.Add(newAction);
	}

	public void AddAction(UInt64 uTimeStamp, RecordActions action, bool _bVisible)
	{
		RecordedEvent newAction = new RecordedEvent();
		newAction.recordedAction = action;
		newAction.position = Vector3.zero;
		newAction.rotation = Vector3.zero;
		newAction.time = uTimeStamp;
		newAction.visible = _bVisible;
		replayData.Add(newAction);
	}
	public void AddAction(UInt64 uTimeStamp, RecordActions action)
	{
		AddAction(uTimeStamp, action, Vector3.zero);
	}
	public void AddAction(UInt64 uTimeStamp, RecordActions action, Vector3 position)
	{
		AddAction(uTimeStamp, action, position, Vector3.zero, 0.0f, 0.0f);
	}
    public void AddAction(UInt64 uTimeStamp, RecordActions action, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        AddAction(uTimeStamp, action, position, rotation, scale.x, scale.y);
    }

    public void AddAction(UInt64 uTimeStamp, RecordActions action, Vector3 position, Vector3 rotation, float scaleX, float scaleY)
	{
		if (!IsRecording())
		{
			//Debug.LogError("Record didn't start!");
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
		if(recordStatus != RecordStatus.recording)
			Debug.LogError("Cant STOP!");
		stoppedAtLength = RecordTime();
		recordStatus = RecordStatus.stopped;


    List<RecordedEvent> _ThisRecording = new List<RecordedEvent>(replayData);


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



	private float stoppedAtLength = 0;
	public float LastRecordLength()
	{
		if (recordStatus == RecordStatus.paused) return pausedAt;
		if (recordStatus != RecordStatus.stopped) return RecordTime();
		return stoppedAtLength;
	}
	public string RecordedDataToReadableString(){
		/*
		string output ="Replay data:\n";
		foreach(RecordedEvent action in replayData){
			output+= action.time+": "+action.recordedAction+"\n";
		}
		return output;*/

		return "";
	}
	public string RecordedDataToString()
	{
		/*
		string output = "";
		foreach (RecordedEvent action in replayData)
		{
			output += action.time + "#" + (int)action.recordedAction + "#" + Utils.Vector3ToString(action.position) + "#" +
				Utils.Vector3ToString(action.rotation) + "\n";
		}
		return output;*/

		return "";
	}
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
	/* //Add the right URL to your upload script
public IEnumerator UploadData()
{
WWWForm wwwForm = new WWWForm();
wwwForm.AddField("replayData", RecordedDataToString());
WWW www = new WWW("http://www.YOURSITE.com/uploadData.php", wwwForm);
yield return www;
Debug.Log("Uploaded replay data!");
}
*/
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
        scaleX = 0;
        scaleY = 0;
    }
}
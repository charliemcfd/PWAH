using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class s_PlayerManager : MonoBehaviour {
	
	public GameObject  m_PrefabPlayer1;
	
	//private List<GameObject> m_ListPlayers;
	private List<s_EntityPlayer> m_ListPlayerScripts;
	
	
	// Use this for initialization
	void Start () {
		//Register with GSP
		GameSystemPointers._instance.m_PlayerManager = this;
        DontDestroyOnLoad(this);
		
		m_ListPlayerScripts = new List<s_EntityPlayer>();

        //=====Events
        s_EventManager.SceneLoadedEvent.AddListener(HandleEvent_SceneLoadedEvent);

	}

	void OnDestroy()
	{
		GameSystemPointers._instance.m_PlayerManager = null;
        s_EventManager.SceneLoadedEvent.RemoveListener(HandleEvent_SceneLoadedEvent);

    }

    // Update is called once per frame
    void Update () {
		ProcessResetPlayer();
	}

	public void RecieveInput(float[] _arrayGameCommands)
	{
        //TODO: Branch this out for multiple players. Not necessary ATM.
        if (m_ListPlayerScripts.Count > 0)
        {
            if (m_ListPlayerScripts[0].GetShouldRecieveInput())
            {
                m_ListPlayerScripts[0].RecieveInput(_arrayGameCommands);
            }
        }
	}
	
	public GameObject GetPlayer( int _iIndex)
	{
        if (m_ListPlayerScripts.Count > _iIndex)
        {
            return m_ListPlayerScripts[_iIndex].gameObject;
        }

        return null;
	}

    public GameObject GetLastCreatedPlayer()
    {
        return m_ListPlayerScripts.Count > 0 ? m_ListPlayerScripts[m_ListPlayerScripts.Count - 1].gameObject : null;
    }

	public s_EntityPlayer GetPlayerScript( int _iIndex)
	{
        if (m_ListPlayerScripts.Count > _iIndex)
        {
            return m_ListPlayerScripts[_iIndex];
        }
        return null;
	}

	private void ProcessResetPlayer()
	{
		for(int i = 0; i < m_ListPlayerScripts.Count; i++)
		{
			//Grab scripts from  player gameobjects. These will be called a lot, so grab them once
			s_EntityPlayer _ScriptPlayer = m_ListPlayerScripts[i];

			if(_ScriptPlayer && !_ScriptPlayer.GetIsReplay() && _ScriptPlayer.GetShouldReset())
			{
                //If the player has signalled that we should reset, reset the level.
                s_BaseLevelScript _LevelScript = GameSystemPointers.instance.m_LevelScript;
                if(_LevelScript)
                {
                    if(_LevelScript.GetLevelState() == s_BaseLevelScript.eLevelState.eLS_Playing)
                    {
                        _LevelScript.ResetLevel();
                    }
				}

			}

		}
	}

    public void RemovePlayer()
    {
		for (int i = m_ListPlayerScripts.Count - 1; i >= 0; i--)
		{
			s_EntityPlayer _ScriptPlayer = m_ListPlayerScripts[i];

			if (_ScriptPlayer && _ScriptPlayer.GetShouldReset())
			{
				s_GameplayRecorder.instance.StopRecording();
				Destroy(_ScriptPlayer.gameObject);
				m_ListPlayerScripts.Remove(_ScriptPlayer);
				Destroy(_ScriptPlayer);
			}
		}
	}

	public void CreatePlayer(int _iNumPlayers = 1, bool _bReplayPlayer = false)
	{
        for(int i = 0; i < _iNumPlayers; i++)
        {
            Vector3 _PlayerSpawnPosition = _bReplayPlayer ? GameSystemPointers._instance.m_LevelScript.GetPlayerSpawnPosition() + new Vector3(0, 0, -0.05f * i) : GameSystemPointers._instance.m_LevelScript.GetPlayerSpawnPosition();
            GameObject _newPlayer = (GameObject)Instantiate(m_PrefabPlayer1, _PlayerSpawnPosition, Quaternion.identity);
			s_EntityPlayer _ScriptPlayer = _newPlayer.GetComponent<s_EntityPlayer>();
			if(_ScriptPlayer)
			{
				m_ListPlayerScripts.Add(_ScriptPlayer);

				if (_bReplayPlayer)
				{
					_ScriptPlayer.SetIsReplay(true);
					_ScriptPlayer.replayData = s_GameplayRecorder.instance.GetEventsList(i);
					_ScriptPlayer.m_replayZValue = -0.05f * i;

					//TODO:: Error handling for getting back null replay data. I think just delete that instance of the player.
				}
				else if (i == _iNumPlayers - 1)
				{
					//If this is the last player in the list (most recently added) and it is not a replay, we should record it.
					_ScriptPlayer.SetIsReplay(false);
					s_GameplayRecorder.instance.ClearPreviousRecording();					s_GameplayRecorder.instance.StartRecording();

				}
			}
			else
			{
				Debug.LogError("s_PlayerManager::CreatePlayer - Created a player but was unable to get the s_EntityPlayer Script. Deleting newly created gameobject");
				Destroy(_newPlayer);
			}			
		}

		s_EventManager.CameraSetTargetObjectEvent.Invoke(GetLastCreatedPlayer());
	}

    public int GetNumPlayers()
    {
        return m_ListPlayerScripts.Count;
    }

    public void HandleEvent_SceneLoadedEvent()
    {
		//=====Destroy all Player scripts and Gameobjects
		for (int i = 0; i < m_ListPlayerScripts.Count; i++)
		{
			Destroy(m_ListPlayerScripts[i].gameObject);
			Destroy(m_ListPlayerScripts[i]);
		}
		m_ListPlayerScripts.Clear();
    }
}
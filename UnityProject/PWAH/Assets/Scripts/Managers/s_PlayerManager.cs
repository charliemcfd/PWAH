using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class s_PlayerManager : MonoBehaviour {
	
	public GameObject  m_PrefabPlayer1;
	
	private List<GameObject> m_ListPlayers;
	private List<s_EntityPlayer> m_ListPlayerScripts;
	
	
	// Use this for initialization
	void Start () {

        Debug.Log("Created player manager");
		//Register with GSP
		GameSystemPointers._instance.m_PlayerManager = this;
        DontDestroyOnLoad(this);

		
		m_ListPlayers = new List<GameObject>();
		m_ListPlayerScripts = new List<s_EntityPlayer>();

        /*
m_ListPlayers.Add(      (GameObject)Instantiate(m_PrefabPlayer1, new Vector3(-999,-999,0) , Quaternion.identity)    );

for(int i = 0; i < m_ListPlayers.Count; i++)
{
    //Grab scripts from  player gameobjects. These will be called a lot, so grab them once

    s_EntityPlayer _ScriptPlayer = (s_EntityPlayer)m_ListPlayers[i].GetComponent(typeof(s_EntityPlayer));
    //_ScriptPlayer.SetGravityManager((s_GravityManager)gameObject.GetComponent(typeof(s_GravityManager)));
    m_ListPlayerScripts.Add(_ScriptPlayer);
}*/

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
        if (m_ListPlayers.Count > _iIndex)
        {
            return m_ListPlayers[_iIndex];
        }

        return null;
	}

    public GameObject GetLastCreatedPlayer()
    {
        return m_ListPlayers.Count > 0 ? m_ListPlayers[m_ListPlayers.Count - 1] : null;
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
		for(int i = 0; i < m_ListPlayers.Count; i++)
		{
			//Grab scripts from  player gameobjects. These will be called a lot, so grab them once
			s_EntityPlayer _ScriptPlayer = (s_EntityPlayer)m_ListPlayers[i].GetComponent(typeof(s_EntityPlayer));

			if(!_ScriptPlayer.m_bIsReplay && _ScriptPlayer.GetShouldReset())
			{
                //If the player has signalled that we should reset, reset the level.
                s_BaseLevelScript _LevelScript = GameSystemPointers.instance.m_LevelScript;
                if(_LevelScript)
                {
                    if(_LevelScript.GetLevelState() == s_BaseLevelScript.eLevelState.eLS_Playing)
                    {
                        Debug.Log("Resetting: Player "+ i.ToString());
                        _LevelScript.ResetLevel();
                    }
                    else if(_LevelScript.GetLevelState() == s_BaseLevelScript.eLevelState.eLS_WaitingForStart)
                    {
                        /*
                        s_GameplayRecorder.SP.StopRecording();
                        Destroy(m_ListPlayers[i]);
                        m_ListPlayers.RemoveAt(i);
                        m_ListPlayerScripts.Remove(_ScriptPlayer);
                        Destroy(_ScriptPlayer);

                        //CreatePlayer(false);

                        //_LevelScript.SetLevelState(s_BaseLevelScript.eLevelState.eLS_Playing);
                        */
                    }
                }
                else
                {
                }
			}

		}
	}

    public void RemovePlayer()
    {
        Debug.Log("Calling Remove Player");
        for (int i = 0; i < m_ListPlayers.Count;)
        {
            s_EntityPlayer _ScriptPlayer = (s_EntityPlayer)m_ListPlayers[i].GetComponent(typeof(s_EntityPlayer));

            if (_ScriptPlayer.GetShouldReset())
            {
                s_GameplayRecorder.SP.StopRecording();
                Destroy(m_ListPlayers[i]);
                m_ListPlayers.RemoveAt(i);
                m_ListPlayerScripts.Remove(_ScriptPlayer);
                Destroy(_ScriptPlayer);
            }
            else
            {
                i++;
            }
        }
    }

	public void CreatePlayer(int _iNumPlayers = 1, bool _bReplayPlayer = false)
	{
        for(int i = 0; i < _iNumPlayers; i++)
        {
            Vector3 _PlayerSpawnPosition = _bReplayPlayer ? GameSystemPointers._instance.m_LevelScript.GetPlayerSpawnPosition() + new Vector3(0, 0, -0.05f * i) : GameSystemPointers._instance.m_LevelScript.GetPlayerSpawnPosition();
            GameObject _newPlayer = (GameObject)Instantiate(m_PrefabPlayer1, _PlayerSpawnPosition, Quaternion.identity);//(0.1f,13.4f,0)
            m_ListPlayers.Add(_newPlayer);
        }

        //Re-add scripts to list
        //m_ListPlayerScripts.Clear();

		for(int i = 0; i < m_ListPlayers.Count; i++)
		{
			//Grab scripts from  player gameobjects. These will be called a lot, so grab them once
			
			s_EntityPlayer _ScriptPlayer = (s_EntityPlayer)m_ListPlayers[i].GetComponent(typeof(s_EntityPlayer));
			//_ScriptPlayer.SetGravityManager((s_GravityManager)gameObject.GetComponent(typeof(s_GravityManager)));
			m_ListPlayerScripts.Add(_ScriptPlayer);

			if(_bReplayPlayer)
			{
				_ScriptPlayer.m_bIsReplay = true;
				_ScriptPlayer.replayData = s_GameplayRecorder.SP.GetEventsList(i);
                _ScriptPlayer.m_replayZValue = -0.05f * i;

                //TODO:: Error handling for getting back null replay data. I think just delete that instance of the player.
            }
			else if (i == m_ListPlayers.Count-1)
			{
                //If this is the last player in the list (most recently added) and it is not a replay, we should record it.
				_ScriptPlayer.m_bIsReplay = false;
                s_GameplayRecorder.SP.ClearPreviousRecording();
                s_GameplayRecorder.SP.StartRecording();
            
			}
		}
	}

    public int GetNumPlayers()
    {
        return m_ListPlayers.Count;
    }

    public void HandleEvent_SceneLoadedEvent()
    {
        //=====Destroy All Players
        for (int i = 0; i < m_ListPlayers.Count; i++)
        {
            s_EntityPlayer _ScriptPlayer = (s_EntityPlayer)m_ListPlayers[i].GetComponent(typeof(s_EntityPlayer));

            Destroy(m_ListPlayers[i]);
            m_ListPlayers.RemoveAt(i);
            m_ListPlayerScripts.Remove(_ScriptPlayer);
            Destroy(_ScriptPlayer);
        }

        m_ListPlayers.Clear();
        m_ListPlayerScripts.Clear();
    }
}
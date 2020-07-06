using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;

public class s_BaseLevelScript : MonoBehaviour {

	public GameObject   m_PlayerSpawn;
	public GameObject   m_GoalObject;
    public string m_sNextLevel;

    private bool m_bHasActivatedPlayer;
    private bool m_bLevelWasCompleted;

    public enum eLevelState
    {
        eLS_Playing,
        eLS_Resetting,
        eLS_WaitingForStart,
        eLS_Complete
    }

    private eLevelState m_eLevelState;
    	
	// Use this for initialization
	public virtual void Start () {

		Debug.Log("Created Level");
		GameSystemPointers._instance.m_LevelScript = this;
        SetLevelState(eLevelState.eLS_Resetting);

        m_bHasActivatedPlayer = false;
        m_bLevelWasCompleted = false;

        s_EventManager.LevelCompleteEvent.AddListener(HandleEvent_LevelCompleteEvent);
    }

	void OnDestroy()
	{
        GameSystemPointers._instance.m_LevelScript = null;

        s_EventManager.LevelCompleteEvent.RemoveListener(HandleEvent_LevelCompleteEvent);
	}
	// Update is called once per frame
	public virtual void Update () {

        switch (m_eLevelState)
        {
            case eLevelState.eLS_Playing:
                {

                    s_PlayerSpawnScript _sPlayerSpawn = m_PlayerSpawn.GetComponent<s_PlayerSpawnScript>();
                    if (_sPlayerSpawn)
                    {
                        if(_sPlayerSpawn.GetAnimationComplete())
                        {
                            s_EntityPlayer _sPlayerScript = GameSystemPointers.instance.m_PlayerManager.GetPlayerScript(0);

                            if (_sPlayerScript && !m_bHasActivatedPlayer)
                            {
                                _sPlayerScript.SetShouldRecieveInput(true);
                                m_bHasActivatedPlayer = true;
                            }
                        }
                    }
                        break;
                }

            case eLevelState.eLS_Resetting:
                {

                    //Check state of fade.
                    s_BlackFadeScript _FadeScript = GameSystemPointers.instance.m_Camera.GetComponentInChildren<tk2dSprite>().GetComponent<s_BlackFadeScript>();
                    if (_FadeScript)
                    {

                        if (_FadeScript.GetFadeState() == s_BlackFadeScript.eFadeState.eFS_FadedIn)
                        {
                            //If we have fully faded in (Screen is black) then we want to perform the reset.
                            Debug.Log("Internal Reset");

                            InternalResetLevel(_FadeScript);
                        }
                        else if (_FadeScript.GetFadeState() == s_BlackFadeScript.eFadeState.eFS_FadedOut)
                        {
                            //If we have fully faded Out (Screen is clear) then we have finished the reset, and should change state

                            SetLevelState(eLevelState.eLS_WaitingForStart);
                        }
                    }

                        break;
                }

            case eLevelState.eLS_WaitingForStart:
                {
                    s_PlayerManager _PlayerManager = GameSystemPointers.instance.m_PlayerManager;
                    if(_PlayerManager)
                    {
                        if (m_bLevelWasCompleted == false)
                        {
                            if (_PlayerManager.GetPlayer(0) == null)
                            {
                                _PlayerManager.CreatePlayer();
                            }
                            else
                            {
                                SetLevelState(s_BaseLevelScript.eLevelState.eLS_Playing);
                                s_EventManager.SpawnTubeOpenEvent.Invoke();
                            }
                        }
                        else
                        {
                            CreateReplayPlayers();
                            SetLevelState(s_BaseLevelScript.eLevelState.eLS_Playing);
                            s_EventManager.SpawnTubeOpenEvent.Invoke();

                        }
                    }
                    break;
                }

            case eLevelState.eLS_Complete:
                {
                    break;
                }
        }
		
	}
	
	public virtual void ResetLevel ()
	{
        //This function is called externally in order to begin the reset progress.
        //It begins the fade on the camera.

        s_BlackFadeScript _FadeScript = GameSystemPointers.instance.m_Camera.GetComponentInChildren<tk2dSprite>().GetComponent<s_BlackFadeScript>();
        if (_FadeScript)
        {
            Debug.Log("Setting level to reset");
            _FadeScript.FadeIn();
            SetLevelState(eLevelState.eLS_Resetting);
        }
	}

    protected virtual void InternalResetLevel(s_BlackFadeScript _sFadeScript)
    {
        //This function will reset the level back it is initial state.
        //This involves resetting positions, timers, deleting any "spawned" objects that could currently be present etc.
        //It also triggers the fade in process

        //TODO: Fire an event that will be picked up by the spawn tube animation that will tell it to reset. We then need to get it to play again when we respawn.
        // Also, it isn't tied into the level start sequence at all right now, it's just set to play on level load. Need to do that too.

        if (_sFadeScript)
        {
            _sFadeScript.FadeOut();
        }

        s_PlayerManager _PlayerManager = GameSystemPointers.instance.m_PlayerManager;
        if (_PlayerManager)
        {
            _PlayerManager.RemovePlayer();
            if (!m_bLevelWasCompleted)
            {
                _PlayerManager.CreatePlayer();
            }
        }

        m_bHasActivatedPlayer = false;

         s_EventManager.SpawnTubeResetEvent.Invoke(1.0f);
    }

    public virtual GameObject GetPlayerSpawn()
	{
		return m_PlayerSpawn;
	}

	public virtual Vector3 GetPlayerSpawnPosition()
	{
        s_PlayerSpawnScript _sPlayerSpawn = m_PlayerSpawn.GetComponent<s_PlayerSpawnScript>();
        if(_sPlayerSpawn)
        {
            return _sPlayerSpawn.GetPlayerRespawnLocation();
        }
        else
        {
            Debug.Log("Couldn't find player spawn script");
            return new Vector3(0, 0, 0);
        }
	}

    public eLevelState GetLevelState()
    {
        return m_eLevelState;
    }

    public void SetLevelState(eLevelState _eNewState)
    {
        m_eLevelState = _eNewState;
    }

    private void CreateReplayPlayers()
    {
        int _iNumReplays = s_GameplayRecorder.instance.GetNumReplays();
        GameSystemPointers.instance.m_PlayerManager.CreatePlayer(_iNumReplays, true);
    }

    public void HandleEvent_LevelCompleteEvent()
    {
        //Loads the next level
        //TODO: Expand this to show time etc
        //TODO: Expand this function to allow for different game modes, e.g Take you back to the map or just replay the current level.
        //Debug.Log("Loading new level");
        //SceneManager.LoadSceneAsync(m_sNextLevel);
        if(m_bLevelWasCompleted)
        {
            //If the level was already completed, this signals that we have finished watching a replay. Take the appropriate response.
            Debug.Log("Successful replay finished");

            GameSystemPointers.instance.m_LoadingScreen.LoadScene(m_sNextLevel);
        }
        else
        {
            ResetLevel();
            m_bLevelWasCompleted = true;
            GameSystemPointers.instance.m_PlayerManager.GetPlayerScript(0).SetForceReset(true);
        }

        
    }

}

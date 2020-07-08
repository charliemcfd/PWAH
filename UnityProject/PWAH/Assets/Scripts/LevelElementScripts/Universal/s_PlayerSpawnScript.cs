using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
    NOTE: This is the script for the player respawn animation. It plays the various animations and then records itself as being in a "ready" state.

    It also returns the coordinates where the player should respawn.
*/


public class s_PlayerSpawnScript : MonoBehaviour {


    /// <summary>
    /// Public Variables
    /// </summary>
    /// 

    public GameObject m_DoorsAnimation;
    public GameObject m_Light1;
    public GameObject m_Light2;
    public GameObject m_Light3;
    public GameObject m_Light4;
    public GameObject m_PlayerSpawnLocation;
    public GameObject m_MainBodyAnimation;

    /// <summary>
    /// Private Variables
    /// </summary>
    /// 

    private float m_fCountDownTimer;
    private const float m_fCountDownTimerMax = 1.5f;
    private bool m_bDoorsOpening;
    private bool m_bAnimationComplete;
    private bool m_bActive;

	//Sprite Animators
	private tk2dSpriteAnimator m_doorsSpriteAnimator;
	private tk2dSpriteAnimator m_mainBodySpriteAnimator;

	//Animation Clips
	private tk2dSpriteAnimationClip m_respawnDoorsOpenClip;
	private tk2dSpriteAnimationClip m_respawnBodyDoorsOpenClip;

	//Light Sprite renderers
	private List<Renderer> m_lightSpriteRenderers;
	private float m_fLightSpriteCountReciprocal;

	// Use this for initialization
	void Start () {

        m_fCountDownTimer = m_fCountDownTimerMax;
        m_bDoorsOpening = false;
        m_bAnimationComplete = false;
        m_bActive = false;

		m_doorsSpriteAnimator = m_DoorsAnimation.GetComponent<tk2dSpriteAnimator>();
		m_mainBodySpriteAnimator = m_MainBodyAnimation.GetComponent<tk2dSpriteAnimator>();

		if (m_doorsSpriteAnimator && m_mainBodySpriteAnimator)
		{
			InitializeAnimationClips();
		}
		else
		{
			Debug.LogError("s_PlayerSpawnScript::Start - Could not get sprite animator components");
		}

		InitializeLightSpriteRenderers();

		//Register for events
		s_EventManager.SpawnTubeResetEvent.AddListener(HandleEvent_SpawnTubeEvent);
        s_EventManager.SpawnTubeOpenEvent.AddListener(HandleEvent_SpawnTubeOpenEvent);

		s_EventManager.CameraResetToSpawnPositionEvent.AddListener(HandleEvent_CameraResetToSpawnPositionEvent);

		HandleEvent_CameraResetToSpawnPositionEvent();

	}

	private void InitializeAnimationClips()
	{
		m_respawnDoorsOpenClip = m_doorsSpriteAnimator.GetClipByName("RespawnDoorsOpen");
		if (m_respawnDoorsOpenClip == null)
		{
			Debug.LogError("s_Ragdoll::InitializeAnimationClips - Could not get sprite animation clip RespawnDoorsOpen");
		}

		m_respawnBodyDoorsOpenClip = m_mainBodySpriteAnimator.GetClipByName("RespawnBodyDoorsOpen");
		if (m_respawnBodyDoorsOpenClip == null)
		{
			Debug.LogError("s_Ragdoll::InitializeAnimationClips - Could not get sprite animation clip RespawnBodyDoorsOpen");
		}
	}
	
	private void InitializeLightSpriteRenderers()
	{
		m_lightSpriteRenderers = new List<Renderer>();
		if (m_Light1)
		{
			m_lightSpriteRenderers.Add(m_Light1.GetComponent<Renderer>());				
		}
		if (m_Light2)
		{
			m_lightSpriteRenderers.Add(m_Light2.GetComponent<Renderer>());
		}
		if (m_Light3)
		{
			m_lightSpriteRenderers.Add(m_Light3.GetComponent<Renderer>());
		}
		if (m_Light4)
		{
			m_lightSpriteRenderers.Add(m_Light4.GetComponent<Renderer>());
		}

		m_fLightSpriteCountReciprocal = 1.0f / (float)m_lightSpriteRenderers.Count;
	}



	void OnDestroy()
    {
        s_EventManager.SpawnTubeResetEvent.RemoveListener(HandleEvent_SpawnTubeEvent);
        s_EventManager.SpawnTubeOpenEvent.RemoveListener(HandleEvent_SpawnTubeOpenEvent);
		s_EventManager.CameraResetToSpawnPositionEvent.RemoveListener(HandleEvent_CameraResetToSpawnPositionEvent);

    }


    // Update is called once per frame
    void Update () {

        if (m_bActive)
        {
            if (!m_bAnimationComplete)
            {
                m_fCountDownTimer -= Time.deltaTime;

                TurnOffLights();
                ProcessDoors();
            }
        }
	}

    private void ProcessDoors()
    {
        if (m_fCountDownTimer < (0 - m_fCountDownTimer * 0.25f)
            && m_bDoorsOpening == false)
        {
			OpenDoors();
        }

        if(!m_doorsSpriteAnimator.IsPlaying(m_respawnDoorsOpenClip) && m_bDoorsOpening)
        {
            m_DoorsAnimation.GetComponent<Renderer>().enabled = false;
            m_bAnimationComplete = true;
        }
    }

	private void OpenDoors ()
	{
		if (m_doorsSpriteAnimator)
		{
			m_doorsSpriteAnimator.Play(m_respawnDoorsOpenClip);
		}
		if (m_mainBodySpriteAnimator)
		{
			m_mainBodySpriteAnimator.Play(m_respawnBodyDoorsOpenClip);
		}
		m_bDoorsOpening = true;
	}

    private void TurnOffLights()
    {
		//Iterates over all light renderers.
		//If one is enabled, a calculation is performed to see if it should be turned off.
		//The calculation is based on dividing the maximum countdown time into equal sections based on the number of renderers. A reciprocal is used in order to avoid division.
		for(int i = 0; i < m_lightSpriteRenderers.Count; i++)
		{
			if(m_lightSpriteRenderers[i].enabled && m_fCountDownTimer < (m_fCountDownTimerMax - (m_fCountDownTimerMax * ((i+1) * m_fLightSpriteCountReciprocal))))
			{
				m_lightSpriteRenderers[i].enabled = false;
			}
		}
     
    }

    private void TurnOnLights()
    {
		for (int i = 0; i < m_lightSpriteRenderers.Count; i++)
		{
			m_lightSpriteRenderers[i].enabled = true;
		}
    }

    public bool GetAnimationComplete()
    {
        return m_bAnimationComplete;
    }

    public Vector3 GetPlayerRespawnLocation()
    {
        return m_PlayerSpawnLocation.transform.position;
    }

    public bool GetActive()
    {
        return m_bActive;
    }

    public void SetActive (bool _bActive)
    {
        m_bActive = _bActive;
    }

    public void HandleEvent_SpawnTubeEvent(float _fTimeIndex)
    {
        //Reset timer
        m_fCountDownTimer = m_fCountDownTimerMax * _fTimeIndex;

        //Reset Flags
        m_bDoorsOpening = false;
        m_bAnimationComplete = false;
        m_bActive = false;

        //Turn doors back on
        m_DoorsAnimation.GetComponent<Renderer>().enabled = true;
        m_doorsSpriteAnimator.StopAndResetFrame();

        //Turn all lights back on
        TurnOnLights();

        //Do a check to see which lights should be turned off
        TurnOffLights();

        //Send an event to reset the camera (As the player will no longer be present)
        s_EventManager.CameraSetPosEvent.Invoke(GetPlayerRespawnLocation());
    }

    public void HandleEvent_SpawnTubeOpenEvent()
    {
        m_bActive = true;
    }

	public void HandleEvent_CameraResetToSpawnPositionEvent()
	{
		s_EventManager.CameraSetPosEvent.Invoke(GetPlayerRespawnLocation());
	}
}

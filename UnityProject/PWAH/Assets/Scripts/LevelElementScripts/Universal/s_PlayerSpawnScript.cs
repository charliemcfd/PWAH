using UnityEngine;
using System.Collections;

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

    /// <summary>
    /// Private Variables
    /// </summary>
    /// 

    private float m_fCountDownTimer;
    private const float m_fCountDownTimerMax = 1.5f;
    private bool m_bDoorsOpening;
    private bool m_bAnimationComplete;
    private bool m_bActive;



	// Use this for initialization
	void Start () {

        m_fCountDownTimer = m_fCountDownTimerMax;
        m_bDoorsOpening = false;
        m_bAnimationComplete = false;
        m_bActive = false;

        Debug.Log("Registering spawn script for event");
        s_EventManager.SpawnTubeResetEvent.AddListener(HandleEvent_SpawnTubeEvent);
        s_EventManager.SpawnTubeOpenEvent.AddListener(HandleEvent_SpawnTubeOpenEvent);

    }

    void OnDestroy()
    {
        s_EventManager.SpawnTubeResetEvent.RemoveListener(HandleEvent_SpawnTubeEvent);
        s_EventManager.SpawnTubeOpenEvent.RemoveListener(HandleEvent_SpawnTubeOpenEvent);

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
            m_DoorsAnimation.GetComponent<tk2dSpriteAnimator>().Play("RespawnDoorsOpen");
            m_bDoorsOpening = true;
        }

        if(!m_DoorsAnimation.GetComponent<tk2dSpriteAnimator>().IsPlaying("RespawnDoorsOpen") && m_bDoorsOpening)
        {
            m_DoorsAnimation.GetComponent<Renderer>().enabled = false;
            m_bAnimationComplete = true;
        }
    }

    private void TurnOffLights()
    {
        //Note: No "Else if" here becuase it will be possible to skip lights when respawning.

        if(m_fCountDownTimer < (m_fCountDownTimerMax*0.75f) && m_Light1.GetComponent<Renderer>().enabled)
        {
            m_Light1.GetComponent<Renderer>().enabled = false;
        }
        if (m_fCountDownTimer < (m_fCountDownTimerMax * 0.5f) && m_Light2.GetComponent<Renderer>().enabled)
        {
            m_Light2.GetComponent<Renderer>().enabled = false;
        }
        if (m_fCountDownTimer < (m_fCountDownTimerMax * 0.25f) && m_Light3.GetComponent<Renderer>().enabled)
        {
            m_Light3.GetComponent<Renderer>().enabled = false;
        }
        if (m_fCountDownTimer <= (0) && m_Light4.GetComponent<Renderer>().enabled)
        {
            m_Light4.GetComponent<Renderer>().enabled = false;
        }
    }

    private void TurnOnLights()
    {
        m_Light1.GetComponent<Renderer>().enabled = true;
        m_Light2.GetComponent<Renderer>().enabled = true;
        m_Light3.GetComponent<Renderer>().enabled = true;
        m_Light4.GetComponent<Renderer>().enabled = true;

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
        m_DoorsAnimation.GetComponent<tk2dSpriteAnimator>().StopAndResetFrame();

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
}

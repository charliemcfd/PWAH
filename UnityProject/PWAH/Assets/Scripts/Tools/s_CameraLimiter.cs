using UnityEngine;
using System.Collections;

public class s_CameraLimiter : MonoBehaviour {

	public float m_fCameraFollowSpeed;//Speed at which camera follows player
	public float m_fMaxCameraDistance; //Maximum distance from the player that the camera can be before it starts to move
    public float m_fMinCameraY; //The minimum Y value that camera can go to.

    //Used for legacy Parallax settings
    public Vector3 m_fvecReferencePosition;
	// Use this for initialization

	protected tk2dParallaxCamera m_tk2dParallaxCamera;
    void Start () {

        //Register with GSP
        GameSystemPointers.instance.m_Camera = this.GetComponent<tk2dCamera>();
        GameSystemPointers.instance.m_CameraLimiter = this;

        //Register for events
        s_EventManager.CameraSetPosEvent.AddListener(HandleEvent_CameraSetPosEvent);

		m_tk2dParallaxCamera = GetComponent<tk2dParallaxCamera>();


	}

    void OnDestroy()
    {
        s_EventManager.CameraSetPosEvent.RemoveListener(HandleEvent_CameraSetPosEvent);
        if(GameSystemPointers.instance)
            GameSystemPointers.instance.m_Camera = null;

    }

    // Update is called once per frame
    void Update () {
		
		//SnapToPlayerPosition();
		//ForceWithinBounds();
		
		
	}

	void FixedUpdate()
	{
		//SnapToPlayerPosition();
	
	}

	void LateUpdate()
	{
        SnapToPlayerPosition();
		if(m_tk2dParallaxCamera)
		{
			m_tk2dParallaxCamera.UpdateParallaxPositions();
		}
        //transform.position = new Vector3(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y), transform.position.z);

    }

	protected void SnapToPlayerPosition(  )
	{
        s_PlayerManager _PlayerManager = GameSystemPointers.instance.m_PlayerManager;
        int _iNumPlayers = _PlayerManager.GetNumPlayers();
        if(_iNumPlayers > 0)
        {
            //This should grab the last created player, which should be the "successful" player in the case of replays.
            GameObject _goEntityPlayer = _PlayerManager.GetPlayer(_iNumPlayers-1);
            if (_goEntityPlayer)
            {
				GameObject followObject = _goEntityPlayer.GetComponent<s_EntityPlayer>().GetFollowObject();

				Vector3 start;
				//If the player is currently following another object (e.g, the ragdoll) then use the follow object's postiion but retain the Z of the Player entity
				if(followObject == null)
				{
					start = transform.position;
				}
				else
				{
					start = new Vector3(followObject.transform.position.x, followObject.transform.position.y, transform.position.z);
				}
									
				Vector3 end = Vector3.MoveTowards(start, _goEntityPlayer.transform.position, m_fCameraFollowSpeed * Time.deltaTime);
                end.z = start.z;

				float _fNewX = end.x * 1000.0f;
                _fNewX = Mathf.FloorToInt(_fNewX);
                _fNewX /= 1000.0f;

                float _fNewY = end.y * 1000.0f;
                _fNewY = Mathf.FloorToInt(_fNewY);
                _fNewY /= 1000.0f;

                //Test for clamping
                _fNewX = CalculatePixelPerfectPosition(end.x);
                _fNewY = CalculatePixelPerfectPosition(end.y);

                //Limit the camera's Y value
                if (_fNewY < m_fMinCameraY)
                {
                    _fNewY = m_fMinCameraY;
                }

                end.x = _fNewX;
                end.y = _fNewY;


                transform.position = end;
                /*
                Rigidbody2D Rigidbody2D = _goEntityPlayer.GetComponent<Rigidbody2D>();
                if (Rigidbody2D != null && cam != null) {
                    float spd = Rigidbody2D.velocity.magnitude;
                    float scl = Mathf.Clamp01((spd - minZoomSpeed) / (maxZoomSpeed - minZoomSpeed));
                    float targetZoomFactor = Mathf.Lerp(1, maxZoomFactor, scl);
                    cam.ZoomFactor = Mathf.MoveTowards(cam.ZoomFactor, targetZoomFactor, 0.2f * Time.deltaTime);
                }*/

            }
        }
	}
	
    protected float CalculatePixelPerfectPosition(float unperfectPosition)
    {
        float PPORecip = 1.0f / 100.0f;
        float resolutionDivision = unperfectPosition / PPORecip; //TODO: change to grab PPU
        int resolutionDivisionInt = Mathf.FloorToInt(resolutionDivision);
        resolutionDivision -= resolutionDivisionInt;
        if(resolutionDivision >= 0.5)
        {
            resolutionDivisionInt++;
        }

        return resolutionDivisionInt * PPORecip;

    }
	protected void ForceWithinBounds()
	{
		
		float _fNewX = this.transform.position.x;
		float _fNewY = this.transform.position.y;
		//Force X position
		if(_fNewX <= 0)
		{
			_fNewX = 0;
		}
		else if (_fNewX >= 50)
		{
			_fNewX = 50;
		}
		
		//Force Y position
		
		if(_fNewY <= 0)
		{
			_fNewY = 0;
		}
		else if (_fNewY >= 50)
		{
			_fNewY = 50;
		}
		
		
		//Set camera position
		this.transform.position = new Vector3(_fNewX,_fNewY,this.transform.position.z);
		
	}

    public void HandleEvent_CameraSetPosEvent(Vector2 _newPos)
    {
        //Sets the camera to a given coordinate

        this.transform.position = new Vector3(_newPos.x, _newPos.y, this.transform.position.z);

    }

    //Stolen from the tk2d tilemap demo:
    /*
    using UnityEngine;
    using System.Collections;

    public class tk2dTileMapDemoFollowCam : MonoBehaviour {

        tk2dCamera cam;
        public Transform target;
        public float followSpeed = 1.0f;

        public float minZoomSpeed = 20.0f;
        public float maxZoomSpeed = 40.0f;

        public float maxZoomFactor = 0.6f;

        void Awake() {
            cam = GetComponent<tk2dCamera>();
        }

        void FixedUpdate() {

        }
    }*/

}

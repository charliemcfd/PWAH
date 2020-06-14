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
	protected GameObject m_TargetObject;
	protected Vector3 m_targetPosition;

	protected float m_cameraZ;
	protected float m_pixelsPerUnit; //Per meter
	protected float m_pixelsPerUnitReciprocal;
    void Start () {

        //Register with GSP
        GameSystemPointers.instance.m_Camera = this.GetComponent<tk2dCamera>();
        GameSystemPointers.instance.m_CameraLimiter = this;

        //Register for events
        s_EventManager.CameraSetPosEvent.AddListener(HandleEvent_CameraSetPosEvent);
		s_EventManager.CameraSetTargetObjectEvent.AddListener(HandleEvent_SetTargetObject);

		m_tk2dParallaxCamera = GetComponent<tk2dParallaxCamera>();

		m_cameraZ = transform.position.z;

		m_targetPosition = transform.position;

		m_pixelsPerUnit = GetComponent<tk2dCamera>().CameraSettings.orthographicPixelsPerMeter;
		m_pixelsPerUnitReciprocal = 1.0f / m_pixelsPerUnit;

	}

    void OnDestroy()
    {
        s_EventManager.CameraSetPosEvent.RemoveListener(HandleEvent_CameraSetPosEvent);
		s_EventManager.CameraSetTargetObjectEvent.RemoveListener(HandleEvent_SetTargetObject);
        if(GameSystemPointers.instance)
            GameSystemPointers.instance.m_Camera = null;

    }

    // Update is called once per frame
    void Update () {
	
	}

	void FixedUpdate()
	{
		//Update the target position.
		if(m_TargetObject)
		{
			m_targetPosition = new Vector3(m_TargetObject.transform.position.x, m_TargetObject.transform.position.y, m_cameraZ);
		}
	}

	void LateUpdate()
	{
        InterpolateToTargetPosition();
		if(m_tk2dParallaxCamera)
		{
			m_tk2dParallaxCamera.UpdateParallaxPositions();
		}
    }

	protected void InterpolateToTargetPosition(  )
	{
		//Interpolate from the current position to the target position
		Vector3 start = transform.position;
		Vector3 result = Vector3.Lerp(start, m_targetPosition, m_fCameraFollowSpeed * Time.deltaTime);

		//Limit the camera's Y value
		if (result.y < m_fMinCameraY)
		{
			result.y = m_fMinCameraY;
		}

		//Calculate positions in Unity co-ordinate space that correspond to pixel-perfect values
		result.x = CalculatePixelPerfectPosition(result.x);
		result.y = CalculatePixelPerfectPosition(result.y);

		//Update the position of the camera transform
        transform.position = result;
	}
	
    protected float CalculatePixelPerfectPosition(float unperfectPosition)
    {
		
		//Calculate the position in pixel-space (Still  a floating point number at this point)
        float resolutionPosition= unperfectPosition * m_pixelsPerUnit;

		//Truncate the calcualted position to an integer and store in a seperate variable
        int resolutionPositionInt = Mathf.FloorToInt(resolutionPosition);

		//Subtract the integer component of the resolution to get a decimal "remainder"
		resolutionPosition -= resolutionPositionInt;

		//If the reaminder is greater than or equal to 0.5 (half a pixel) then increase the value of the integer position
        if(resolutionPosition >= 0.5)
        {
			resolutionPositionInt++;
        }

		//Convert the resolution position back into Unity co-ordinate space by multiplying by the reciprocal (1/PixelsPerUnit)
        return resolutionPositionInt * m_pixelsPerUnitReciprocal;

    }


    public void HandleEvent_CameraSetPosEvent(Vector2 _newPos)
    {
        //Sets the camera to a given coordinate. This will circumvent the interpolation that is normally carried out.
        this.transform.position = new Vector3(_newPos.x, _newPos.y, this.transform.position.z);
		m_targetPosition = this.transform.position;

    }

	public void HandleEvent_SetTargetObject(GameObject newTargetObject)
	{
		m_TargetObject = newTargetObject;
	}

}

using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

[System.Serializable]
public class tk2dParallaxLayer
{
	public Camera camera = null;
    [FormerlySerializedAs("speed")]
    [SerializeField]
    public float speedX = 1.0f;

    [SerializeField]
    public float speedY = 1.0f;
    [HideInInspector][System.NonSerialized]
	public Transform transform = null;
}

public class tk2dParallaxCamera : MonoBehaviour 
{
	public tk2dParallaxLayer[] layers;
	public Vector3 rootPosition;

    protected s_CameraLimiter m_masterCameraLimiter;
	//PWAH
	protected float m_pixelsPerUnit; //Per meter
	protected float m_pixelsPerUnitReciprocal;
	//~PWAH

	// Use this for initialization
	void Start () 
	{
		rootPosition = transform.position;
		if (layers == null)
		{
			layers = new tk2dParallaxLayer[0];
		}
		
		foreach (var layer in layers)
		{
			if (layer.camera != null)
			{
				layer.transform = layer.camera.transform;
			}
		}

        m_masterCameraLimiter = GetComponent<s_CameraLimiter>();

		//PWAH
		m_pixelsPerUnit = GetComponent<tk2dCamera>().CameraSettings.orthographicPixelsPerMeter;
		m_pixelsPerUnitReciprocal = 1.0f / m_pixelsPerUnit;
		//~PWAH

	}
	
	/// <summary>
	/// Resets the parallax offsets. The cameras will not exhibit any parallax at this current position.
	/// </summary>
	public void ResetOffsets()
	{
		rootPosition = transform.position;
		Vector3 rootOffset = transform.position - rootPosition;
		foreach (var layer in layers)
		{
			if (layer.transform != null)
			{
                layer.transform.position = new Vector3(rootPosition.x + rootOffset.x * layer.speedX, rootPosition.y + rootOffset.y * layer.speedY, rootPosition.z);
            }
        }		
	}
	
	// Update is called once per frame
	void LateUpdate () 
	{
		//UpdateParallaxPositions();
	}

	public void UpdateParallaxPositions()
	{
		Vector3 rootOffset = transform.position - rootPosition;
		foreach (var layer in layers)
		{
			if (layer.transform != null)
			{
				float unperfectX = rootPosition.x + rootOffset.x * layer.speedX;
				float unperfectY = rootPosition.y + rootOffset.y * layer.speedY;
				layer.transform.position = CalculatePixelPerfectPosition(unperfectX, unperfectY, layer.transform.position);
			}
		}
	}

    protected Vector3 CalculatePixelPerfectPosition(float unperfectPositionX, float unperfectPositionY, Vector3 oldPosition)
    {
		//Calculate the position in pixel-space (Still  a floating point number at this point)
		float resolutionPositionX = unperfectPositionX * m_pixelsPerUnit;
        float resolutionPositionY = unperfectPositionY * m_pixelsPerUnit;

		//Truncate the calcualted position to an integer and store in a seperate variable
		int resolutionPositionIntX = Mathf.FloorToInt(resolutionPositionX);
		int resolutionPositionIntY = Mathf.FloorToInt(resolutionPositionY);

		//Subtract the integer component of the resolution to get a decimal "remainder"
		resolutionPositionX -= resolutionPositionIntX;
		resolutionPositionY -= resolutionPositionIntY;

		//If the reaminder is greater than or equal to 0.5 (half a pixel) then increase the value of the integer position
		if (resolutionPositionX >= 0.5)
        {
			resolutionPositionIntX++;
        }

        if (resolutionPositionY >= 0.5)
        {
			resolutionPositionIntY++;
        }

		//Convert the resolution position back into Unity co-ordinate space by multiplying by the reciprocal (1/PixelsPerUnit)

		float newX = resolutionPositionIntX * m_pixelsPerUnitReciprocal;
		float newY = resolutionPositionIntY * m_pixelsPerUnitReciprocal;

		return new Vector3(newX, newY, oldPosition.z); //Z should never change.

    }
}

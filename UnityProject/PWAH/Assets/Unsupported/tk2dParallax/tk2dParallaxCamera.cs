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
				//layer.transform.position = new Vector3(unperfectX, unperfectY, rootPosition.z);
			}
		}
	}

    protected Vector3 CalculatePixelPerfectPosition(float unperfectPositionX, float unperfectPositionY, Vector3 oldPosition)
    {
        float PPURecip = 1.0f / 100.0f; //TODO: change to grab PPU
        float resolutionDivisionX = unperfectPositionX / PPURecip;
        float resolutionDivisionY = unperfectPositionY / PPURecip;

        //Calculate X
        int resolutionDivisionIntX = Mathf.FloorToInt(resolutionDivisionX);
        resolutionDivisionX -= resolutionDivisionIntX;
        if (resolutionDivisionX >= 0.5)
        {
            resolutionDivisionIntX++;
        }

        //Calculate Y
        int resolutionDivisionIntY = Mathf.FloorToInt(resolutionDivisionY);
        resolutionDivisionY -= resolutionDivisionIntY;
        if (resolutionDivisionY >= 0.5)
        {
            resolutionDivisionIntY++;
        }

		float newX = resolutionDivisionIntX * PPURecip;
		float newY = resolutionDivisionIntY * PPURecip;

		return new Vector3(newX, newY, oldPosition.z); //Z should never change.

    }
}

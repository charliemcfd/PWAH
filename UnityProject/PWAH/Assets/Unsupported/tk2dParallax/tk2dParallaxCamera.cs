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
	void Update () 
	{
		Vector3 rootOffset = transform.position - rootPosition;
		foreach (var layer in layers)
		{
			if (layer.transform != null)
			{
                layer.transform.position = new Vector3(rootPosition.x + rootOffset.x * layer.speedX, rootPosition.y + rootOffset.y * layer.speedY, rootPosition.z);
            }
        }
	}
}

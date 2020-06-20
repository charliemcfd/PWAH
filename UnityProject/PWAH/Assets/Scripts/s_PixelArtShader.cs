using UnityEngine;
using System.Collections;

public class s_PixelArtShader : MonoBehaviour {

    private tk2dCamera mainCamera;
    private float defaultTexelsPerPixel;

	private int m_lastScreenHeight;
	private int m_lastScreenWidth;

    // Use this for initialization
    void Start () {
        mainCamera = gameObject.GetComponent<tk2dCamera>();

		m_lastScreenHeight = Screen.height;
		m_lastScreenWidth = Screen.width;

		RecalculateShaderParameters();

	}
	
	// Update is called once per frame
	void Update () 
	{
		//If the screen resolution has changed, we need to recalculate the shader parameters
		if(m_lastScreenHeight != Screen.height || m_lastScreenWidth != Screen.width)
		{
			m_lastScreenHeight = Screen.height;
			m_lastScreenWidth = Screen.width;

			RecalculateShaderParameters();
		}

		Shader.SetGlobalFloat("texelsPerPixel", defaultTexelsPerPixel / mainCamera.ZoomFactor);
    }

	void RecalculateShaderParameters()
	{
		float nativeAspectRatio = mainCamera.NativeResolution.x / mainCamera.NativeResolution.y;
		float aspectRatio = (float)Screen.width / (float)Screen.height;

		if (aspectRatio > nativeAspectRatio)
		{
			defaultTexelsPerPixel = mainCamera.NativeResolution.y / Screen.height;
		}
		else
		{
			defaultTexelsPerPixel = mainCamera.NativeResolution.x / Screen.width;
		}
	}
}



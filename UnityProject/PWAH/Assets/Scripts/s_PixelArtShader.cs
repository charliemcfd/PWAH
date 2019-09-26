using UnityEngine;
using System.Collections;

public class s_PixelArtShader : MonoBehaviour {

    private tk2dCamera mainCamera;
    private float defaultTexelsPerPixel;

    // Use this for initialization
    void Start () {
        mainCamera = gameObject.GetComponent<tk2dCamera>();

        float nativeAspectRatio = mainCamera.NativeResolution.x / mainCamera.NativeResolution.y;
        float aspectRatio = (float)Screen.width / (float)Screen.height;

        if (aspectRatio > nativeAspectRatio)
        {
            defaultTexelsPerPixel = mainCamera.NativeResolution.y / Screen.height;
        }
        else {
            defaultTexelsPerPixel = mainCamera.NativeResolution.x / Screen.width;
        }
    }
	
	// Update is called once per frame
	void Update () {

        Shader.SetGlobalFloat("texelsPerPixel", defaultTexelsPerPixel / mainCamera.ZoomFactor);
    }
}



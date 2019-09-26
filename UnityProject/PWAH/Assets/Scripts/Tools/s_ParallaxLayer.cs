using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
public class s_ParallaxLayer : MonoBehaviour {

    public float m_fDistance;

    private const float m_fMAX_DISTANCE = 80.0f;
    private float m_fParallaxFactor;

    private Vector3 m_vecLayerRefPosition;
    private Vector3 m_vecCameraRefPosition;
    private Transform m_CameraTransform;


	// Use this for initialization
	void Start () {

        m_CameraTransform = null;

        m_fParallaxFactor = m_fDistance / m_fMAX_DISTANCE;
        m_fParallaxFactor = m_fParallaxFactor > 1 ? 1 : m_fParallaxFactor;

        m_vecLayerRefPosition = transform.position;



    }

    void FirstUpdate()
    {
        
    }
	
	// Update is called once per frame
	void Update () {

        if(m_CameraTransform == null)
        {
            m_CameraTransform = GameSystemPointers.instance.m_Camera.transform;
            m_vecCameraRefPosition = GameSystemPointers.instance.m_CameraLimiter.m_fvecReferencePosition;
        }

        Vector3 _vecCameraDisplacement = (m_CameraTransform.position - m_vecCameraRefPosition) * m_fParallaxFactor;

        _vecCameraDisplacement.z = m_fDistance;

        transform.position = m_vecLayerRefPosition + _vecCameraDisplacement;
	}
}

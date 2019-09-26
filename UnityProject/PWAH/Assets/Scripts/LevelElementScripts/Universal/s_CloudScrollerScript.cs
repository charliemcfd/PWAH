using UnityEngine;
using System.Collections;

public class s_CloudScrollerScript : MonoBehaviour {

    public float m_StartPositionX; //Position to reset the cloud to
    public float m_EndPositionX;

    public float m_Speed = 0.01f;

	// Use this for initialization
	void Start () {

    }
	
	// Update is called once per frame
	void Update () {


        ProcessReset();

        //Only need basic translation as clouds don't interact with anything
        this.transform.localPosition = new Vector3(this.transform.localPosition.x + m_Speed * Time.deltaTime, this.transform.localPosition.y, this.transform.localPosition.z);
	
	}

    private void ProcessReset()
    {
        //Check to see if we have exceeded the maximum position.
        bool _bReset = false;
        if (this.transform.localPosition.x > m_EndPositionX)
        {
            if (m_Speed > 0)
            {
                _bReset = true;
            }
        }
        else if (m_Speed < 0)
        {
            _bReset = true;
        }
        if (_bReset)
        {
            this.transform.localPosition = new Vector3(m_StartPositionX, this.transform.localPosition.y, this.transform.localPosition.z);
        }
    }
}

﻿using UnityEngine;
using System.Collections;

public class s_HeadTrigger : MonoBehaviour {

	private bool m_bTriggered;
	private float m_fCollisionVelocity;
    private s_EntityPlayer m_PlayerScript;


    // Use this for initialization
    void Start () {
		
		m_bTriggered = false;

        m_PlayerScript = GetComponentInParent<s_EntityPlayer>();

        if (!m_PlayerScript)
            Debug.Log("HEADTRIGGER Couldnt get player script");

    }
	
	// Update is called once per frame
	void Update () {

        if (! m_bTriggered)
		{
			//This will take note of the velocity of the Rigidbody2D, unless a collision has occurred. This should allow us to get the velocity before a collision occurred.
			m_fCollisionVelocity = transform.parent.GetComponent<Rigidbody2D>().velocity.magnitude;
		}
		
	}
	
	void OnTriggerEnter2D(Collider2D other)
	{
        //Ignore triggers from parent
        if ( m_PlayerScript.GetShouldIgnore(other.tag))
		{
			return;
		}

		m_bTriggered = true;
	}


	public float GetCollisionVelocity()
	{
		return m_fCollisionVelocity;
	}
	
	public bool GetTriggered()
	{
		return m_bTriggered;
	}
	
	public void SetTriggered( bool _bTriggered)
	{
		m_bTriggered = _bTriggered;
	}
}

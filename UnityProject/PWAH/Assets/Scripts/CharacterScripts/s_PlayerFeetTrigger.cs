using UnityEngine;
using System.Collections;

public class s_PlayerFeetTrigger : MonoBehaviour {

	private bool m_bFeetTriggered;
	private bool m_bFeetTriggerStay;
    private s_EntityPlayer m_PlayerScript;

    void Start () {

		m_bFeetTriggered = false;
		m_bFeetTriggerStay = false;

        m_PlayerScript = GetComponentInParent<s_EntityPlayer>();

        if (!m_PlayerScript)
            Debug.Log("FEETTRIGGER Couldnt get player script");
    }

	void FixedUpdate()
	{

		//m_bFeetTriggerStay = false;
		//m_bFeetTriggered = false;
	}
	

	void OnTriggerEnter2D(Collider2D other)
	{
        //Ignore triggers from parent
        if (m_PlayerScript.GetShouldIgnore(other.tag))
        {
            return;
		}
        //Debug.Log("Collision Trigger");

        m_bFeetTriggered = true;

        m_PlayerScript.OnChildTriggerEnter(other, s_EntityPlayer.eTriggerType.eTT_PlayerFeetTrigger);
	}

	void OnTriggerStay2D(Collider2D other)
	{
        //Ignore triggers from parent
        if (m_PlayerScript.GetShouldIgnore(other.tag))
        {
            return;
		}
		
		m_bFeetTriggerStay = true;
	}
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (m_PlayerScript.GetShouldIgnore(other.tag))
        {
            return;
        }

        m_bFeetTriggerStay = false;
        m_bFeetTriggered = false;
    }

    public void SetFeetTriggered(bool _bTriggered)
    {
        m_bFeetTriggered = _bTriggered;
    }
	public bool GetFeetTriggered()
	{
		return m_bFeetTriggered;
	}

	public bool GetFeetTriggerStay()
	{
		return m_bFeetTriggerStay;
	}

}




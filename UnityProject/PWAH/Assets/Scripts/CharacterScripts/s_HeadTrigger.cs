using UnityEngine;
using System.Collections;

public class s_HeadTrigger : MonoBehaviour {

	private bool m_bTriggered;
	private float m_fCollisionVelocity;
    // Number of fixed updates that the head trigger has had items present within it. If this exceeds a certain value the player should be determined to be "stuck" and should be destroyed
    private float m_HeadTriggerActiveTimer; 
    private s_EntityPlayer m_PlayerScript;

    private int m_itemsInTrigger;
    // Use this for initialization
    void Start () {
		
		m_bTriggered = false;

        m_PlayerScript = GetComponentInParent<s_EntityPlayer>();

        m_itemsInTrigger = 0;

		if (!m_PlayerScript)
		{
			Debug.LogError("s_HeadTrigger::Start - Could not get player script");
		}

    }
	
	// Update is called once per frame
	void Update () {

        if (!m_bTriggered)
		{
			//This will take note of the velocity of the Rigidbody2D, unless a collision has occurred. This should allow us to get the velocity before a collision occurred.
			m_fCollisionVelocity = transform.parent.GetComponent<Rigidbody2D>().velocity.magnitude;
		}
		
	}

    private void FixedUpdate()
    {
        if(m_itemsInTrigger > 0)
        {
            m_HeadTriggerActiveTimer++;
            if(m_HeadTriggerActiveTimer > 10)
            {
                m_PlayerScript.OnPlayerStuck();
            }
        }
        else
        {
            m_HeadTriggerActiveTimer = 0;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
	{
        //Ignore triggers from parent
        if ( m_PlayerScript.GetShouldIgnore(other))
		{
			return;
		}
        m_itemsInTrigger++;
        m_PlayerScript.OnChildTriggerEnter(other, s_EntityPlayer.eTriggerType.eTT_HeadTrigger);
		m_bTriggered = true;
	}


    private void OnTriggerExit2D(Collider2D other)
    {
		if (m_PlayerScript.GetShouldIgnore(other))
		{
			return;
		}
		m_itemsInTrigger--;
    }

    public int GetNumItemsInTrigger()
    {
        return m_itemsInTrigger;
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

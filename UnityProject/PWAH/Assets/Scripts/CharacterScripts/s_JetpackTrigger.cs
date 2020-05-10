using UnityEngine;
using System.Collections;

public class s_JetpackTrigger : MonoBehaviour {

	private bool m_bTriggered;
	private float m_fStayTimer;
    private s_EntityPlayer m_PlayerScript;
    private int m_itemsInTrigger;
	// Use this for initialization
	void Start () {
	
		m_bTriggered = false;
		m_fStayTimer = 0.0f;

        m_PlayerScript = GetComponentInParent<s_EntityPlayer>();

        m_itemsInTrigger = 0;

        if (!m_PlayerScript)
            Debug.Log("JETPACKTRIGGER Couldnt get player script");


    }

    // Update is called once per frame
    void Update () {
	
		//m_fStayTimer = 
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		//Ignore triggers from parent
		if(m_PlayerScript.GetShouldIgnore(other.tag))
		{
			return;
		}
        m_itemsInTrigger++;

		m_bTriggered = true;
        m_PlayerScript.OnChildTriggerEnter(other, s_EntityPlayer.eTriggerType.eTT_JetPackTrigger);

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        m_itemsInTrigger--;
    }

    void OnTriggerStay2D(Collider2D other)
	{
	/*	//Ignore triggers from parent
		if(other.tag == "PlayerBody" || other.tag == "PlayerJetpack" || other.tag == "PlayerFeet")
		{
			return;
		}
		
		m_bTriggered = true;*/
	}

	public bool GetTriggered()
	{
		return m_bTriggered;
	}

	public void SetTriggered( bool _bTriggered)
	{
		m_bTriggered = _bTriggered;
	}
    public int GetNumItemsInTrigger()
    {
        return m_itemsInTrigger;
    }

    public bool IsTouching(Collider2D other)
    {
        if (other)
        {
            Collider2D thisCollider = GetComponent<Collider2D>();
            if (thisCollider)
            {
                return thisCollider.IsTouching(other);
            }
        }
        return false;
    }
}

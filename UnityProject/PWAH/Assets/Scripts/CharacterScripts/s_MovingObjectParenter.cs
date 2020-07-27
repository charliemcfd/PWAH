using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class s_MovingObjectParenter : MonoBehaviour
{

	Collider2D m_collider2D = null;


    // Start is called before the first frame update
    void Start()
    {
		m_collider2D = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if(collision.gameObject.CompareTag("PlayerJetpack"))
		{
			HandlePlayerCollision(collision);
		}
		else
		{
			HandleGenericCollision(collision);
		}

		//if(m_feetTrigger && m_playerScript)
		{
			//Collider2D _feetCollider = m_feetTrigger.GetComponent<Collider2D>();
			//if (_feetCollider)
			{
				//if(_feetCollider.IsTouching(collision.collider)
					//&& m_playerScript.IsGrounded())
				{
					//transform.parent = collision.transform;// SetParent(collision.collider.transform);
				}
			}
		}
	}
	/*On Collision
	 * - Check if colliding object allows us to parent to it
	 * - Check if feet trigger is also colliding with this same object - Maybe this should just be done via  a callback from the feet trigger?
	 * - check if entityplayer IsGrounded
		
		HANDLE EDGE CASE FOR WHEN SOMETHING CLIPS UNDER THE JETPACK TO THE LEFT OR RIGHT
		-Get Collision location
		- If collision is at the left or right edge of the feet trigger, do not parent.
	 * - Set Parent
	*/

	private void HandlePlayerCollision(Collision2D collision)
	{
		if (m_collider2D)
		{
			s_PlayerFeetTrigger _feetTrigger = collision.gameObject.GetComponentInChildren<s_PlayerFeetTrigger>();
			s_EntityPlayer _playerScript = collision.gameObject.GetComponent<s_EntityPlayer>();
			if (_feetTrigger && _playerScript/* && _playerScript.IsGrounded()*/)
			{
				Collider2D _feetCollider = _feetTrigger.GetComponent<Collider2D>();
				if (_feetCollider )
				{
					if (_feetCollider.IsTouching(m_collider2D))
					{

						collision.transform.parent = transform;
					}
				}
			}
		}
	}

	private void HandleGenericCollision(Collision2D collision)
	{
		collision.transform.parent = transform;
	}
}

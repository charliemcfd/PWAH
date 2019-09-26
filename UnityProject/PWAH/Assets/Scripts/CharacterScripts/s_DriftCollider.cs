using UnityEngine;
using System.Collections;

public class s_DriftCollider : MonoBehaviour {

	private bool m_bCollisionThisFrame;
	public BoxCollider2D m_Collider;
	// Use this for initialization
	void Start () {

        m_Collider.enabled = false;
		m_bCollisionThisFrame = false;
		//GetComponent<Rigidbody2D>().detectCollisions = false;
	}
	
	// Update is called once per frame
	void Update () {
	
		m_bCollisionThisFrame = false;
	}

	public void SetActive(bool _bActive)
	{
		m_Collider.enabled = _bActive;
		//GetComponent<Rigidbody2D>().detectCollisions = _bActive;
	}
	public bool GetCollisionThisFrame()
	{
		return m_bCollisionThisFrame;
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		if(collision.collider.tag == "PlayerBody" || collision.collider.tag == "PlayerJetpack")
		{
			return;
		}

		Debug.Log (collision.collider.tag);

		m_bCollisionThisFrame = true;
	}
}

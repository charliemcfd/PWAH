using UnityEngine;
using System.Collections;

public class s_DriftCollider : MonoBehaviour {

	private BoxCollider2D m_Collider;
	// Use this for initialization
	void Start () {

		m_Collider = GetComponent<BoxCollider2D>();
		if (m_Collider)
		{
			m_Collider.enabled = false;
		}
		else
		{
			Debug.LogError("s_DriftCollider::Start - Could not get collider component");
		}
	}	

	public void SetActive(bool _bActive)
	{
		if (m_Collider)
		{
			m_Collider.enabled = _bActive;
		}
		else
		{
			Debug.LogError("s_DriftCollider::SetActive - Collider Component is Null");
		}
	}
}

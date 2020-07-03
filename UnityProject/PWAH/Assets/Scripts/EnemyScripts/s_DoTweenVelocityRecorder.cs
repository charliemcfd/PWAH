using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This is a helper script that records a "fake" velocity on DoTween objects. 
 * By using DoTween on an object with a rigidbody, the velocity of the rigidbody is not internally changed.
 * With this script, we can record what the velocity is (via use of delta-positions) so that we can use it in other functions, such as collision resolution.
 */
public class s_DoTweenVelocityRecorder : MonoBehaviour
{

	protected Vector2 m_previousPosition;
	protected Vector2 m_velocity;
	protected Rigidbody2D m_rigidbody2D;

    // Start is called before the first frame update
    void Start()
    {
		m_rigidbody2D = GetComponent<Rigidbody2D>();
		if (m_rigidbody2D)
		{
			m_previousPosition = m_rigidbody2D.position;
		}
		m_velocity = Vector2.zero;


	}

    // Update is called once per frame
    void Update()
    {
		if (m_rigidbody2D)
		{
			m_velocity = (m_rigidbody2D.position - m_previousPosition) / Time.deltaTime;
			m_previousPosition = m_rigidbody2D.position;
		}

	}

	public Vector2 GetRecordedVelocity()
	{
		return m_velocity;
	}
}

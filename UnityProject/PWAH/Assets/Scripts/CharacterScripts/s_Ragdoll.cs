﻿using UnityEngine;
using System.Collections;

public class s_Ragdoll : MonoBehaviour {


	private bool m_bShouldStop;
    private bool m_bFirstImpactOccurred;
	public float m_fMaxSpeed;
    public float m_lowVelocityTimer;

	protected Rigidbody2D m_rigidBody2D;
	// Use this for initialization
	void Start () {
	
		m_bShouldStop = false;
        m_bFirstImpactOccurred = false;

        m_lowVelocityTimer = 0;

		m_rigidBody2D = GetComponent<Rigidbody2D>();

	}
	
	// Update is called once per frame
	void Update () {

        float _fVelocityMagnitude = GetComponent<Rigidbody2D>().velocity.magnitude;
        if (_fVelocityMagnitude <= 0.3 && !m_bShouldStop && m_bFirstImpactOccurred)
        {
            m_lowVelocityTimer += Time.deltaTime;
            if (m_lowVelocityTimer > 0.075f)
            {
                tk2dSpriteAnimator _Animator = GetComponent<tk2dSpriteAnimator>();
                if (!_Animator.IsPlaying("DeathHitToRest"))
                {

                    float _fRotationValue = GetComponent<Rigidbody2D>().rotation;
                    _fRotationValue = _fRotationValue % 360;
                    if (_fRotationValue < 0)
                    {
                        _fRotationValue += 360;
                    }

                    //Invert the scale of the entity based on the rotation value so that the death to rest animation always plays facing "upward"
                    transform.localScale = new Vector3(_fRotationValue > 180 ? 1 : -1, 1, 1);

                    m_bShouldStop = true;
                    _Animator.Play("DeathHitToRest");
                }
            }
        }
        else if(_fVelocityMagnitude > 0.1f)
        {
            m_lowVelocityTimer = 0.0f;
        }


    }

	public bool GetAtRest()
	{
		//if(Rigidbody2D.velocity.sqrMagnitude < 0.1f)
		if(GetComponent<Rigidbody2D>().IsSleeping())
		{
			return true;
		}

		return false;
	}
	

	void OnCollisionEnter2D( Collision2D collision)
	{
        //m_bShouldStop = true;


        //Take the first contact as the one we will use
        if(collision.contacts.GetLength(0) > 0)
        {
            //Debug.Log(collision.contacts[0].normal);
            CalculateImapctDirection(collision.contacts[0].point);
        }

        m_bFirstImpactOccurred = true;

    }

	void FixedUpdate()
	{
		LimitVelocity();
	}

	private void LimitVelocity()
	{
		if(m_rigidBody2D.velocity.magnitude > m_fMaxSpeed)
		{
			if(m_rigidBody2D.isKinematic == false)
			{
				Vector3 clampedVelocityVector = Vector3.ClampMagnitude(m_rigidBody2D.velocity, m_fMaxSpeed);
				Vector3 interpolatedVelocityVector = Vector3.Lerp(m_rigidBody2D.velocity, clampedVelocityVector, Time.fixedDeltaTime * 5.0f);
				m_rigidBody2D.velocity = interpolatedVelocityVector;
			}
		}
	}

    private void CalculateImapctDirection(Vector2 _vecImpact)
    {
        Vector2 _LocalImpact = transform.InverseTransformPoint(_vecImpact);

        tk2dSpriteAnimator _Animator = GetComponent<tk2dSpriteAnimator>();
        float _fVelocityMagnitude = GetComponent<Rigidbody2D>().velocity.magnitude;

        if (_fVelocityMagnitude > 1.5 || !m_bFirstImpactOccurred)
        {
            m_bShouldStop = false;
            if (_LocalImpact.x > 0)
            {
                _Animator.Play("DeathHitRight");
            }
            else
            {
                _Animator.Play("DeathHitLeft");
            }
        }
    }
}

using UnityEngine;
using System.Collections;

public class s_Ragdoll : MonoBehaviour {

	//Note:: Not actually a ragdoll: A Sprite with a collider that bounces around and changes animation as it collides/comes to rest.

	private bool m_bShouldStop;
    private bool m_bFirstImpactOccurred;
	public float m_fMaxSpeed;
    public float m_lowVelocityTimer;

	//Component references
	protected Rigidbody2D m_rigidBody2D;
	protected tk2dSpriteAnimator m_spriteAnimator;

	//Animation Clips
	protected tk2dSpriteAnimationClip m_deathHitToRestClip;
	protected tk2dSpriteAnimationClip m_deathHitLeftClip;
	protected tk2dSpriteAnimationClip m_deathHitRightClip;

	// Use this for initialization
	void Start () {
	
		m_bShouldStop = false;
        m_bFirstImpactOccurred = false;

        m_lowVelocityTimer = 0;

		m_rigidBody2D = GetComponent<Rigidbody2D>();
		if(!m_rigidBody2D)
		{
			Debug.LogError("s_Ragdoll::Start - Could not get rigid body 2d component");
		}

		m_spriteAnimator = GetComponent<tk2dSpriteAnimator>();
		if (m_spriteAnimator)
		{
			InitializeAnimationClips();
		}
		else
		{
			Debug.LogError("s_Ragdoll::Start - Could not get sprite animator component");
		}
	}

	void InitializeAnimationClips()
	{
		m_deathHitToRestClip = m_spriteAnimator.GetClipByName("DeathHitToRest");
		if (m_deathHitToRestClip == null)
		{
			Debug.LogError("s_Ragdoll::InitializeAnimationClips - Could not get sprite animation clip DeathHitToRest");
		}

		m_deathHitLeftClip = m_spriteAnimator.GetClipByName("DeathHitLeft");
		if (m_deathHitToRestClip == null)
		{
			Debug.LogError("s_Ragdoll::InitializeAnimationClips - Could not get sprite animation clip DeathHitLeft");
		}

		m_deathHitRightClip = m_spriteAnimator.GetClipByName("DeathHitRight");
		if (m_deathHitToRestClip == null)
		{
			Debug.LogError("s_Ragdoll::InitializeAnimationClips - Could not get sprite animation clip DeathHitRight");
		}
	}
	
	// Update is called once per frame
	void Update () {

		if (m_rigidBody2D
			&& m_spriteAnimator
			&& m_deathHitToRestClip != null)
		{
			float _fVelocityMagnitude = m_rigidBody2D.velocity.magnitude;
			if (_fVelocityMagnitude <= 0.3f && !m_bShouldStop && m_bFirstImpactOccurred)
			{
				m_lowVelocityTimer += Time.deltaTime;
				if (m_lowVelocityTimer > 0.075f)
				{
					//If we are not yet playing the clip that tranisitions us to the resting animation
					if (!m_spriteAnimator.IsPlaying(m_deathHitToRestClip))
					{

						float _fRotationValue = m_rigidBody2D.rotation;
						_fRotationValue = _fRotationValue % 360;
						if (_fRotationValue < 0)
						{
							_fRotationValue += 360;
						}

						//Invert the scale of the entity based on the rotation value so that the death to rest animation always plays facing "upward"
						transform.localScale = new Vector3(_fRotationValue > 180 ? 1 : -1, 1, 1);

						m_bShouldStop = true;
						m_spriteAnimator.Play(m_deathHitToRestClip);
					}
				}
			}
			else if (_fVelocityMagnitude > 0.1f)
			{
				m_lowVelocityTimer = 0.0f;
			}
		}

    }

	public bool GetAtRest()
	{
		if(m_rigidBody2D && m_rigidBody2D.IsSleeping())
		{
			return true;
		}

		return false;
	}
	

	void OnCollisionEnter2D( Collision2D collision)
	{
        //Take the first contact as the one we will use
        if(collision.contacts.GetLength(0) > 0)
        {
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
		if (m_rigidBody2D && m_spriteAnimator)
		{
			Vector2 _LocalImpact = transform.InverseTransformPoint(_vecImpact);

			float _fVelocityMagnitude = m_rigidBody2D.velocity.magnitude;

			if (_fVelocityMagnitude > 1.5 || !m_bFirstImpactOccurred)
			{
				m_bShouldStop = false;
				if (_LocalImpact.x > 0)
				{
					m_spriteAnimator.Play(m_deathHitLeftClip);
				}
				else
				{
					m_spriteAnimator.Play(m_deathHitRightClip);
				}
			}
		}
    }
}

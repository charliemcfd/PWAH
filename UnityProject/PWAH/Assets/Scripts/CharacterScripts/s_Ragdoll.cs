using UnityEngine;
using System.Collections;

public class s_Ragdoll : MonoBehaviour {


	private bool m_bShouldStop;
    private bool m_bFirstImpactOccurred;
	public float m_fMaxSpeed;
	// Use this for initialization
	void Start () {
	
		m_bShouldStop = false;
        m_bFirstImpactOccurred = false;


    }
	
	// Update is called once per frame
	void Update () {

        float _fVelocityMagnitude = GetComponent<Rigidbody2D>().velocity.magnitude;
        if (_fVelocityMagnitude <= 0.3 && !m_bShouldStop && m_bFirstImpactOccurred)
        {
            tk2dSpriteAnimator _Animator = GetComponent<tk2dSpriteAnimator>();
            if (!_Animator.IsPlaying("DeathHitSlow2"))
            {
                if(_Animator.IsPlaying("DeathHitLeft"))
                {
                    transform.localScale = new Vector3(-1, 1, 1);
                }
                m_bShouldStop = true;
                _Animator.Play("DeathHitSlow2");
            }
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
		if(GetComponent<Rigidbody2D>().velocity.magnitude > m_fMaxSpeed)
		{
			if(GetComponent<Rigidbody2D>().isKinematic == false)
			{
				GetComponent<Rigidbody2D>().velocity = Vector3.ClampMagnitude(GetComponent<Rigidbody2D>().velocity, m_fMaxSpeed);
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

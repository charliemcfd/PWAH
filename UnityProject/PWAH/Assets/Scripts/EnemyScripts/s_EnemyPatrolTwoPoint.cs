using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DraggablePointRelative : PropertyAttribute { }
public class s_EnemyPatrolTwoPoint : MonoBehaviour
{
	// Start is called before the first frame update

	[DraggablePointRelative] public Vector3 m_pointA = new Vector3(-1,0,0); //Starting point of the path
	[DraggablePointRelative] public Vector3 m_pointB = new Vector3(1, 0, 0); //End point of the path

	public float m_AtoBTime; //Time taken between point A and Point B
	public float m_BtoATime; //Time taken between point B and point A

	public float m_pointAPauseTime; //Length of pause when point A is reached
	public float m_pointBPauseTime; //Length of pause when point B is reached

	public bool m_startAtPointA; //Indicates whether the object should be snapped to point A upon instantiation
	

    void Start()
    {
		if (m_startAtPointA)
		{
			//Set transform to be the location of point A
			transform.position = transform.position + m_pointA;

			//Adjust point B so that it maintains the same world-space position
			m_pointB -= m_pointA;

			//Set point A to be zero
			m_pointA = Vector3.zero;

		}
		CreateSequence();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	protected void CreateSequence()
	{
		//Get pointer to rigidbody
		Rigidbody2D rigidBody = GetComponent<Rigidbody2D>();

		//Create Tween parameters
		TweenParams _TweenParams = new TweenParams();
		_TweenParams.SetEase(Ease.Linear);
		//_TweenParams.SetRelative(true);
		Sequence _movementSequence = DOTween.Sequence();
		_movementSequence.Append(rigidBody.DOMove(transform.position + m_pointB,
										m_AtoBTime)
										.SetAs(_TweenParams));
		_movementSequence.AppendInterval(m_pointAPauseTime).OnComplete(PlayAnimation);
		_movementSequence.Append(rigidBody.DOMove(transform.position + m_pointA,
												m_BtoATime)
												.SetAs(_TweenParams));
		_movementSequence.AppendInterval(m_pointBPauseTime).OnComplete(PlayAnimation);

		_movementSequence.SetLoops(-1);
		//_movementSequence.SetUpdate(UpdateType.Fixed);
		_movementSequence.Play();

	}

	protected void PlayAnimation()
	{
		Debug.Log("Test - Would play animation");
	}

}

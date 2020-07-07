using UnityEngine;
using System.Collections;

public class s_SmallExplosion : MonoBehaviour {


	public GameObject m_PrefabSmokeSprite;


	private tk2dSpriteAnimator m_Animator;
	// Use this for initialization
	void Start () {
		
		
		m_Animator = GetComponent<tk2dSpriteAnimator>();
		m_Animator.AnimationCompleted += AnimationComplete;
		m_Animator.AnimationEventTriggered += HandleAnimationEvent;

		//Set Z Position of this so that its always going to be infront of everything else

		this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -5.0f);		
	}
		
	void AnimationComplete(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip)
	{
		//Instantiate(m_PrefabSmokeSprite, this.transform.position, Quaternion.identity);
		Destroy(this.gameObject);
	}

	void HandleAnimationEvent(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frameNum)
	{
		if(clip.GetFrame(frameNum).eventInfo == "SpawnSmoke")
		{
			Instantiate(m_PrefabSmokeSprite, new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z + 0.1f), Quaternion.identity);

		}
	}
}
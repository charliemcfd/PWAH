using UnityEngine;
using System.Collections;

public class s_SmallSmoke : MonoBehaviour {

	private tk2dSpriteAnimator m_Animator;
	// Use this for initialization
	void Start () {
		
		
		m_Animator = GetComponent<tk2dSpriteAnimator>();
		m_Animator.AnimationCompleted += AnimationComplete;

		
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void AnimationComplete(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip)
	{
		Destroy(this.gameObject);
	}

}

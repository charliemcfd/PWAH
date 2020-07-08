using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using InputNamespace;
using AnimationCommandsNameSpace;
using DG.Tweening;

public class s_EntityPlayer : MonoBehaviour {


	#region Enumerations
	//===Enumerations
	enum eJetpackState {
		eJS_Normal,
		eJS_Broken
	};

	enum eJetpackID {
		eJID_Left = 0,
		eJID_Right
	};

	enum eCharacterState {
		eCS_Normal, //Normal state - Input
		eCS_Dizzy, //Dizzy State - No input after a head collision
		eCS_Damaged, // Damaged state- limited input after a collision
		eCS_Broken, // The "Dead" state. No input
		eCS_LevelEnd //Character state for when you have completed a level
	};

	enum eDriftState {
		eDS_None,
		eDS_DriftIn,
		eDS_DriftHold,
		eDS_DriftOut
	}

	enum eBoostState {
		eBS_None,
		eBS_BoostStart,
		eBS_BoostHold,
		eBS_BoostEnd
	}

	public enum eTriggerType
	{
		eTT_HeadTrigger,
		eTT_JetPackTrigger,
		eTT_PlayerFeetTrigger
	}

	public enum eCollisionType
	{
		eCT_DriftCollider
	}
	#endregion


	#region Variables - Public
	//====Public Variables 
	public float m_fRotationVelocity;
	public float m_fGravityMultiplier;
	public float m_fMaxSpeed;
	public float m_fMaxSpeedBoost;
	public float m_fThrustTimeBeforeGravity;
	public float m_fDoubleClickInterval;
	/*
	 * Note: At the moment, this is not used as it was too jerky and erattic when being applied.
	 * Solution was to simply allow the max speed to change, but apply the regular thrust force.
	 * This results in a more controlable effect.
	 * 
	public float m_fBoostForce;
*/

	public float m_fThrustForce;

	public float m_fBoostDuration;
	public float m_fDeathForce;
	public float m_fDamageKnockbackForce;
	public float m_fHeadImpactKnockbackForce;
	public float m_fMaxDizzytime;
	public float m_fDizzyTurnValue;
	public float m_fDamageKnockbackTime;
	public float m_fDriftSpeedBonus;
	public float m_fDriftTurnBonus;
	public float m_fBoostSpeedBonus;
	public float m_fBoostMaxCapacity;
	public float m_fBoostDrain;
	public float m_fBoostRefill;

	public float m_thrustDrag = 6.0f;
	public float m_noThrustDrag = 2.0f;
	public float m_driftDrag = 0.6f;

	//===Public Components
	public GameObject m_goMainCharacter;


	//=== Prefabs
	public GameObject m_PrefabFlames;
	public GameObject m_PrefabRagdoll;
	public GameObject m_PrefabSmallExplosion;
	public GameObject m_PrefabLargeExplosion;

	//=== Public Scripts
	public s_JetpackTrigger m_LeftTrigger;
	public s_JetpackTrigger m_RightTrigger;
	public s_HeadTrigger m_HeadTrigger;

	#endregion

	#region Variables - Private
	//=== Private variables
	private float m_fDoubleClickTimer;
	private float m_fCurrentMaxSpeed;
	private float m_fGravityValue;
	private float m_fBoostTimer;
	private bool m_bBoostHeld;
	private bool m_bHasThrustedSinceLastLanding;
	private float m_fBoostQuantity;
	private eBoostState m_eBoostState;
	private eCharacterState m_eCharacterState;
	private float m_velocityClampInterpolationSpeed; //1-100 scale, 100 = instant

	//Collision
	/*This property is passed used in conjunction with child colliders and their gameobjects. 
	It is used when we wish to ignore a collision/trigger response in the children, but not in the main
	player collider.*/
	LayerMask m_CollisionLayersToIgnore;

	private eJetpackState[] m_arrayEngineStatus;

	private float m_fTimeDead;
	private bool m_bDeathSkipInput;

	private bool m_bShouldRecieveInput;

	private float m_fTimeThrusting;
	private float m_fBoostRefillGrace;
	private bool m_bThrustThisFrame;
	private bool m_bThrustFromDamage;
	private float m_fGatheredRotationOther;
	private float m_fGatheredRotationController;
	private bool m_bAttemptedTurn;

	private bool m_bVisible;
	private bool m_bActive;
	private bool m_bCreateRagDoll;
	private bool m_bForceReset;

	private Vector2 m_vecPortalLocation;
	private Sequence m_EndLevelTweenSequence;
	private Sequence m_EndLevelVelocitySequence;

	private GameObject m_RagDoll;
	private s_Ragdoll m_ragDollScript;
	private GameObject m_FollowObject;

	private List<tk2dSpriteAnimator> m_listAnimators;

	private float m_fDizzyTimer;
	private eDriftState m_eDriftState;
	private bool m_bApplyDriftTurn;

	private float m_fAngleBoostAccumulated;

	//Components
	private Rigidbody2D m_RigidBody2D;
	private s_PlayerFeetTrigger m_playerFeetTrigger;
	private List<tk2dSpriteAnimator> m_listFlameAnimators;

	//Animations
	private Dictionary<string, tk2dSpriteAnimationClip> m_dAnimationClips;

	//Replay Data - TODO: Fix this up
	public List<RecordedEvent> replayData;
	private bool m_bIsReplay;
	public float m_replayZValue;
	public bool m_bReplayReachedEnd;
	private int m_lastReplayActionIndex;

	private Dictionary<tk2dSpriteAnimator, sAnimationCommand> m_dAnimationCommands;
	private Dictionary<tk2dSpriteAnimator, sAnimationCommand> m_dAnimationCommandsPrevious;


	//=====Previous Frame Variables
	private eCharacterState m_ePrevCharacterState;
	private bool m_bPrevVisible;
	private Vector3 m_PrevPos;
	private Vector3 m_PrevRot;
	private Vector3 m_PrevScale;

	//=====Collision Tracking Variables
	private Collider2D m_DamageTriggeringCollider;
	private int m_CollisionIFrames;

	private bool m_PlayerStuck;



	//TODO: Replace this with something neater later so that the input manager can be queried to find out if buttons are currently being pressed.
	bool m_bDriftHeld;
	bool m_bGrounded;
	UInt64 m_uFrameStamp;

	#endregion

	#region Methods - Initialisation
	// Use this for initialization
	void Start() {
		m_fTimeThrusting = 0.0f;
		m_fBoostRefillGrace = 0.8f;
		m_bThrustThisFrame = false;
		m_bThrustFromDamage = false;
		m_fGravityValue = 9.81f;
		m_fCurrentMaxSpeed = m_fMaxSpeed;
		m_eCharacterState = eCharacterState.eCS_Normal;
		m_ePrevCharacterState = eCharacterState.eCS_Normal;
		m_bPrevVisible = true;
		m_PrevPos = Vector3.zero;
		m_PrevRot = Vector3.zero;
		m_PrevScale = new Vector3(1, 1, 1);
		m_bVisible = true;
		m_bActive = true;
		m_bCreateRagDoll = false;
		m_fDizzyTimer = 0.0f;
		m_fDizzyTurnValue = 0.0f;
		m_eDriftState = eDriftState.eDS_None;
		m_bDriftHeld = false;
		m_bGrounded = true;
		m_bApplyDriftTurn = false;
		m_uFrameStamp = 0;
		m_eBoostState = eBoostState.eBS_None;
		m_bBoostHeld = false;
		m_fBoostQuantity = m_fBoostMaxCapacity;
		m_bAttemptedTurn = false;
		m_fAngleBoostAccumulated = 0;
		m_fTimeDead = 0;
		m_bHasThrustedSinceLastLanding = false;
		m_bShouldRecieveInput = false;
		m_EndLevelTweenSequence = null;
		m_velocityClampInterpolationSpeed = 100.0f;

		m_listFlameAnimators = new List<tk2dSpriteAnimator>();
		m_listAnimators = new List<tk2dSpriteAnimator>();
		m_arrayEngineStatus = new eJetpackState[2] { eJetpackState.eJS_Normal, eJetpackState.eJS_Normal };

		m_dAnimationClips = new Dictionary<string, tk2dSpriteAnimationClip>();
		m_dAnimationCommands = new Dictionary<tk2dSpriteAnimator, sAnimationCommand>();
		m_dAnimationCommandsPrevious = new Dictionary<tk2dSpriteAnimator, sAnimationCommand>();

		m_DamageTriggeringCollider = null;
		m_CollisionIFrames = 0;
		m_PlayerStuck = false;

		m_CollisionLayersToIgnore = LayerMask.GetMask("EndLevelPortal");

		//Get Component References
		m_RigidBody2D = GetComponent<Rigidbody2D>();
		if (m_bIsReplay && m_RigidBody2D)
		{
			m_RigidBody2D.isKinematic = true;
		}

		m_playerFeetTrigger = GetComponentInChildren<s_PlayerFeetTrigger>();

		CreateFlames();

		IndexAnimators();
		InitializeAnimationClips();

		m_lastReplayActionIndex = 0;
		m_bReplayReachedEnd = false;
	}

	private void CreateFlames()
	{
		GameObject _Flame1 = Instantiate(m_PrefabFlames, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;
		GameObject _Flame2 = Instantiate(m_PrefabFlames, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity) as GameObject;

		_Flame1.transform.parent = transform;
		_Flame2.transform.parent = transform;

		_Flame1.transform.localPosition = new Vector3(0.194f, -0.336f, 0.005f);
		_Flame2.transform.localPosition = new Vector3(-0.163f, -0.336f, 0.005f);

		tk2dSpriteAnimator _flame1Animator = _Flame1.GetComponent<tk2dSpriteAnimator>();
		tk2dSpriteAnimator _flame2Animator = _Flame2.GetComponent<tk2dSpriteAnimator>();
		m_listFlameAnimators.Add(_flame1Animator);
		m_listFlameAnimators.Add(_flame2Animator);

	}

	private void IndexAnimators()
	{

		/*
		 * Rather than tagging the component, this function pushes the animators into a list.
		 * This list is used to determine which animator should be used when replaying game data. It 
		 * is not used during normal game execution.
		 * */
		tk2dSpriteAnimator _Animator = GetComponentInChildren<tk2dSpriteAnimator>();
		m_listAnimators.Add(_Animator);


		for (int i = 0; i < m_listFlameAnimators.Count; i++)
		{
			m_listAnimators.Add(m_listFlameAnimators[i]);
		}

		//Subscribe to animationcomplete event for all animators
		for (int i = 0; i < m_listAnimators.Count; i++)
		{
			m_listAnimators[i].AnimationCompleted += AnimationComplete;
		}

	}

	private void InitializeAnimationClips()
	{
		//Grab pointers to the animation clips so that we don't have to do lots of string comparisons each time we want to play one.

		StringBuilder _stringBuilder = new StringBuilder();
		tk2dSpriteAnimationClip _clip = null;

		//Player Animations
		tk2dSpriteAnimator _playerAnimator = GetAnimatorFromIndex(0);
		if (_playerAnimator)
		{
			for(int i = 0; i < _playerAnimator.GetNumClips(); i++)
			{
				_clip = _playerAnimator.GetClipById(i);
				if(_clip != null)
				{
					m_dAnimationClips[_clip.name] = _clip;
					_clip = null;
				}
			}

		}

		//Flame Animations
		//Note:: Since both flames share the same sprite, this should suffice for both
		tk2dSpriteAnimator _flameAnimator = GetAnimatorFromIndex(1);
		if(_flameAnimator)
		{

			for (int i = 0; i < _flameAnimator.GetNumClips(); i++)
			{
				_clip = _flameAnimator.GetClipById(i);
				if (_clip != null)
				{
					m_dAnimationClips[_clip.name] = _clip;
					_clip = null;
				}
			}
		}
	}
	#endregion

	#region Destroy

	void OnDestroy()
	{
		if (m_RagDoll)
		{
			Destroy(m_RagDoll);
		}
	}
	#endregion

	#region Methods - Get/Set

	public float GetBoostQuantity()
	{
		return m_fBoostQuantity;
	}

	#endregion

	#region Methods - GameUpdate

	void Update() {

		if (m_bIsReplay)
		{
			return;
		}

		ProcessGrounded();

		switch (m_eCharacterState)
		{
			case eCharacterState.eCS_Normal:
				{
					ProcessBoost();
					ProcessThrust();
					ProcessDrag();
					break;
				}

			case eCharacterState.eCS_Dizzy:
				{
					m_bThrustThisFrame = false;
					m_fDizzyTimer -= Time.deltaTime;
					if (m_fDizzyTimer <= 0)
					{
						m_eCharacterState = eCharacterState.eCS_Normal;
					}
					break;
				}
			case eCharacterState.eCS_Damaged:
				{
					m_fDamageKnockbackTime -= Time.deltaTime;
					if (m_fDamageKnockbackTime <= 0)
					{
						m_bThrustFromDamage = true;
					}
					ProcessBoost();
					ProcessThrust();
					ProcessDrag();
					break;
				}

			case eCharacterState.eCS_Broken:
				{
					break;
				}

			case eCharacterState.eCS_LevelEnd:
				{
					break;
				}
		}

		ProcessJetpackState();


		if (m_bVisible)
		{
			UpdateAnimationState();
		}

		ProcessDrift();

		m_bAttemptedTurn = false;


	}


	void FixedUpdate()
	{
		m_CollisionIFrames--;
		if (m_bIsReplay)
		{
			ProcessReplay();
			//Increase replay frame-stamp
			m_uFrameStamp++;
			return;
		}

		ApplyGravity(m_fTimeThrusting);


		switch (m_eCharacterState)
		{
			case eCharacterState.eCS_Normal:
			case eCharacterState.eCS_Damaged:
				{
					if (m_eBoostState == eBoostState.eBS_BoostStart ||
					   m_eBoostState == eBoostState.eBS_BoostHold)
					{
						if (CanUseBoost())
						{
							ApplyBoost();
							DrainBoost(m_fBoostDrain);
						}

					}
					if (m_bThrustThisFrame || m_bThrustFromDamage)
					{
						ApplyThrust();
					}
					break;
				}
		}


		LimitVelocity();
		//Apply rotation forces here;
		ApplyRotation();

		RecordData();
		//Increase non-replay frame stamp
		m_uFrameStamp++;
	}

	void LateUpdate()
	{
		if (m_bCreateRagDoll)
		{
			CreateRagDoll();
		}
	}

	#endregion
	#region Methods - Animation



	private tk2dSpriteAnimator GetAnimatorFromIndex(int _iIndex)
	{
		/*
		 * This function takes an Index (From replay data) and then returns the animator at that index.
		 * We have to do this as the animator object that were recorded no longer exist (Deleted when the player was deleted).
		 * However, each player "Copy" will have the same number and types of animators. Using this list they should all
		 * be at the same indexes.
		 * 
		 * This means we can play back the appropriate clip on the appropriate animator.
		 * */

		tk2dSpriteAnimator _Animator = m_listAnimators[_iIndex];

		if (_Animator)
		{
			return _Animator;
		}
		return null;
	}

	private int GetIndexFromAnimator(tk2dSpriteAnimator _Animator)
	{
		for (int i = 0; i < m_listAnimators.Count; i++)
		{
			if (_Animator == m_listAnimators[i])
			{
				return i;
			}
		}
		return -1;

	}

	public void ChangeAnimation(tk2dSpriteAnimator _Animator, eAnimationCommands _eCommand, string _animationClipString, int _iFrame = 0, float _fTime = 0, bool _bReplayCommand = false)
	{
		/*
		 * This function abstracts the setting of animation into a separate control structure that can be recorded.
		 * 
		 * The animation will be set here (Meaning that multiple animaions can be set on the same animator per frame if this is called
		 * as part of the regular update loop,however, an animation command list will be held for every animator that exists. 
		 * The value commands in this list will be updated, so that when it comes to fixedupdate, only one animation can be recoreded per animator.
		 * */

		//Do not process this animation data if we are currently in replay mode, and the command that has come through has NOT come from a replay.
		if (m_bIsReplay && _bReplayCommand == false)
		{
			return;
		}

		//Return if this animator was not found in the list of animators
		int _iAnimatorIndex = GetIndexFromAnimator(_Animator);
		if(_iAnimatorIndex == -1)
		{
			return;
		}
		tk2dSpriteAnimationClip _animationClip = GetAnimationClipFromString(_animationClipString);

		switch (_eCommand)
		{
			case eAnimationCommands.Play:
				{
					_Animator.Play(_animationClip);
					_Animator.GetComponent<Renderer>().enabled = true;
					break;
				}
			case eAnimationCommands.PlayFromFrame:
				{
					_Animator.PlayFromFrame(_animationClip, _iFrame);
					break;
				}

			case eAnimationCommands.PlayFrom:
				{
					_Animator.PlayFrom(_animationClip, _fTime);
					break;
				}

			case eAnimationCommands.Stop:
				{
					_Animator.Stop();
					_Animator.GetComponent<Renderer>().enabled = false;

					break;
				}

			case eAnimationCommands.Pause:
				{
					_Animator.Pause();
					break;
				}

			case eAnimationCommands.Resume:
				{
					_Animator.Resume();
					break;
				}

			case eAnimationCommands.SetFrame:
				{
					_Animator.SetFrame(_iFrame);
					break;
				}
		}

		sAnimationCommand _AnimationCommand;
		_AnimationCommand.m_eCommand = _eCommand;
		_AnimationCommand.m_pAnimator = _Animator;
		_AnimationCommand.m_iAnimatorIndex = _iAnimatorIndex;
		_AnimationCommand.m_sClipname = _animationClipString;
		_AnimationCommand.m_iFrame = _iFrame;
		_AnimationCommand.m_fTime = _fTime;

		if (m_dAnimationCommands.ContainsKey(_Animator))
		{
			m_dAnimationCommands[_Animator] = _AnimationCommand;
		}
		else
		{
			m_dAnimationCommands.Add(_Animator, _AnimationCommand);
		}

	}

	private void AnimateFromCommand(sAnimationCommand _Command)
	{
		//Get the appropriate animator based on the recorded index
		tk2dSpriteAnimator _Animator = GetAnimatorFromIndex(_Command.m_iAnimatorIndex);
		if (_Animator)
		{
			_Command.m_pAnimator = _Animator;

			ChangeAnimation(_Command.m_pAnimator,
							_Command.m_eCommand,
							_Command.m_sClipname,
							_Command.m_iFrame,
							_Command.m_fTime,
							true);
		}
	}

	private tk2dSpriteAnimationClip GetAnimationClipFromString(string _animationClipString)
	{
		if(m_dAnimationClips.ContainsKey(_animationClipString))
		{
			return m_dAnimationClips[_animationClipString];
		}
		return null;
	}

	private void ProcessFlameAnimation( )
	{
		for(int i = 0; i < m_listFlameAnimators.Count; i++)
		{
			if (m_listFlameAnimators[i])
			{
				if (m_bThrustThisFrame && m_eBoostState == eBoostState.eBS_None)
				{
					ChangeAnimation(m_listFlameAnimators[i], eAnimationCommands.Play, "JetpackFlames");
				}
				else if (m_eBoostState == eBoostState.eBS_None)
				{
					ChangeAnimation(m_listFlameAnimators[i], eAnimationCommands.Stop, "");
				}
			}
		}
	}
	
	private void ProcessLandingAnimation(tk2dSpriteAnimator _pAnimator)
	{
		//Check if feet have been triggered
		if(m_playerFeetTrigger)
		{
			if(m_playerFeetTrigger.GetFeetTriggered())
			{

                float _fRotationValue = m_RigidBody2D.rotation;
				if(_fRotationValue >= 330 || _fRotationValue <= 30)
				{
                    m_bHasThrustedSinceLastLanding = false;
                    ChangeAnimation(_pAnimator, eAnimationCommands.Play, "LandingQuick");
					m_eDriftState=eDriftState.eDS_None;
					ActivateDriftCollider(false);
					m_playerFeetTrigger.SetFeetTriggered(false);
				}
                else if (_fRotationValue >= 290 || _fRotationValue <= 70)
                {
                    m_bHasThrustedSinceLastLanding = false;
                    m_eDriftState = eDriftState.eDS_None;
                    ActivateDriftCollider(false);
					m_playerFeetTrigger.SetFeetTriggered(false);
                }

			}
		}
	}
	
	private void ProcessFlyingAnimation(tk2dSpriteAnimator _pAnimator)
	{
		
		if(!_pAnimator.IsPlaying("LandingQuick") || m_bThrustThisFrame)
		{
			//Check for Movement, select flying animation
			if(m_bGrounded == false)
			{
                switch (m_eCharacterState)
                {
                    case eCharacterState.eCS_Normal:
                        {
                            switch (m_eDriftState)
                            {
                                case eDriftState.eDS_None:
                                    {

                                        if (m_eBoostState == eBoostState.eBS_BoostStart || m_eBoostState == eBoostState.eBS_BoostHold)
                                        {
                                            ChangeAnimation(_pAnimator, eAnimationCommands.Play, "FlyingFast");

                                        }
                                        else if(m_bHasThrustedSinceLastLanding)
                                        {
                                            ChangeAnimation(_pAnimator, eAnimationCommands.Play, "FlyingNormal");
                                        }
                                        else
                                        {
                                            if (transform.rotation.z > 0)
                                            {
                                                ChangeAnimation(_pAnimator, eAnimationCommands.Play, "BalanceLeft");

                                            }
                                            else
                                            {
                                                ChangeAnimation(_pAnimator, eAnimationCommands.Play, "BalanceRight");

                                            }
                                        }
                                        break;
                                    }
                                case eDriftState.eDS_DriftIn:
                                    {
                                        if (_pAnimator.CurrentClip.name != "TuckIn")
                                        {
                                            ChangeAnimation(_pAnimator, eAnimationCommands.Play, "TuckIn");
                                        }
                                        break;
                                    }
                                case eDriftState.eDS_DriftHold:
                                    {
                                        ChangeAnimation(_pAnimator, eAnimationCommands.Play, "TuckHold");
                                        break;
                                    }
                                case eDriftState.eDS_DriftOut:
                                    {
                                        ChangeAnimation(_pAnimator, eAnimationCommands.Play, "TuckOut");
                                        break;
                                    }
                            }

                            break;
                        }
                    case eCharacterState.eCS_Damaged:
                        {
                            ChangeAnimation(_pAnimator, eAnimationCommands.Play, "FlyingDanger");
                            break;
                        }
                }
            }
			else
			{
				if(!m_bAttemptedTurn)
				{

                    float _fAngleLanding = Vector2.Angle(Vector2.up, transform.up);

                    if(_fAngleLanding > 4)
                    {
                        if(transform.rotation.z > 0)
                        {
                            ChangeAnimation(_pAnimator, eAnimationCommands.Play, "BalanceLeft");

                        }
                        else
                        {
                            ChangeAnimation(_pAnimator, eAnimationCommands.Play, "BalanceRight");
                        }
                    }
                    else
                    {
                        ChangeAnimation(_pAnimator, eAnimationCommands.Play, "Idle");
                    }
				}
				
			}
		}
	}

	private void UpdateAnimationState()
	{
		// Get animatior
		//TODO::Update this so that we are using something other than a random int to identify the player's animator.
		tk2dSpriteAnimator _pAnimator = GetAnimatorFromIndex(0);
		
		switch(m_eCharacterState)
		{
		case eCharacterState.eCS_Normal:
		case eCharacterState.eCS_Damaged:
		{
			ProcessFlameAnimation();
			ProcessLandingAnimation(_pAnimator);
			ProcessFlyingAnimation(_pAnimator);
			break;
		}
			
		case eCharacterState.eCS_Dizzy:
		{
			if(m_eCharacterState == eCharacterState.eCS_Dizzy)
			{
				if(!_pAnimator.IsPlaying(GetAnimationClipFromString("DizzyHit")))
				{
					ChangeAnimation(_pAnimator, eAnimationCommands.Play, "DizzyLoop");
				}
				
			}
			
			for(int i = 0; i < m_listFlameAnimators.Count; i++)
			{
				if (m_listFlameAnimators[i])
				{
					ChangeAnimation(m_listFlameAnimators[i], eAnimationCommands.Stop, "");
				}
			}
			break;
		}
		}
		
	}

	public void SetVisible( bool _bVisible)
	{
		//Set this
		this.gameObject.GetComponent<Renderer>().enabled = _bVisible;

        //Set children
        Renderer[] _RendererComponents = this.GetComponentsInChildren<Renderer>();

        for(int _iComponent = 0;  _iComponent < _RendererComponents.Length; _iComponent++)
		{
            _RendererComponents[_iComponent].enabled = _bVisible;
		}
		
		m_bVisible = _bVisible;
	}

	void AnimationComplete(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip)
	{
		if(m_bIsReplay)
		{
			//Don't take any action if it's from a replay.
			return;
		}
		switch(clip.name)
		{
		case "TuckIn":
		{
			m_eDriftState = eDriftState.eDS_DriftHold;
			
			/*
			 * At this point, we turn off the regular Rigidbody2D and turn on the "DriftCollider".
			 * */
			ActivateDriftCollider(true);
			
			break;
		}
		case "TuckOut":
		{
			m_eDriftState = eDriftState.eDS_None;
			ActivateDriftCollider(false);
			break;
		}
		case "JetpackBoost_Start":
		{
			if(m_eCharacterState != eCharacterState.eCS_Broken)
			{
				for(int i = 0; i < m_listFlameAnimators.Count; i++)
				{
					if (m_listFlameAnimators[i])
					{
						ChangeAnimation(m_listFlameAnimators[i], eAnimationCommands.Play, "JetpackBoost_Loop");
					}
				}
			}
			break;
		}
			
		case "JetpackBoost_End":
		{
			m_eBoostState = eBoostState.eBS_None;
			break;
		}

		}
		
		
	}

	#endregion
	
	#region Input

    public void SetShouldRecieveInput(bool _bShouldRecieve)
    {
        m_bShouldRecieveInput = _bShouldRecieve;
    }

    public bool GetShouldRecieveInput()
    {
        return m_bShouldRecieveInput;
    }
	public void RecieveInput(float[] _arrayGameCommands)
	{
		if(m_bIsReplay)
		{
			return;
		}

        if(GameSystemPointers.instance.m_LevelScript.GetLevelState() != s_BaseLevelScript.eLevelState.eLS_Playing)
        {
            return;
        }

        int _numCommands = (int)eInputCommand_Game.eIC_Game_MAX;

        //If the state is currently in the "Broken" or "Dizzy" state, we dont want to accept any input
        if (m_eCharacterState == eCharacterState.eCS_Broken || m_eCharacterState == eCharacterState.eCS_Dizzy)
		{
            //If the character is broken (dead) and there is thrust input, activate DeathSkipInput. This forces a respawn.
            if (m_eCharacterState == eCharacterState.eCS_Broken)
            {
                float _thrustInputValue = _arrayGameCommands[(int)eInputCommand_Game.eIC_Game_Thrust];
                if(_thrustInputValue != 0)
                {
                    //Note: This bool will not be reset as it is the "final" thing a player can do
                    m_bDeathSkipInput = true;
                }
            }

            return;
		}

        //Reset Flags based on players holding input here, as they may no longer be holding them.
        m_bThrustThisFrame = false;
        m_bDriftHeld = false;
        m_bBoostHeld = false;



        for ( int _iCommand = 0; _iCommand < _numCommands; _iCommand++)
		{
            //Get the value of the input
			float _fInputValue = _arrayGameCommands[_iCommand];

            //Get the enumeration this relates to
            eInputCommand_Game _GameCommand = (eInputCommand_Game)_iCommand;

            //We don't want to do anything if the input value is zero.
            if (_fInputValue != 0)
			{
				switch(_GameCommand)
				{
				case eInputCommand_Game.eIC_Game_MoveLeft:
				{
					if(_fInputValue < 0)
					{
						_fInputValue*= -1;
					}

					if(m_bApplyDriftTurn)
					{
						TurnBody(_fInputValue, m_fDriftTurnBonus);
					}
					else
					{
						TurnBody(_fInputValue);
					}
					break;
				}

				case eInputCommand_Game.eIC_Game_MoveRight:
				{
					if(_fInputValue > 0)
					{
						_fInputValue*= -1;
					}
					if(m_bApplyDriftTurn)
					{
						TurnBody(_fInputValue, m_fDriftTurnBonus);
					}
					else
					{
						TurnBody(_fInputValue);
					}
					break;
				}

				case eInputCommand_Game.eIC_Game_Thrust:
				{
					m_bThrustThisFrame = true;
                    m_bHasThrustedSinceLastLanding = true;


                    break;
				}

				case eInputCommand_Game.eIC_Game_Boost:
				{
					StartBoost();
					break;
				}

				case eInputCommand_Game.eIC_Game_Drift:
				{
					//Can only Drift if in the normal flying state
					if(m_eCharacterState == eCharacterState.eCS_Normal)
					{
						if(m_eDriftState != eDriftState.eDS_DriftHold &&
						   m_eDriftState != eDriftState.eDS_DriftIn)
						{
							m_eDriftState = eDriftState.eDS_DriftIn;
						}
						m_bDriftHeld = true;
					}
					else
					{
						m_eDriftState = eDriftState.eDS_None;
						m_bDriftHeld = false;
						ActivateDriftCollider(false);
					}
					break;
				}
				}
			}
		}
	}

	#endregion

	#region Methods - GamePlay

	public bool GetShouldReset()
	{
        //Note: It is possible for the ragdoll to be at rest, triggering a reset, only to be woken up again.
        //In this instance, subsequent calls to "GetShouldReset" would return false, causing issues with the reset logic
        //and eventually triggering the spawning of multiple player characters. For this reason, if we ever return true from this function, we should also
        //force the player to reset, so that any subsequent calls provide the same result as the first.

        if ( m_bForceReset)
        {
            return true;
        }
		//A function that returns whether the player should be reset by the playermanager
		if(m_RagDoll)
		{
            //Increase dead timer
            m_fTimeDead += Time.deltaTime;

			if(m_ragDollScript.GetAtRest())
			{
                SetForceReset(true);
                return true;
			}

            if(m_fTimeDead >= 1.0 && m_bDeathSkipInput)
            {
                SetForceReset(true);
                return true;
            }
		}
        else if(m_PlayerStuck)
        {
            //Increase dead timer
            m_fTimeDead += Time.deltaTime;
        }

        float maxDeadTime = m_PlayerStuck ? 2.0f : 4.0f;
        if (m_fTimeDead >= maxDeadTime)
        {
            //If we have been dead for over X seconds, then just say we shold reset anyway.
            SetForceReset(true);
            return true;
        }

        return false;
	}
    public void SetForceReset(bool _bForce)
    {
        m_bForceReset = true;
    }
	private void ApplyDeath()
	{
		if(	m_eCharacterState == eCharacterState.eCS_Broken)
		{
			return;
		}

		//Set State of this
		m_eCharacterState = eCharacterState.eCS_Broken;

		//Create Explosion
		GameObject _Explosion = Instantiate(m_PrefabLargeExplosion, this.transform.position, Quaternion.identity) as GameObject;
		_Explosion.transform.localScale = new Vector3(2.0f, 2.0f, 1.0f);

		//Hide current body, create a new "ragdoll"
		SetVisible(false);

        //Only create ragdoll if player is not stuck
        if (!m_PlayerStuck)
        {
            m_bCreateRagDoll = true;
        }
		SetActive(false);

	}

	private void CreateRagDoll()
	{
		m_RagDoll = Instantiate(m_PrefabRagdoll, this.transform.position, Quaternion.identity) as GameObject;
		m_ragDollScript = m_RagDoll.GetComponent<s_Ragdoll>();

		m_RagDoll.transform.rotation = this.transform.rotation;
		Rigidbody2D _ragdollRigidBody = m_RagDoll.GetComponent<Rigidbody2D>();
		_ragdollRigidBody.AddForce(transform.up * -m_fDeathForce, ForceMode2D.Impulse);
		_ragdollRigidBody.AddTorque(0.2f, ForceMode2D.Impulse);

		if (!m_bIsReplay)
		{
			s_EventManager.CameraSetTargetObjectEvent.Invoke(m_RagDoll);
		}
		m_bCreateRagDoll = false;
	}

    public void SetActive(bool _bActive, bool _bLevelEnd = false)
    {
        //If No change, just bail out
        if(_bActive == m_bActive)
        {
            return;
        }
        //Set this
        if (_bActive)
        {
            LayerMask _characterLayer = LayerMask.NameToLayer("PlayerCharacter");
            gameObject.layer = _characterLayer;

            //Jetpack Triggers
            m_LeftTrigger.gameObject.layer = _characterLayer;
            m_RightTrigger.gameObject.layer = _characterLayer;

            //Drift Trigger
            s_DriftCollider _DriftCollider = GetComponentInChildren<s_DriftCollider>();
            _DriftCollider.gameObject.layer = _characterLayer;

        }
        else
        {
            LayerMask _voidLayer = LayerMask.NameToLayer("Void");
            gameObject.layer = _voidLayer;

            //Jetpack Triggers
            m_LeftTrigger.gameObject.layer = _voidLayer;
            m_RightTrigger.gameObject.layer = _voidLayer;

            //Drift Trigger
            s_DriftCollider _DriftCollider = GetComponentInChildren<s_DriftCollider>();
            _DriftCollider.gameObject.layer = _voidLayer;

        }

        if (!_bLevelEnd)
        {
			m_RigidBody2D.isKinematic = !_bActive;
        }

        //Set children
        Rigidbody2D[] _RigidBody2DComponents = this.GetComponentsInChildren<Rigidbody2D>();
        for(int _iComponent = 0; _iComponent < _RigidBody2DComponents.Length; _iComponent++)
		{
            if(!_bLevelEnd)
            {
                _RigidBody2DComponents[_iComponent].isKinematic = !_bActive;
            }

            if (_bActive)
            {
                LayerMask _characterLayer = LayerMask.NameToLayer("PlayerCharacter");
                _RigidBody2DComponents[_iComponent].gameObject.layer = _characterLayer;
            }
            else
            {
                LayerMask _voidLayer = LayerMask.NameToLayer("Void");
                _RigidBody2DComponents[_iComponent].gameObject.layer = _voidLayer;
            }
        }
        BoxCollider2D[] _BoxCollider2DComponents = this.GetComponentsInChildren<BoxCollider2D>();
        for (int _iComponent = 0; _iComponent < _BoxCollider2DComponents.Length; _iComponent++)
        {
            if (_bActive)
            {
                LayerMask _characterLayer = LayerMask.NameToLayer("PlayerCharacter");
                _BoxCollider2DComponents[_iComponent].gameObject.layer = _characterLayer;
            }
            else
            {
                LayerMask _voidLayer = LayerMask.NameToLayer("Void");
                _BoxCollider2DComponents[_iComponent].gameObject.layer = _voidLayer;
            }
        }

		m_bActive = _bActive;		
	}
	
    public void SetLevelCompleteState( Vector2 _vecPortalLocation)
    {
        if (m_eCharacterState != eCharacterState.eCS_LevelEnd)
        {
            //This function sets the required states etc. for being at the end of a level.
            m_bThrustThisFrame = false;
			m_RigidBody2D.drag = 0;
            m_eCharacterState = eCharacterState.eCS_LevelEnd;
            m_vecPortalLocation = _vecPortalLocation;

            //Calculate the angle we will need to be pointing once the player has slowed down
            Vector2 _targetDir = m_vecPortalLocation - new Vector2(this.transform.position.x, this.transform.position.y);


            float _fAngle = Mathf.Atan2(_targetDir.normalized.y, _targetDir.x) * 180 / Mathf.PI;
            _fAngle -= 90;

            //Create tween sequence for the end of level animation
            m_EndLevelVelocitySequence = DOTween.Sequence();
            m_EndLevelVelocitySequence.Append(DOTween.To(() => m_RigidBody2D.velocity, x => m_RigidBody2D.velocity = x, new Vector2(0, 0), 0.55f).SetEase(Ease.InQuad));
            m_EndLevelVelocitySequence.Join(transform.DORotate(new Vector3(0, 0, _fAngle), 0.7f).SetEase(Ease.InOutQuad));

            m_EndLevelVelocitySequence.Append(m_RigidBody2D.DOMove(m_vecPortalLocation, 0.5f).SetEase(Ease.InOutQuad));
            m_EndLevelVelocitySequence.Join(transform.DOScale(0, 0.4f).SetEase(Ease.InQuad).OnComplete(LevelEndComplete));
			m_EndLevelVelocitySequence.SetUpdate(UpdateType.Fixed);


		}


	}

    private void LevelEndComplete()
    {
        s_EventManager.LevelCompleteEvent.Invoke();
    }

    public void SetFollowObject( GameObject _FollowObject)
	{
		m_FollowObject = _FollowObject;
	}
	
	public GameObject GetFollowObject()
	{
		return m_FollowObject;
	}

	private void CreateSmallExplosion(Vector3 _vecLocation)
	{
		Instantiate(m_PrefabSmallExplosion, _vecLocation, Quaternion.identity);
		
		if(!m_bIsReplay)
		{
			s_GameplayRecorder.instance.AddAction(m_uFrameStamp, RecordActions.InstantiateSmallExplosion, _vecLocation);
		}
	}
	
	private void ProcessJetpackState()
	{
		if(m_eCharacterState != eCharacterState.eCS_Broken)
		{
			if(m_arrayEngineStatus[(int)eJetpackID.eJID_Left] == eJetpackState.eJS_Broken)
			{
				TurnBody(-1.0f, 0.8f, false);
			}
			
			if(m_arrayEngineStatus[(int)eJetpackID.eJID_Right] == eJetpackState.eJS_Broken)
			{
				TurnBody(1.0f, 0.8f, false);
			}
		}
	}

	public bool CanUseBoost()
	{
		return m_fBoostQuantity > 0;
	}

	public void DrainBoost(float _fDrainAmount)
	{
		//This method is public, as it may be the case that some obstacles in the game will drain your boost.

		m_fBoostQuantity -= _fDrainAmount;

		//Cap boost quantity at 0

		if (m_fBoostQuantity < 0)
		{
			m_fBoostQuantity = 0;
		}
	}

	public void AccumulateBoostRotation( float _fRotation)
	{
		/*This function refills the player's boost.
		 * This is done by accumulating an angle whenever the player rotates whilst NOT thrusting (with a grace period)
		 * */
		if(m_fBoostRefillGrace <= 0.8f)
		{
			m_fAngleBoostAccumulated += _fRotation;

			if(m_fAngleBoostAccumulated >= 360)
			{
				//Refill boost
				m_fBoostQuantity += m_fBoostRefill;


				m_fAngleBoostAccumulated -= 360;
			}
			else if(m_fAngleBoostAccumulated <= -360)
			{
				//RefillBoost

				m_fBoostQuantity += m_fBoostRefill;

				//Reset Angle
				m_fAngleBoostAccumulated += 360;

			}

			m_fBoostQuantity = Mathf.Clamp(m_fBoostQuantity,0.0f, m_fBoostMaxCapacity);
		}
		else
		{
			m_fAngleBoostAccumulated = 0.0f;
		}
	}


	#endregion

	#region Methods - Physics/Movement

	private void ApplyThrust()
	{
		if(m_eBoostState == eBoostState.eBS_None ||
		   m_eBoostState == eBoostState.eBS_BoostEnd)
		{
			//Add force in an upwards direction (Relative to character's orientation)
			m_RigidBody2D.AddForce(m_RigidBody2D.transform.up * m_fThrustForce);
			m_bThrustThisFrame = true;
		}
	}

	private void ApplyGravity(float _fTimeThrusting)
	{
		switch(m_eCharacterState)
		{
		case eCharacterState.eCS_Normal:
		case eCharacterState.eCS_Damaged:
		{
			//Cap time thrusting at 1 or 0
			if(_fTimeThrusting > m_fThrustTimeBeforeGravity)
			{
				_fTimeThrusting = m_fThrustTimeBeforeGravity;
			}
			else if (_fTimeThrusting < 0)
			{
				_fTimeThrusting = 0;
			}
			//Add force in a downwards direction
			//Multiply by 1-Timethrusting so that after an elapsed period of time, we stop applying gravity.
			float _fForceMultiplier = (-(m_fGravityValue *m_fGravityMultiplier) * (1 - _fTimeThrusting) );
			
			m_RigidBody2D.AddForce(Vector3.up * _fForceMultiplier);
			break;
		}
			
		case eCharacterState.eCS_Broken:
		case eCharacterState.eCS_Dizzy:
		{
			float _fForceMultiplier = (-(m_fGravityValue * m_fGravityMultiplier ));
			
			m_RigidBody2D.AddForce(Vector3.up * _fForceMultiplier);
			break;
			
		}
		}
	}


    private void ApplyRotation()
	{
		//We determine the rotation to be applied by the value that has come from the controler plus the value that is set by other sources (Such as being damaged)
		float _fAppliedRotation = m_fGatheredRotationController + m_fGatheredRotationOther;

        AccumulateBoostRotation(_fAppliedRotation);
		m_RigidBody2D.MoveRotation(m_RigidBody2D.rotation + _fAppliedRotation);

		if (_fAppliedRotation == 0)
		{
			float rotationDelta = m_RigidBody2D.rotation - m_PrevRot.z;
			if (m_bGrounded && rotationDelta == 0.0f)
			{
				float absAngle = Mathf.Abs(m_RigidBody2D.rotation);
				if (absAngle <= 2.0f)
				{
					m_RigidBody2D.rotation = 0.0f;
				}
			}
		}

		ResetRotation();
	}
	
	private void TurnBody(float _fAxisValue, float _fSpeedModifer = 1.0f, bool _bControler = true)
	{
		if(_bControler)
		{
			if(!m_bGrounded)
			{
				m_fGatheredRotationController = ((m_fRotationVelocity ) * _fSpeedModifer) * _fAxisValue;
			}
			else
			{
				m_bAttemptedTurn = true;
				//Todo::Replace that integer with a value that explicitly relates to the animator for the player
				tk2dSpriteAnimator _Animator = GetAnimatorFromIndex(0);

                if (!_Animator.IsPlaying(GetAnimationClipFromString("BalanceLeft")) && !_Animator.IsPlaying(GetAnimationClipFromString("BalanceRight")))
                {

                    if (_fAxisValue < 0)
                    {
                        if (!_Animator.IsPlaying(GetAnimationClipFromString("CantWalkRight")))
                        {
                            ChangeAnimation(_Animator, eAnimationCommands.Play, "CantWalkRight", 0, 0);
                        }
                    }
                    else
                    {
                        if (!_Animator.IsPlaying(GetAnimationClipFromString("CantWalkLeft")) && !_Animator.IsPlaying(GetAnimationClipFromString("BalanceRight")))
                        {
                            ChangeAnimation(_Animator, eAnimationCommands.Play, "CantWalkLeft", 0, 0);
                        }
                    }
                }


			}
		}
		else
		{
			m_fGatheredRotationOther  = ((m_fRotationVelocity ) * _fSpeedModifer) * _fAxisValue;
		}
		
	}
	
	private void ResetRotation()
	{
		m_fGatheredRotationController = 0;
		m_fGatheredRotationOther = 0;
	}
	
	private void LimitVelocity()
	{
		if(m_fCurrentMaxSpeed > m_fMaxSpeed)
		{
			m_fCurrentMaxSpeed -= Time.deltaTime;
		}
		
		if(m_fCurrentMaxSpeed < m_fMaxSpeed)
		{
			m_fCurrentMaxSpeed = m_fMaxSpeed;
		}
		
		float _fAmmendedMaxSpeed = m_fCurrentMaxSpeed;
		//Process Top speed extra from drifting
		if(m_eDriftState == eDriftState.eDS_DriftIn || m_eDriftState == eDriftState.eDS_DriftHold)
		{
			_fAmmendedMaxSpeed =  m_fCurrentMaxSpeed * m_fDriftSpeedBonus;
		}

        if(m_eBoostState == eBoostState.eBS_BoostStart || m_eBoostState == eBoostState.eBS_BoostHold)
        {
            _fAmmendedMaxSpeed = _fAmmendedMaxSpeed * m_fBoostSpeedBonus;
        }

		InterpolateClampSpeed();

		if(m_RigidBody2D.velocity.magnitude > m_fCurrentMaxSpeed )
		{
			if(m_RigidBody2D.isKinematic == false)
			{
 				Vector3 clampedVelocityVector = Vector3.ClampMagnitude(m_RigidBody2D.velocity, _fAmmendedMaxSpeed);
				Vector3 interpolatedVelocityVector = Vector3.Lerp(m_RigidBody2D.velocity, clampedVelocityVector, Time.fixedDeltaTime * m_velocityClampInterpolationSpeed);
   				m_RigidBody2D.velocity = interpolatedVelocityVector;
			}
		}

    }
	private void InterpolateClampSpeed()
	{
		//m_velocityClampInterpolationSpeed should always be trying to reach a value of 100. The value will be reset when collisions occur to allow for smoother collision response
		m_velocityClampInterpolationSpeed = Mathf.Lerp(m_velocityClampInterpolationSpeed, 100.0f, Time.fixedDeltaTime);
	}
	private void ApplyBoost()
	{
		m_RigidBody2D.AddForce(m_RigidBody2D.transform.up * m_fThrustForce);
		m_bThrustThisFrame = true;
	}

	private void ProcessBoost()
	{
		m_fBoostTimer -= Time.deltaTime;

		if(m_fBoostTimer <= 0 && m_eBoostState != eBoostState.eBS_None)
		{
			if (m_bBoostHeld)
			{
				//If we are holding boost, set the state
				m_eBoostState = eBoostState.eBS_BoostHold;

				//Then update the animators to play the appropriate animation
				for (int i = 0; i < m_listFlameAnimators.Count; i++)
				{
					if (m_listFlameAnimators[i])
					{
						if (!m_listFlameAnimators[i].IsPlaying(GetAnimationClipFromString("JetpackBoost_Loop")))
						{
							ChangeAnimation(m_listFlameAnimators[i], eAnimationCommands.Play, "JetpackBoost_Loop");
						}
					}
				}
			}
			else
			{
				if (m_eBoostState != eBoostState.eBS_BoostEnd)
				{
					//If we are not holding boost and our booststate was not already in the boost end state, set it
					m_eBoostState = eBoostState.eBS_BoostEnd;

					//Then update the animators to play the approriate animation
					for (int i = 0; i < m_listFlameAnimators.Count; i++)
					{
						if (m_listFlameAnimators[i])
						{
							ChangeAnimation(m_listFlameAnimators[i], eAnimationCommands.Play, "JetpackBoost_End");
						}
					}
				}
			}

		}
	}


	private void StartBoost()
	{

		if(CanUseBoost())
		{

			m_bBoostHeld = true;

			switch(m_eBoostState)
			{
			case eBoostState.eBS_None:
			{
				m_fBoostTimer = m_fBoostDuration;
				//Clear all forces on the body
				m_RigidBody2D.velocity = Vector3.zero;
			
				//Set max speed to boost speed
				m_fCurrentMaxSpeed = m_fMaxSpeedBoost;
				
				//Do Boost Animation
				for(int i = 0; i < m_listFlameAnimators.Count; i++)
				{
					ChangeAnimation(m_listFlameAnimators[i], eAnimationCommands.Play, "JetpackBoost_Start");
				}

				m_eBoostState = eBoostState.eBS_BoostStart;
				break;
			}
			}
		}
		else
		{
			m_bBoostHeld = false;
		}

	}

	private void ProcessDrift()
	{
		//Drift processing
		if(m_eDriftState == eDriftState.eDS_DriftHold)
		{
			if(!m_bDriftHeld)
			{
				m_eDriftState = eDriftState.eDS_DriftOut;
			}
		}
		if(m_bGrounded)
		{
			m_eDriftState = eDriftState.eDS_None;
			ActivateDriftCollider(false);
		}
		
		//Turn processing
		m_bApplyDriftTurn = false;
		if(m_eDriftState == eDriftState.eDS_DriftIn || m_eDriftState == eDriftState.eDS_DriftHold)
		{
			if(!m_bThrustThisFrame)
			{
				m_bApplyDriftTurn = true;
			}
		}

	}
	
	private void ProcessThrust()
	{
		if(m_bThrustThisFrame)
		{
			m_fTimeThrusting += Time.deltaTime;
			m_fBoostRefillGrace += Time.deltaTime;
		}
		else
		{
			m_fBoostRefillGrace = 0.0f;
			if(m_eDriftState == eDriftState.eDS_DriftIn || m_eDriftState == eDriftState.eDS_DriftHold)
			{
				//Cap time back to something reasonable so we dont drift without gravity forever
				m_fTimeThrusting = Mathf.Clamp(m_fTimeThrusting,0.0f,1.0f);
				m_fTimeThrusting -= Time.deltaTime;
			}
			else
			{
				m_fTimeThrusting = 0;
			}
			
		}
		
	}
	
	private void ProcessDrag()
	{

		if(m_eCharacterState == eCharacterState.eCS_Normal)
		{
			switch(m_eDriftState)
			{
			case eDriftState.eDS_DriftIn:
			case eDriftState.eDS_DriftHold:
			{
				
				if(m_bThrustThisFrame)
				{
					m_RigidBody2D.drag = m_thrustDrag;
				}
				else
				{
					m_RigidBody2D.drag = m_driftDrag;
				}
				break;
			}
			case eDriftState.eDS_DriftOut:
			case eDriftState.eDS_None:
			{
				if(m_bThrustThisFrame)
				{
					m_RigidBody2D.drag = m_thrustDrag;
				}
				else
				{
					m_RigidBody2D.drag = m_noThrustDrag;
				}
				break;
			}
			}
		}
		else
		{
			if(m_bThrustThisFrame)
			{
				m_RigidBody2D.drag = m_thrustDrag;
			}
			else
			{
				m_RigidBody2D.drag = m_noThrustDrag;
			}
		}
	}

	#endregion
	
	#region Methods - Collision
	private void ActivateDriftCollider(bool _bActivate)
	{
		//This should not need to be recorded for replays, as all physics will be turned off for the character anyway.
		s_DriftCollider _DriftCollider = GetComponentInChildren<s_DriftCollider>();
		if(_DriftCollider)
		{
			_DriftCollider.SetActive(_bActivate);
		}
	}

	private void ProcessCollisions(Collider2D other)
	{
        bool _LeftCollision = m_LeftTrigger.GetTriggered();
        bool _RightCollision = m_RightTrigger.GetTriggered();

        //If collision has happened on both triggers simultaneously
        if(_LeftCollision && _RightCollision)
        {
            ApplyDeath();
            return;
        }
        else if ( _LeftCollision || _RightCollision)
		{
            int numItemsInTrigger = _LeftCollision ? m_LeftTrigger.GetNumItemsInTrigger() : m_RightTrigger.GetNumItemsInTrigger();
            //Prevents the character from instantly being put into the death state if two collisions occur on the same trigger before the collision volume can depenetrate.
            if (m_eCharacterState == eCharacterState.eCS_Damaged && numItemsInTrigger <= 1)
            {
                bool stillTouchingDamageTriggeringCollider = _LeftCollision ? m_RightTrigger.IsTouching(m_DamageTriggeringCollider) : m_LeftTrigger.IsTouching(m_DamageTriggeringCollider);
                if (!stillTouchingDamageTriggeringCollider && m_CollisionIFrames <= 0)
                {
                    ApplyDeath();
                    return;
                }
                else
                {
                    return;
                }
            }
		}

		if(m_eCharacterState != eCharacterState.eCS_Broken)
		{
			if(_LeftCollision)
			{
				m_arrayEngineStatus[(int)eJetpackID.eJID_Left] = eJetpackState.eJS_Broken;
				m_eCharacterState = eCharacterState.eCS_Damaged;
				
				CreateSmallExplosion(m_LeftTrigger.transform.position);			
			}
			if(_RightCollision)
			{
				m_arrayEngineStatus[(int)eJetpackID.eJID_Right] = eJetpackState.eJS_Broken;
				m_eCharacterState = eCharacterState.eCS_Damaged;
				
				CreateSmallExplosion(m_RightTrigger.transform.position);			
			}

			//Force Application
			//Add additional force for moving objects
			s_DoTweenVelocityRecorder _velocityRecorder = other.gameObject.GetComponent<s_DoTweenVelocityRecorder>();
			if(_velocityRecorder && _velocityRecorder.GetRecordedVelocity().sqrMagnitude !=0)
			{
				Vector2 _direction = m_RigidBody2D.position - other.attachedRigidbody.position;
 				_direction.Normalize();
				Vector2 _applyforce = _direction * _velocityRecorder.GetRecordedVelocity().sqrMagnitude;
				m_RigidBody2D.AddForce(_applyforce, ForceMode2D.Impulse);
			}
			else if(_LeftCollision)
			{
				m_RigidBody2D.AddForce(transform.right * m_fDamageKnockbackForce, ForceMode2D.Impulse);

			}
			else if(_RightCollision)
			{
				m_RigidBody2D.AddForce(transform.right * -m_fDamageKnockbackForce, ForceMode2D.Impulse);
			}

			m_velocityClampInterpolationSpeed = 5.0f;
			m_CollisionIFrames = 20;
            m_DamageTriggeringCollider = other;

            //If both broken, go straight to broken
            if (m_arrayEngineStatus[0] == eJetpackState.eJS_Broken &&
			   m_arrayEngineStatus[1] == eJetpackState.eJS_Broken)
			{
				ApplyDeath();
			}
			
			//Reset Triggers
			m_LeftTrigger.SetTriggered(false);
			m_RightTrigger.SetTriggered(false);
		
		}				
	}

	private void ProcessFeetCollision()
	{
        if (m_eCharacterState == eCharacterState.eCS_Dizzy)
        {
            float _fRotationValue = m_RigidBody2D.rotation;
            if (_fRotationValue >= 330 || _fRotationValue <= 30)
            {
                m_eCharacterState = eCharacterState.eCS_Normal;
            }
        }	
	}

	private void ProcessGrounded()
	{		
		m_bGrounded = false;
		
		if(m_playerFeetTrigger)
		{
			if(m_playerFeetTrigger.GetFeetTriggerStay())
			{
                //Get the difference between the world up-vector and the character's up vector in order to determine orientation.
                float upVecDiff = Vector2.Angle(Vector2.up, transform.up);
                if (upVecDiff < 4.0f) 
                {
                    m_bGrounded = true;
				}	
			}
		}
    }

    public void OnChildTriggerEnter(Collider2D other, eTriggerType triggerType)
    {
        if(m_bIsReplay)
        {
            return;
        }
        switch(triggerType)
        {
            case eTriggerType.eTT_HeadTrigger:
                {
                    ProcessHeadCollisions();
                    break;
                }

            case eTriggerType.eTT_JetPackTrigger:
                {
                    ProcessCollisions(other);
                    break;
                }
            case eTriggerType.eTT_PlayerFeetTrigger:
                {
                    ProcessFeetCollision();
                    break;
                }
        }
    }

    public void OnPlayerStuck()
    {
        //If the player is stuck, apply death
        m_PlayerStuck = true;
        ApplyDeath();
		//Remove any velocity acting upon the player
		m_RigidBody2D.velocity = Vector3.zero;

    }

    private void ProcessHeadCollisions()
	{
        switch (m_eCharacterState)
        {
            case eCharacterState.eCS_Dizzy:
                {
                    //If we have recently entered the dizzy state, return early to allow for collision resolution to occur naturally.
                    if (m_HeadTrigger.GetNumItemsInTrigger() > 1)
                    {
                        return;
                    }
                    ApplyDeath();
                    return;
                }
            case eCharacterState.eCS_Damaged:
                {
                    ApplyDeath();
                    return;
                }
            case eCharacterState.eCS_Broken:
                {
                    return;
                }
        }

        m_HeadTrigger.SetTriggered(false);

        //Get Current velocity. Use it to scale the legnth of the dizzytimer.

        float _fCurrentSpeed = m_HeadTrigger.GetCollisionVelocity();
        if (_fCurrentSpeed > m_fCurrentMaxSpeed)
        {
            _fCurrentSpeed = m_fCurrentMaxSpeed;
        }

        float _fScaleTime = _fCurrentSpeed / m_fCurrentMaxSpeed;

        m_fDizzyTimer = m_fMaxDizzytime * _fScaleTime;


		//Clear current velocity, add force in opposite direction
		m_RigidBody2D.velocity = Vector3.zero;

		m_RigidBody2D.AddForce(m_RigidBody2D.transform.up * -m_fHeadImpactKnockbackForce, ForceMode2D.Impulse);

        m_eCharacterState = eCharacterState.eCS_Dizzy;
        m_eBoostState = eBoostState.eBS_None;

		//Change Animation
		//TODO: Replace int for something that more appropriately identifies the player's animator
		tk2dSpriteAnimator _pAnimator = GetAnimatorFromIndex(0);
        ChangeAnimation(_pAnimator, eAnimationCommands.Play, "DizzyHit");

		//Change Drag of Rigidbody2D to mimic falling
		m_RigidBody2D.drag = m_noThrustDrag;
    }

    public bool GetShouldIgnore(Collider2D collider)
    {
		/*
        This function returns what should be ignored when an "OnTrigger" function is fired. It is called by the scripts responsible for the player's child colliders.
        To elaborate, we want to detect the trigger enter and take response within s_EntityPlayer, but we do not want to take response within the child script.

		TODO:: Add the children colliders to different layers so that this can be handled by the collision matrix instead.
        */

		if (collider)
		{
			if ((m_CollisionLayersToIgnore.value & (1 << collider.gameObject.layer)) == 0)
			{
				return false;
			}

		}
		return true;
    }

	#endregion


	#region Methods - Recording

	/*
	 * ==================================
	 * ==================================
	 * Recording Code
	 * ==================================
	 * ==================================
	 */
	
	private void RecordData()
	{
		RecordPosition();
		RecordAnimation();
		RecordState();
		RecordVisible();
	}

	private void RecordPosition()
	{
		//Test all previous transform variables against current to see if there is a change. Only record the positional movement if a change has occurred.
		//TODO: Optimize variables so that conversion to vector3 is not necessary.

		Vector3 rigidbodyPos = new Vector3(m_RigidBody2D.position.x, m_RigidBody2D.position.y, this.transform.position.z);
		Vector3 rigidbodyRot = new Vector3(0, 0, m_RigidBody2D.rotation);
		if (m_PrevPos != rigidbodyPos
			|| m_PrevRot != rigidbodyRot
			|| m_PrevScale != this.transform.localScale)
		{
			if (m_uFrameStamp > 0)
			{
				s_GameplayRecorder.instance.AddAction(m_uFrameStamp, RecordActions.PlayerMovement, rigidbodyPos, rigidbodyRot, transform.localScale.x, transform.localScale.y);
			}
		}

		//Update previous locaiton/roataion/scale values
		m_PrevPos = rigidbodyPos;
		m_PrevRot = rigidbodyRot;
		m_PrevRot = rigidbodyRot;
		m_PrevScale = this.transform.localScale;
	}

	private void RecordAnimation()
	{
        /*
		 * Before we record the animation,we must check that the animation for each sprite animator has actually changed. If it has not, we don't wish to
		 * record it.
		 * */

        //Using enumerator rather than .ElementAt as it does not allocate memory, therefore better for garbage collection.
        var enumerator = m_dAnimationCommands.GetEnumerator();
        while(enumerator.MoveNext())
		{
            KeyValuePair<tk2dSpriteAnimator, sAnimationCommand> _Entry = enumerator.Current;

            if (m_dAnimationCommandsPrevious.ContainsKey(_Entry.Key))
			{
				bool _bCanAddCommand = true;

				sAnimationCommand _PreviousCommand = m_dAnimationCommandsPrevious[_Entry.Key];
				sAnimationCommand _ThisCommand = _Entry.Value;

				//If all the values are the same, we do NOT want to add the command, unless it is a "Playfrom" command, as these can be used for creating loops
				if(_ThisCommand.m_eCommand != eAnimationCommands.PlayFrom ||
				   _ThisCommand.m_eCommand != eAnimationCommands.PlayFromFrame)
				{
					if(_PreviousCommand.m_eCommand == _ThisCommand.m_eCommand &&
					   _PreviousCommand.m_sClipname == _ThisCommand.m_sClipname &&
					   _PreviousCommand.m_iFrame == _ThisCommand.m_iFrame &&
					   _PreviousCommand.m_fTime == _ThisCommand.m_fTime)
					{
						_bCanAddCommand = false;
					}
				}

				if (_bCanAddCommand)
				{
					s_GameplayRecorder.instance.AddAction(m_uFrameStamp, RecordActions.AnimationChange, _ThisCommand);
				}

				//Regardless of whether the command is different or not, we should record the NEW command as the "last" command for future comparisons
				m_dAnimationCommandsPrevious[_Entry.Key] = _ThisCommand;
			}
			else
			{
				//If the previous commands do not contain a command for this animator, then we can just add it.
				sAnimationCommand _ThisCommand = _Entry.Value;

				s_GameplayRecorder.instance.AddAction(m_uFrameStamp, RecordActions.AnimationChange, _ThisCommand);

				//Record the new command as the last command
				m_dAnimationCommandsPrevious.Add(_ThisCommand.m_pAnimator, _ThisCommand);

			}
		}



		/*
		 * Note: The following is not needed, as entries are never removed from m_dAnimationCommands after they are initially added.
		 * At the moment it is likely unnecessary to remove them due to the small number of entries in the dictionary 
		 * (Overhead of constantly adding/removing not worth it for the time it takes to iterate over the dictionary), however
		 * if more animators are added then it may be sensible to remove them.
		 * 
		 * Keeping this code in comments incase it needs to be re-implemented.
		 * 
        //TODO: Avoid add/removing unnecessarily by using a flag within the struct to dictate whether or not
        //it can be immediately overridden next time there is a valid command

        // We now need to clear out commands in the "last" dictionary that were NOT updated this frame

        enumerator = m_dAnimationCommandsPrevious.GetEnumerator();
        while (enumerator.MoveNext())
        {
            KeyValuePair<tk2dSpriteAnimator, sAnimationCommand> _Entry = enumerator.Current;

            tk2dSpriteAnimator _Animator = _Entry.Key;
			if(m_dAnimationCommands.ContainsKey(_Animator) == false)
			{
				m_dAnimationCommandsPrevious.Remove(_Animator);
			}
		}*/
	}

	private void RecordState()
	{
		if(m_eCharacterState != m_ePrevCharacterState)
		{
			int _iEnumState = (int)m_eCharacterState;

			s_GameplayRecorder.instance.AddAction(m_uFrameStamp, RecordActions.PlayerStateChange, _iEnumState);
		}

		m_ePrevCharacterState = m_eCharacterState;

	}

	private void RecordVisible()
	{
		if(m_bVisible != m_bPrevVisible)
		{
			s_GameplayRecorder.instance.AddAction(m_uFrameStamp, RecordActions.PlayerVisibilityChange, m_bVisible);
		}

		m_bPrevVisible = m_bVisible;
	}

	#endregion

	#region Methods - Playback

	/*
	 * ==================================
	 * ==================================
	 * Playback Code
	 * ==================================
	 * ==================================
	 */

	public void SetIsReplay(bool isReplay)
	{
		m_bIsReplay = isReplay;
		if (m_RigidBody2D)
		{
			m_RigidBody2D.isKinematic = m_bIsReplay;
		}
	}

	public bool GetIsReplay()
	{
		return m_bIsReplay;
	}
	private void ProcessReplay()
	{
		/*
		 * This is the method for looping through the various actions in a replay.
		 * 
		 * We start iterating over the list until we find an action that has a timestamp that does NOT match our current timestamp
		 * At this point, we note which action this was via m_lastReplayActionIndex and then return.
		 * 
		 * On the next call to ProcessReplay (Called from fixed update), the timestamp of the entity should have increased.
		 * We then start iterating from our "last index" and continue executing and iterating over the actions until we find another
		 * action where the timestamp does not match our current timestamp. The process is then repeated.
		 * 
		 * Note that this method requires and assumes that the actions in the array be added in a time-stamp correct order. If any timestamps are out of order
		 * then the system will fail and action execution will not be able to occur.
		 */
		for (int _iAction = m_lastReplayActionIndex; _iAction < replayData.Count; _iAction++)
        {
            if (m_uFrameStamp == replayData[_iAction].time)
			{
                //Process this event
                ProcessRecordedEvent(replayData[_iAction]);

                //If this is the final action
                if (_iAction == replayData.Count - 1)
                {
                    if (!m_bReplayReachedEnd)
                    {
                        m_bReplayReachedEnd = true;

                        //If this is the "Successful player" (The last one to be recorded)
                        if (this.gameObject == GameSystemPointers.instance.m_PlayerManager.GetLastCreatedPlayer())
                        {
                            //Load the next level
                            LevelEndComplete();
                        }
                    }
                }
            }
			else
			{
				m_lastReplayActionIndex = _iAction;
				break;
			}


		}
	}

    private void ProcessRecordedEvent(RecordedEvent _event)
    {
		switch (_event.recordedAction)
        {
            case RecordActions.PlayerMovement:
                {
					 ReplayMovement(_event);

					break;
                }
            case RecordActions.AnimationChange:
                {
                    ReplayAnimation(_event);
                    break;
                }
            case RecordActions.PlayerStateChange:
                {
                    ReplayPlayerStateChange(_event);
                    break;
                }
            case RecordActions.PlayerVisibilityChange:
                {
                    SetVisible(_event.visible);
                    break;
                }

            case RecordActions.InstantiateSmallExplosion:
                {
                    CreateSmallExplosion(_event.position);
                    break;
                }

        }
    }
	
	private void ReplayMovement(RecordedEvent _event)
	{
		//We use Move functions here instead of setting the position directly because these methods are intended to comply with interpolation settings
		//and create a smooth movement over the frames.
		m_RigidBody2D.MovePosition(new Vector2(_event.position.x, _event.position.y));
		m_RigidBody2D.MoveRotation(_event.rotation.z);
		this.transform.localScale = new Vector3(_event.scaleX, _event.scaleY, this.transform.localScale.z);

	}
	
	private void ReplayAnimation(RecordedEvent _event)
	{
		sAnimationCommand _sCommand = _event.animationcommand;
		AnimateFromCommand(_sCommand);
	}
	
	private void ReplayPlayerStateChange(RecordedEvent _event)
	{
		eCharacterState _newState = (eCharacterState)_event.enumState;
		
		if(_newState == eCharacterState.eCS_Broken)
		{
			ApplyDeath();
		}
		
		m_eCharacterState = _newState;
		
	}
	#endregion
}

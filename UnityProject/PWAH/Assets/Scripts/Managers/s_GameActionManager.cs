using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using DG.Tweening;
using AnimationCommandsNameSpace;
using InputNamespace;

public class s_GameActionManager : MonoBehaviour {

    public enum eGameAction
    {
        eGA_NULL,
        eGA_MOVE,
        eGA_ANIMATION,
        eGA_ROTATE,
        eGA_PATH
    }

    public enum eGameActionPauseType
    {
        ePT_NULL,
        ePT_INPUT,
        ePT_FUNCTION
    }

    public class GameActionClass
    {

        public GameActionClass(string _sObjectId)
        {
            m_sObjectId = _sObjectId;
            m_eGameAction = eGameAction.eGA_NULL;
            m_fStartTime = 0.0f;
            m_fDuration = 0.0f;
            m_vecPosition = new Vector3(0, 0, 0);

            m_fRotation = 0.0f;
            m_eRotateMode = RotateMode.Fast;

            m_bActionComplete = false;
            m_eEaseType = Ease.Linear;

            m_eAnimationCommand = eAnimationCommands.Pause;
            m_sAnimation = "";
            m_iAnimationStartFrame = 0;
            m_fAnimationStartTime = 0;

            m_lWaypoints = new List<Vector3>();
            m_ePathType = PathType.Linear;
            m_bLookAt = false;
            m_vecForward = new Vector3(1, 0, 0);
            m_ePauseType = eGameActionPauseType.ePT_NULL;
        }
        public string m_sObjectId;
        public eGameAction m_eGameAction;
        public float m_fStartTime;
        public float m_fDuration;
        public Vector3 m_vecPosition;

        //===Rotation Variables
        public float m_fRotation;
        public RotateMode m_eRotateMode;

        //====Path Variables
        public List<Vector3> m_lWaypoints;
        public PathType m_ePathType;
        public bool m_bLookAt;
        public Vector3 m_vecForward;

        //=======Animation Vars
        public string m_sAnimation;
        public eAnimationCommands m_eAnimationCommand;
        public int m_iAnimationStartFrame;
        public float m_fAnimationStartTime;

        //======Pause Funcionality
        public eGameActionPauseType m_ePauseType;

        //======Additional
        public bool m_bActionComplete;
        public Ease m_eEaseType;




    }
    //====Member Variables
    protected Dictionary<GameObject, List<GameActionClass>> m_dObjectActions;
    protected float m_fCutSceneTimer;
    protected bool m_bPauseForInput;

	// Use this for initialization
	void Start () {

        //Register with GSP
        GameSystemPointers.instance.m_GameActionManager = this;

        //Initialise dictionary
        m_dObjectActions = new Dictionary<GameObject, List<GameActionClass>>();

        //Initialise Timer
        m_fCutSceneTimer = 0;

        //TODO:: Change this so that it takes a given json file rather than hard coded
        //ParseJSON("Assets/Resources/GameJSONData/Cutscenes/TestCutscene.json");
        SortActions();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void FixedUpdate()
    {
        //Note: Processing occurrs here because we want the times to be based on frame-rate. Will change this out after testing with lower frame rates if it breaks.

        //Increase timer
        if (!m_bPauseForInput)
        {
            m_fCutSceneTimer += Time.fixedDeltaTime;

            //Iterate over dictionary of actions
			//Use integer iterations rather than iterators to avoid allocations
            for (int i = 0; i < m_dObjectActions.Count; i++)
            {
                ProcessActionList(m_dObjectActions.ElementAt(i).Key, m_dObjectActions.ElementAt(i).Value);
            }
        }




    }

    protected void ProcessActionList(GameObject _GameObject ,List<GameActionClass> _ActionList)
    {
        /*
        We create a sequence here (Though we do not assign it).
        This sequence will be assigned if a new action is detected, AND if the start time of the new action is different from the
        start time of the previously processed action.

        This allows us to "Join" actions together.

        TODO NOTE: Will have to do a test case to see how this works if we have two separate sequences running at the same time. For example, if we have a "move"
        that lasts for 10 seconds, and at 5 second we want to start a rotation.

        OTHER NOTE: It may be possible to just push all of these into one big sequence. If we are able to pause all sequences. Not sure about this yet though.
        */
        Sequence _ActionSequence = null;
        float _fLastStartTime = -1; 

        //Iterate over the individual action list
        for(int i = 0; i < _ActionList.Count; i++)
        {
            //If the action is already complete, just skip it.
            if(!_ActionList[i].m_bActionComplete)
            {
                if(m_fCutSceneTimer >= _ActionList[i].m_fStartTime)
                {
                    //Flag this action as being completed;
                    _ActionList[i].m_bActionComplete = true;

                    bool _bNewSequence = false;

                    //=======Create Tween parameters
                    TweenParams _TweenParams = new TweenParams();
                    _TweenParams.SetEase(_ActionList[i].m_eEaseType);
                    if (_ActionList[i].m_ePauseType != eGameActionPauseType.ePT_NULL)
                    {
                        _TweenParams.OnComplete(()=>SetPausedCallback(_ActionList[i].m_ePauseType));
                    }


                    //Create a new sequence if the start time of this action is different from the last recorded start time
                    //AND the action is not an animation. We do not need to create sequences for animations.
                    if (_ActionList[i].m_fStartTime != _fLastStartTime
                        && _ActionList[i].m_eGameAction != eGameAction.eGA_ANIMATION)
                    {
                        _ActionSequence = DOTween.Sequence();
                        _bNewSequence = true;
                        _fLastStartTime = _ActionList[i].m_fStartTime;
                    }

                    switch (_ActionList[i].m_eGameAction)
                    {
                        case eGameAction.eGA_MOVE:
                            {
                                if (_bNewSequence)
                                {
                                    _ActionSequence.Append(_GameObject.transform.DOMove(_ActionList[i].m_vecPosition,
                                                                                        _ActionList[i].m_fDuration)
                                                                                        .SetAs(_TweenParams));
                                }
                                else
                                {
                                    _ActionSequence.Join(_GameObject.transform.DOMove(_ActionList[i].m_vecPosition,
                                                                                        _ActionList[i].m_fDuration)
                                                                                        .SetAs(_TweenParams));
                                }
                                break;
                            }
                        case eGameAction.eGA_ROTATE:
                            {
                                if (_bNewSequence)
                                {
                                    _ActionSequence.Append(_GameObject.transform.DORotate(new Vector3(0,0,_ActionList[i].m_fRotation),
                                                                                                    _ActionList[i].m_fDuration,
                                                                                                    _ActionList[i].m_eRotateMode)
                                                                                                    .SetAs(_TweenParams));
                                }
                                else
                                {
                                    _ActionSequence.Join(_GameObject.transform.DORotate(new Vector3(0, 0, _ActionList[i].m_fRotation),
                                                                                                    _ActionList[i].m_fDuration,
                                                                                                    _ActionList[i].m_eRotateMode)
                                                                                                    .SetAs(_TweenParams));
                                }
                                break;
                            }
                        case eGameAction.eGA_NULL:
                            {
                                Debug.LogError("ERROR: Action does not have a valid Enumeration type\nObject ID: " + _ActionList[i].m_sObjectId + "\nArray Index: " + i.ToString() + "\nStartTime: " + _ActionList[i].m_fStartTime.ToString());
                                break;
                            }
                        case eGameAction.eGA_ANIMATION:
                            {
                                Component[] _Animators = _GameObject.GetComponentsInChildren<tk2dSpriteAnimator>();
                                for(int _iAnimators = 0; _iAnimators < _Animators.Count(); _iAnimators++)
                                {
                                    tk2dSpriteAnimator _IndividualAnimator = (tk2dSpriteAnimator)_Animators[_iAnimators];

                                    ChangeAnimation(_IndividualAnimator,
                                                    _ActionList[i].m_eAnimationCommand,
                                                    _ActionList[i].m_sAnimation,
                                                    _ActionList[i].m_iAnimationStartFrame,
                                                    _ActionList[i].m_fAnimationStartTime);
                                }
                                break;
                            }
                        case eGameAction.eGA_PATH:
                            {
                                if (_bNewSequence)
                                {
                                    _ActionSequence.Append(_GameObject.transform.DOPath(_ActionList[i].m_lWaypoints.ToArray(),
                                                                                        _ActionList[i].m_fDuration,
                                                                                        PathType.CatmullRom,
                                                                                        _ActionList[i].m_bLookAt ? PathMode.TopDown2D : PathMode.Ignore)
                                                                                        .SetLookAt(0,_GameObject.transform.forward, _ActionList[i].m_vecForward)
                                                                                        .SetAs(_TweenParams));
                                }
                                else
                                {
                                    _ActionSequence.Join(_GameObject.transform.DOPath(_ActionList[i].m_lWaypoints.ToArray(),
                                                                                        _ActionList[i].m_fDuration,
                                                                                        PathType.CatmullRom,
                                                                                        _ActionList[i].m_bLookAt ? PathMode.TopDown2D : PathMode.Ignore)
                                                                                        .SetLookAt(0, _GameObject.transform.forward, _ActionList[i].m_vecForward)
                                                                                        .SetAs(_TweenParams));
                                }
                                break;

                            }
                        default:
                            {
                                Debug.LogError("Error: Default case triggered when attempting to create an action sequence");
                                break;
                            }
                    }
                }
                else
                {
                    //Since the action lists have been sorted into chronological order, if we have not yet reached a time it means all subsequent actions will not yet be reached. Just return.
                    return;
                }
            }
        }
    }

    public void ParseJSON(string _sFilePath)
    {
        if (File.Exists(_sFilePath))
        {
            string _sEncodedString;
            using (StreamReader _Reader = new StreamReader(_sFilePath, Encoding.Default))
            {
                _sEncodedString = _Reader.ReadToEnd();
                _Reader.Close();
            }

            JSONObject _JSONObject = new JSONObject(_sEncodedString);

            try
            {
                for(int i = 0; i < _JSONObject.Count; i++)
                {
                    JSONObject _ActionArray = _JSONObject[i];

                    //First Check to see if the action has a related Object (It must, else we can't do anything)
                    if (_JSONObject.keys[i] != null)
                    {
                        string _sObjectId = (_JSONObject.keys[i]);
                        if (_sObjectId == "")
                        {
                            Debug.LogError("The action at index " + i.ToString() + " in the file " + _sFilePath + " has an ObjectId, but that ID is blank");
                            continue;
                        }

                        //Search the object ID for an underscore. If there is an underscore, it means we are wanting to make a new object using the same prefab as an object we made previously.

                        string _sPrefabObjectId = _sObjectId;
                        int _iUnderscoreIndex = _sObjectId.IndexOf("_");
                        if (_iUnderscoreIndex > 0)
                        {
                            _sPrefabObjectId = _sObjectId.Substring(0, _iUnderscoreIndex);
                        }


                        //Now look over the dictionary of Objects/Actions. Check if an object with this ID has already been added.
                        if (!GetObjectExistsInDictionary(_sObjectId))
                        {
                            GameObject _NewGameObject = null;
                            if (_sObjectId == "Camera")
                            {
                                _NewGameObject = GameSystemPointers.instance.m_Camera.gameObject;
                            }
                            else
                            { 
                                _NewGameObject = Instantiate(Resources.Load("CutscenePrefab/" + _sPrefabObjectId, typeof(GameObject))) as GameObject;
                                _NewGameObject.name = _sObjectId;
                                //TODO:: Put the gameobject into an inactive state. Only activate it once its action is called in-game.
                            }

                            m_dObjectActions.Add(_NewGameObject, new List<GameActionClass>());
                        }

                        for (int j = 0; j < _ActionArray.Count; j++)
                        {

                            JSONObject _IndividualAction = _ActionArray[j];

                            GameActionClass _newActionClass = new GameActionClass(_sObjectId);
                            _newActionClass.m_sObjectId = _sObjectId;

                            //Fill in the rest of the details for the action
                            if (_IndividualAction.GetField("GameAction"))
                            {
                                try
                                {
                                    _newActionClass.m_eGameAction = (eGameAction)Enum.Parse(typeof(eGameAction), _IndividualAction.GetField("GameAction").str);
                                }
                                catch (ArgumentException)
                                {
                                    Debug.LogError(_IndividualAction.GetField("GameAction").str + " is not a memeber of the GameAction enumeration");
                                    continue;
                                }
                            }

                            if (_IndividualAction.GetField("StartTime"))
                            {
                                _newActionClass.m_fStartTime = _IndividualAction.GetField("StartTime").n;
                            }

                            if (_IndividualAction.GetField("Duration"))
                            {
                                _newActionClass.m_fDuration = _IndividualAction.GetField("Duration").n;
                            }

                            if (_IndividualAction.GetField("Duration"))
                            {
                                _newActionClass.m_fDuration = _IndividualAction.GetField("Duration").n;
                            }

                            if (_IndividualAction.GetField("PositionX"))
                            {
                                _newActionClass.m_vecPosition.x = _IndividualAction.GetField("PositionX").n;
                            }

                            if (_IndividualAction.GetField("PositionY"))
                            {
                                _newActionClass.m_vecPosition.y = _IndividualAction.GetField("PositionY").n;
                            }

                            if (_IndividualAction.GetField("PositionZ"))
                            {
                                _newActionClass.m_vecPosition.z = _IndividualAction.GetField("PositionZ").n;
                            }

                            if (_IndividualAction.GetField("Rotation"))
                            {
                                _newActionClass.m_fRotation = _IndividualAction.GetField("Rotation").n;
                            }

                            if(_IndividualAction.GetField("RotateMode"))
                            {
                                _newActionClass.m_eRotateMode = (RotateMode)Enum.Parse(typeof(RotateMode), _IndividualAction.GetField("RotateMode").str);

                            }

                            if (_IndividualAction.GetField("Ease"))
                            {
                                try
                                {
                                    _newActionClass.m_eEaseType = (Ease)Enum.Parse(typeof(Ease), _IndividualAction.GetField("Ease").str);
                                }
                                catch (ArgumentException)
                                {
                                    Debug.LogError(_IndividualAction.GetField("Ease").str + " is not a memeber of the Ease enumeration");
                                    continue;
                                }
                            }

                            //===============Sprite Animation
                            if(_IndividualAction.GetField("Animation"))
                            {
                                _newActionClass.m_sAnimation = _IndividualAction.GetField("Animation").str;
                            }
                            if (_IndividualAction.GetField("AnimationCommand"))
                            {
                                try
                                {
                                    _newActionClass.m_eAnimationCommand = (eAnimationCommands)Enum.Parse(typeof(eAnimationCommands), _IndividualAction.GetField("AnimationCommand").str);
                                }
                                catch (ArgumentException)
                                {
                                    Debug.LogError(_IndividualAction.GetField("AnimationCommand").str + " is not a memeber of the eAnimationCommands enumeration");
                                    continue;
                                }
                            }
                            if(_IndividualAction.GetField("AnimationStartFrame"))
                            {
                                _newActionClass.m_iAnimationStartFrame = (int)_IndividualAction.GetField("AnimationStartFrame").n;
                            }
                            if (_IndividualAction.GetField("AnimationStartFrame"))
                            {
                                _newActionClass.m_fAnimationStartTime = _IndividualAction.GetField("AnimationStartFrame").n;
                            }

                            //===============Path waypoints
                            if(_IndividualAction.GetField("Waypoints"))
                            {
                                //Grab array of waypoints.
                                JSONObject _WayPointArray = _IndividualAction.GetField("Waypoints");
                                for(int _iWaypoint = 0; _iWaypoint < _WayPointArray.Count; _iWaypoint++)
                                {
                                    //Note that each waypoint is a JSON array. We must grab the array and then access its elements.
                                    JSONObject _IndividualWaypoint = _WayPointArray[_iWaypoint];

                                    if (_IndividualWaypoint.Count == 2)
                                    {
                                        _newActionClass.m_lWaypoints.Add(new Vector3(_IndividualWaypoint[0].n, _IndividualWaypoint[1].n));
                                    }
                                    else
                                    {
                                        Debug.LogError("Waypoint " + _iWaypoint.ToString() + " does not have two co-ordinates. Start Time: " + _newActionClass.m_fStartTime.ToString());
                                    }
                                }

                            }

                            if(_IndividualAction.GetField("PathType"))
                            {
                                _newActionClass.m_ePathType = (PathType)Enum.Parse(typeof(PathType), _IndividualAction.GetField("PathType").str);
                            }

                            if(_IndividualAction.GetField("LookAt"))
                            {
                                _newActionClass.m_bLookAt = _IndividualAction.GetField("LookAt").b;
                            }

                            if(_IndividualAction.GetField("ForwardVector"))
                            {
                                JSONObject _ForwardVector = _IndividualAction.GetField("ForwardVector");
                                if(_ForwardVector.Count == 3)
                                {
                                    _newActionClass.m_vecForward = new Vector3(_ForwardVector[0].n, _ForwardVector[1].n, _ForwardVector[2].n);
                                }
                                else
                                {
                                    Debug.LogError("ForwardVector does not have three co-ordinates. Start Time: " + _newActionClass.m_fStartTime.ToString());

                                }

                            }

                            if(_IndividualAction.GetField("PauseType"))
                            {
                                _newActionClass.m_ePauseType = (eGameActionPauseType)Enum.Parse(typeof(eGameActionPauseType), _IndividualAction.GetField("PauseType").str);
                            }

                            //Add the action
                            m_dObjectActions.ElementAt(GetObjectIndexInDictionary(_sObjectId)).Value.Add(_newActionClass);
                        }
                    


                    }
                    else
                    {
                        Debug.LogError("The action at index " + i.ToString() + " in the file " + _sFilePath + " does not have an ObjectId");
                        continue;
                    }

                }
            }
            catch(ArgumentException e)
            {
                Debug.LogError("Error Parsing Cutscene file: " + _sFilePath + " Exception: " + e.ToString() );
            }

            Debug.Log("Successfully Loaded Cutscene JSON file");

        }
        else
        {
            Debug.LogError("The following file does not exist: " + _sFilePath);
        }
    }

    protected void ChangeAnimation(tk2dSpriteAnimator _Animator, eAnimationCommands _eCommand, string _AnimationClip, int _iFrame = 0, float _fTime = 0)
    {
        /*
		 * This function is lifted from the entity player script. Applies animation commands to a given animator.
		 * */

        //Before continuing, do a test to ensure that this clip exists on the current animator.

        if (_Animator.GetClipByName(_AnimationClip) == null)
        {
            return;
        }

        switch (_eCommand)
        {
            case eAnimationCommands.Play:
                {
                    _Animator.Play(_AnimationClip);
                    _Animator.GetComponent<Renderer>().enabled = true;
                    break;
                }
            case eAnimationCommands.PlayFromFrame:
                {
                    _Animator.PlayFromFrame(_AnimationClip, _iFrame);
                    break;
                }

            case eAnimationCommands.PlayFrom:
                {
                    _Animator.PlayFrom(_AnimationClip, _fTime);
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
    }

    protected void SortActions( )
    {
        //This function iterates over the dictionary of actions. It takes the action list at each element, and sorts them by time.
        //Note that whilst the json files should be made in such a way that actions are added in chronological order, this will ensure that
        //they are in such an order when being processed.

        for(int i = 0; i < m_dObjectActions.Count; i++)
        {
            List<GameActionClass> _ActionList = m_dObjectActions.ElementAt(i).Value;
            _ActionList.Sort((a, b) => a.m_fStartTime.CompareTo(b.m_fStartTime));
        }
    }

    public bool GetObjectExistsInDictionary(string _sObjectName)
    {
        return GetObjectIndexInDictionary(_sObjectName) != -1;
    }

    public bool GetObjectExistsInDictionary(GameObject _GameObject)
    {
        for (int i = 0; i < m_dObjectActions.Count; i++)
        {
            if (m_dObjectActions.ElementAt(i).Key == _GameObject)
            {
                return true;
            }
        }
        return false;
    }

    public int GetObjectIndexInDictionary(string _sObjectName)
    {
        for (int i = 0; i < m_dObjectActions.Count; i++)
        {
            if (m_dObjectActions.ElementAt(i).Key.name == _sObjectName)
            {
                return i;
            }
        }
        return -1;
    }

    public void RecieveInput(float[] _arrayMenuCommandValues)
    {
        eInputCommand_Menu[] _MenuCommandArray = (eInputCommand_Menu[])Enum.GetValues(typeof(eInputCommand_Menu));
        for (int _iCommand = 0; _iCommand < _MenuCommandArray.Length; _iCommand++)
        {
            if (_arrayMenuCommandValues[(int)eInputCommand_Menu.eIC_Menu_Select] > 0
                || _arrayMenuCommandValues[(int)eInputCommand_Menu.eIC_Menu_Back] > 0)
            {
                m_bPauseForInput = false;
            }
        }
    }

    public void SetPausedCallback( eGameActionPauseType _ePauseType )
    {
        //Callback for setting the manager to be paused once an action has completed

        //TODO:: Expand this so that the function takes an enumeration of various pause types.
        m_bPauseForInput = true;
    }

}

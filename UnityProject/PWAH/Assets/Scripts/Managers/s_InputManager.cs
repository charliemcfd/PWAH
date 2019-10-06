using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using InputNamespace;

namespace InputNamespace
{
	public enum eInputCommand_Menu
	{
		eIC_Menu_MoveLeft = 0,
		eIC_Menu_MoveRight,
		eIC_Menu_MoveUp,
		eIC_Menu_MoveDown,
		eIC_Menu_Select,
		eIC_Menu_Back,
		eIC_Menu_MAX,
	}
	
	public enum eInputCommand_Game
	{
		eIC_Game_MoveLeft = 0,
		eIC_Game_MoveRight,
		eIC_Game_Thrust,
		eIC_Game_Boost,
		eIC_Game_Drift,
		eIC_Game_MAX,
	}
}


public class s_InputManager : MonoBehaviour {

    protected enum eInputManagerState
    {
        eIMS_Processing = 0,
        eIMS_BindingMenu,
        eIMS_BindingGame
    }

	public enum eBindType
	{
		eBT_JoystickButton = 0,
		eBT_KeyboardButton,
		eBT_Axis,
	}

    [Serializable]
	public struct sKeyBind
	{
		//Joystick button, Keyboard Button or Axis
		public eBindType m_eBindType;

		/*
		 * Used for Axis'. So that Positive and Negative values
		 * of the same axis can be mapped to
		 * different commands (E.g Move left/Right)
		 */
		public bool m_bPositive;

		//Binding name used for Axis and Joystick buttons
		public string m_sBindName;

		//Binding enum used for Keyboard buttons
		public KeyCode m_KeyCode;

	}

    //======Member variables
    protected eInputManagerState m_eInputManagerState;
	
	private s_PlayerManager m_PlayerManager;

    private StringBuilder m_StringBuilder;

    private eInputCommand_Game m_eNextGameCommand; //Used in the manual binding process
    private eInputCommand_Menu m_eNextMenuCommand; //Used in the manual binding process

    private float m_fClearBindTimer;

    //Frame input arrays
    private float[] m_arrayFrameInput_Game;
	private float[] m_arrayFrameInput_Menu;

	//===KeyBind Maps
	Dictionary<eInputCommand_Menu, List<sKeyBind> > m_dMenuBinds;
	Dictionary<eInputCommand_Game, List<sKeyBind> > m_dGameBinds;

	// Use this for initialization
	void Start () {

        Input.ResetInputAxes();

        //Register with GSP
        GameSystemPointers._instance.m_InputManager = this;

        //Set up state
        m_eInputManagerState = eInputManagerState.eIMS_Processing;
        m_eNextGameCommand = eInputCommand_Game.eIC_Game_MAX;
        m_eNextMenuCommand = eInputCommand_Menu.eIC_Menu_MAX;

        m_StringBuilder = new StringBuilder();

        InitialiseBinds();
        LoadKeyBinds();
		//SetDefaultKeyBinds();

		m_arrayFrameInput_Game = new float[Enum.GetValues(typeof(eInputCommand_Game)).Length];
		m_arrayFrameInput_Menu = new float[Enum.GetValues(typeof(eInputCommand_Menu)).Length];

		m_PlayerManager = (s_PlayerManager) gameObject.GetComponent(typeof(s_PlayerManager));

        DontDestroyOnLoad(this);

        m_fClearBindTimer = 0;

    }

	void OnDestroy()
	{
		//Unregister with GSP
		GameSystemPointers._instance.m_InputManager = null;
	}

	private void InitialiseBinds()
	{
		//This function initialises the dictionaries. It does not apply any binds.
		//===Menu Binds
		m_dMenuBinds = new Dictionary<eInputCommand_Menu, List<sKeyBind> >();

        for(int i = 0; i < (int)eInputCommand_Menu.eIC_Menu_MAX; i++)
        {
            m_dMenuBinds.Add((eInputCommand_Menu)i, new List<sKeyBind>());

        }

		//===Game Binds
		m_dGameBinds = new Dictionary<eInputCommand_Game, List<sKeyBind> >();

        for(int i = 0; i < (int)eInputCommand_Game.eIC_Game_MAX; i++)
        {
            m_dGameBinds.Add((eInputCommand_Game)i, new List<sKeyBind>());
        }


	}
	private void SetDefaultKeyBinds()
	{
        //Todo:: Platform/Controler specific

        //===Windows - 360
        //===Menu Binds
        {

            sKeyBind _Bind = new sKeyBind();
            _Bind.m_eBindType = eBindType.eBT_JoystickButton;
            _Bind.m_bPositive = true;
            _Bind.m_sBindName = "Button_0";
            m_dMenuBinds[eInputCommand_Menu.eIC_Menu_Select].Add(_Bind);
        }

        //===Game Binds
        {
            //=Move Left
            sKeyBind _Bind = new sKeyBind();
            _Bind.m_eBindType = eBindType.eBT_Axis;
            _Bind.m_bPositive = false;
            _Bind.m_sBindName = "Axis_1";

            m_dGameBinds[eInputCommand_Game.eIC_Game_MoveLeft].Add(_Bind);

            /*
            _Bind = new sKeyBind();
            _Bind.m_eBindType = eBindType.eBT_Axis;
            _Bind.m_bPositive = false;
            _Bind.m_sBindName = "Axis_6";

            m_dGameBinds[eInputCommand_Game.eIC_Game_MoveLeft].Add(_Bind);*/

            //=Move Right

            _Bind = new sKeyBind();
            _Bind.m_eBindType = eBindType.eBT_Axis;
            _Bind.m_bPositive = true;
            _Bind.m_sBindName = "Axis_1";

            m_dGameBinds[eInputCommand_Game.eIC_Game_MoveRight].Add(_Bind);

            /*
            _Bind = new sKeyBind();
            _Bind.m_eBindType = eBindType.eBT_Axis;
            _Bind.m_bPositive = true;
            _Bind.m_sBindName = "Axis_6";

            m_dGameBinds[eInputCommand_Game.eIC_Game_MoveRight].Add(_Bind);*/

            //=Thrust

            _Bind = new sKeyBind();
            _Bind.m_eBindType = eBindType.eBT_JoystickButton;
            _Bind.m_bPositive = true;
            _Bind.m_sBindName = "Button_0";
            m_dGameBinds[eInputCommand_Game.eIC_Game_Thrust].Add(_Bind);


            //=Boost
            _Bind = new sKeyBind();
            _Bind.m_eBindType = eBindType.eBT_Axis;
            _Bind.m_bPositive = true;
            _Bind.m_sBindName = "Axis_10";
            m_dGameBinds[eInputCommand_Game.eIC_Game_Boost].Add(_Bind);

            //=Drift
            _Bind = new sKeyBind();
            _Bind.m_eBindType = eBindType.eBT_Axis;
            _Bind.m_bPositive = true;
            _Bind.m_sBindName = "Axis_9";
            m_dGameBinds[eInputCommand_Game.eIC_Game_Drift].Add(_Bind);
        }



	}

	private void ProcessBoundInputs()
	{
        //Input is collated for both game an menu regardless of actual game state. Places the input is passed should decide what they're going to do with the data and when.


        /*Notes:: Return value from input processing, 
		 * pass to another function along with the 
		 * Key enumeration. This function will generate commands to pass to the
		 * relevant game system.
		*/

        var _gameBindEnumerator = m_dGameBinds.GetEnumerator();
        while (_gameBindEnumerator.MoveNext())
        {
            List<sKeyBind> _BindList = _gameBindEnumerator.Current.Value;

            for (int j = 0; j < _BindList.Count; j++ )
            {
                switch(_BindList[j].m_eBindType)
                {
                    case eBindType.eBT_JoystickButton:
                        {
                            AddGameInput(_gameBindEnumerator.Current.Key, GetJoystickButtonInput(_BindList[j]));
                            break;
                        }
                    case eBindType.eBT_KeyboardButton:
                        {
                            AddGameInput(_gameBindEnumerator.Current.Key, GetKeyboardInput(_BindList[j]));
                            break;
                        }
                    case eBindType.eBT_Axis:
                        {
                            AddGameInput(_gameBindEnumerator.Current.Key, GetAxisInput(_BindList[j]));
                            break;
                        }
                }
            }
        }

        var _menuBindEnumerator = m_dMenuBinds.GetEnumerator();
        while(_menuBindEnumerator.MoveNext())
        {
            List<sKeyBind> _BindList = _menuBindEnumerator.Current.Value;

            for (int j = 0; j < _BindList.Count; j++)
            {
                switch (_BindList[j].m_eBindType)
                {
                    case eBindType.eBT_JoystickButton:
                        {
                            AddMenuInput(_menuBindEnumerator.Current.Key, GetJoystickButtonInput(_BindList[j]));
                            break;
                        }
                    case eBindType.eBT_KeyboardButton:
                        {
                            AddMenuInput(_menuBindEnumerator.Current.Key, GetKeyboardInput(_BindList[j]));
                            break;
                        }
                    case eBindType.eBT_Axis:
                        {
                            AddMenuInput(_menuBindEnumerator.Current.Key, GetAxisInput(_BindList[j]));
                            break;
                        }
                }
            }

        }

        //Old foreach code. Probably made lots of garbabge, but left here just for reference.
        /*
		foreach(KeyValuePair<eInputCommand_Game, List<sKeyBind>> _Entry in m_dGameBinds)
		{
			foreach(sKeyBind _Bind in _Entry.Value)
			{
				switch(_Bind.m_eBindType)
				{
				case eBindType.eBT_JoystickButton:
				{
					AddGameInput(_Entry.Key, GetJoystickButtonInput(_Bind));
					break;
				}
				case eBindType.eBT_KeyboardButton:
				{
					AddGameInput(_Entry.Key, GetKeyboardInput(_Bind));
					break;
				}
				case eBindType.eBT_Axis:
				{
					AddGameInput(_Entry.Key, GetAxisInput(_Bind));
					break;
				}
				}
			}
		}*/
    }

	private float GetJoystickButtonInput(sKeyBind _Bind)
	{
		//For buttons, but we intentionally use GetAxis
		return  Input.GetAxis(_Bind.m_sBindName);
	}

	private float GetKeyboardInput(sKeyBind _Bind)
	{
		if(Input.GetKey(_Bind.m_KeyCode))
		{
			return 1.0f;
		}

		return 0.0f;
	}

	private float GetAxisInput(sKeyBind _Bind)
	{
		float _fAxisValue = Input.GetAxis(_Bind.m_sBindName);
		if(_Bind.m_bPositive)
		{
			if( _fAxisValue > 0.01f)
			{
				return _fAxisValue;
			}
		}
		else
		{
			if( _fAxisValue < -0.01f)
			{
				return _fAxisValue;
			}
		}

		return 0.0f;
	}

	private void AddGameInput(eInputCommand_Game _eCommand, float _fValue)
	{
		//If value is zero, input is disregarded.
		if(_fValue == 0)
		{
			return;
		}

		/*Here we build an overall "Frame input" array that will be passed into the game once all the binds have been processed.
		 * In the case that we have inputs from two separate sources, relating to the same bind (E.g MoveLeft from both a keyboard button and a controler
		 * axis) then whichever bind is processed LATER will be used.
		 * */

		m_arrayFrameInput_Game[(int)_eCommand] = _fValue;
    }

    private void AddMenuInput(eInputCommand_Menu _eCommand, float _fValue)
	{
		//If value is zero, input is disregarded.
		if(_fValue == 0)
		{
			return;
		}
		
		m_arrayFrameInput_Menu[(int)_eCommand] = _fValue;
	}

	private void ClearInputs()
	{
		/*
		 * Clears out the input used in the last frame. Sets all values back to zero.
		 * */

        for(int i = 0; i < (int)eInputCommand_Menu.eIC_Menu_MAX; i++)
        {
            m_arrayFrameInput_Menu[i] = 0;
        }

        for(int i = 0; i < (int)eInputCommand_Game.eIC_Game_MAX; i++)
        {
            m_arrayFrameInput_Game[i] = 0;
        }
	}
	
	
	// NOTE: Depding on project, it may be preferable to have this within Update, rather than FixedUpdate. 
	void FixedUpdate () {   

		ClearInputs();

        switch(m_eInputManagerState)
        {
            case eInputManagerState.eIMS_Processing:
                {
                    ProcessBoundInputs();
                    PassInput();

                    break;
                }

            case eInputManagerState.eIMS_BindingMenu:
            case eInputManagerState.eIMS_BindingGame:
                {
                    //====Clear case - Escape is held for 1.5s
                    if(Input.GetKey(KeyCode.Escape))
                    {
                        //Note, since this is fixedupdate, should return the fixed delta anyway, but just keep an eye out for any problems.
                        m_fClearBindTimer += Time.deltaTime;

                        if(m_fClearBindTimer >= 1.5f)
                        {
                            ClearBinds();
                            m_fClearBindTimer = 0;
                            ExitBindingProcess();
                        }
                        //We return here to ensure that an escape key is not registered to any bind.
                        return;

                    }
                    //====Exit case - Escape is released
                    if (Input.GetKeyUp(KeyCode.Escape))
                    {
                        ExitBindingProcess();
                        m_fClearBindTimer = 0;
                        return;
                    }

                    //Keyboard Input
                    if (m_eInputManagerState == eInputManagerState.eIMS_BindingGame)
                    {
                        if (CreateBindFromInput(eBindType.eBT_KeyboardButton))
                        {
                            ExitBindingProcess();
                            return;
                        }
                    }

                    //Axis loop
                    for (int i = 1; i < 11; i++ )
                    {
                        //Clear stringbuilder
                        m_StringBuilder.Remove(0, m_StringBuilder.Length);

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_XBOXONE

                        //Hacky skip for shitey Xbox axis 3 and 6 (triggers -1 to +1)
                        if (i == 3 || i == 6)
                        {
                            bool _bXbox = false;
                            for ( int j = 0; j < Input.GetJoystickNames().Length; j++)
                            {
                                if (Input.GetJoystickNames()[j].Contains("Xbox"))
                                {
                                    _bXbox = true;
                                }
                            }

                            if(_bXbox)
                            {
                                continue;
                            }
                        }
#endif

                        //Make new stirng
                        m_StringBuilder.Append("Axis_");
                        m_StringBuilder.Append(i);

                        //Attempt to create new binds

                        bool _bCreatedBind = CreateBindFromInput(eBindType.eBT_Axis);


                        //If the bind was created, Bail and reset state
                        if(_bCreatedBind)
                        {
                            ExitBindingProcess();
                            return;
                        }
                    }

                    //Button Loop
                    for(int i = 0; i < 19; i++)
                    {
                        //Clear stringbuilder
                        m_StringBuilder.Remove(0, m_StringBuilder.Length);

                        //Make new stirng
                        m_StringBuilder.Append("Button_");
                        m_StringBuilder.Append(i);

                        //Attempt to create new binds
                        bool _bCreatedBind = CreateBindFromInput(eBindType.eBT_JoystickButton);

                        //If the bind was created, Bail and reset state
                        if (_bCreatedBind)
                        {
                            ExitBindingProcess();
                            return;
                        }

                    }



                    //
                    break;
                }
        }

        /*
        Debug.Log("==================================");
        Debug.Log("Axis 1 Value: " + Input.GetAxis("Axis_1").ToString());
        Debug.Log("Axis 2 Value: " + Input.GetAxis("Axis_2").ToString());
        Debug.Log("Axis 3 Value: " + Input.GetAxis("Axis_3").ToString());
        Debug.Log("Axis 4 Value: " + Input.GetAxis("Axis_4").ToString());
        Debug.Log("Axis 6 Value: " + Input.GetAxis("Axis_6").ToString());
        Debug.Log("Axis 7 Value: " + Input.GetAxis("Axis_7").ToString());
        Debug.Log("Axis 8 Value: " + Input.GetAxis("Axis_8").ToString());
        Debug.Log("Axis 9 Value: " + Input.GetAxis("Axis_9").ToString());
        Debug.Log("Axis 10 Value: " + Input.GetAxis("Axis_10").ToString());

        */
        /*
        if (Input.GetKeyDown(KeyCode.R))
		{
			s_GameplayRecorder.SP.StartRecording();
		}*/
    }

    private void PassInput()
	{
		/*TODO At some point, this should get some info from a game manager which would tell us
		 * whether the menu input or game input should be used. For now, It'll go to PlayerManager directly.
		 * 
		 * Also, if we're going to, this would be where we record input.
		*/

		m_PlayerManager.RecieveInput(m_arrayFrameInput_Game);
        if(GameSystemPointers.instance.m_GameActionManager)
        {
            GameSystemPointers.instance.m_GameActionManager.RecieveInput(m_arrayFrameInput_Menu);
        }
	}

    public List<sKeyBind> GetGameBindData(eInputCommand_Game _eCommand)
    {
        return m_dGameBinds[_eCommand];
    }

    public bool GetBindsValid (bool _bGameBinds)
    {
        //Checks to make sure there is an entry for each bind. Returns false if one is missing. 

        if(_bGameBinds)
        {
            for (int i = 0; i < (int)eInputCommand_Game.eIC_Game_MAX; i++)
            {
                if(m_dGameBinds[(eInputCommand_Game)i].Count() <= 0)
                {
                    return false;
                }
            }
        }
        else
        {
            for (int i = 0; i < (int)eInputCommand_Menu.eIC_Menu_MAX; i++)
            {
                if(m_dMenuBinds[(eInputCommand_Menu)i].Count() <= 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    #region Bind Removal Process

    public void ClearBinds()
    {
        //This will clear the currently selected "nextCommands"

        if(m_eNextGameCommand != eInputCommand_Game.eIC_Game_MAX)
        {
            m_dGameBinds[m_eNextGameCommand].Clear();
        }

        if(m_eNextMenuCommand != eInputCommand_Menu.eIC_Menu_MAX)
        {
            m_dMenuBinds[m_eNextMenuCommand].Clear();
        }
    }

    public void ClearAllBinds()
    {
        for (int i = 0; i < (int)eInputCommand_Game.eIC_Game_MAX; i++)
        {
            m_dGameBinds[(eInputCommand_Game)i].Clear();
        }

        for(int i = 0; i < (int)eInputCommand_Menu.eIC_Menu_MAX; i++)
        {
            m_dMenuBinds[(eInputCommand_Menu)i].Clear();
        }
    }

    protected void ClearDuplicateGameBinds(sKeyBind _Bind)
    {
        //===Iterate over previously created binds. If any have the same input, clear them out. (FIFO)
        for (int i = 0; i < m_dGameBinds.Count; i++)
        {
            List<sKeyBind> _BindList = m_dGameBinds.ElementAt(i).Value;

            for (int j = 0; j < _BindList.Count;)
            {
                if (_BindList[j].m_eBindType == _Bind.m_eBindType)
                {
                    switch (_Bind.m_eBindType)
                    {
                        case eBindType.eBT_Axis:
                            {
                                if (_Bind.m_sBindName == _BindList[j].m_sBindName && _Bind.m_bPositive == _BindList[j].m_bPositive)
                                {
                                    _BindList.RemoveAt(j);
                                    continue;
                                }
                                break;
                            }
                        case eBindType.eBT_JoystickButton:
                            {
                                if (_Bind.m_sBindName == _BindList[j].m_sBindName)
                                {
                                    _BindList.RemoveAt(j);
                                    continue;
                                }
                                break;
                            }

                        case eBindType.eBT_KeyboardButton:
                            {
                                if (_Bind.m_KeyCode == _BindList[j].m_KeyCode)
                                {
                                    _BindList.RemoveAt(j);
                                    continue;
                                }
                                break;
                            }
                    }
                }

                j++;
            }
        }
    }

    protected void ClearDuplicateMenuBinds(sKeyBind _Bind)
    {
        //===Iterate over previously created binds. If any have the same input, clear them out. (FIFO)
        for (int i = 0; i < m_dMenuBinds.Count; i++)
        {
            List<sKeyBind> _BindList = m_dMenuBinds.ElementAt(i).Value;

            for (int j = 0; j < _BindList.Count;)
            {
                if (_BindList[j].m_eBindType == _Bind.m_eBindType)
                {
                    switch (_Bind.m_eBindType)
                    {
                        case eBindType.eBT_Axis:
                            {
                                if (_Bind.m_sBindName == _BindList[j].m_sBindName && _Bind.m_bPositive == _BindList[j].m_bPositive)
                                {
                                    _BindList.RemoveAt(j);
                                    continue;
                                }
                                break;
                            }
                        case eBindType.eBT_JoystickButton:
                            {
                                if (_Bind.m_sBindName == _BindList[j].m_sBindName)
                                {
                                    _BindList.RemoveAt(j);
                                    continue;
                                }
                                break;
                            }

                        case eBindType.eBT_KeyboardButton:
                            {
                                if (_Bind.m_KeyCode == _BindList[j].m_KeyCode)
                                {
                                    _BindList.RemoveAt(j);
                                    continue;
                                }
                                break;
                            }
                    }
                }

                j++;
            }
        }
    }

    #endregion

    #region Bind Creation Process

    public void CreateGameBind(eInputCommand_Game _eCommand)
    {
        /*
        This funciton is called (externally) in order to begin the binding process.

        A command is passed (and stored), and the manager will be set into the "BindGame" state.
         */


        m_eInputManagerState = eInputManagerState.eIMS_BindingGame;
        m_eNextGameCommand = _eCommand;
    }

    public void CreateMenuBind(eInputCommand_Menu _eCommand)
    {
        /*
        This funciton is called (externally) in order to begin the binding process.

        A command is passed (and stored), and the manager will be set into the "BindMenu" state.
         */


        m_eInputManagerState = eInputManagerState.eIMS_BindingMenu;
        m_eNextMenuCommand = _eCommand;
    }

    private bool CreateBindFromInput(eBindType _eBindType)
    {
        //Returns true if a new bind was created (Which signals that the state should change from creating a bind)

        if(m_eNextGameCommand != eInputCommand_Game.eIC_Game_MAX && 
           m_eNextMenuCommand != eInputCommand_Menu.eIC_Menu_MAX)
        {
            Debug.LogError("Both NextGameCommand and NextMenuCommand are set to valid values. This should not happen");
            return false;
        }

        sKeyBind _Bind = new sKeyBind();
        bool _bCreatedBind = false; //Need to have a flag to see if we actually made a new bind



        switch (_eBindType)
        {
            case eBindType.eBT_Axis:
                {

                    if (Input.GetAxis(m_StringBuilder.ToString()) > 0.01)
                    {
                        //===Positive Axis Bind
                        _Bind.m_eBindType = eBindType.eBT_Axis;
                        _Bind.m_bPositive = true;
                        _Bind.m_sBindName = m_StringBuilder.ToString();
                        _bCreatedBind = true;



                    }
                    else if (Input.GetAxis(m_StringBuilder.ToString()) < -0.01)
                    {
                        //===Negative Axis Bind
                        _Bind.m_eBindType = eBindType.eBT_Axis;
                        _Bind.m_bPositive = false;
                        _Bind.m_sBindName = m_StringBuilder.ToString();
                        _bCreatedBind = true;

                    }

                    break;
                }
            case eBindType.eBT_JoystickButton:
                {
                    if (Input.GetAxis(m_StringBuilder.ToString()) > 0 )
                    {
                        _Bind.m_eBindType = eBindType.eBT_JoystickButton;
                        _Bind.m_bPositive = true;
                        _Bind.m_sBindName = m_StringBuilder.ToString();
                        _bCreatedBind = true;

                    }
                    break;
                }
            case eBindType.eBT_KeyboardButton:
                {

                    int e = System.Enum.GetNames(typeof(KeyCode)).Length;
                    for (int i = 0; i < e; i++)
                    {
                        if (Input.GetKey((KeyCode)i))
                        {
                            _Bind.m_eBindType = eBindType.eBT_KeyboardButton;
                            _Bind.m_bPositive = true;
                            _Bind.m_KeyCode = ((KeyCode)i);
                            _bCreatedBind = true;


                        }
                    }

                    // Mouse button stuff. For some reason it is not included in the above loop?
                    for(int i = (int)KeyCode.Mouse0; i <= (int)KeyCode.Mouse6; i++)
                    {
                        if(Input.GetKey((KeyCode)i))
                        {
                            _Bind.m_eBindType = eBindType.eBT_KeyboardButton;
                            _Bind.m_bPositive = true;
                            _Bind.m_KeyCode = ((KeyCode)i);
                            _bCreatedBind = true;

                        }
                    }

                    break;
                }
        }

        //Add binds to dictionary
        if (_bCreatedBind)
        {
            if (m_eInputManagerState == eInputManagerState.eIMS_BindingGame && m_eNextGameCommand != eInputCommand_Game.eIC_Game_MAX)
            {
                //===Clear any duplicated
                ClearDuplicateGameBinds(_Bind);

                m_dGameBinds[m_eNextGameCommand].Add(_Bind);

                Debug.Log("Axis value" + Input.GetAxis(m_StringBuilder.ToString()).ToString());

                Debug.Log("Successfully created new game binding " + _eBindType.ToString() + " " + m_eNextGameCommand.ToString() + " with bind " + _Bind.m_sBindName);
                return true;
            }
            else if (m_eInputManagerState == eInputManagerState.eIMS_BindingMenu && m_eNextMenuCommand != eInputCommand_Menu.eIC_Menu_MAX)
            {
                //===Clear any duplicates
                ClearDuplicateMenuBinds(_Bind);

                m_dMenuBinds[m_eNextMenuCommand].Add(_Bind);

                Debug.Log("Successfully created new menu binding " + m_eNextMenuCommand.ToString() + " with bind " + _Bind.m_sBindName);

                return true;
            }
            else
            {
                //If we have not returned at this point, neither next command was valid. This shouldn't happen, but log an error. The struct should be removed by the GC when it goes out of scope ( >:( )
                Debug.LogError("Neither NextGameCommand nor NextMenuCommand were set to valid values, or the state was incorectly set. This should not happen \nInputManagerState = " + m_eInputManagerState.ToString()
                    + "\nNextGameCommand = " + m_eNextGameCommand.ToString() + "\nNextMenuCommand = " + m_eNextMenuCommand.ToString());
            }
        }


        return false;
    }

    private void ExitBindingProcess(  )
    {
        m_eInputManagerState = eInputManagerState.eIMS_Processing;
        m_eNextGameCommand = eInputCommand_Game.eIC_Game_MAX;
        m_eNextMenuCommand = eInputCommand_Menu.eIC_Menu_MAX;

        Debug.Log("Exiting binding process");
    }

    #endregion

    #region Save/Load

    public void SaveKeyBinds()
    {
        JSONObject _JSONObject = new JSONObject(JSONObject.Type.OBJECT);

        //======Game Binds
        JSONObject _GameBindObject = new JSONObject(JSONObject.Type.OBJECT);
        for (int i = 0; i < (int)eInputCommand_Game.eIC_Game_MAX; i++)
        {
            List<sKeyBind> _CurrentList = m_dGameBinds[(eInputCommand_Game)i];
            JSONObject _BindArray = new JSONObject(JSONObject.Type.ARRAY);

            for (int j = 0; j < _CurrentList.Count; j++)
            {
                JSONObject _IndividualBind = new JSONObject(JSONObject.Type.OBJECT);
                _IndividualBind.AddField("BindType", ((int)_CurrentList[j].m_eBindType));
                _IndividualBind.AddField("Positive", _CurrentList[j].m_bPositive);
                _IndividualBind.AddField("BindName", _CurrentList[j].m_sBindName);
                _IndividualBind.AddField("KeyCode", ((int)_CurrentList[j].m_KeyCode));

                _BindArray.Add(_IndividualBind);
            }

            _GameBindObject.AddField(((eInputCommand_Game)i).ToString(), _BindArray);
        }

        //======Menu Binds
        JSONObject _MenuBindObject = new JSONObject(JSONObject.Type.OBJECT);
        for (int i = 0; i < (int)eInputCommand_Menu.eIC_Menu_MAX; i++)
        {
            List<sKeyBind> _CurrentList = m_dMenuBinds[(eInputCommand_Menu)i];
            JSONObject _BindArray = new JSONObject(JSONObject.Type.ARRAY);

            for (int j = 0; j < _CurrentList.Count; j++)
            {
                JSONObject _IndividualBind = new JSONObject(JSONObject.Type.OBJECT);
                _IndividualBind.AddField("BindType", ((int)_CurrentList[j].m_eBindType));
                _IndividualBind.AddField("Positive", _CurrentList[j].m_bPositive);
                _IndividualBind.AddField("BindName", _CurrentList[j].m_sBindName);
                _IndividualBind.AddField("KeyCode", ((int)_CurrentList[j].m_KeyCode));

                _BindArray.Add(_IndividualBind);
            }

            _MenuBindObject.AddField(((eInputCommand_Menu)i).ToString(), _BindArray);
        }

        //======Add to JSON Object
        _JSONObject.AddField("GameBinds", _GameBindObject);
        _JSONObject.AddField("MenuBinds", _MenuBindObject);

        //======Write to file
        string path = null;
#if UNITY_EDITOR
        path = "Assets/Resources/GameJSONData/KeyBinds.json";
        if (!Directory.Exists("Assets/Resources/GameJSONData"))
        {
            Directory.CreateDirectory("Assets/Resources/GameJSONData");
        }
#elif UNITY_STANDALONE
        // You cannot add a subfolder, at least it does not work for me
        path =  Application.persistentDataPath + "_Data/Resources/KeyBinds.json";

        if (!Directory.Exists( Application.persistentDataPath + "_Data/Resources/KeyBinds"))
        {
            Directory.CreateDirectory( Application.persistentDataPath + "_Data/Resources/KeyBinds");
        }

#endif

        string _sEncodedString = _JSONObject.Print();
        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.Write(_sEncodedString);
            }
        }

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

    }

    public void LoadKeyBinds()
    {
        string path = null;

#if UNITY_EDITOR
        path = "Assets/Resources/GameJSONData/KeyBinds.json";
        if (!File.Exists("Assets/Resources/GameJSONData/KeyBinds.json"))
        {
            SetDefaultKeyBinds();
            return;
        }
#elif UNITY_STANDALONE
        // You cannot add a subfolder, at least it does not work for me

        path =  Application.persistentDataPath + "_Data/Resources/KeyBinds.json";

        Debug.Log("Full Data Path: " + path);

        if (!File.Exists(path))
        {
            SetDefaultKeyBinds();
            return;
        }
#endif
        string _sEncodedString;
        using (StreamReader _Reader = new StreamReader(path, Encoding.Default))
        {
            _sEncodedString = _Reader.ReadToEnd();
            _Reader.Close();
        }

        JSONObject _JSONObject = new JSONObject(_sEncodedString);

        //=========Game Binds
        {
            JSONObject _GameBindObject = new JSONObject(JSONObject.Type.OBJECT);
            if (_JSONObject.GetField("GameBinds"))
            {
                _GameBindObject = _JSONObject.GetField("GameBinds");
            }

            for (int i = 0; i < (int)eInputCommand_Game.eIC_Game_MAX; i++)
            {
                JSONObject _BindArray = _GameBindObject[((eInputCommand_Game)i).ToString()];
                for (int j = 0; j < _BindArray.Count; j++)
                {
                    JSONObject _IndividualBind = _BindArray[j];

                    sKeyBind _RetrievedBind = new sKeyBind();

                    try
                    {
                        if (_IndividualBind.GetField("BindType"))
                        {
                            _RetrievedBind.m_eBindType = ((eBindType)_IndividualBind.GetField("BindType").n);
                        }

                        if (_IndividualBind.GetField("Positive"))
                        {
                            _RetrievedBind.m_bPositive = _IndividualBind.GetField("Positive").b;
                        }

                        if (_IndividualBind.GetField("BindName"))
                        {
                            _RetrievedBind.m_sBindName = _IndividualBind.GetField("BindName").str;
                        }

                        if (_IndividualBind.GetField("KeyCode"))
                        {
                            _RetrievedBind.m_KeyCode = ((KeyCode)_IndividualBind.GetField("KeyCode").n);
                        }

                        m_dGameBinds[(eInputCommand_Game)i].Add(_RetrievedBind);

                    }
                    catch
                    {
                        Debug.LogError("Missing JSON field");

                    }
                }
            }
        }

        //=========Menu Binds
        {
            JSONObject _MenuBindObject = new JSONObject(JSONObject.Type.OBJECT);
            if (_JSONObject.GetField("MenuBinds"))
            {
                _MenuBindObject = _JSONObject.GetField("MenuBinds");
            }

            for (int i = 0; i < (int)eInputCommand_Menu.eIC_Menu_MAX; i++)
            {
                JSONObject _BindArray = _MenuBindObject[((eInputCommand_Menu)i).ToString()];
                for (int j = 0; j < _BindArray.Count; j++)
                {
                    JSONObject _IndividualBind = _BindArray[j];

                    sKeyBind _RetrievedBind = new sKeyBind();

                    try
                    {
                        if (_IndividualBind.GetField("BindType"))
                        {
                            _RetrievedBind.m_eBindType = ((eBindType)_IndividualBind.GetField("BindType").n);
                        }

                        if (_IndividualBind.GetField("Positive"))
                        {
                            _RetrievedBind.m_bPositive = _IndividualBind.GetField("Positive").b;
                        }

                        if (_IndividualBind.GetField("BindName"))
                        {
                            _RetrievedBind.m_sBindName = _IndividualBind.GetField("BindName").str;
                        }

                        if (_IndividualBind.GetField("KeyCode"))
                        {
                            _RetrievedBind.m_KeyCode = ((KeyCode)_IndividualBind.GetField("KeyCode").n);
                        }

                        m_dMenuBinds[(eInputCommand_Menu)i].Add(_RetrievedBind);

                    }
                    catch
                    {
                        Debug.LogError("Missing JSON field");

                    }
                }
            }


        }

    }

    #endregion

}
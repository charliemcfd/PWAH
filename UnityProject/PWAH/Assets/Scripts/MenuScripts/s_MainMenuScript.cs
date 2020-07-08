using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class s_MainMenuScript : s_BaseMenuScript {

    public enum eMainMenuState
    {
        eMMS_Front  = 0 ,
        eMMS_Settings
    }

    //===Public Variables

    //=Keybind elements
    public GameObject m_KeybindButton_Thrust;
    public GameObject m_KeybindButton_TurnLeft;
    public GameObject m_KeybindButton_TurnRight;
    public GameObject m_KeybindButton_Boost;
    public GameObject m_KeybindButton_Drift;

    public GameObject m_ThrustBindText;
    public GameObject m_LeftBindText;
    public GameObject m_RightBindText;
    public GameObject m_BoostBindText;
    public GameObject m_DriftBindText;

	public GameObject m_ExampleText;

    //=Settings Elements
    public GameObject m_SettingsDoneButton;



    //=Main menu elements
    public GameObject m_PlayButton;
    public GameObject m_SettingsButton;
	public GameObject m_ScriptedSequenceButton;

    //====Member Variables
    protected eMainMenuState m_eMainMenuState;

    protected List<GameObject> m_ListKeyBindElements;
    protected List<GameObject> m_ListMainMenuElements;

    private StringBuilder m_StringBuilder;



    // Use this for initialization
    void Start () {
        base.Start();

        m_StringBuilder = new StringBuilder();

        m_ListKeyBindElements = new List<GameObject>();
        m_ListMainMenuElements = new List<GameObject>();

        m_eMainMenuState = eMainMenuState.eMMS_Front;

		m_ExampleText.GetComponent<Renderer>().enabled = false;


		AddToMainMenuElementsList();
        AddToKeybindElementsList();

        SetSettingsVisible(false);
    }

    protected void AddToMainMenuElementsList()
    {
        if(m_PlayButton)
        {
            m_ListMainMenuElements.Add(m_PlayButton);
        }

        if(m_SettingsButton)
        {
            m_ListMainMenuElements.Add(m_SettingsButton);
        }

		if(m_ScriptedSequenceButton)
		{
			m_ListMainMenuElements.Add(m_ScriptedSequenceButton);
		}
    }

    protected void AddToKeybindElementsList()
    {

        if(m_KeybindButton_Thrust)
        {
            m_ListKeyBindElements.Add(m_KeybindButton_Thrust);
        }

        if (m_KeybindButton_TurnLeft)
        {
            m_ListKeyBindElements.Add(m_KeybindButton_TurnLeft);
        }

        if (m_KeybindButton_TurnRight)
        {
            m_ListKeyBindElements.Add(m_KeybindButton_TurnRight);
        }

        if (m_KeybindButton_Boost)
        {
            m_ListKeyBindElements.Add(m_KeybindButton_Boost);
        }

        if (m_KeybindButton_Drift)
        {
            m_ListKeyBindElements.Add(m_KeybindButton_Drift);
        }

        //===Text Objects

        if (m_ThrustBindText)
        {
            m_ListKeyBindElements.Add(m_ThrustBindText);
        }
        if (m_LeftBindText)
        {
            m_ListKeyBindElements.Add(m_LeftBindText);
        }
        if (m_RightBindText)
        {
            m_ListKeyBindElements.Add(m_RightBindText);
        }
        if (m_BoostBindText)
        {
            m_ListKeyBindElements.Add(m_BoostBindText);
        }
        if (m_DriftBindText)
        {
            m_ListKeyBindElements.Add(m_DriftBindText);
        }
    }

    // Update is called once per frame
    void Update () {
        base.Update();

        switch(m_eMainMenuState)
        {
            case eMainMenuState.eMMS_Front:
                {
                    break;
                }

            case eMainMenuState.eMMS_Settings:
                {
                    RefreshBindText();
                    break;
                }
        }
	}

    public void ButttonClicked( tk2dUIItem _ClickedItem)
    {

        if(_ClickedItem.gameObject == m_KeybindButton_Thrust)
        {
            GameSystemPointers.instance.m_InputManager.CreateGameBind(InputNamespace.eInputCommand_Game.eIC_Game_Thrust);
        }
        else if(_ClickedItem.gameObject == m_KeybindButton_TurnLeft)
        {
            GameSystemPointers.instance.m_InputManager.CreateGameBind(InputNamespace.eInputCommand_Game.eIC_Game_MoveLeft);
        }
        else if (_ClickedItem.gameObject == m_KeybindButton_TurnRight)
        {
            GameSystemPointers.instance.m_InputManager.CreateGameBind(InputNamespace.eInputCommand_Game.eIC_Game_MoveRight);
        }
        else if (_ClickedItem.gameObject == m_KeybindButton_Boost)
        {
            GameSystemPointers.instance.m_InputManager.CreateGameBind(InputNamespace.eInputCommand_Game.eIC_Game_Boost);
        }
        else if (_ClickedItem.gameObject == m_KeybindButton_Drift)
        {
            GameSystemPointers.instance.m_InputManager.CreateGameBind(InputNamespace.eInputCommand_Game.eIC_Game_Drift);
        }
        else if(_ClickedItem.gameObject == m_SettingsButton)
        {
            SetSettingsVisible(true);
            SetMainMenuVisible(false);
            m_eMainMenuState = eMainMenuState.eMMS_Settings;
        }
        else if(_ClickedItem.gameObject == m_SettingsDoneButton)
        {
            //Checks for game binds being valid before allowing the user to save
            if (GameSystemPointers.instance.m_InputManager.GetBindsValid(true))
            {
                SetSettingsVisible(false);
                SetMainMenuVisible(true);
                m_eMainMenuState = eMainMenuState.eMMS_Front;

                GameSystemPointers.instance.m_InputManager.SaveKeyBinds();
            }

        }
        else if(_ClickedItem.gameObject == m_PlayButton)
        {
            GameSystemPointers.instance.m_LoadingScreen.LoadScene("Level1_Rework");
        }
		else if(_ClickedItem.gameObject == m_ScriptedSequenceButton)
		{
			GameSystemPointers.instance.m_GameActionManager.ParseJSON("GameJSONData/Cutscenes/TestCutscene");
			m_ListMainMenuElements.Remove(m_ScriptedSequenceButton);
			Destroy(m_ScriptedSequenceButton);
			m_ExampleText.GetComponent<Renderer>().enabled = true;

		}
	}

    public void SetSettingsVisible(bool _bVisible)
    {
        for(int i = 0; i < m_ListKeyBindElements.Count; i++)
        {
            m_ListKeyBindElements[i].SetActive(_bVisible);
        }

        m_SettingsDoneButton.SetActive(_bVisible);
    }

    public void SetMainMenuVisible(bool _bVisible)
    {
        for(int i = 0; i < m_ListMainMenuElements.Count; i++)
        {
            m_ListMainMenuElements[i].SetActive(_bVisible);
        }
    }

    private void RefreshBindText(  )
    {
        List<s_InputManager.sKeyBind> _ListBinds;

        //===Thrust Text
        if(m_ThrustBindText)
        {
            tk2dTextMesh _TextMesh = m_ThrustBindText.GetComponent<tk2dTextMesh>();
            if (_TextMesh)
            {
                _ListBinds = GameSystemPointers.instance.m_InputManager.GetGameBindData(InputNamespace.eInputCommand_Game.eIC_Game_Thrust);

                _TextMesh.text = GetStringFromBindData(_ListBinds);
            }
        }

        //===Left Text
        if (m_LeftBindText)
        {
            tk2dTextMesh _TextMesh = m_LeftBindText.GetComponent<tk2dTextMesh>();
            if (_TextMesh)
            {
                _ListBinds = GameSystemPointers.instance.m_InputManager.GetGameBindData(InputNamespace.eInputCommand_Game.eIC_Game_MoveLeft);

                _TextMesh.text = GetStringFromBindData(_ListBinds);
            }
        }

        //===Right Text
        if (m_RightBindText)
        {
            tk2dTextMesh _TextMesh = m_RightBindText.GetComponent<tk2dTextMesh>();
            if (_TextMesh)
            {
                _ListBinds = GameSystemPointers.instance.m_InputManager.GetGameBindData(InputNamespace.eInputCommand_Game.eIC_Game_MoveRight);

                _TextMesh.text = GetStringFromBindData(_ListBinds);
            }
        }

        //===Boost Text
        if (m_BoostBindText)
        {
            tk2dTextMesh _TextMesh = m_BoostBindText.GetComponent<tk2dTextMesh>();
            if (_TextMesh)
            {
                _ListBinds = GameSystemPointers.instance.m_InputManager.GetGameBindData(InputNamespace.eInputCommand_Game.eIC_Game_Boost);

                _TextMesh.text = GetStringFromBindData(_ListBinds);
            }
        }

        //===Drift Text
        if (m_DriftBindText)
        {
            tk2dTextMesh _TextMesh = m_DriftBindText.GetComponent<tk2dTextMesh>();
            if (_TextMesh)
            {
                _ListBinds = GameSystemPointers.instance.m_InputManager.GetGameBindData(InputNamespace.eInputCommand_Game.eIC_Game_Drift);

                _TextMesh.text = GetStringFromBindData(_ListBinds);
            }
        }
    }

    protected string GetStringFromBindData(List<s_InputManager.sKeyBind> _ListBinds)
    {
        //Clear stringbuilder
        m_StringBuilder.Remove(0, m_StringBuilder.Length);

        for (int i = 0; i < _ListBinds.Count; i++)
        {
            if (m_StringBuilder.Length != 0)
            {
                m_StringBuilder.Append(", ");
            }

            switch (_ListBinds[i].m_eBindType)
            {
                case s_InputManager.eBindType.eBT_Axis:
                    {
                        m_StringBuilder.Append(_ListBinds[i].m_sBindName);
                        if (_ListBinds[i].m_bPositive)
                        {
                            m_StringBuilder.Append(" Positive");
                        }
                        else
                        {
                            m_StringBuilder.Append(" Negative");

                        }
                        break;
                    }

                case s_InputManager.eBindType.eBT_JoystickButton:
                    {
                        m_StringBuilder.Append(_ListBinds[i].m_sBindName);
                        break;
                    }

                case s_InputManager.eBindType.eBT_KeyboardButton:
                    {
                        m_StringBuilder.Append(_ListBinds[i].m_KeyCode.ToString());
                        break;
                    }
            }
        }

        return m_StringBuilder.ToString();
    }
}

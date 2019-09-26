using UnityEngine;
using System.Collections;

public class GameSystemPointers : MonoBehaviour {

	public static GameSystemPointers _instance;
	
	public s_GameplayRecorder	 	m_GameplayRecorder;
	public s_CameraLimiter 			m_CameraLimiter;
	public s_InputManager 			m_InputManager;
	public s_PlayerManager			m_PlayerManager;
	public s_BaseLevelScript		m_LevelScript;
    public s_BaseMenuScript         m_sMenuScript;
    public tk2dCamera               m_Camera;
    public s_LoadingScreen          m_LoadingScreen;
    public s_GameActionManager      m_GameActionManager;

    protected GameObject            m_goLoadingScreen;

	
	public static GameSystemPointers instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = GameObject.FindObjectOfType<GameSystemPointers>();
                if (_instance)
                {
                    Debug.LogError("GSP has been called externally before it's own awake has been called. Ensure that you are not trying to access GSP from within another system's Awake function");
                    DontDestroyOnLoad(_instance.gameObject);
                }
                else
                {
                    Debug.LogError("Could not find instance of GSP");
                }
			}
			
			return _instance;
		}
	}
	
	void Awake()
	{
		if(_instance == null)
		{
			Debug.Log ("Created GSP");
			_instance = this;
			DontDestroyOnLoad(this);
            Application.targetFrameRate = -1;

            //==============Set Up loading Screen
            if (m_goLoadingScreen == null)
            {
                m_goLoadingScreen = Instantiate(Resources.Load("ResourcesPrefabs/LoadingScreen", typeof(GameObject))) as GameObject;
                //Grab script
                m_LoadingScreen = m_goLoadingScreen.GetComponent<s_LoadingScreen>();
            }
            else
            {
                Debug.LogError("Trying to create more than one instance of the Loading Screen. Find out why");
            }
        }
		else
		{
			if(this != _instance)
			{
				Destroy(this);
			}
		}
		
	}
}

using UnityEngine;
using System.Collections;

public class s_FPS : MonoBehaviour {

	float deltaTime = 0.0f;
	
	void Update()
	{
		deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
	}
	
	void OnGUI()
	{

		//Display FPS
		int w = Screen.width, h = Screen.height;
		
		GUIStyle style = new GUIStyle();

		Rect rect = new Rect(0, 0, w, h * 2 / 100);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h * 2 / 100;
		style.normal.textColor = new Color (0.0f, 0.0f, 0.5f, 1.0f);
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
		GUI.Label(rect, text, style);


		//Display Boost Volume

		rect = new Rect(0,20, w, h * 2 / 100);
        float _fBoostVolume = 0;

        s_EntityPlayer _PlayerScript = GameSystemPointers._instance.m_PlayerManager.GetPlayerScript(0);
        if(_PlayerScript)
        {
            _fBoostVolume = _PlayerScript.GetBoostQuantity();
        }

        text = string.Format("Boost Remaining: {0}",_fBoostVolume);
		GUI.Label(rect, text, style);

        //Display Camera XY
        rect = new Rect(0, 40, w, h * 2 / 100);
        text = string.Format("Camera Pos: {0}, {0}", transform.position.x, transform.position.y);
        GUI.Label(rect, text, style);


    }
}




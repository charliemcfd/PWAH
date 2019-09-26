using UnityEngine;
using System.Collections;

public class W1_L1_LevelScript : s_BaseLevelScript {

	void  Start()
	{
		base.Start();
		Debug.Log ("W1L1");
		//GameSystemPointers._instance.m_PlayerManager.CreatePlayer();
	}

	void Update()
	{
        base.Update();
	}
	
}

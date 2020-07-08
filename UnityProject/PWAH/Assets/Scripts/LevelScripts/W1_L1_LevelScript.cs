using UnityEngine;
using System.Collections;

public class W1_L1_LevelScript : s_BaseLevelScript {

	/*
	 * Note: This is a test for inheriting from the base level script. 
	 * If levels required additional logic (Such as running of timers for spawning entities etc.) it could be carried out in a derived class such as this
	 * As it stands, this is not currently necessary as the levels are not yet suitably complex to warrant that behavior.
	 */
	void  Start()
	{
		base.Start();
	}

	void Update()
	{
        base.Update();
	}
	
}

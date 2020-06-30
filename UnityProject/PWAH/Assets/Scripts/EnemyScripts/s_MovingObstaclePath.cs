using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class s_MovingObstaclePath : MonoBehaviour
{
	public enum MovableOstacleActionType
	{
		Move,
		Delay,
		Shake,
	};
	[System.Serializable]
	public struct MovableObstacleAction
	{
		public MovableOstacleActionType Type;
		public float Time;
	}

	public MovableObstacleAction[] Actions;
	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class s_EndPortalScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter2D(Collider2D _OtherCollider)
    {
        //Try and get player script from the collider. If we can get it, then it is the player
        s_EntityPlayer _PlayerScript = _OtherCollider.GetComponent<s_EntityPlayer>();


        if (!_PlayerScript)
        {
            //If we couldn't get it, try to get it in the collided object's parent.
            _PlayerScript = _OtherCollider.GetComponentInParent<s_EntityPlayer>();
        }

        if(_PlayerScript)
        {
            //Don't do anything if this is a replay player
            if (_PlayerScript.m_bIsReplay)
            {
                return;
            }

            HandlePlayerTrigger(_PlayerScript);
        }
        


    }

    private void HandlePlayerTrigger(s_EntityPlayer _PlayerScript)
    {
        //Set Player inactive (Prevents movement and collision)
        _PlayerScript.SetActive(false, true);

        //Set Player's state to be the "Level Complete" State. This sets it up for the end-level animation sequence.
        _PlayerScript.SetLevelCompleteState(this.transform.position);

        //Send message to player that it should no longer recieve input / gameplay actions
        _PlayerScript.SetShouldRecieveInput(false);

    }
}

using UnityEngine;
using System.Collections;

public class s_BaseMenuScript : MonoBehaviour {

    // Use this for initialization
    public virtual void Start () {
        GameSystemPointers._instance.m_sMenuScript = this;

    }

    // Update is called once per frame
    public virtual void Update () {
	
	}
}

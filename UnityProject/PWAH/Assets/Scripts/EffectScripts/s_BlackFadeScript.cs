using UnityEngine;
using System.Collections;
using DG.Tweening;

public class s_BlackFadeScript : MonoBehaviour {

    public enum eFadeState
    {
        eFS_FadedOut,
        eFS_Fading,
        eFS_FadedIn
    }

    private tk2dSprite m_FadeSprite;
    private eFadeState m_eFadeState;

	// Use this for initialization
	void Start () {

        m_FadeSprite = GetComponent<tk2dSprite>();
        m_eFadeState = eFadeState.eFS_FadedIn;

        SetInitialState();
		FadeOut();
	}
	
	// Update is called once per frame
	void Update () {

	}

    private void SetInitialState()
    {
        m_FadeSprite.GetComponent<Renderer>().enabled = true;

    }

    private void FadeOutComplete()
    {
        m_FadeSprite.GetComponent<Renderer>().enabled = false;
        m_eFadeState = eFadeState.eFS_FadedOut;
    }

    private void FadeInComplete()
    {
        m_eFadeState = eFadeState.eFS_FadedIn;
		s_EventManager.CameraResetToSpawnPositionEvent.Invoke();
	}

	public void FadeIn()
    {
        m_FadeSprite.GetComponent<Renderer>().enabled = true;
        m_FadeSprite.DOColor(new Color(1.0f, 1.0f, 1.0f, 1.0f), 0.5f).OnComplete(FadeInComplete);
        m_eFadeState = eFadeState.eFS_Fading;
    }

    public void FadeOut()
    {
        m_FadeSprite.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.0f), 0.5f).OnComplete(FadeOutComplete);
        m_eFadeState = eFadeState.eFS_Fading;

    }

    public eFadeState GetFadeState()
    {
        return m_eFadeState;
    }
}

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class s_LoadingScreen : MonoBehaviour {

    private tk2dSprite m_bgSprite;
    private bool m_bFiredLoadCompleteEvent;
    private bool m_AnchorScript;



    // Use this for initialization
    void Start () {
        m_bgSprite = GetComponent<tk2dSprite>();
        m_bgSprite.GetComponent<Renderer>().enabled = false;

        DontDestroyOnLoad(this.gameObject);

        m_bFiredLoadCompleteEvent = false;

        m_AnchorScript = GetComponent<tk2dCameraAnchor>();
    }

    // Update is called once per frame
    void Update () {

    }

    public void LoadScene( string _sScene)
    {
        //This will be responsible for storing the string for the scene name, beginning transition animations etc.
        //The actual call to Async will be made in update, passing the stored string once everything has been sorted out

        //For now, it just calls the async function and puts a black quad over everything

        s_BlackFadeScript _FadeScript = GameSystemPointers.instance.m_Camera.GetComponentInChildren<tk2dSprite>().GetComponent<s_BlackFadeScript>();
        if (_FadeScript)
        {
            _FadeScript.FadeIn();
        }

        StartCoroutine(AsynchronousLoad(_sScene));
    }

    protected void CloseLoadingScreen()
    {
        //This function is what will clean up the various aspects of the screen.
        m_bgSprite.GetComponent<Renderer>().enabled = false;

        //Reset the fired flag for the next time the screen is needed
        m_bFiredLoadCompleteEvent = false;

    }


    IEnumerator AsynchronousLoad(string scene)
    {

        yield return null;

        AsyncOperation ao = SceneManager.LoadSceneAsync(scene);
        ao.allowSceneActivation = false;

        while (!ao.isDone)
        {
            // [0, 0.9] > [0, 1]
            float progress = Mathf.Clamp01(ao.progress / 0.9f);
            //Debug.Log("Loading progress: " + (progress * 100) + "%");

            // Loading completed
            if (ao.progress == 0.9f)
            {
                s_BlackFadeScript _FadeScript = GameSystemPointers.instance.m_Camera.GetComponentInChildren<tk2dSprite>().GetComponent<s_BlackFadeScript>();
                if (_FadeScript.GetFadeState() == s_BlackFadeScript.eFadeState.eFS_FadedIn)
                {

                    if (!m_bFiredLoadCompleteEvent)
                    {
                        s_EventManager.SceneLoadedEvent.Invoke();
                        m_bFiredLoadCompleteEvent = true;
                    }

                    if (Input.anyKey)
                    {
                        ao.allowSceneActivation = true;
                        CloseLoadingScreen();
                    }
                }
            }

            yield return null;
        }
        }
    }

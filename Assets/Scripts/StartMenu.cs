using System.Collections;
using UnityEngine;

public class StartMenu : MonoBehaviour {

    private FadeCanvas logoFade;
    private FadeCanvas optionsFade;
    private FadeCanvas displayFade;
    private PlayerController playerController;

    public float logoDisplayTime = 2.0f;
    public float initalWait = 2.0f;

    private void Awake()
    {
        foreach (FadeCanvas t in GetComponentsInChildren<FadeCanvas>())
        {
            if (t.gameObject.name == "Logo")
            {
                logoFade = t;
                continue;
            }
            else if(t.gameObject.name == "DisplayOptions")
            {
                displayFade = t;
                continue;
            }

        }

        optionsFade = GetComponent<FadeCanvas>();
    }



    // Use this for initialization
    void Start () {
        playerController = GetComponentInParent<PlayerController>();
        StartCoroutine(Begin());
		
	}

    // Update is called once per frame
    void Update () {
		
	}

    public IEnumerator Begin()
    {
        yield return new WaitForSeconds(initalWait);
        logoFade.ToggleFade();
        yield return new WaitForSeconds(logoDisplayTime);
        logoFade.ToggleFade();
        yield return new WaitForSeconds(logoFade.timeToFade);
        displayFade.ToggleFade();

    }

    public IEnumerator OnVRButtonPressed()
    {
        optionsFade.ToggleFade();
        yield return new WaitForSeconds(optionsFade.timeToFade);
        playerController.UpdateState(AppState.VIDEO_VR);
        gameObject.SetActive(false);
    }

    public IEnumerator OnPortraitButtonPressed()
    {
        optionsFade.ToggleFade();
        yield return new WaitForSeconds(optionsFade.timeToFade);
        playerController.UpdateState(AppState.VIDEO_PORTRAIT);
        gameObject.SetActive(false);
    }
}

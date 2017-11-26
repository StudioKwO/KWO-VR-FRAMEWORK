using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum StartButton
{
    VR,
    PORTRAIT
}


public class StartMenu : MonoBehaviour
{

    private FadeCanvas logoFade;
    private FadeCanvas optionsFade;
    private FadeCanvas displayFade;
    private PlayerController playerController;
    private Button vrModeButton;
    private Rect vrRect;
    private Button portraitModeButton;
    private Rect portraitRect;

    public float logoDisplayTime = 2.0f;
    public float initalWait = 2.0f;
    public bool skipOptions = false;
    public bool skipLogo = false;

    private bool canInteract = false;

    public bool CanInteract { get { return canInteract; } }

    private void Awake()
    {
        foreach (FadeCanvas t in GetComponentsInChildren<FadeCanvas>())
        {
            if (t.gameObject.name == "Logo")
            {
                logoFade = t;
                continue;
            }
            else if (t.gameObject.name == "DisplayOptions")
            {
                displayFade = t;
                continue;
            }

        }

        foreach (Button button in GetComponentsInChildren<Button>())
        {
            if (button.gameObject.name == "VrModeButton")
            {
                vrModeButton = button;
            }
            else if (button.gameObject.name == "PortraitModeButton")
            {
                portraitModeButton = button;
            }
        }

        optionsFade = GetComponent<FadeCanvas>();
    }



    // Use this for initialization
    void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
        StartCoroutine(Begin());
        vrRect = new Rect(new Vector2(vrModeButton.transform.position.x, vrModeButton.transform.position.y), vrModeButton.GetComponent<RectTransform>().rect.size);
        Debug.Log(string.Format("The rect of the vr button: {0}", vrRect));
        portraitRect = new Rect(new Vector2(portraitModeButton.transform.position.x, portraitModeButton.transform.position.y), portraitModeButton.GetComponent<RectTransform>().rect.size);
        Debug.Log(string.Format("The rect of the portrait button: {0}", portraitRect));

    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator Begin()
    {
        if (!skipLogo)
        {
            canInteract = false;
            yield return new WaitForSeconds(initalWait);
            logoFade.ToggleFade();
            yield return new WaitForSeconds(logoDisplayTime);
            logoFade.ToggleFade();
            yield return new WaitForSeconds(logoFade.timeToFade);
            logoFade.gameObject.SetActive(false);
            displayFade.ToggleFade();
            canInteract = true;
        }

        if(skipOptions)
        {
            StartCoroutine(VRButtonAction());
        }

    }

    public bool ContaintButton(StartButton type, Vector2 pos)
    {
        bool toReturn = false;

        switch (type)
        {
            case StartButton.VR:
                if (vrRect.Contains(pos))
                    toReturn = true;
                break;
            case StartButton.PORTRAIT:
                if (portraitRect.Contains(pos))
                    toReturn = true;
                break;
            default:
                toReturn = false;
                break;
        }

        return toReturn;
    }

    public void BeginButtonPressed(StartButton type)
    {
        switch (type)
        {
            case StartButton.VR:
                vrModeButton.onClick.Invoke();
                break;
            case StartButton.PORTRAIT:
                portraitModeButton.onClick.Invoke();
                break;
            default:
                break;
        }
    }

    public void OnVRButtonPressed()
    {
        StartCoroutine(VRButtonAction());
    }

    public IEnumerator VRButtonAction()
    {
        optionsFade.ToggleFade();

        yield return new WaitForSeconds(optionsFade.timeToFade);
        playerController.UpdateState(AppState.VIDEO_VR);

        gameObject.SetActive(false);
    }

    public void OnPortraitButtonPressed()
    {
        StartCoroutine(PortraitButtonAction());
    }

    public IEnumerator PortraitButtonAction()
    {
        optionsFade.ToggleFade();

        yield return new WaitForSeconds(optionsFade.timeToFade);
        playerController.UpdateState(AppState.VIDEO_PORTRAIT);

        gameObject.SetActive(false);
    }
}

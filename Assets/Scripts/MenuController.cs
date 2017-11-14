using System.Collections;
using UnityEngine;

public enum MenuState
{
    HIDE,
    SHOW
}

public enum MenuType
{
    VR,
    PORTRAIT
}


public class MenuController : MonoBehaviour
{

    private MenuState menuState;

    public float fadeTime = 3.0f;
    public float minRotationAngle = 60.0f;
    public MenuType type;
    private bool isRotating = false;
    private bool isInteracting = false;

    private MeshRenderer reticleMeshRender;

    /// <summary>
    /// Display canvas of the video player, where the fade effect will be applied
    /// </summary>
    private FadeCanvas fadeCanvas;

    public MenuState GetMenuState()
    {
        return menuState;
    }

    public bool IsRotating { get { return isRotating; } }

    public bool IsInteracting { get { return isInteracting; } }

    public void SetState(MenuState newState)
    {

        switch (newState)
        {
            case MenuState.HIDE:
                if (newState != menuState)
                {
                    OnFadeOut();
                }
                break;
            case MenuState.SHOW:
                if (newState != menuState)
                {
                    OnFadeIn();
                }
                break;
            default:
                break;
        }

        menuState = newState;
    }

    //private void OnEnable()
    //{
    //    PlayerController.OnPlayerStateUpdated += OnPlayerStateUpdated;
    //}

    //private void OnDisable()
    //{
    //    PlayerController.OnPlayerStateUpdated -= OnPlayerStateUpdated;
    //}

    //private void OnPlayerStateUpdated(AppState newState)
    //{
    //    switch (newState)
    //    {
    //        case AppState.VIDEO_VR:
    //            inVR = true;
    //            break;
    //        case AppState.VIDEO_PORTRAIT:
    //            inVR = false;
    //            break;
    //        case AppState.MENU_VR:
    //            break;
    //        default:
    //            break;
    //    }
    //}

    private void Awake()
    {
        GameObject reticlePointer = GameObject.FindGameObjectWithTag("MainCamera").GetComponentInChildren<GvrReticlePointer>().gameObject;
        reticleMeshRender = reticlePointer.GetComponent<MeshRenderer>();
    }

    // Use this for initialization
    void Start()
    {
        fadeCanvas = GetComponentInChildren<FadeCanvas>();
        menuState = MenuState.HIDE;
        if (type == MenuType.VR)
            reticleMeshRender.enabled = false;

    }

    public void OnFadeIn()
    {
        if(type == MenuType.VR)
            reticleMeshRender.enabled = true;

        fadeCanvas.OnFadeIn("video_controller_fadein");
        StartCoroutine(FadeTimer());
    }

    private IEnumerator FadeTimer()
    {
        yield return new WaitForSeconds(fadeTime);

        while (type == MenuType.VR && (isRotating || isInteracting))
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.75f);

        if (menuState == MenuState.SHOW)
        {
            SetState(MenuState.HIDE);
        }
    }

    public void OnFadeOut()
    {
        //This function will be called to fade out the video controls, using ITween
        fadeCanvas.OnFadeOut("video_controller_fadeout");
        if (type == MenuType.VR)
            reticleMeshRender.enabled = false;
    }

    public void OnPointerEnter()
    {
        isInteracting = true;
    }

    public void OnPointerExit()
    {
        isInteracting = false;
    }

    public void RotateAdd(Vector3 value, GameObject gameObject)
    {
        Hashtable table = new Hashtable();
        isRotating = true;

        table.Add("name", "rotateControls");
        table.Add("time", 1.0f);
        table.Add("easetype", iTween.EaseType.easeInOutCubic);
        table.Add("oncomplete", "OnRotationComplete");
        table.Add("oncompletetarget", gameObject);
        table.Add("amount", value);
        iTween.RotateAdd(gameObject, table);
    }

    void OnRotationComplete()
    {
        isRotating = false;
    }

}

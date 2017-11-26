using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitMenu : MonoBehaviour {

    private FadeCanvas fadeCanvas;
    private VideoController videoController;
    private MenuController menuController;
    private PlayerController playerController;

    private MeshRenderer reticleMeshRender;

    public MenuType type;


    // Use this for initialization
    void Start () {
       

    }

    private void Awake()
    {
        fadeCanvas = GetComponent<FadeCanvas>();
        videoController = GetComponentInParent<VideoController>();
        menuController = GetComponentInParent<MenuController>();
        GameObject reticlePointer = GameObject.FindGameObjectWithTag("MainCamera").GetComponentInChildren<GvrReticlePointer>().gameObject;
        reticleMeshRender = reticlePointer.GetComponent<MeshRenderer>();
        playerController = GetComponentInParent<PlayerController>();
    }

    private void OnEnable()
    {
        if(type == MenuType.VR)
        {
            videoController.OnVideoVrEnd += OnVideoEnd;
        }
        else
        {
            videoController.OnVideoPortraitEnd += OnVideoEnd;
        }

        
    }


    private void OnDisable()
    {
        if(type == MenuType.VR)
        {
            videoController.OnVideoVrEnd -= OnVideoEnd;
        }
        else
        {
            videoController.OnVideoPortraitEnd -= OnVideoEnd;
        }
        
    }

    private void OnVideoEnd()
    {
        menuController.OnFadeOut();
        fadeCanvas.OnFadeIn();
        reticleMeshRender.enabled = true;
    }


    // Update is called once per frame
    void Update () {
		
	}

    public void OnRestart()
    {
        videoController.VideoPlayer.Stop();
        videoController.VideoPlayer.frame = 0;
        StartCoroutine(RestartVideo());
        //start fade , wait for fade and start the video
    }

    private IEnumerator RestartVideo()
    {
        fadeCanvas.OnFadeOut();
        yield return new WaitUntil(IsFaded);
        playerController.UpdateState(AppState.VIDEO_VR);
        reticleMeshRender.enabled = false;


    }

    public void OnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

    private bool IsFaded()
    {
        return fadeCanvas.IsFaded;
    }

}

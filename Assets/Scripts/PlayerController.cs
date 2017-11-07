using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR;

//TODO: Figure out how to connect the touch input with the curveUI.
// https://gamedev.stackexchange.com/questions/93592/graphics-raycaster-of-unity-how-does-it-work check this for the graphics raycast
// Solution maybe that I change the ui to be on world space instead of screen space.
// Change the videoplayer ui render type when on portrait mode

public enum AppState
{
    VIDEO_VR,
    VIDEO_PORTRAIT,
    MENU
}

/**
* <summary>
* This class will be responsible for handling all of the player input. Use virtual venues as a base.
* Take into account that the input methods can vary by platform. ( see google VR and Vive)
* </summary>
*/
public class PlayerController : MonoBehaviour
{

    public delegate void PlayerStateUpdate(AppState newState);
    public static event PlayerStateUpdate OnPlayerStateUpdated;

    private AppState currentState;
    public AppState State { get { return currentState; } }

    private VideoController videoController;
    private MenuController menuController;
    private StartMenu startMenu;
    private Transform menuPivot;
    private Transform cameraTrans;
    private float lastYAngle;
    private GvrPointerInputModule vrInputModule;
    private StandaloneInputModule touchInputModule;

    public GameObject controls;
    public GameObject eventSystem;


    public void UpdateState(AppState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case AppState.VIDEO_VR:
                vrInputModule.enabled = true;
                touchInputModule.enabled = false;
                StartCoroutine(PlayerVRUpdate());
                break;
            case AppState.VIDEO_PORTRAIT:
                vrInputModule.enabled = false;
                touchInputModule.enabled = true;
                StartCoroutine(PlayerPortraitUpdate());
                break;
            case AppState.MENU:
                vrInputModule.enabled = false;
                touchInputModule.enabled = true;
                StartCoroutine(PlayerMenuUpdate());
                break;
            default:
                break;
        }

        if (OnPlayerStateUpdated != null)
            OnPlayerStateUpdated(newState);
    }

    // Use this for initialization
    void Start()
    {
        videoController = controls.GetComponent<VideoController>();
        menuController = controls.GetComponent<MenuController>();
        startMenu = GetComponentInChildren<StartMenu>();
        menuPivot = controls.transform.parent;
        cameraTrans = GetComponentInChildren<Camera>().gameObject.transform;
        vrInputModule = eventSystem.GetComponent<GvrPointerInputModule>();
        touchInputModule = eventSystem.GetComponent<StandaloneInputModule>();

        //Start the application in the menu state
        UpdateState(AppState.MENU);
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            DeveloperConsole.Instance.Toggle();
        }
#endif
    }

    public IEnumerator PlayerMenuUpdate()
    {
        if (XRSettings.enabled)
        {
            XRSettings.LoadDeviceByName("");
            yield return null;
            Camera.main.ResetAspect();
        }

        //TODO: this is the place where the postion of the canvas should be set for the portrait mode
        //Debug.Log(string.Format("The position of the vr mode button on the viewport is: {0}", vrModeButton.transform.position));
        //Debug.Log(string.Format("The position of the portrait mode button on the viewport is: {0}",portraitModeButton.transform.position));
        while (currentState == AppState.MENU)
        {

            if (Input.touchCount > 0 && startMenu.CanInteract)
            {

            }

            yield return null;
        }
       
    }


    public IEnumerator PlayerPortraitUpdate()
    {
        while (currentState == AppState.VIDEO_PORTRAIT)
        {
            Input.gyro.enabled = true;
            if (!XRSettings.enabled && Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight)
            {
                yield return new WaitForSeconds(1);

                if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft || Input.deviceOrientation == DeviceOrientation.LandscapeRight)
                {

                    XRSettings.LoadDeviceByName("Cardboard");
                    yield return null;

                    XRSettings.enabled = true;
                    UpdateState(AppState.VIDEO_VR);
                    continue;
                }
            }

            //Check the way the video is rotating in each device orientation.
            if (SystemInfo.supportsGyroscope && Input.deviceOrientation == DeviceOrientation.Portrait)
            {
                yield return new WaitForSeconds(1);

                if (Input.deviceOrientation == DeviceOrientation.Portrait)
                {
                    Camera.main.transform.localRotation = new Quaternion(Input.gyro.attitude.x, Input.gyro.attitude.y, Input.gyro.attitude.z, -Input.gyro.attitude.w);
                    Camera.main.transform.Rotate(90, 0, 0);
                    Camera.main.transform.rotation = Quaternion.Euler(new Vector3(0f, Camera.main.transform.rotation.eulerAngles.y, 0f));
                }

            }


            yield return null;
        }
    }

    public IEnumerator PlayerVRUpdate()
    {
        lastYAngle = 0.0f;

        while (currentState == AppState.VIDEO_VR)
        {

            if (XRSettings.enabled && Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
            {
                yield return new WaitForSeconds(1);

                if (Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
                {
                    XRSettings.LoadDeviceByName("");
                    yield return null;

                    Camera.main.ResetAspect();
                    UpdateState(AppState.VIDEO_PORTRAIT);
                    continue;
                }
            }

            //Change to world position of the ray so that I can get the position of the player. Update the player so that in the portrait mode, the player can be clicked. It can get tricky.
            if (Input.GetMouseButtonDown(0))
            {
                //Debug.Log("Button clicked");
                if (menuController.GetMenuState() == MenuController.MenuState.HIDE)
                {
                    menuPivot.transform.localRotation = new Quaternion(menuPivot.transform.localRotation.x, cameraTrans.rotation.y, menuPivot.transform.localRotation.z, menuPivot.transform.localRotation.w);
                    menuController.SetState(MenuController.MenuState.SHOW);
                }

            }
            float deltaY = Mathf.DeltaAngle(menuPivot.rotation.eulerAngles.y, cameraTrans.rotation.eulerAngles.y);

            //Debug.Log(string.Format("The delta angle between the player and the menu is: {0}", deltaY));
            if (!menuController.IsRotating && Mathf.Abs(deltaY) > menuController.minRotationAngle)
            {
                //Need to check the rotation of the camera, something is going wrong 
                //Debug.Log(string.Format("The delta of the rotation of the camera and the video player pivot is: {0}", Mathf.Abs(Mathf.DeltaAngle(lastYAngle, cameraTrans.rotation.eulerAngles.y))));
                if (Mathf.Abs(Mathf.DeltaAngle(lastYAngle,cameraTrans.rotation.eulerAngles.y)) <= 0.001f)
                {
                    //Rotate the menu options to the camera rotation. TODO:Add the contribution of the player speed to catch up with him.
                    menuController.RotateAdd(new Vector3(0.0f, deltaY), menuPivot.gameObject);
                }
                else
                {
                    lastYAngle = cameraTrans.rotation.eulerAngles.y;
                }

            }

            yield return null;
        }

        //Debug.Log("Exit Player video update");

    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}

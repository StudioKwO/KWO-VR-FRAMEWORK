using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR;

//TODO: 
// https://gamedev.stackexchange.com/questions/93592/graphics-raycaster-of-unity-how-does-it-work check this for the graphics raycast

public enum AppState
{
    VIDEO_VR,
    VIDEO_PORTRAIT,
    MENU_VR,
    MENU_PORTRAIT,
    EXIT_MENU_VR,
    EXIT_MENU_PORTRAIT
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

    private MenuController vrMenuController;
    private MenuController portraitMenuController;
    private StartMenu startMenu;
    private Transform menuPivot;
    private Transform cameraTrans;
    private float lastYAngle;
    private GvrPointerInputModule vrInputModule;
    private StandaloneInputModule touchInputModule;

    public string deviceName;
    public GameObject vrControls;
    public GameObject portraitControls;
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
            case AppState.MENU_VR:
                vrInputModule.enabled = true;
                touchInputModule.enabled = false;
                StartCoroutine(PlayerVrMenuUpdate());
                break;
            case AppState.MENU_PORTRAIT:
                vrInputModule.enabled = false;
                touchInputModule.enabled = true;
                StartCoroutine(PlayerPortraitMenuUpdate());
                break;
            case AppState.EXIT_MENU_VR:
                vrInputModule.enabled = true;
                touchInputModule.enabled = false;
                StartCoroutine(PlayerVrExitUpdate());
                break;
            case AppState.EXIT_MENU_PORTRAIT:
                vrInputModule.enabled = false;
                touchInputModule.enabled = true;
                StartCoroutine(PlayerPortraitExitUpdate());
                break;
            default:
                break;
        }

        if (OnPlayerStateUpdated != null)
            OnPlayerStateUpdated(newState);
    }

    private void Awake()
    {
        if (deviceName != "Oculus")
        {
            StartCoroutine(ChangeToPortrait());
        }
        else
        {
            StartCoroutine(ChangeToVr());
        }
    }

    // Use this for initialization
    void Start()
    {
        vrMenuController = vrControls.GetComponent<MenuController>();
        portraitMenuController = portraitControls.GetComponent<MenuController>();
        startMenu = GetComponentInChildren<StartMenu>();
        menuPivot = vrControls.transform.parent;
        cameraTrans = GetComponentInChildren<Camera>().gameObject.transform;
        vrInputModule = eventSystem.GetComponent<GvrPointerInputModule>();
        touchInputModule = eventSystem.GetComponent<StandaloneInputModule>();

        //Start the application in the menu state
        UpdateState(AppState.MENU_PORTRAIT);
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

    public IEnumerator PlayerVrMenuUpdate()
    {
        //TODO: this is the place where the postion of the canvas should be set for the portrait mode
        //Debug.Log(string.Format("The position of the vr mode button on the viewport is: {0}", vrModeButton.transform.position));
        //Debug.Log(string.Format("The position of the portrait mode button on the viewport is: {0}",portraitModeButton.transform.position));
        yield return ChangeToVr();

        while (currentState == AppState.MENU_VR)
        {
            yield return null;
        }

    }

    public IEnumerator PlayerPortraitMenuUpdate()
    {
        yield return ChangeToPortrait();
        while (currentState == AppState.MENU_PORTRAIT)
        {
            yield return null;
        }
    }

    public IEnumerator PlayerVrExitUpdate()
    {
        yield return ChangeToVr();

        while (currentState == AppState.EXIT_MENU_VR)
        {

            yield return null;
        }
    }

    public IEnumerator PlayerPortraitExitUpdate()
    {
        yield return ChangeToPortrait();
        while (currentState == AppState.EXIT_MENU_PORTRAIT)
        {

            yield return null;
        }
    }


    public IEnumerator PlayerPortraitUpdate()
    {
        yield return ChangeToPortrait();
        Input.gyro.enabled = true;

        while (currentState == AppState.VIDEO_PORTRAIT)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //Debug.Log("Button clicked");
                if (portraitMenuController.GetMenuState() == MenuState.HIDE)
                {
                    portraitMenuController.SetState(MenuState.SHOW);
                }

            }

            //Check the way the video is rotating in each device orientation.
            if (Input.deviceOrientation == DeviceOrientation.Portrait || Input.deviceOrientation == DeviceOrientation.FaceDown || Input.deviceOrientation == DeviceOrientation.FaceUp || Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown || Input.deviceOrientation == DeviceOrientation.LandscapeLeft)
            {
                Camera.main.transform.localRotation = new Quaternion(Input.gyro.attitude.x, Input.gyro.attitude.y, Input.gyro.attitude.z, -Input.gyro.attitude.w);
                Camera.main.transform.Rotate(90, 0, 0);
                Camera.main.transform.rotation = Quaternion.Euler(new Vector3(-Camera.main.transform.rotation.eulerAngles.x, Camera.main.transform.rotation.eulerAngles.y, 0f));
            }

            yield return null;
        }
    }

    public IEnumerator PlayerVRUpdate()
    {
        lastYAngle = 0.0f;
        yield return ChangeToVr();

        while (currentState == AppState.VIDEO_VR)
        {

            //Change to world position of the ray so that I can get the position of the player. Update the player so that in the portrait mode, the player can be clicked. It can get tricky.
            if (Input.GetMouseButtonDown(0))
            {
                //Debug.Log("Button clicked");
                if (vrMenuController.GetMenuState() == MenuState.HIDE)
                {
                    menuPivot.transform.localRotation = new Quaternion(menuPivot.transform.localRotation.x, cameraTrans.rotation.y, menuPivot.transform.localRotation.z, menuPivot.transform.localRotation.w);
                    vrMenuController.SetState(MenuState.SHOW);
                }

            }
            float deltaY = Mathf.DeltaAngle(menuPivot.rotation.eulerAngles.y, cameraTrans.rotation.eulerAngles.y);

            //Debug.Log(string.Format("The delta angle between the player and the menu is: {0}", deltaY));
            if (!vrMenuController.IsRotating && Mathf.Abs(deltaY) > vrMenuController.minRotationAngle)
            {
                yield return new WaitForSeconds(0.2f);
                //Need to check the rotation of the camera, something is going wrong 
                //Debug.Log(string.Format("The delta of the rotation of the camera and the video player pivot is: {0}", Mathf.Abs(Mathf.DeltaAngle(lastYAngle, cameraTrans.rotation.eulerAngles.y))));
                if (Mathf.Abs(lastYAngle - cameraTrans.rotation.eulerAngles.y) <= 0.001f)
                {
                    //Rotate the menu options to the camera rotation. TODO:Add the contribution of the player speed to catch up with him.
                    vrMenuController.RotateAdd(new Vector3(0.0f, deltaY), menuPivot.gameObject);
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

    private IEnumerator ChangeToVr()
    {
        if (!XRSettings.enabled)
        {

            XRSettings.LoadDeviceByName(deviceName);
            yield return null;

            XRSettings.enabled = true;

        }
    }

    private IEnumerator ChangeToPortrait()
    {
        if (XRSettings.enabled)
        {

            XRSettings.LoadDeviceByName("");
            yield return null;

            Camera.main.ResetAspect();

        }
    }



}

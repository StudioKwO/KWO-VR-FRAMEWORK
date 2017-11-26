
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

/*TODO: Make the input from cardboard be registered on the player to make the play, stop functionality
* - Finish the usability of the player
*/

/**
* <summary>
* This class will be responsible for handling the video player. 
* It controls the play/stop functionality, fade in, fade out, seek controls and volume controls
* </summary>
*/
public class VideoController : MonoBehaviour
{

    /// <summary>
    /// 360 video sphere
    /// </summary>
    public GameObject videoSphere;
    public float secondsToSkip = 3.0f;

    public delegate void VideoEnd();
    public event VideoEnd OnVideoVrEnd;
    public event VideoEnd OnVideoPortraitEnd;

    public MenuType type;

    /// <summary>
    /// Video player controller
    /// </summary>
    private VideoPlayer videoPlayer;
    public VideoPlayer VideoPlayer { get { return videoPlayer; } }

    private PlayerController playerController;

    private GameObject pauseSprite;
    private GameObject playSprite;

    private Slider videoScrubber;
    private Slider volumeSlider;
    private GameObject bufferedBackground;
    private GameObject volumeWidget;
    private Vector3 basePosition;
    private Text videoPosition;
    private Text videoDuration;

    public bool IsWidgetOpen { get { return volumeWidget.activeSelf; } }

    private void Awake()
    {
        foreach (Text t in GetComponentsInChildren<Text>())
        {
            if (t.gameObject.name == "curpos_text")
            {
                videoPosition = t;
            }
            else if (t.gameObject.name == "duration_text")
            {
                videoDuration = t;
            }
        }

        foreach (RawImage raw in GetComponentsInChildren<RawImage>(true))
        {
            if (raw.gameObject.name == "playImage")
            {
                playSprite = raw.gameObject;
            }
            else if (raw.gameObject.name == "pauseImage")
            {
                pauseSprite = raw.gameObject;
            }
        }

        foreach (Slider s in GetComponentsInChildren<Slider>(true))
        {
            if (s.gameObject.name == "video_slider")
            {
                videoScrubber = s;
                videoScrubber.maxValue = 100;
                videoScrubber.minValue = 0;
                foreach (Image i in videoScrubber.GetComponentsInChildren<Image>())
                {
                    if (i.gameObject.name == "BufferedBackground")
                    {
                        bufferedBackground = i.gameObject;
                    }
                }
            }
            else if (s.gameObject.name == "volume_slider")
            {
                volumeSlider = s;
            }
        }

        foreach (RectTransform obj in GetComponentsInChildren<RectTransform>(true))
        {
            if (obj.gameObject.name == "volume_widget")
            {
                volumeWidget = obj.gameObject;
            }
        }
    }

    private void OnEnable()
    {
        PlayerController.OnPlayerStateUpdated += OnPlayerStateUpdated;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerStateUpdated -= OnPlayerStateUpdated;
    }

    private void OnPlayerStateUpdated(AppState newState)
    {

        switch (newState)
        {
            case AppState.VIDEO_VR:
                if (!videoPlayer.isPlaying && type == MenuType.VR)
                {
                    videoPlayer.Play();
                    StartCoroutine(UpdateVideoVR());
                }
                break;
            case AppState.VIDEO_PORTRAIT:
                if (!videoPlayer.isPlaying && type == MenuType.PORTRAIT)
                {
                    videoPlayer.Play();
                    StartCoroutine(UpdateVideoPortrait());
                }
                break;
            case AppState.MENU_VR:
                //Do nothing, the video player should not be interactive on a menu
                break;
            default:
                break;
        }
    }

    // Use this for initialization
    void Start()
    {
        videoPlayer = videoSphere.GetComponent<VideoPlayer>();
        playerController = GetComponentInParent<PlayerController>();
        foreach (ScrubberEvents s in GetComponentsInChildren<ScrubberEvents>(true))
        {
            s.VideoManager = this;
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator UpdateVideoPortrait()
    {
        while (playerController.State == AppState.VIDEO_PORTRAIT)
        {
            if ((!videoPlayer.isPrepared || !videoPlayer.isPlaying))
            {
                pauseSprite.SetActive(false);
                playSprite.SetActive(true);
            }
            else if (videoPlayer.isPrepared && videoPlayer.isPlaying)
            {
                pauseSprite.SetActive(true);
                playSprite.SetActive(false);
            }

            if (videoPlayer.isPrepared)
            {
                if (basePosition == Vector3.zero)
                {
                    basePosition = videoScrubber.handleRect.localPosition;
                }
                videoScrubber.maxValue = videoPlayer.frameCount;
                videoScrubber.value = videoPlayer.frame;

                videoPosition.text = FormatTime(videoPlayer.frame / videoPlayer.frameRate);
                videoDuration.text = FormatTime(videoPlayer.frameCount / videoPlayer.frameRate);

                //Need to change so that the portrait menu is the one called, and not the regular VR menu
                if (videoScrubber.value == videoScrubber.maxValue)
                {
                    if (OnVideoPortraitEnd != null)
                    {
                        OnVideoPortraitEnd();
                        playerController.UpdateState(AppState.EXIT_MENU_PORTRAIT);
                    }
                }


            }
            else
            {
                videoScrubber.value = 0;
            }




            yield return null;
        }
    }

    public IEnumerator UpdateVideoVR()
    {
        while (playerController.State == AppState.VIDEO_VR)
        {
            if ((!videoPlayer.isPrepared || !videoPlayer.isPlaying))
            {
                pauseSprite.SetActive(false);
                playSprite.SetActive(true);
            }
            else if (videoPlayer.isPrepared && videoPlayer.isPlaying)
            {
                pauseSprite.SetActive(true);
                playSprite.SetActive(false);
            }

            if (videoPlayer.isPrepared)
            {
                if (basePosition == Vector3.zero)
                {
                    basePosition = videoScrubber.handleRect.localPosition;
                }
                videoScrubber.maxValue = videoPlayer.frameCount;
                videoScrubber.value = videoPlayer.frame;

                videoPosition.text = FormatTime(videoPlayer.frame / videoPlayer.frameRate);
                videoDuration.text = FormatTime(videoPlayer.frameCount / videoPlayer.frameRate);

                if (videoScrubber.value == videoScrubber.maxValue)
                {
                    if (OnVideoVrEnd != null)
                    {
                        OnVideoVrEnd();
                        playerController.UpdateState(AppState.EXIT_MENU_VR);
                    }
                }


            }
            else
            {
                videoScrubber.value = 0;
            }
            yield return null;
        }
    }

    public void OnSkipForwards()
    {
        ulong newFrame = (ulong)(videoPlayer.frame + (long)(secondsToSkip * videoPlayer.frameRate));

        if (newFrame <= videoPlayer.frameCount)
        {
            videoPlayer.frame = (long)newFrame;
        }
        else
        {
            videoPlayer.frame = (long)videoPlayer.frameCount;
        }

    }

    public void OnSkipBackwards()
    {
        ulong newFrame = (ulong)(videoPlayer.frame - (long)(secondsToSkip * videoPlayer.frameRate));

        if (newFrame <= 0)
        {
            videoPlayer.frame = 0;
        }
        else
        {
            videoPlayer.frame = (long)newFrame;
        }
    }

    public void OnVideoPositionChange(float val)
    {
        long newFrame = (long)(val * videoPlayer.frameCount);

        if (videoPlayer.isPrepared)
        {
            videoPlayer.frame = newFrame;
        }
    }

    public void OnVolumeUp()
    {
        float volume = videoPlayer.GetTargetAudioSource(0).volume;
        if (volume < 1.0f)
        {
            volume += 0.2f;
            videoPlayer.GetTargetAudioSource(0).volume = volume;
        }
        else
        {
            videoPlayer.GetTargetAudioSource(0).volume = 1.0f;
        }
    }

    public void OnVolumeDown()
    {
        float volume = videoPlayer.GetTargetAudioSource(0).volume;
        if (volume > 0.0f)
        {
            volume -= 0.2f;
            videoPlayer.GetTargetAudioSource(0).volume = volume;
        }
        else
        {
            videoPlayer.GetTargetAudioSource(0).volume = 0.0f;
        }
    }

    public void OnToggleVolume()
    {
        bool visible = !volumeWidget.activeSelf;
        volumeWidget.SetActive(visible);
    }

    public void OnPlayPause()
    {
        bool isPaused = !videoPlayer.isPlaying;
        if (isPaused)
        {
            videoPlayer.Play();
        }
        else
        {
            videoPlayer.Pause();
        }
        pauseSprite.SetActive(isPaused);
        playSprite.SetActive(!isPaused);
        CloseSubPanels();
    }

    public void OnVolumePositionChanged(float val)
    {
        if (videoPlayer.isPrepared)
        {
            Debug.Log("Setting current volume to " + val);
            videoPlayer.GetTargetAudioSource(0).volume = val;
        }
    }

    public void CloseSubPanels()
    {
        volumeWidget.SetActive(false);
    }

    private string FormatTime(float s)
    {
        int sec = (int)s;
        int mn = sec / 60;
        sec = sec % 60;
        mn = mn % 60;

        return string.Format("{0:00}:{1:00}", mn, sec);
    }
}

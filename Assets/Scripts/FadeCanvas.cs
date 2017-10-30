using System.Collections;
using UnityEngine;
using CurvedUI;

public class FadeCanvas : MonoBehaviour
{

    private CanvasGroup canvasGroup;

    public bool startFaded = true;
    private bool isFaded = true;
    public float timeToFade = 0.35f;
    public iTween.EaseType easeType = iTween.EaseType.easeInCubic;

    // Use this for initialization
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (startFaded && canvasGroup)
        {

            canvasGroup.alpha = 0.0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

        }
        else
        {
            canvasGroup.alpha = 1.0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        isFaded = startFaded;

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnFadeIn(string name = "", iTween.LoopType loopType = iTween.LoopType.none)
    {

        Hashtable table = new Hashtable();
        isFaded = false;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        table.Add("from", canvasGroup.alpha);
        table.Add("to", 1.0f);
        table.Add("time", timeToFade);
        table.Add("name", name);
        table.Add("looptype", loopType);
        table.Add("onupdatetarget", gameObject);
        table.Add("onupdate", "OnFadeCanvasCallback");
        table.Add("easetype", easeType);

        iTween.ValueTo(gameObject, table);

    }

    public void OnFadeOut(string name = "", iTween.LoopType loopType = iTween.LoopType.none)
    {
        Hashtable table = new Hashtable();
        isFaded = true;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        table.Add("from", canvasGroup.alpha);
        table.Add("to", 0.0f);
        table.Add("time", timeToFade);
        table.Add("name", name);
        table.Add("onupdatetarget", gameObject);
        table.Add("onupdate", "OnFadeCanvasCallback");
        table.Add("easetype", easeType);

        iTween.ValueTo(gameObject, table);

    }

    public void ToggleFade()
    {
        if (isFaded)
        {
            OnFadeIn();
        }
        else
        {
            OnFadeOut();
        }
    }

    void OnFadeCanvasCallback(float newValue)
    {
        canvasGroup.alpha = newValue;
    }


}

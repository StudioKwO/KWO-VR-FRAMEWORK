using System.Collections;
using UnityEngine;

public class FadeCanvas : MonoBehaviour
{

    private CanvasGroup canvasGroup;

    public bool startFaded = true;
    private bool isFaded = true;
    public bool IsFaded {  get { return isFaded; } }
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
       
        table.Add("from", canvasGroup.alpha);
        table.Add("to", 1.0f);
        table.Add("time", timeToFade);
        table.Add("name", name);
        table.Add("looptype", loopType);
        table.Add("onupdatetarget", this.gameObject);
        table.Add("oncomplete", "OnFadeInComplete");
        table.Add("onupdate", "OnFadeCanvasCallback");
        table.Add("easetype", easeType);

        iTween.ValueTo(gameObject, table);

    }

    public void OnFadeOut(string name = "", iTween.LoopType loopType = iTween.LoopType.none)
    {
        Hashtable table = new Hashtable();

        table.Add("from", canvasGroup.alpha);
        table.Add("to", 0.0f);
        table.Add("time", timeToFade);
        table.Add("name", name);
        table.Add("looptype", loopType);
        table.Add("onupdatetarget", this.gameObject);
        table.Add("oncomplete", "OnFadeOutComplete");
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

    void OnFadeInComplete()
    {
        isFaded = false;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    void OnFadeOutComplete()
    {
        isFaded = true;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }


}

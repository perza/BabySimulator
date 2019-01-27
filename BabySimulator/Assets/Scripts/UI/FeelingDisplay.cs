using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Feeling {
    Angry,
    Sleepy,
    Hungry,
    Happy,
    Cry,
    Shit
}

public class FeelingDisplay : MonoBehaviour
{
    Transform objectToFollow;
    public float showtime = 2f;

    public Sprite happyImg;
    public Sprite cryImg;
    public Sprite hungryImg;
    public Sprite angryImg;
    public Sprite shitImg;
    public Sprite sleepImg;

    Vector3 offset = new Vector3(-10, 20, 0);
    RectTransform imageTransform;
    RectTransform canvasTransform;
    Image imageDisplay;

    private Dictionary<Feeling, Sprite> feelingDict = new Dictionary<Feeling, Sprite>();

    // RectTransform moodDisplay;
    // Start is called before the first frame update
    void Start()
    {
        objectToFollow = gameObject.GetComponentInParent<Transform>();
        imageDisplay = GetComponentInChildren<Canvas>()
            .GetComponentInChildren<Image>();

        canvasTransform = GetComponentInChildren<Canvas>()
            .GetComponent<RectTransform>();
        imageTransform = GetComponentInChildren<Canvas>()
            .GetComponentInChildren<Image>()
            .rectTransform;

        feelingDict.Add(Feeling.Happy, happyImg);
        feelingDict.Add(Feeling.Angry, angryImg);
        feelingDict.Add(Feeling.Hungry, hungryImg);
        feelingDict.Add(Feeling.Cry, cryImg);
        feelingDict.Add(Feeling.Sleepy, sleepImg);
        feelingDict.Add(Feeling.Shit, shitImg);

        imageDisplay.enabled = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 followPoint = Camera.main.WorldToScreenPoint(objectToFollow.transform.position);
        followPoint += offset;
        imageTransform.position = followPoint;
    }

    public void DisplayMood(Feeling feeling) {

        if (!imageDisplay.enabled)
        {
            imageDisplay.enabled = true;
            imageDisplay.sprite = feelingDict[feeling];
            StartCoroutine(ShowMood());
        }
    }

    IEnumerator ShowMood() {
        float endTime = Time.time + showtime;
        while (Time.time < endTime) {
            yield return null;
        }
        imageDisplay.enabled = false;
    }

}

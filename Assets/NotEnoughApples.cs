using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NotEnoughApplesUI : MonoBehaviour
{
    public static NotEnoughApplesUI instance;

    [SerializeField] private GameObject messagePanel;
    [SerializeField] private Text messageText;
    [SerializeField] private float displayTime = 2f;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowMessage(string msg)
    {
        StopAllCoroutines();
        messagePanel.SetActive(true);
        messageText.text = msg;
        StartCoroutine(HideAfterTime());
    }

    private IEnumerator HideAfterTime()
    {
        yield return new WaitForSeconds(displayTime);
        messagePanel.SetActive(false);
    }
}

using TMPro;
using UnityEngine;

public class SimpleFeedback : MonoBehaviour
{
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private float destroyDelay = 1.5f;

    public void Show(string message, Color color)
    {
        feedbackText.text = message;
        feedbackText.color = color;
        Destroy(gameObject, destroyDelay);
    }
}

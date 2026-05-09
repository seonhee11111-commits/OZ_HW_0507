using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CriticalHitController : MonoBehaviour
{
    public static CriticalHitController Instance;
    [SerializeField] private Image flashImage;
    private Coroutine currentRoutine;

    private float lastHitTime;
    private const float comboTime = 1.0f;

    private void Awake() => Instance = this;

    public void StartFlash(float duration, float maxAlpha)
    {
        float currentTime = Time.time;

        if (Time.time - lastHitTime <= comboTime)
        {
            Debug.Log("Time.time - lastHitTime <= comboTime");
            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
            }
            currentRoutine = StartCoroutine(CHRoutine(duration, maxAlpha));
        }

        lastHitTime = currentTime;
    }

    private IEnumerator CHRoutine(float CHduration, float maxAlpha)
    {
        float elapsed = 0f;
        Color color = flashImage.color;
        while (elapsed < CHduration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(maxAlpha, 0f, elapsed / CHduration);
            flashImage.color = new Color(flashImage.color.r, flashImage.color.g, flashImage.color.b, alpha);
            yield return null;
        }

        flashImage.color = new Color(color.r, color.g, color.b, 0f);
        
    }



}

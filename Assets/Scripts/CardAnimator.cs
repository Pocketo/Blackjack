using UnityEngine;
using System.Collections;

/// <summary>
/// Añade animaciones suaves a las cartas
/// Attach a cada carta o úsalo desde otro script
/// </summary>
public class CardAnimator : MonoBehaviour
{
    [Header("Deal Animation")]
    [SerializeField] private float dealDuration = 0.3f;
    [SerializeField] private AnimationCurve dealCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Vector3 dealStartOffset = new Vector3(0, 3f, 0);

    [Header("Flip Animation")]
    [SerializeField] private float flipDuration = 0.2f;

    [Header("Hover Effect")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float hoverDuration = 0.1f;

    private Vector3 originalScale;
    private Vector3 targetPosition;
    private bool isAnimating = false;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    /// <summary>
    /// Anima la carta entrando desde arriba
    /// </summary>
    public void AnimateDeal(Vector3 finalPosition)
    {
        targetPosition = finalPosition;
        StartCoroutine(DealAnimation());
    }

    private IEnumerator DealAnimation()
    {
        isAnimating = true;
        
        Vector3 startPos = targetPosition + dealStartOffset;
        transform.position = startPos;
        
        float elapsed = 0f;
        
        while (elapsed < dealDuration)
        {
            elapsed += Time.deltaTime;
            float t = dealCurve.Evaluate(elapsed / dealDuration);
            transform.position = Vector3.Lerp(startPos, targetPosition, t);
            yield return null;
        }
        
        transform.position = targetPosition;
        isAnimating = false;
    }

    /// <summary>
    /// Anima el volteo de la carta
    /// </summary>
    public IEnumerator AnimateFlip(System.Action onFlipMidpoint = null)
    {
        isAnimating = true;
        
        float halfDuration = flipDuration / 2f;
        
        // Primera mitad: rotar a 90 grados
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float rotY = Mathf.Lerp(0, 90, t);
            transform.rotation = Quaternion.Euler(0, rotY, 0);
            yield return null;
        }
        
        // Punto medio: cambiar el sprite
        onFlipMidpoint?.Invoke();
        
        // Segunda mitad: rotar de 90 a 0
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float rotY = Mathf.Lerp(90, 0, t);
            transform.rotation = Quaternion.Euler(0, rotY, 0);
            yield return null;
        }
        
        transform.rotation = Quaternion.identity;
        isAnimating = false;
    }

    /// <summary>
    /// Efecto de hover al pasar el mouse
    /// </summary>
    private void OnMouseEnter()
    {
        if (!isAnimating)
        {
            StopAllCoroutines();
            StartCoroutine(ScaleAnimation(originalScale * hoverScale));
        }
    }

    private void OnMouseExit()
    {
        if (!isAnimating)
        {
            StopAllCoroutines();
            StartCoroutine(ScaleAnimation(originalScale));
        }
    }

    private IEnumerator ScaleAnimation(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        
        while (elapsed < hoverDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / hoverDuration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        
        transform.localScale = targetScale;
    }

    /// <summary>
    /// Shake animation para indicar bust
    /// </summary>
    public IEnumerator ShakeAnimation(float intensity = 0.1f, float duration = 0.3f)
    {
        Vector3 originalPos = transform.position;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = Random.Range(-intensity, intensity);
            float y = Random.Range(-intensity, intensity);
            transform.position = originalPos + new Vector3(x, y, 0);
            yield return null;
        }
        
        transform.position = originalPos;
    }

    /// <summary>
    /// Punch scale para celebrar blackjack
    /// </summary>
    public IEnumerator PunchScale(float scale = 1.3f, float duration = 0.3f)
    {
        Vector3 originalScaleLocal = originalScale;
        
        // Expandir
        float halfDuration = duration / 2f;
        float elapsed = 0f;
        
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            transform.localScale = Vector3.Lerp(originalScaleLocal, originalScaleLocal * scale, t);
            yield return null;
        }
        
        // Contraer
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            transform.localScale = Vector3.Lerp(originalScaleLocal * scale, originalScaleLocal, t);
            yield return null;
        }
        
        transform.localScale = originalScaleLocal;
    }
}

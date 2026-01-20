using UnityEngine;
using TMPro;

/// <summary>
/// Script helper para crear el popup de daño desde código
/// Útil si no quieres crear el prefab manualmente
/// </summary>
public class DamagePopup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private float floatSpeed = 50f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private AnimationCurve scaleCurve;

    private TextMeshProUGUI textComponent;
    private float elapsedTime = 0f;
    private Vector3 initialScale;
    private Color initialColor;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
            textComponent = GetComponentInChildren<TextMeshProUGUI>();

        if (scaleCurve == null || scaleCurve.length == 0)
        {
            scaleCurve = new AnimationCurve(
                new Keyframe(0, 0.5f),
                new Keyframe(0.1f, 1.2f),
                new Keyframe(0.2f, 1f),
                new Keyframe(1f, 1f)
            );
        }

        initialScale = transform.localScale;
        if (textComponent != null)
            initialColor = textComponent.color;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        float normalizedTime = elapsedTime / lifetime;

        // Mover hacia arriba
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // Escalar
        float scale = scaleCurve.Evaluate(normalizedTime);
        transform.localScale = initialScale * scale;

        // Fade
        if (textComponent != null)
        {
            Color c = initialColor;
            c.a = fadeCurve.Evaluate(normalizedTime);
            textComponent.color = c;
        }

        // Destruir cuando termine
        if (elapsedTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Configura el popup con el daño y si es crítico
    /// </summary>
    public void Setup(int damage, bool isCritical)
    {
        if (textComponent != null)
        {
            textComponent.text = isCritical ? $"-{damage}!" : $"-{damage}";
            textComponent.fontSize = isCritical ? 48 : 36;
            
            initialColor = isCritical 
                ? new Color(1f, 0.8f, 0f) // Dorado para crítico
                : new Color(1f, 0.3f, 0.3f); // Rojo para normal
            
            textComponent.color = initialColor;
        }
    }

    /// <summary>
    /// Crea un popup de daño en la posición indicada
    /// </summary>
    public static DamagePopup Create(Vector3 position, int damage, bool isCritical, Transform parent = null)
    {
        // Crear GameObject
        GameObject popupObj = new GameObject("DamagePopup");
        popupObj.transform.position = position;
        
        if (parent != null)
            popupObj.transform.SetParent(parent);

        // Añadir Canvas para UI en world space (opcional, depende de tu setup)
        // Si usas UI canvas, el popup debería ser hijo del canvas

        // Añadir TextMeshPro
        TextMeshProUGUI text = popupObj.AddComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;

        // Añadir este componente
        DamagePopup popup = popupObj.AddComponent<DamagePopup>();
        popup.Setup(damage, isCritical);

        return popup;
    }
}

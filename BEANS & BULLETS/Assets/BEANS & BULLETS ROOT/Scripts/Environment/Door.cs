using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [Header("DOOR")]
    [SerializeField] private Transform doorPanel;
    [SerializeField] private float openHeight = 4f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stayOpenTime = 3f;

    [Header("COLORS")]
    [SerializeField] private MeshRenderer doorRenderer;
    [SerializeField] private float emissionIntensity = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip lockedSound;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen;
    private bool isMoving;
    private bool isLocked = true;
    private float openTimer;
    private AudioSource audioSource;
    private Material doorMaterial;

    private void Start()
    {
        closedPosition = doorPanel.localPosition;
        openPosition = closedPosition + Vector3.up * openHeight;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;

        if (doorRenderer != null)
        {
            doorMaterial = doorRenderer.material;
        }

        ApplyColor();
    }

    private void Update()
    {
        // Mover la puerta
        if (isMoving)
        {
            Vector3 target = isOpen ? openPosition : closedPosition;

            doorPanel.localPosition = Vector3.MoveTowards(
                doorPanel.localPosition,
                target,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(doorPanel.localPosition, target) < 0.01f)
            {
                doorPanel.localPosition = target;
                isMoving = false;
            }
        }

        // Timer para cerrar automáticamente
        if (isOpen && stayOpenTime > 0f)
        {
            openTimer -= Time.deltaTime;

            if (openTimer <= 0f)
            {
                isOpen = false;
                isMoving = true;
                PlaySound(closeSound);
            }
        }
    }

    // Player pulsa E
    public void Interact()
    {
        if (isMoving) return;

        if (isLocked)
        {
            PlaySound(lockedSound);
            return;
        }

        if (!isOpen)
        {
            Open();
        }
    }

    // Abrir
    private void Open()
    {
        isOpen = true;
        isMoving = true;
        openTimer = stayOpenTime;
        PlaySound(openSound);
    }

    #region CONTROL EXTERNO

    public void Lock()
    {
        isLocked = true;
        ApplyColor();

        // Si está abierta, cerrarla
        if (isOpen)
        {
            isOpen = false;
            isMoving = true;
            PlaySound(closeSound);
        }
    }

    public void Unlock()
    {
        isLocked = false;
        ApplyColor();
    }

    public bool IsLocked()
    {
        return isLocked;
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    #endregion

    #region COLOR

    private void ApplyColor()
    {
        if (doorMaterial == null) return;

        Color color = isLocked ? Color.red : Color.green;
        doorMaterial.color = color;
        doorMaterial.SetColor("_EmissionColor", color * emissionIntensity);
        doorMaterial.EnableKeyword("_EMISSION");
    }

    #endregion

    #region AUDIO

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    #endregion

    #region DEBUG

    private void OnDrawGizmosSelected()
    {
        if (doorPanel == null) return;

        Gizmos.color = isLocked ? Color.red : Color.green;
        Vector3 openPos = doorPanel.position + Vector3.up * openHeight;
        Gizmos.DrawWireCube(openPos, doorPanel.lossyScale);
        Gizmos.DrawLine(doorPanel.position, openPos);
    }

    #endregion
}
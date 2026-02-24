using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [Header("DOOR")]
    [SerializeField] private Transform doorPanel;
    [SerializeField] private float openHeight = 4f;
    [SerializeField] private float moveSpeed = 3f;

    [Header("COLORS")]
    [SerializeField] private MeshRenderer doorRenderer;
    [SerializeField] private float emissionIntensity = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip lockedSound;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen;
    private bool isMoving;
    private bool isLocked = true;
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
        if (!isMoving) return;

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

    public void Interact()
    {
        if (isMoving) return;

        if (isLocked)
        {
            if (lockedSound != null) audioSource.PlayOneShot(lockedSound);
            return;
        }

        isOpen = !isOpen;
        isMoving = true;

        if (openSound != null) audioSource.PlayOneShot(openSound);
    }

    public void Lock()
    {
        isLocked = true;

        // Cerrar si está abierta
        if (isOpen)
        {
            isOpen = false;
            isMoving = true;
        }

        ApplyColor();
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

    private void ApplyColor()
    {
        if (doorMaterial == null) return;

        Color color = isLocked ? Color.red : Color.green;
        doorMaterial.color = color;
        doorMaterial.SetColor("_EmissionColor", color * emissionIntensity);
        doorMaterial.EnableKeyword("_EMISSION");
    }
}
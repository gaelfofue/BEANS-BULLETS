using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [Header("=== DOOR ===")]
    [SerializeField] private Transform doorPanel;
    [SerializeField] private float openHeight = 4f;
    [SerializeField] private float moveSpeed = 3f;

    [Header("=== COLORS ===")]
    [SerializeField] private MeshRenderer doorRenderer;
    [SerializeField] private float emissionIntensity = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip lockedSound;

    public enum DoorState { Open, Closed, Locked }

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isMoving;
    private DoorState currentState = DoorState.Locked;
    private AudioSource audioSource;
    private Material doorMaterial;

    private void Start()
    {
        closedPosition = doorPanel.localPosition;
        openPosition = closedPosition + Vector3.up * openHeight;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;

        if (doorRenderer != null)
            doorMaterial = doorRenderer.material;

        ApplyVisuals();
    }

    private void Update()
    {
        if (!isMoving) return;

        bool shouldBeUp = (currentState == DoorState.Open);
        Vector3 target = shouldBeUp ? openPosition : closedPosition;

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

    // Player pulsa E
    public void Interact()
    {
        if (isMoving) return;

        if (currentState == DoorState.Locked)
        {
            PlaySound(lockedSound);
            return;
        }

        if (currentState == DoorState.Closed)
        {
            SetState(DoorState.Open);
        }
    }

    public void SetState(DoorState newState)
    {
        DoorState oldState = currentState;
        currentState = newState;

        Debug.Log($"DOOR {gameObject.name}: {oldState} → {newState}");

        if (newState == DoorState.Open && oldState != DoorState.Open)
        {
            isMoving = true;
            PlaySound(openSound);
        }
        else if (newState != DoorState.Open && oldState == DoorState.Open)
        {
            isMoving = true;
            PlaySound(closeSound);
        }

        ApplyVisuals();
    }

    public DoorState GetState() { return currentState; }

    private void ApplyVisuals()
    {
        if (doorMaterial == null) return;

        Color color;
        switch (currentState)
        {
            case DoorState.Locked:
                color = Color.red;
                break;
            case DoorState.Closed:
                color = Color.green;
                break;
            case DoorState.Open:
                color = Color.green;
                break;
            default:
                color = Color.red;
                break;
        }

        doorMaterial.color = color;
        doorMaterial.SetColor("_EmissionColor", color * emissionIntensity);
        doorMaterial.EnableKeyword("_EMISSION");
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}
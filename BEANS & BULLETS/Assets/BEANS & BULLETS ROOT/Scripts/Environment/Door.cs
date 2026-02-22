using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [Header("DOOR")]
    [SerializeField] private Transform doorPanel;
    [SerializeField] private float openHeight = 4f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stayOpenTime = 3f;

    [Header("Audio (opcional)")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen;
    private bool isMoving;
    private float openTimer;
    private AudioSource audioSource;

    private void Start()
    {
        closedPosition = doorPanel.localPosition;
        openPosition = closedPosition + Vector3.up * openHeight;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.maxDistance = 15f;
    }

    private void Update()
    {
        HandleTimer();
        MoveDoor();
    }

    // Llamado por PlayerInteract cuando el raycast impacta esta puerta
    public void Interact()
    {
        if (isMoving) return;

        if (!isOpen)
        {
            isOpen = true;
            isMoving = true;
            openTimer = stayOpenTime;
            PlaySound(openSound);
        }
        else
        {
            isOpen = false;
            isMoving = true;
            PlaySound(closeSound);
        }
    }

    private void HandleTimer()
    {
        if (!isOpen) return;
        if (stayOpenTime <= 0f) return;

        openTimer -= Time.deltaTime;

        if (openTimer <= 0f)
        {
            isOpen = false;
            isMoving = true;
            PlaySound(closeSound);
        }
    }

    private void MoveDoor()
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

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (doorPanel == null) return;

        Gizmos.color = Color.green;
        Vector3 openPos = doorPanel.position + Vector3.up * openHeight;
        Gizmos.DrawWireCube(openPos, doorPanel.lossyScale);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(doorPanel.position, openPos);
    }
}
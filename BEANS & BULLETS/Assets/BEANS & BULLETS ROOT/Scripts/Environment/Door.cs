// Door.cs
// Adjuntar al GameObject padre de cada puerta

using UnityEngine;
using UnityEngine.InputSystem;

public class Door : MonoBehaviour, IInteractable
{
    [Header("DOOR")]
    [SerializeField] private Transform doorPanel;
    [SerializeField] private float openHeight = 4f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stayOpenTime = 3f;

    [Header("LOCK SYSTEM")]
    [SerializeField] private MeshRenderer doorRenderer;
    [SerializeField] private Color lockedColor = Color.red;
    [SerializeField] private Color unlockedColor = Color.green;
    [SerializeField] private float emissionIntensity = 2f;

    [Header("Audio (opcional)")]
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;
    [SerializeField] private AudioClip lockedSound;

    // Estado
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
        audioSource.maxDistance = 15f;

        // Crear instancia del material para no afectar a otras puertas
        if (doorRenderer != null)
        {
            doorMaterial = doorRenderer.material;
        }

        // Empieza bloqueada
        SetLocked(true);
    }

    private void Update()
    {
        HandleTimer();
        MoveDoor();
    }

    #region LOCK SYSTEM

    public void SetLocked(bool locked)
    {
        isLocked = locked;

        if (doorMaterial == null) return;

        Color color = locked ? lockedColor : unlockedColor;
        Color emission = color * emissionIntensity;

        doorMaterial.color = color;
        doorMaterial.SetColor("_EmissionColor", emission);
        doorMaterial.EnableKeyword("_EMISSION");

        // Si se bloquea mientras está abierta, cerrarla
        if (locked && isOpen)
        {
            isOpen = false;
            isMoving = true;
        }
    }

    public bool IsLocked()
    {
        return isLocked;
    }

    #endregion

    #region INTERACT

    public void Interact()
    {
        if (isMoving) return;

        if (isLocked)
        {
            PlaySound(lockedSound);
            Debug.Log("Puerta bloqueada");
            return;
        }

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

    #endregion

    #region MOVEMENT

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

    #endregion

    #region CONTROL EXTERNO

    public void ForceOpen()
    {
        if (isLocked) return;

        isOpen = true;
        isMoving = true;
        openTimer = stayOpenTime;
        PlaySound(openSound);
    }

    public void ForceClose()
    {
        isOpen = false;
        isMoving = true;
        PlaySound(closeSound);
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
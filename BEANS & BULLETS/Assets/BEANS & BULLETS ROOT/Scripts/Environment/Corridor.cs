using UnityEngine;

public class Corridor : MonoBehaviour
{
    [Header("CONNECTION POINTS")]
    [SerializeField] private Transform entryPoint;
    [SerializeField] private Transform exitPoint;

    public Transform GetEntryPoint() { return entryPoint; }
    public Transform GetExitPoint() { return exitPoint; }
}
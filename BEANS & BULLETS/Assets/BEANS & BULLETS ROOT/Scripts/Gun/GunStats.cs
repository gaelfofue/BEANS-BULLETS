using UnityEngine;

[CreateAssetMenu(menuName = "Bean&Bullets/Gun Stats")]
public class GunStats : ScriptableObject
{
    [Header("Damage")]
    public float damage = 35f;
    public float headshotMultiplier = 2f;

    [Header("Fire")]
    public float fireRate = 0.35f;
    public int magSize = 6;
    public float reloadTime = 1f;
    public float range = 200f;
}
using UnityEngine;

[CreateAssetMenu(menuName = "Bean&Bullets/Fire Modes/Shotgun")]
public class FM_Shotgun : FireMode
{
    public int pelletCount = 8;
    public float spreadAngle = 0.05f;

    private void OnEnable()
    {
        modeName = "Shotgun";
    }

    public override bool CanFire(bool inputDown, bool inputHeld, float timeSinceLastShot, float fireRate)
    {
        // Semi-auto igual que el default, un click un disparo
        return inputDown && timeSinceLastShot >= fireRate;
    }

    // Lanza 8 rayos en vez de 1
    public override int GetRayCount() { return pelletCount; }

    // Cada rayo tiene dispersión
    public override float GetSpreadAngle() { return spreadAngle; }
}
using UnityEngine;

[CreateAssetMenu(menuName = "Bean&Bullets/Fire Modes/Full Auto")]
public class FM_FullAuto : FireMode
{
    private void OnEnable()
    {
        modeName = "Full Auto";
    }

    public override bool CanFire(bool inputDown, bool inputHeld, float timeSinceLastShot, float fireRate)
    {
        // Dispara mientras mantengas presionado
        return inputHeld && timeSinceLastShot >= fireRate;
    }
}
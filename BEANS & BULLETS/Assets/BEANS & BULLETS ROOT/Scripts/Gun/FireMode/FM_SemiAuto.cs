using UnityEngine;

[CreateAssetMenu(menuName = "Bean&Bullets/Fire Modes/Semi Auto")]
public class FM_SemiAuto : FireMode
{
    private void OnEnable()
    {
        modeName = "Semi Auto";
    }

    public override bool CanFire(bool inputDown, bool inputHeld, float timeSinceLastShot, float fireRate)
    {
        // Solo dispara en el frame que se pulsa, no manteniendo
        // Y solo si pasó suficiente tiempo desde el último disparo
        return inputDown && timeSinceLastShot >= fireRate;
    }
}
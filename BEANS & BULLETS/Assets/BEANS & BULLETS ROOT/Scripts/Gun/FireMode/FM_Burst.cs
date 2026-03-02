using UnityEngine;

[CreateAssetMenu(menuName = "Bean&Bullets/Fire Modes/Burst")]
public class FM_Burst : FireMode
{
    public int burstCount = 3;
    public float burstDelay = 0.08f;

    private void OnEnable()
    {
        modeName = "Burst";
    }

    public override bool CanFire(bool inputDown, bool inputHeld, float timeSinceLastShot, float fireRate)
    {
        return inputDown && timeSinceLastShot >= fireRate;
    }

    public override int GetRayCount() { return burstCount; }
}
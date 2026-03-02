using UnityEngine;

public abstract class FireMode : ScriptableObject
{
    public string modeName = "Default";
    public Sprite icon;

    public abstract bool CanFire(bool inputDown, bool inputHeld, float timeSinceLastShot, float fireRate);

    public virtual int GetRayCount() { return 1; }

    public virtual float GetSpreadAngle() { return 0f; }
}
using UnityEngine;

public abstract class FireMode : ScriptableObject
{
    public string modeName = "Defeault";
    public string icon;

    // ¿Puede disparar este frame?
    public abstract bool CanFire(bool inputDown, bool inputHeld, float timeSinceLastShot, float fireRate);

    // ¿Cuantos rayos lanza por disparo?
    public virtual int GetRayCount() { return 1; }

    // ¿Angulo de dispersion entre rayos?
    public virtual float GetSpreadAngle() { return 0; }
}

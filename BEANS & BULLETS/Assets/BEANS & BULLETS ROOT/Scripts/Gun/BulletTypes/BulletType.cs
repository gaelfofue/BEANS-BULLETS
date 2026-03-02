using UnityEngine;

public abstract class BulletType : ScriptableObject
{
    public string bulletName = "Standard";
    public Sprite icon;

    // ¿Qué pasa al impactar?
    public abstract void OnHit(RaycastHit hit, float damage, Vector3 shootDirection);

    // Modificador de daño
    public virtual float GetDamageMultiplier() { return 1f; }
}
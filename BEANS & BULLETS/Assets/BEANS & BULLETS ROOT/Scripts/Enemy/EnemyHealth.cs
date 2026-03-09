using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("HealthSystem Management")]
    [SerializeField] int maxHealth = 100; //Vida maxima del enemigo
    [SerializeField] int health; //Vida actual del enemigo

    [Header("Feedback Configuration")]
    [SerializeField] Material damagedMat; //Material feedback de daño
    [SerializeField] GameObject deathVFX; //Efecto de particulas de muerte
    [SerializeField] MeshRenderer enemyRend; //Ref al componente que dibuja los materiales del enemigo en pantalla
    Material baseMat; //Almacen del material base del enemigo

    private void Awake()
    {
        health = maxHealth; //La vida se pone en el maximo
        baseMat = enemyRend.material; //Se referencia el material base

    }

    void Update()
    {
        if (health <= 0)
        {
            health = 0; //La vida no puede bajar de 0
            deathVFX.SetActive(true);
            deathVFX.transform.position = transform.position;
            gameObject.SetActive(false); //El enemigo se apaga = "muere"
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage; //Quitarle una cantidad determinada de vida al enemigo
        enemyRend.material = damagedMat; //Se cambia al material de feedback de daño
        Invoke(nameof(ResetEnemyMaterial), 0.1f); //Espera de tiempo que permite ver el parpadeo
    }

    void ResetEnemyMaterial()
    {
        //Devuelve el material del enemigo a su material original
        enemyRend.material = baseMat;
    }
}

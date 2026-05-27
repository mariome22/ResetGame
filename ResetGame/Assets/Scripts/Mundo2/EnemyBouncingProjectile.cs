using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBouncingProjectile : MonoBehaviour
{
    public int damage = 1;
    public int maxBounces = 2;
    [Tooltip("Energía que conserva tras cada rebote. 1 = No pierde fuerza, 0.5 = Pierde la mitad de la altura.")]
    public float bounceRestitution = 0.5f;

    // Lista estática para llevar el registro de todas las granadas activas en la escena
    private static System.Collections.Generic.List<Collider2D> activeProjectiles = new System.Collections.Generic.List<Collider2D>();

    private Rigidbody2D rb;
    private Collider2D myCollider;
    private int bounceCount = 0;
    private Vector2 lastVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>();
        // Destrucción de seguridad por si se cae al vacío
        Destroy(gameObject, 5f);
    }

    private void Start()
    {
        if (myCollider != null)
        {
            // Ignorar colisiones con todas las granadas que ya estén activas en la escena
            foreach (Collider2D otherCollider in activeProjectiles)
            {
                if (otherCollider != null)
                {
                    Physics2D.IgnoreCollision(myCollider, otherCollider);
                }
            }
            // Añadirnos a la lista de proyectiles activos
            activeProjectiles.Add(myCollider);
        }
    }

    private void OnDestroy()
    {
        if (myCollider != null)
        {
            // Limpieza al destruirse para evitar fugas de memoria
            activeProjectiles.Remove(myCollider);
        }
    }

    public void Fire(Vector2 initialVelocity)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = initialVelocity;
    }

    private void FixedUpdate()
    {
        // Guardamos la velocidad justo ANTES de chocar contra cualquier cosa
        // para poder calcular un rebote perfecto sin usar PhysicsMaterial2D
        lastVelocity = rb.linearVelocity;

        // Girar el proyectil visualmente hacia donde se mueve (opcional pero queda bien)
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si choca con el jugador, le hace daño y desaparece
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerPlatformerController player = collision.gameObject.GetComponent<PlayerPlatformerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            Destroy(gameObject);
            return;
        }

        // Si choca con el entorno, cuenta como rebote
        bounceCount++;
        if (bounceCount > maxBounces)
        {
            Destroy(gameObject);
        }
        else
        {
            // Calculamos el rebote manualmente
            ContactPoint2D contact = collision.GetContact(0);
            Vector2 reflectDir = Vector2.Reflect(lastVelocity, contact.normal);

            // Reducimos la velocidad (pierde altura/fuerza)
            rb.linearVelocity = reflectDir * bounceRestitution;
        }
    }
}

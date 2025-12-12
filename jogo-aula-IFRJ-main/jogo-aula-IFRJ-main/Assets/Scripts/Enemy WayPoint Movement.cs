using UnityEngine;
using System.Collections.Generic;

public class EnemyWaypointMovement : MonoBehaviour
{
    // --- Waypoints and Movement Settings ---
    [Header("Waypoints")]
    public List<Transform> waypoints;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float waypointReachedDistance = 0.1f;
    public bool loop = true;

    [Header("Visual")]
    public Transform visual; // Elemento para aplicar o 'flip' visual.

    // --- Combat Settings ---
    [Header("Combat Settings")]
    public float damage = 10f;
    public float attackCooldown = 1f;
    public float knockbackForce = 15f;

    // --- Private Variables ---
    private Rigidbody2D rb;
    private int currentWaypointIndex = 0;
    private Vector2 movementDirection;
    private float lastAttackTime; // Para controlar o cooldown do ataque.

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (waypoints == null || waypoints.Count == 0)
        {
            Debug.LogError("No waypoints assigned to the enemy!");
            enabled = false;
            return;
        }

        SetTargetWaypoint(currentWaypointIndex);
        FlipVisual(); // Define o visual inicial
    }

    void FixedUpdate()
    {
        MoveTowardsWaypoint();
        CheckIfWaypointReached();
    }

    void SetTargetWaypoint(int index)
    {
        if (waypoints.Count == 0) return;

        currentWaypointIndex = index;
        Vector2 targetPosition = waypoints[currentWaypointIndex].position;
        movementDirection = (targetPosition - (Vector2)transform.position).normalized;
    }

    void MoveTowardsWaypoint()
    {
        if (waypoints.Count == 0) return;

        Vector2 targetPosition = waypoints[currentWaypointIndex].position;
        // Recalcula a direção continuamente
        movementDirection = (targetPosition - (Vector2)transform.position).normalized;

        // Aplica a velocidade APENAS no eixo X
        rb.linearVelocity = new Vector2(movementDirection.x * moveSpeed, rb.linearVelocity.y);
    }

    /// <summary>
    /// Vira o visual do inimigo com base na direção X do movimento.
    /// (Lógica Invertida, como solicitado)
    /// </summary>
    void FlipVisual()
    {
        if (visual == null) return;

        // Se movendo para a direita (X positivo)
        if (movementDirection.x > 0.01f) 
        {
            // Vira para a direita: Escala X negativa (inverte o sprite)
            visual.localScale = new Vector3(-Mathf.Abs(visual.localScale.x), visual.localScale.y, visual.localScale.z); 
        }
        // Se movendo para a esquerda (X negativo)
        else if (movementDirection.x < -0.01f)
        {
            // Vira para a esquerda: Escala X positiva (mantém o sprite)
            visual.localScale = new Vector3(Mathf.Abs(visual.localScale.x), visual.localScale.y, visual.localScale.z);
        }
    }

    void CheckIfWaypointReached()
    {
        if (waypoints.Count == 0) return;

        float distanceToWaypoint = Vector2.Distance(transform.position, waypoints[currentWaypointIndex].position);

        if (distanceToWaypoint <= waypointReachedDistance)
        {
            GoToNextWaypoint();
        }
    }

    void GoToNextWaypoint()
    {
        currentWaypointIndex++;

        if (currentWaypointIndex >= waypoints.Count)
        {
            if (loop)
            {
                currentWaypointIndex = 0;
            }
            else
            {
                enabled = false;
                rb.linearVelocity = Vector2.zero;
                return;
            }
        }

        SetTargetWaypoint(currentWaypointIndex);
        FlipVisual(); // Chama o flip para virar o inimigo na nova direção!
    }

    // --- Lógica de Combate ---

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TryAttackPlayer(collision.gameObject);
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            TryAttackPlayer(collision.gameObject);
        }
    }

    void TryAttackPlayer(GameObject player)
    {
        // Verifica o cooldown de ataque
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Calcula a direção do knockback (do inimigo para o jogador)
                Vector2 knockbackDirection = (player.transform.position - transform.position).normalized;
                
                // Aplica dano e knockback
                playerHealth.TakeDamage(damage, knockbackDirection, knockbackForce);
                
                // Reinicia o timer do cooldown
                lastAttackTime = Time.time;
            }
        }
    }
    
    // Função Jump
    public void Jump(float jumpForce)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }
}
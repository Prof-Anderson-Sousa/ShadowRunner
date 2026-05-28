using UnityEngine;

public class ThrownWeapon : MonoBehaviour
{
    [Header("Configurações do Arremesso")]
    public float speed = 15f;           // Velocidade do voo
    public float spinSpeed = -720f;     // Velocidade do giro (graus por segundo)
    public float lifetime = 2f;         // Tempo de vida antes de sumir

    private Rigidbody2D rb;

    void Start()
    {
        // Garante que a arma não fique voando infinitamente pela memória do jogo
        Destroy(gameObject, lifetime);
    }

    // Este método será chamado pelo jogador no exato momento do arremesso
    public void Initialize(float direction)
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Aplica a força para frente (direção será -1 ou 1)
        rb.linearVelocity = new Vector2(speed * direction, 0f);
        
        // Inverte o sentido do giro se o jogador atirar para trás
        spinSpeed = spinSpeed * direction;
    }

    void Update()
    {
        // Faz a imagem girar no próprio eixo a cada frame
        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Lógica de colisão futura:
        // if (collision.CompareTag("Enemy")) { causar dano... }
        
        // Descomente a linha abaixo se quiser que a arma seja destruída ao bater em algo
        // Destroy(gameObject);
    }
}
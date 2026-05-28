using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MorcegoPatrulhaController : MonoBehaviour
{
    [Header("Movimento Horizontal")]
    [SerializeField] private float velocidade = 2f;
    [SerializeField] private int direcaoInicial = -1;
    [SerializeField] private float distanciaPatrulhaHorizontal = 2.5f;
    [SerializeField] private float atrasoInicio = 2f;

    [Header("Altura")]
    [SerializeField] private bool alinharAlturaComJogador = true;
    [Tooltip("Altere este valor para subir/descer os morcegos. Negativo desce, positivo sobe.")]
    [SerializeField] private float offsetAlturaJogador = -0.45f;

    [Header("Deteccao de Parede")]
    [SerializeField] private Transform detectorParede;
    [SerializeField] private float distanciaDeteccaoParede = 0.25f;
    [SerializeField] private LayerMask camadaParedeOuTilemap;

    [Header("Pontuacao")]
    [SerializeField] private int pontosAoEncostarNoPlayer = 25;
    [SerializeField] private int pontosPerdidosAoBaterSemMatar = 40;
    [SerializeField] private int pontosBaseAoMatar = 100;
    [SerializeField] private int multiplicadorPontosAoMatar = 3;
    [SerializeField] private int essenciaAoMatar = 1;

    [Header("Empurrao no Player")]
    [SerializeField] private float forcaEmpurraoHorizontal = 7f;
    [SerializeField] private float forcaEmpurraoVertical = 4f;
    [SerializeField] private float cooldownDanoPlayer = 0.8f;

    [Header("Morte")]
    [SerializeField] private float tempoParaDestruir = 0.25f;

    [Header("Gerador de Morcegos no Tilemap")]
    [Tooltip("Marque somente no objeto SpawnerMorcego. Deixe desmarcado no prefab Morcego.")]
    [SerializeField] private bool usarComoGerador = false;
    [SerializeField] private bool gerarAoIniciar = true;
    [SerializeField] private bool limparGeradosAoIniciar = true;
    [SerializeField] private MorcegoPatrulhaController prefabMorcego;
    [SerializeField] private Tilemap cenarioTilemap;
    [SerializeField] private int quantidadeDeMorcegos = 8;
    [SerializeField] private float alturaAcimaDoChao = 2.2f;
    [SerializeField] private int margemEsquerdaEmTiles = 5;
    [SerializeField] private int margemDireitaEmTiles = 5;
    [SerializeField] private float variacaoAltura = 0.6f;
    [SerializeField] private string nomeContainerGerados = "Morcegos Gerados";

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    private int direcaoAtual;
    private float xInicial;
    private bool morto;
    private bool ativo;
    private bool alturaAlinhada;
    private float proximoDanoLiberado;
    private bool configuradoComoGerado;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        direcaoAtual = direcaoInicial >= 0 ? 1 : -1;
        xInicial = transform.position.x;

        if (!usarComoGerador)
        {
            GarantirDetectorParede();
            AtualizarVisual();
        }
    }

    private IEnumerator Start()
    {
        if (usarComoGerador)
        {
            if (gerarAoIniciar)
                GerarMorcegosAoLongoDoTilemap();

            yield break;
        }

        TentarAlinharAlturaComJogador();

        yield return new WaitUntil(() =>
            MenuController.Instance == null || MenuController.Instance.JogoIniciado);
        yield return new WaitForSeconds(atrasoInicio);
        ativo = true;
    }

    private void FixedUpdate()
    {
        if (usarComoGerador || morto || !ativo)
            return;

        if (!alturaAlinhada)
            TentarAlinharAlturaComJogador();

        VerificarLimiteDaPatrulha();
        Patrulhar();
        VerificarParede();
    }

    public void GerarMorcegosAoLongoDoTilemap()
    {
        if (!usarComoGerador)
            return;

        if (prefabMorcego == null)
        {
            Debug.LogWarning("[SpawnerMorcego] O campo Prefab Morcego nao foi preenchido.", this);
            return;
        }

        if (cenarioTilemap == null)
        {
            Debug.LogWarning("[SpawnerMorcego] O campo Cenario Tilemap nao foi preenchido.", this);
            return;
        }

        if (quantidadeDeMorcegos <= 0)
            return;

        Transform container = PrepararContainerGerados();
        BoundsInt bounds = cenarioTilemap.cellBounds;

        int inicioX = bounds.xMin + margemEsquerdaEmTiles;
        int fimX = bounds.xMax - margemDireitaEmTiles;

        if (fimX <= inicioX)
        {
            inicioX = bounds.xMin;
            fimX = bounds.xMax;
        }

        for (int i = 0; i < quantidadeDeMorcegos; i++)
        {
            float t = quantidadeDeMorcegos == 1 ? 0.5f : i / (float)(quantidadeDeMorcegos - 1);
            int cellX = Mathf.RoundToInt(Mathf.Lerp(inicioX, fimX, t));

            if (!TentarEncontrarPontoNoChao(cellX, bounds, out Vector3 posicaoBase))
                continue;

            float variacao = variacaoAltura > 0f ? Random.Range(-variacaoAltura, variacaoAltura) : 0f;
            Vector3 posicaoMorcego = posicaoBase + Vector3.up * (alturaAcimaDoChao + variacao);

            MorcegoPatrulhaController morcego = Instantiate(prefabMorcego, posicaoMorcego, Quaternion.identity, container);
            morcego.ConfigurarComoMorcegoGerado(i);
        }
    }

    private Transform PrepararContainerGerados()
    {
        Transform container = transform.Find(nomeContainerGerados);

        if (container == null)
        {
            GameObject novoContainer = new GameObject(nomeContainerGerados);
            novoContainer.transform.SetParent(transform);
            novoContainer.transform.localPosition = Vector3.zero;
            container = novoContainer.transform;
        }

        if (limparGeradosAoIniciar)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Transform filho = container.GetChild(i);

                if (Application.isPlaying)
                    Destroy(filho.gameObject);
                else
                    DestroyImmediate(filho.gameObject);
            }
        }

        return container;
    }

    private bool TentarEncontrarPontoNoChao(int cellX, BoundsInt bounds, out Vector3 pontoMundo)
    {
        for (int y = bounds.yMax; y >= bounds.yMin; y--)
        {
            Vector3Int celula = new Vector3Int(cellX, y, 0);

            if (!cenarioTilemap.HasTile(celula))
                continue;

            pontoMundo = cenarioTilemap.GetCellCenterWorld(celula);
            return true;
        }

        pontoMundo = Vector3.zero;
        return false;
    }

    private void ConfigurarComoMorcegoGerado(int indice)
    {
        usarComoGerador = false;
        gerarAoIniciar = false;
        limparGeradosAoIniciar = false;
        configuradoComoGerado = true;
        morto = false;
        alturaAlinhada = true;
        ativo = true;
        xInicial = transform.position.x;
        direcaoAtual = indice % 2 == 0 ? -1 : 1;
        gameObject.name = "Morcego_Inimigo_" + (indice + 1).ToString("00");

        GarantirDetectorParede();
        AtualizarVisual();
    }

    private void TentarAlinharAlturaComJogador()
    {
        if (configuradoComoGerado)
        {
            alturaAlinhada = true;
            return;
        }

        if (!alinharAlturaComJogador)
        {
            alturaAlinhada = true;
            return;
        }

        Transform jogador = GameManager.Instance != null
            ? GameManager.Instance.JogadorAtual
            : null;

        if (jogador == null)
        {
            GameObject jogadorObj = GameObject.FindGameObjectWithTag("Player");

            if (jogadorObj != null)
                jogador = jogadorObj.transform;
        }

        if (jogador == null)
            return;

        Collider2D jogadorCollider = jogador.GetComponent<Collider2D>();

        if (jogadorCollider == null)
            jogadorCollider = jogador.GetComponentInChildren<Collider2D>();

        float alturaJogador = jogadorCollider != null
            ? jogadorCollider.bounds.center.y
            : jogador.position.y;

        transform.position = new Vector3(
            transform.position.x,
            alturaJogador + offsetAlturaJogador,
            transform.position.z
        );

        alturaAlinhada = true;
    }

    private void VerificarLimiteDaPatrulha()
    {
        float distanciaDoInicio = transform.position.x - xInicial;

        if (direcaoAtual > 0 && distanciaDoInicio >= distanciaPatrulhaHorizontal)
        {
            Virar();
            return;
        }

        if (direcaoAtual < 0 && distanciaDoInicio <= -distanciaPatrulhaHorizontal)
        {
            Virar();
        }
    }

    private void Patrulhar()
    {
        if (rb == null)
            return;

        rb.linearVelocity = new Vector2(direcaoAtual * velocidade, rb.linearVelocity.y);
    }

    private void VerificarParede()
    {
        if (detectorParede == null)
            return;

        RaycastHit2D hit = Physics2D.Raycast(
            detectorParede.position,
            Vector2.right * direcaoAtual,
            distanciaDeteccaoParede,
            camadaParedeOuTilemap
        );

        if (hit.collider != null)
        {
            Virar();
        }
    }

    private void Virar()
    {
        direcaoAtual *= -1;
        AtualizarVisual();
    }

    private void AtualizarVisual()
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = direcaoAtual < 0;

        if (detectorParede != null)
        {
            float x = Mathf.Abs(detectorParede.localPosition.x) * direcaoAtual;
            detectorParede.localPosition = new Vector3(x, detectorParede.localPosition.y, 0f);
        }
    }

    private void GarantirDetectorParede()
    {
        if (detectorParede != null)
            return;

        Transform encontrado = transform.Find("DetectorParede");

        if (encontrado != null)
        {
            detectorParede = encontrado;
            return;
        }

        GameObject novoDetector = new GameObject("DetectorParede");
        novoDetector.transform.SetParent(transform);
        novoDetector.transform.localPosition = new Vector3(0.45f, 0f, 0f);
        detectorParede = novoDetector.transform;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (usarComoGerador || morto)
            return;

        if (collision.collider.CompareTag("Player"))
        {
            BaterNoPlayer(collision.collider);
            return;
        }

        if (EstaNaLayerSelecionada(collision.collider.gameObject.layer, camadaParedeOuTilemap))
        {
            Virar();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (usarComoGerador || morto) return;

        if (other.CompareTag("Projetil"))
        {
            Destroy(other.gameObject);
            ReceberAtaqueEspada();
            return;
        }

        if (other.CompareTag("Player"))
        {
            VerificarContatoComPlayer(other);
            return;
        }

        if (EstaNaLayerSelecionada(other.gameObject.layer, camadaParedeOuTilemap))
            Virar();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (usarComoGerador || morto) return;

        if (other.CompareTag("Player"))
            VerificarContatoComPlayer(other);
    }

    private void VerificarContatoComPlayer(Collider2D playerCollider)
    {
        PlayerController controller = playerCollider.GetComponent<PlayerController>()
                                   ?? playerCollider.GetComponentInParent<PlayerController>();
        if (controller != null && controller.IsDashing)
        {
            ReceberAtaqueEspada();
            return;
        }

        PlayerCombat combat = playerCollider.GetComponent<PlayerCombat>()
                           ?? playerCollider.GetComponentInParent<PlayerCombat>();
        if (combat != null && combat.isDashing)
        {
            ReceberAtaqueEspada();
            return;
        }

        BaterNoPlayer(playerCollider);
    }

    private bool EstaNaLayerSelecionada(int layer, LayerMask layerMask)
    {
        return ((1 << layer) & layerMask.value) != 0;
    }

    private void BaterNoPlayer(Collider2D playerCollider)
    {
        if (Time.time < proximoDanoLiberado)
            return;

        proximoDanoLiberado = Time.time + cooldownDanoPlayer;

        GameManager.Instance?.RemoverPontos(pontosAoEncostarNoPlayer);

        PlayerCombat combat = playerCollider.GetComponent<PlayerCombat>()
                           ?? playerCollider.GetComponentInParent<PlayerCombat>();
        combat?.TakeDamage(1);

        EmpurrarPlayer(playerCollider);
    }

    public void ReceberAtaqueEspada()
    {
        if (usarComoGerador || morto)
            return;

        int pontosFinais = pontosBaseAoMatar * multiplicadorPontosAoMatar;

        GameManager.Instance?.RegistrarMorcegoDerrotado(pontosFinais, essenciaAoMatar);

        Morrer();
    }

    public void BaterSemMatar(Collider2D playerCollider)
    {
        if (usarComoGerador || morto)
            return;

        GameManager.Instance?.RemoverPontos(pontosPerdidosAoBaterSemMatar);

        EmpurrarPlayer(playerCollider);
    }

    private void EmpurrarPlayer(Collider2D playerCollider)
    {
        if (playerCollider == null)
            return;

        Movimento movimento = playerCollider.GetComponent<Movimento>() ?? playerCollider.GetComponentInParent<Movimento>();

        if (movimento == null)
            return;

        Vector2 direcaoEmpurrao = playerCollider.transform.position - transform.position;

        if (Mathf.Abs(direcaoEmpurrao.x) < 0.01f)
            direcaoEmpurrao.x = -direcaoAtual;

        movimento.ReceberEmpurrao(
            direcaoEmpurrao,
            forcaEmpurraoHorizontal,
            forcaEmpurraoVertical
        );
    }

    private void Morrer()
    {
        morto = true;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (col != null)
            col.enabled = false;

        if (spriteRenderer != null)
            spriteRenderer.color = Color.red;

        Destroy(gameObject, tempoParaDestruir);
    }

    private void OnDrawGizmosSelected()
    {
        if (usarComoGerador)
            return;

        GarantirDetectorParede();

        if (detectorParede == null)
            return;

        int direcaoGizmo = direcaoAtual == 0
            ? (direcaoInicial >= 0 ? 1 : -1)
            : direcaoAtual;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            detectorParede.position,
            detectorParede.position + Vector3.right * direcaoGizmo * distanciaDeteccaoParede
        );
    }
}

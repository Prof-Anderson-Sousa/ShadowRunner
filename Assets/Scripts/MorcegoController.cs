using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MorcegoController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float velocidade = 2f;
    [SerializeField] private float distanciaDoJogador = 2.5f;

    [Header("Posicao do Morcego no Eixo Y")]
    [Tooltip("Ajuste para descer/subir o morcego no spawn. Valores negativos descem.")]
    [SerializeField] private float offsetSpawnY = -1.0f;

    [Header("Tutorial")]
    [Tooltip("Se marcado, o morcego nao morre antes de terminar o dialogo do tutorial.")]
    [SerializeField] private bool protegerDuranteTutorial = true;

    [Header("Fuga apos dialogo")]
    [Tooltip("Velocidade horizontal ao fugir, em unidades por segundo.")]
    [SerializeField] private float velocidadeFugaX = 4f;

    [Tooltip("Velocidade vertical ao fugir. Positivo faz subir.")]
    [SerializeField] private float velocidadeFugaY = 5f;

    [Tooltip("Quantas unidades acima da camera o morcego precisa chegar para ser destruido.")]
    [SerializeField] private float distanciaParaDestruir = 8f;

    [Header("Falas do Tutorial")]
    [SerializeField, TextArea(2, 4)] private string fala1 = "Você acordou sem saber onde esta... bem-vindo ao pesadelo.";
    [SerializeField, TextArea(2, 4)] private string fala2 = "Esta cidade foi tomada pelas criaturas das sombras. Elas nos perseguem sem parar.";
    [SerializeField, TextArea(2, 4)] private string fala3 = "Colete Essência derrotando inimigos. Ela é sua única fonte de poder aqui.";
    [SerializeField, TextArea(2, 4)] private string fala4 = "Use a Ação Especial com sabedoria — ela consome Essência, mas pode salvar sua vida.";
    [SerializeField, TextArea(2, 4)] private string fala5 = "Sobreviva o máximo que puder. A cidade só fica mais hostil com o tempo. Boa sorte, sobrevivente.";

    [Header("Tempo automatico das falas")]
    [SerializeField] private float tempoParaAvancarFala = 10f;

    [Header("Recompensa")]
    [SerializeField] private int recompensaPontos = 100;
    [SerializeField] private int recompensaEssencia = 1;
    [SerializeField] private int recompensaPontosAcaoEspecial = 60;

    [Header("Visual do Dialogo")]
    [Tooltip("Largura do painel em pixels, referencia 1920x1080.")]
    [SerializeField] private float painelLargura = 760f;

    [Tooltip("Altura do painel em pixels.")]
    [SerializeField] private float painelAltura = 160f;

    [Tooltip("Distancia do painel ate a borda inferior da tela em pixels.")]
    [SerializeField] private float painelMargemInferior = 55f;

    [Tooltip("Tamanho da fonte do texto principal da fala.")]
    [SerializeField] private float fonteTamanhoFala = 28f;

    [Tooltip("Tamanho da fonte do indicador [ Enter ].")]
    [SerializeField] private float fonteTamanhoIndicador = 24f;

    [Tooltip("Tamanho da fonte do label MORCEGO:.")]
    [SerializeField] private float fonteTamanhoNome = 20f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform jogador;
    private Movimento movimentoJogador;
    private Camera camPrincipal;

    private GameObject canvasRaiz;
    private TMP_Text textoNome;
    private TMP_Text textoBalao;
    private TMP_Text textoIndicador;

    private bool morto;
    private bool chegouPertoDoJogador;
    private bool dialogoAtivo;
    private bool dialogoEncerrado;
    private bool fugindo;

    private string[] falas;
    private int indiceFalaAtual;
    private float contadorTempoFala;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        camPrincipal = Camera.main;

        falas = new string[] { fala1, fala2, fala3, fala4, fala5 };

        transform.position = new Vector3(
            transform.position.x,
            transform.position.y + offsetSpawnY,
            transform.position.z
        );

        TentarEncontrarJogador();
    }

    private void Update()
    {
        if (morto)
            return;

        if (fugindo)
        {
            VerificarSaidaDeTela();
            return;
        }

        if (!dialogoAtivo || dialogoEncerrado)
            return;

        contadorTempoFala += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            AvancarFala();
            return;
        }

        if (contadorTempoFala >= tempoParaAvancarFala)
            AvancarFala();
    }

    private void FixedUpdate()
    {
        if (morto || fugindo || dialogoAtivo)
            return;

        if (jogador == null)
            TentarEncontrarJogador();

        if (jogador == null || chegouPertoDoJogador)
            return;

        float distanciaHorizontal = transform.position.x - jogador.position.x;

        if (Mathf.Abs(distanciaHorizontal) <= distanciaDoJogador)
        {
            chegouPertoDoJogador = true;

            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            IniciarDialogo();
            return;
        }

        float direcaoX = jogador.position.x > transform.position.x ? 1f : -1f;

        if (rb != null)
        {
            rb.MovePosition(new Vector2(
                transform.position.x + direcaoX * velocidade * Time.fixedDeltaTime,
                transform.position.y
            ));
        }

        AtualizarDirecaoVisual(direcaoX);
    }

    private void TentarEncontrarJogador()
    {
        if (GameManager.Instance != null && GameManager.Instance.JogadorAtual != null)
        {
            jogador = GameManager.Instance.JogadorAtual;
            movimentoJogador = jogador.GetComponent<Movimento>() ?? jogador.GetComponentInParent<Movimento>();
            return;
        }

        GameObject obj = GameObject.FindGameObjectWithTag("Player");

        if (obj != null)
        {
            jogador = obj.transform;
            movimentoJogador = obj.GetComponent<Movimento>() ?? obj.GetComponentInParent<Movimento>();
        }
    }

    private void AtualizarDirecaoVisual(float direcaoX)
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = direcaoX < 0f;
    }

    private void BloquearControleJogador(bool bloquear)
    {
        if (movimentoJogador == null)
        {
            if (jogador == null)
                TentarEncontrarJogador();

            if (jogador != null)
                movimentoJogador = jogador.GetComponent<Movimento>() ?? jogador.GetComponentInParent<Movimento>();
        }

        if (movimentoJogador != null)
            movimentoJogador.SetControlesBloqueados(bloquear);
    }

    private void IniciarDialogo()
    {
        if (canvasRaiz != null || dialogoAtivo || dialogoEncerrado)
            return;

        indiceFalaAtual = 0;
        contadorTempoFala = 0f;
        dialogoAtivo = true;
        dialogoEncerrado = false;

        BloquearControleJogador(true);

        CriarPainelTela();
        ExibirFalaAtual();
    }

    private void AvancarFala()
    {
        indiceFalaAtual++;

        if (indiceFalaAtual >= falas.Length)
        {
            EncerrarDialogo();
            IniciarFuga();
            return;
        }

        ExibirFalaAtual();
    }

    private void ExibirFalaAtual()
    {
        contadorTempoFala = 0f;

        if (textoBalao == null)
            return;

        textoBalao.text = falas[indiceFalaAtual];

        bool ultima = indiceFalaAtual >= falas.Length - 1;

        if (textoIndicador != null)
            textoIndicador.text = ultima ? "[ Enter ]  Fechar" : "[ Enter ]  Continuar  ▼";
    }

    private void EncerrarDialogo()
    {
        dialogoAtivo = false;
        dialogoEncerrado = true;

        BloquearControleJogador(false);

        if (canvasRaiz != null)
        {
            Destroy(canvasRaiz);
            canvasRaiz = null;
        }
    }

    private void IniciarFuga()
    {
        if (morto)
            return;

        BloquearControleJogador(false);

        fugindo = true;
        dialogoAtivo = false;
        dialogoEncerrado = true;

        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
            col.enabled = false;

        if (rb != null)
        {
            rb.gravityScale = 0f;

            float direcaoFugaX = jogador != null
                ? -Mathf.Sign(jogador.position.x - transform.position.x)
                : 1f;

            if (Mathf.Approximately(direcaoFugaX, 0f))
                direcaoFugaX = 1f;

            rb.linearVelocity = new Vector2(direcaoFugaX * velocidadeFugaX, velocidadeFugaY);

            AtualizarDirecaoVisual(direcaoFugaX);
        }
    }

    private void VerificarSaidaDeTela()
    {
        if (morto)
            return;

        if (camPrincipal == null)
        {
            Destroy(gameObject);
            return;
        }

        float limiteSupTela = camPrincipal.transform.position.y
                            + camPrincipal.orthographicSize
                            + distanciaParaDestruir;

        if (transform.position.y >= limiteSupTela)
        {
            GameManager.Instance?.RegistrarMorcegoDerrotado(0, 0);
            Destroy(gameObject);
        }
    }

    private void CriarPainelTela()
    {
        canvasRaiz = new GameObject(
            "DialogoMorcego_Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        Canvas canvas = canvasRaiz.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasRaiz.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject painel = new GameObject("Painel", typeof(RectTransform), typeof(Image));
        painel.transform.SetParent(canvasRaiz.transform, false);

        RectTransform painelRect = painel.GetComponent<RectTransform>();
        painelRect.anchorMin = new Vector2(0.5f, 0f);
        painelRect.anchorMax = new Vector2(0.5f, 0f);
        painelRect.pivot = new Vector2(0.5f, 0f);
        painelRect.sizeDelta = new Vector2(painelLargura, painelAltura);
        painelRect.anchoredPosition = new Vector2(0f, painelMargemInferior);

        Image painelImg = painel.GetComponent<Image>();
        painelImg.color = new Color(0.08f, 0.08f, 0.12f, 0.93f);
        painelImg.raycastTarget = false;

        GameObject bordaObj = new GameObject("Borda", typeof(RectTransform), typeof(Image));
        bordaObj.transform.SetParent(painel.transform, false);

        RectTransform bordaRect = bordaObj.GetComponent<RectTransform>();
        bordaRect.anchorMin = Vector2.zero;
        bordaRect.anchorMax = Vector2.one;
        bordaRect.offsetMin = new Vector2(-2f, -2f);
        bordaRect.offsetMax = new Vector2(2f, 2f);

        Image bordaImg = bordaObj.GetComponent<Image>();
        bordaImg.color = new Color(0.48f, 0.31f, 0.75f, 0.7f);
        bordaImg.raycastTarget = false;
        bordaObj.transform.SetSiblingIndex(0);

        float tagBaseY = painelMargemInferior + painelAltura + 4f;

        GameObject nomeFundoObj = new GameObject("NomeFundo", typeof(RectTransform), typeof(Image));
        nomeFundoObj.transform.SetParent(canvasRaiz.transform, false);

        RectTransform nomeFundoRect = nomeFundoObj.GetComponent<RectTransform>();
        nomeFundoRect.anchorMin = new Vector2(0.5f, 0f);
        nomeFundoRect.anchorMax = new Vector2(0.5f, 0f);
        nomeFundoRect.pivot = new Vector2(0f, 0f);
        nomeFundoRect.sizeDelta = new Vector2(painelLargura * 0.30f + 4f, fonteTamanhoNome + 16f);
        nomeFundoRect.anchoredPosition = new Vector2(-(painelLargura / 2f) - 2f, tagBaseY);

        Image nomeFundoImg = nomeFundoObj.GetComponent<Image>();
        nomeFundoImg.color = new Color(0.18f, 0.10f, 0.30f, 0.95f);
        nomeFundoImg.raycastTarget = false;

        GameObject nomeObj = new GameObject("NomeParlante", typeof(RectTransform), typeof(TextMeshProUGUI));
        nomeObj.transform.SetParent(canvasRaiz.transform, false);

        RectTransform nomeRect = nomeObj.GetComponent<RectTransform>();
        nomeRect.anchorMin = new Vector2(0.5f, 0f);
        nomeRect.anchorMax = new Vector2(0.5f, 0f);
        nomeRect.pivot = new Vector2(0f, 0f);
        nomeRect.sizeDelta = new Vector2(painelLargura * 0.30f, fonteTamanhoNome + 14f);
        nomeRect.anchoredPosition = new Vector2(-(painelLargura / 2f) + 8f, tagBaseY + 2f);

        textoNome = nomeObj.GetComponent<TMP_Text>();
        textoNome.text = "MORCEGO:";
        textoNome.fontSize = fonteTamanhoNome;
        textoNome.fontStyle = FontStyles.Bold;
        textoNome.color = new Color(0.72f, 0.52f, 1f, 1f);
        textoNome.alignment = TextAlignmentOptions.MidlineLeft;
        textoNome.textWrappingMode = TextWrappingModes.NoWrap;
        textoNome.raycastTarget = false;

        GameObject textoObj = new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
        textoObj.transform.SetParent(painel.transform, false);

        RectTransform textoRect = textoObj.GetComponent<RectTransform>();
        textoRect.anchorMin = new Vector2(0f, 0.28f);
        textoRect.anchorMax = Vector2.one;
        textoRect.offsetMin = new Vector2(22f, 0f);
        textoRect.offsetMax = new Vector2(-22f, -10f);

        textoBalao = textoObj.GetComponent<TMP_Text>();
        textoBalao.fontSize = fonteTamanhoFala;
        textoBalao.color = Color.white;
        textoBalao.alignment = TextAlignmentOptions.MidlineLeft;
        textoBalao.textWrappingMode = TextWrappingModes.Normal;
        textoBalao.raycastTarget = false;

        GameObject linhaObj = new GameObject("Linha", typeof(RectTransform), typeof(Image));
        linhaObj.transform.SetParent(painel.transform, false);

        RectTransform linhaRect = linhaObj.GetComponent<RectTransform>();
        linhaRect.anchorMin = new Vector2(0.02f, 0.27f);
        linhaRect.anchorMax = new Vector2(0.98f, 0.29f);
        linhaRect.offsetMin = Vector2.zero;
        linhaRect.offsetMax = Vector2.zero;

        Image linhaImg = linhaObj.GetComponent<Image>();
        linhaImg.color = new Color(1f, 1f, 1f, 0.15f);
        linhaImg.raycastTarget = false;

        GameObject indObj = new GameObject("Indicador", typeof(RectTransform), typeof(TextMeshProUGUI));
        indObj.transform.SetParent(painel.transform, false);

        RectTransform indRect = indObj.GetComponent<RectTransform>();
        indRect.anchorMin = Vector2.zero;
        indRect.anchorMax = new Vector2(1f, 0.27f);
        indRect.offsetMin = new Vector2(22f, 4f);
        indRect.offsetMax = new Vector2(-22f, 0f);

        textoIndicador = indObj.GetComponent<TMP_Text>();
        textoIndicador.fontSize = fonteTamanhoIndicador;
        textoIndicador.color = new Color(0.75f, 0.65f, 1f, 1f);
        textoIndicador.alignment = TextAlignmentOptions.MidlineRight;
        textoIndicador.textWrappingMode = TextWrappingModes.NoWrap;
        textoIndicador.raycastTarget = false;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (morto || fugindo)
            return;

        if (col.CompareTag("Projetil"))
        {
            Destroy(col.gameObject);

            if (PodeReceberDano())
                Derrotar(recompensaPontos, recompensaEssencia);

            return;
        }

        if (col.CompareTag("Player"))
        {
            Movimento movimentoPlayer = col.GetComponent<Movimento>() ?? col.GetComponentInParent<Movimento>();

            bool veioDeCima = false;
            bool estaCaindo = false;

            if (movimentoPlayer != null)
            {
                veioDeCima = movimentoPlayer.AlturaDoPe() > transform.position.y + 0.1f;
                estaCaindo = movimentoPlayer.EstaCaindo();
            }

            if (movimentoPlayer != null && veioDeCima && estaCaindo)
            {
                if (PodeReceberDano())
                    Derrotar(recompensaPontos, recompensaEssencia);

                movimentoPlayer.QuicarAposPisar();
                return;
            }

            PlayerCombat playerCombat = col.GetComponent<PlayerCombat>() ?? col.GetComponentInParent<PlayerCombat>();

            if (playerCombat != null)
                playerCombat.TakeDamage(1);
            else
                GameManager.Instance?.EncerrarJogo();
        }
    }

    private bool PodeReceberDano()
    {
        if (!protegerDuranteTutorial)
            return true;

        return dialogoEncerrado && !dialogoAtivo;
    }

    public void ReceberAtaqueEspada()
    {
        if (!PodeReceberDano())
            return;

        Derrotar(recompensaPontos, recompensaEssencia);
    }

    public void DerrotarPorAtaqueEspada()
    {
        ReceberAtaqueEspada();
    }

    public void DerrotarPorAcaoEspecial()
    {
        if (!PodeReceberDano())
            return;

        Derrotar(recompensaPontosAcaoEspecial, 0);
    }

    private void Derrotar(int pontos, int essencia)
    {
        if (morto)
            return;

        GameManager.Instance?.RegistrarMorcegoDerrotado(pontos, essencia);
        Morrer();
    }

    private void Morrer()
    {
        morto = true;
        fugindo = false;
        dialogoAtivo = false;
        dialogoEncerrado = true;

        BloquearControleJogador(false);

        if (canvasRaiz != null)
        {
            Destroy(canvasRaiz);
            canvasRaiz = null;
        }

        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
            col.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
        }

        Destroy(gameObject, 0.4f);
    }

    private void OnDisable()
    {
        BloquearControleJogador(false);
    }

    private void OnDestroy()
    {
        BloquearControleJogador(false);
    }
}

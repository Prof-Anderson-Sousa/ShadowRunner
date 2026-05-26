using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtaqueEspada : MonoBehaviour
{
    [Header("Controles")]
    [SerializeField] private KeyCode teclaAtaque = KeyCode.J;

    [Header("Ataque")]
    [SerializeField] private int dano = 1;
    [SerializeField] private float raioAtaque = 1.15f;
    [SerializeField] private float cooldownAtaque = 0.45f;
    [SerializeField] private LayerMask camadaInimigos;

    [Header("Ponto de Ataque")]
    [Tooltip("Objeto filho que marca onde o golpe da espada acontece.")]
    [SerializeField] private Transform pontoAtaque;

    [Tooltip("Usado apenas se o PontoAtaque não estiver configurado no Inspector.")]
    [SerializeField] private float distanciaDoCentro = 0.072f;

    [Tooltip("Usado apenas se o PontoAtaque não estiver configurado no Inspector.")]
    [SerializeField] private float offsetVerticalAtaque = 0.638f;

    [Header("Animacao")]
    [SerializeField] private string parametroAnimacaoAtaque = "attack";
    [SerializeField] private string estadoAnimacaoAtaque = "attack";
    [SerializeField] private float duracaoAnimacaoAtaque = 0.35f;
    [SerializeField] private string estadoRetornoAposAtaque = "idle";

    [Header("Audio")]
    [SerializeField] private AudioClip somAtaque;
    [SerializeField] private float volumeSomAtaque = 1f;
    [SerializeField] private string caminhoSomAtaque = "Assets/Audio/espada_sound.mp3";

    [Header("Debug")]
    [SerializeField] private bool mostrarDebugAtaque = true;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerCombat playerCombat;
    private AnimacaoPersonagemPorCodigo animacaoPorCodigo;
    private AudioSource audioSource;

    private float proximoAtaqueLiberado;
    private int attackHash;
    private bool atacando;
    private Coroutine rotinaAtaque;

    private Vector3 posicaoLocalOriginalPontoAtaque;

    public bool EstaAtacando => atacando;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCombat = GetComponent<PlayerCombat>();
        animacaoPorCodigo = GetComponent<AnimacaoPersonagemPorCodigo>();
        audioSource = GetComponent<AudioSource>();
        attackHash = Animator.StringToHash(parametroAnimacaoAtaque);

#if UNITY_EDITOR
        CarregarSomAtaqueNoEditor();
#endif

        GarantirPontoAtaque();

        if (pontoAtaque != null)
            posicaoLocalOriginalPontoAtaque = pontoAtaque.localPosition;
    }

    private void LateUpdate()
    {
        AtualizarLadoDoPontoAtaque();
    }

    private void Update()
    {
        if (playerCombat != null && playerCombat.IsDead)
            return;

        if (Time.timeScale == 0f)
            return;

        if (Input.GetKeyDown(teclaAtaque))
            TentarAtacar();
    }

    private void TentarAtacar()
    {
        if (atacando || Time.time < proximoAtaqueLiberado)
            return;

        atacando = true;
        proximoAtaqueLiberado = Time.time + cooldownAtaque;

        TocarSomAtaque();
        TocarAnimacaoAtaque();

        if (rotinaAtaque != null)
            StopCoroutine(rotinaAtaque);

        rotinaAtaque = StartCoroutine(FinalizarAtaqueDepoisDaAnimacao());

        if (pontoAtaque == null)
        {
            Debug.LogWarning("AtaqueEspada: PontoAtaque nao foi encontrado.");
            return;
        }

        Vector2 posicaoAtaque = pontoAtaque.position;

        Collider2D[] inimigosAtingidos = camadaInimigos.value == 0
            ? Physics2D.OverlapCircleAll(posicaoAtaque, raioAtaque)
            : Physics2D.OverlapCircleAll(posicaoAtaque, raioAtaque, camadaInimigos);

        if (mostrarDebugAtaque)
            Debug.Log("Ataque da espada detectou " + inimigosAtingidos.Length + " collider(s).");

        HashSet<GameObject> inimigosJaAtacados = new HashSet<GameObject>();

        foreach (Collider2D inimigo in inimigosAtingidos)
        {
            if (inimigo == null)
                continue;

            GameObject raiz = inimigo.attachedRigidbody != null
                ? inimigo.attachedRigidbody.gameObject
                : inimigo.transform.root.gameObject;

            if (inimigosJaAtacados.Contains(raiz))
                continue;

            inimigosJaAtacados.Add(raiz);
            AtacarInimigo(inimigo);
        }
    }

    private void AtacarInimigo(Collider2D inimigo)
    {
        if (inimigo == null)
            return;

        MorcegoPatrulhaController morcegoPatrulha = inimigo.GetComponent<MorcegoPatrulhaController>()
            ?? inimigo.GetComponentInParent<MorcegoPatrulhaController>();

        if (morcegoPatrulha != null)
        {
            morcegoPatrulha.ReceberAtaqueEspada();
            return;
        }

        MorcegoController morcegoTutorial = inimigo.GetComponent<MorcegoController>()
            ?? inimigo.GetComponentInParent<MorcegoController>();

        if (morcegoTutorial != null)
        {
            morcegoTutorial.ReceberAtaqueEspada();
            return;
        }

        ShadowCreatureAI criaturaDasSombras = inimigo.GetComponent<ShadowCreatureAI>()
            ?? inimigo.GetComponentInParent<ShadowCreatureAI>();

        if (criaturaDasSombras != null)
        {
            criaturaDasSombras.TakeDamage(dano);
            return;
        }

        if (mostrarDebugAtaque)
            Debug.Log("Collider atingido, mas nenhum script de inimigo conhecido foi encontrado: " + inimigo.name);
    }

    private void AtualizarLadoDoPontoAtaque()
    {
        if (pontoAtaque == null)
            return;

        bool olhandoParaEsquerda = spriteRenderer != null && spriteRenderer.flipX;

        float x = Mathf.Abs(posicaoLocalOriginalPontoAtaque.x);
        if (x < 0.01f)
            x = Mathf.Abs(distanciaDoCentro);

        pontoAtaque.localPosition = new Vector3(
            olhandoParaEsquerda ? -x : x,
            posicaoLocalOriginalPontoAtaque.y,
            posicaoLocalOriginalPontoAtaque.z
        );

        pontoAtaque.localRotation = Quaternion.identity;
    }

    private IEnumerator FinalizarAtaqueDepoisDaAnimacao()
    {
        yield return new WaitForSeconds(duracaoAnimacaoAtaque);

        atacando = false;

        if (animator != null && HasAnimatorParameter(attackHash))
            animator.ResetTrigger(attackHash);

        if (!TentarTocarEstado(estadoRetornoAposAtaque, 0.05f))
        {
            if (!TentarTocarEstado("idle", 0.05f) && !TentarTocarEstado("1 - Idle", 0.05f))
                TentarTocarEstado("Parado", 0.05f);
        }

        rotinaAtaque = null;
    }

    private void TocarAnimacaoAtaque()
    {
        if (animacaoPorCodigo != null)
        {
            animacaoPorCodigo.TocarAtaque();
            return;
        }

        if (animator == null)
            return;

        if (HasAnimatorParameter(attackHash))
        {
            animator.ResetTrigger(attackHash);
            animator.SetTrigger(attackHash);
            return;
        }

        if (TentarTocarEstado(estadoAnimacaoAtaque, 0f)) return;
        if (TentarTocarEstado(parametroAnimacaoAtaque, 0f)) return;
        if (TentarTocarEstado("attack", 0f)) return;
        if (TentarTocarEstado("10 - attack 1", 0f)) return;
        if (TentarTocarEstado("5. Attack 131 x 56", 0f)) return;
        if (TentarTocarEstado("9 - idle-up-attack", 0f)) return;

        TentarTocarClipComNomeDeAtaque();
    }

    private void GarantirPontoAtaque()
    {
        if (pontoAtaque != null)
            return;

        Transform pontoEncontrado = transform.Find("PontoAtaque");

        if (pontoEncontrado != null)
        {
            pontoAtaque = pontoEncontrado;
            return;
        }

        GameObject novoPonto = new GameObject("PontoAtaque");
        novoPonto.transform.SetParent(transform);
        novoPonto.transform.localPosition = new Vector3(distanciaDoCentro, offsetVerticalAtaque, 0f);
        novoPonto.transform.localRotation = Quaternion.identity;
        pontoAtaque = novoPonto.transform;
    }

    private void OnDrawGizmosSelected()
    {
        if (pontoAtaque == null)
            GarantirPontoAtaque();

        if (pontoAtaque == null)
            return;

        Gizmos.color = new Color(0f, 1f, 0.5f, 0.8f);
        Gizmos.DrawWireSphere(pontoAtaque.position, 0.05f);

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.8f);
        Gizmos.DrawWireSphere(pontoAtaque.position, raioAtaque);
    }

    private bool HasAnimatorParameter(int parameterHash)
    {
        if (animator == null)
            return false;

        foreach (AnimatorControllerParameter p in animator.parameters)
        {
            if (p.nameHash == parameterHash)
                return true;
        }

        return false;
    }

    private bool TentarTocarEstado(string stateName, float duracaoTransicao)
    {
        if (animator == null || string.IsNullOrEmpty(stateName))
            return false;

        int hash = Animator.StringToHash(stateName);
        int baseHash = Animator.StringToHash("Base Layer." + stateName);

        if (animator.HasState(0, hash))
        {
            animator.CrossFade(hash, duracaoTransicao, 0);
            return true;
        }

        if (animator.HasState(0, baseHash))
        {
            animator.CrossFade(baseHash, duracaoTransicao, 0);
            return true;
        }

        return false;
    }

    private bool TentarTocarClipComNomeDeAtaque()
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return false;

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip == null || !clip.name.ToLowerInvariant().Contains("attack"))
                continue;

            if (TentarTocarEstado(clip.name, 0f))
                return true;
        }

        return false;
    }

    private void TocarSomAtaque()
    {
        if (somAtaque == null)
            return;

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.PlayOneShot(somAtaque, volumeSomAtaque);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        CarregarSomAtaqueNoEditor();
    }

    private void CarregarSomAtaqueNoEditor()
    {
        if (somAtaque != null || string.IsNullOrEmpty(caminhoSomAtaque))
            return;

        somAtaque = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(caminhoSomAtaque);
    }
#endif
}

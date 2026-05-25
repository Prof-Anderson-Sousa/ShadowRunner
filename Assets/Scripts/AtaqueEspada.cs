using System.Collections;
using UnityEngine;

public class AtaqueEspada : MonoBehaviour
{
    [Header("Controles")]
    [SerializeField] private KeyCode teclaAtaque = KeyCode.J;

    [Header("Ataque")]
    [SerializeField] private int dano = 1;
    [SerializeField] private float alcanceAtaque = 0.9f;
    [SerializeField] private float alturaAtaque = 0.8f;
    [SerializeField] private float distanciaDoCentro = 0.75f;
    [SerializeField] private float cooldownAtaque = 0.45f;
    [SerializeField] private LayerMask camadaInimigos;

    [Header("Referencias")]
    [SerializeField] private Transform pontoAtaque;

    [Header("Animacao")]
    [SerializeField] private string parametroAnimacaoAtaque = "Attack";
    [SerializeField] private string estadoAnimacaoAtaque = "Attack";
    [SerializeField] private float duracaoAnimacaoAtaque = 0.35f;
    [SerializeField] private string estadoRetornoAposAtaque = "Idle";

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerCombat playerCombat;

    private float proximoAtaqueLiberado;
    private int attackHash;
    private bool atacando;
    private Coroutine rotinaAtaque;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        attackHash = Animator.StringToHash(parametroAnimacaoAtaque);
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCombat = GetComponent<PlayerCombat>();

        GarantirPontoAtaque();
    }

    private void Update()
    {
        if (playerCombat != null && playerCombat.IsDead)
            return;

        if (Time.timeScale == 0f)
            return;

        if (Input.GetKeyDown(teclaAtaque))
        {
            TentarAtacar();
        }
    }

    private void TentarAtacar()
    {
        if (atacando || Time.time < proximoAtaqueLiberado)
            return;

        atacando = true;
        proximoAtaqueLiberado = Time.time + cooldownAtaque;

        TocarAnimacaoAtaque();

        if (rotinaAtaque != null)
            StopCoroutine(rotinaAtaque);

        rotinaAtaque = StartCoroutine(FinalizarAtaqueDepoisDaAnimacao());

        GarantirPontoAtaque();
        AtualizarPosicaoPontoAtaque();

        Collider2D[] inimigosAtingidos = Physics2D.OverlapBoxAll(
            pontoAtaque.position,
            new Vector2(alcanceAtaque, alturaAtaque),
            0f,
            camadaInimigos
        );

        foreach (Collider2D inimigo in inimigosAtingidos)
        {
            MorcegoPatrulhaController morcegoPatrulha = inimigo.GetComponent<MorcegoPatrulhaController>();

            if (morcegoPatrulha == null)
                morcegoPatrulha = inimigo.GetComponentInParent<MorcegoPatrulhaController>();

            if (morcegoPatrulha != null)
            {
                morcegoPatrulha.ReceberAtaqueEspada();
                continue;
            }

            MorcegoController morcegoTutorial = inimigo.GetComponent<MorcegoController>();

            if (morcegoTutorial == null)
                morcegoTutorial = inimigo.GetComponentInParent<MorcegoController>();

            if (morcegoTutorial != null)
            {
                morcegoTutorial.DerrotarPorAtaqueEspada();
                continue;
            }
        }
    }

    private IEnumerator FinalizarAtaqueDepoisDaAnimacao()
    {
        yield return new WaitForSeconds(duracaoAnimacaoAtaque);

        atacando = false;

        if (animator != null && HasAnimatorParameter(attackHash))
            animator.ResetTrigger(attackHash);

        if (!TentarTocarEstado(estadoRetornoAposAtaque, 0.05f))
            TentarTocarEstado("Parado", 0.05f);

        rotinaAtaque = null;
    }

    private void TocarAnimacaoAtaque()
    {
        if (animator == null)
            return;

        if (HasAnimatorParameter(attackHash))
        {
            animator.ResetTrigger(attackHash);
            animator.SetTrigger(attackHash);
            return;
        }

        if (TentarTocarEstado(estadoAnimacaoAtaque, 0f))
            return;

        TentarTocarEstado(parametroAnimacaoAtaque, 0f);
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
        novoPonto.transform.localPosition = new Vector3(distanciaDoCentro, 0f, 0f);
        pontoAtaque = novoPonto.transform;
    }

    private void AtualizarPosicaoPontoAtaque()
    {
        if (pontoAtaque == null)
            return;

        bool olhandoParaEsquerda = spriteRenderer != null && spriteRenderer.flipX;
        float direcao = olhandoParaEsquerda ? -1f : 1f;

        pontoAtaque.localPosition = new Vector3(
            distanciaDoCentro * direcao,
            0f,
            0f
        );
    }

    private void OnDrawGizmosSelected()
    {
        GarantirPontoAtaque();
        AtualizarPosicaoPontoAtaque();

        if (pontoAtaque == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            pontoAtaque.position,
            new Vector3(alcanceAtaque, alturaAtaque, 0f)
        );
    }

    private bool HasAnimatorParameter(int parameterHash)
    {
        if (animator == null)
            return false;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == parameterHash)
                return true;
        }

        return false;
    }

    private bool TentarTocarEstado(string stateName, float duracaoTransicao)
    {
        if (animator == null || string.IsNullOrEmpty(stateName))
            return false;

        int stateHash = Animator.StringToHash(stateName);
        int baseLayerStateHash = Animator.StringToHash("Base Layer." + stateName);

        if (animator.HasState(0, stateHash))
        {
            animator.CrossFade(stateHash, duracaoTransicao, 0);
            return true;
        }

        if (animator.HasState(0, baseLayerStateHash))
        {
            animator.CrossFade(baseLayerStateHash, duracaoTransicao, 0);
            return true;
        }

        return false;
    }
}

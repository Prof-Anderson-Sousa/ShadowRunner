using UnityEngine;

public class AnimacaoPersonagemPorCodigo : MonoBehaviour
{
    [Header("Fonte")]
    [SerializeField] private SpriteRenderer spriteRendererAlvo;
    [SerializeField] private bool desativarAnimator = true;
    [SerializeField] private string caminhoAseprite = "Assets/SHADOW Series - The Blind Huntress/SHADOW Series - The Blind Huntress.aseprite";

    [Header("Tempo")]
    [SerializeField] private float quadrosPorSegundo = 12f;

    [Header("Clips")]
    [SerializeField] private Sprite[] idle;
    [SerializeField] private Sprite[] run;
    [SerializeField] private Sprite[] jump;
    [SerializeField] private Sprite[] fall;
    [SerializeField] private Sprite[] attack;
    [SerializeField] private Sprite[] hit;
    [SerializeField] private Sprite[] death;

    private Sprite[] clipAtual;
    private float tempoClip;
    private int frameAtual = -1;
    private bool loopAtual = true;
    private bool tocandoAcao;

    public bool EstaTocandoAcao => tocandoAcao;

    private void Awake()
    {
        if (spriteRendererAlvo == null)
            spriteRendererAlvo = GetComponent<SpriteRenderer>();

#if UNITY_EDITOR
        if (!TemFrames(idle) || !TemFrames(run) || !TemFrames(attack))
            PreencherClipsNoEditor();
#endif

        if (desativarAnimator)
        {
            Animator animator = GetComponent<Animator>();

            if (animator != null)
                animator.enabled = false;
        }
    }

    private void Start()
    {
        TocarLoop(idle);
    }

    private void Update()
    {
        AtualizarFrame();
    }

    public void AtualizarMovimento(bool movendo, bool noChao, float velocidadeVertical)
    {
        if (tocandoAcao)
            return;

        if (!noChao && velocidadeVertical < -0.1f && TemFrames(fall))
        {
            TocarLoop(fall);
            return;
        }

        if (!noChao && TemFrames(jump))
        {
            TocarLoop(jump);
            return;
        }

        if (movendo && TemFrames(run))
        {
            TocarLoop(run);
            return;
        }

        TocarLoop(idle);
    }

    public void TocarAtaque()
    {
        TocarUmaVez(attack);
    }

    public void TocarHit()
    {
        TocarUmaVez(hit);
    }

    public void TocarMorte()
    {
        TocarUmaVez(death);
    }

    private void TocarLoop(Sprite[] clip)
    {
        if (!TemFrames(clip))
            return;

        if (clipAtual == clip && loopAtual)
            return;

        clipAtual = clip;
        loopAtual = true;
        tocandoAcao = false;
        ReiniciarClip();
    }

    private void TocarUmaVez(Sprite[] clip)
    {
        if (!TemFrames(clip))
            return;

        clipAtual = clip;
        loopAtual = false;
        tocandoAcao = true;
        ReiniciarClip();
    }

    private void ReiniciarClip()
    {
        tempoClip = 0f;
        frameAtual = -1;
        AplicarFrame(0);
    }

    private void AtualizarFrame()
    {
        if (!TemFrames(clipAtual) || spriteRendererAlvo == null)
            return;

        tempoClip += Time.deltaTime;
        int proximoFrame = Mathf.FloorToInt(tempoClip * quadrosPorSegundo);

        if (loopAtual)
            proximoFrame %= clipAtual.Length;
        else if (proximoFrame >= clipAtual.Length)
        {
            tocandoAcao = false;
            TocarLoop(idle);
            return;
        }

        AplicarFrame(proximoFrame);
    }

    private void AplicarFrame(int indiceFrame)
    {
        if (frameAtual == indiceFrame || !TemFrames(clipAtual) || spriteRendererAlvo == null)
            return;

        frameAtual = indiceFrame;
        spriteRendererAlvo.sprite = clipAtual[indiceFrame];
    }

    private bool TemFrames(Sprite[] clip)
    {
        return clip != null && clip.Length > 0;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (spriteRendererAlvo == null)
            spriteRendererAlvo = GetComponent<SpriteRenderer>();

        PreencherClipsNoEditor();
    }

    [ContextMenu("Preencher clips do Aseprite")]
    private void PreencherClipsNoEditor()
    {
        if (string.IsNullOrEmpty(caminhoAseprite))
            return;

        Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(caminhoAseprite);

        if (assets == null || assets.Length == 0)
            return;

        idle = BuscarSpritesDoClip(assets, "idle", idle);
        run = BuscarSpritesDoClip(assets, "run", run);
        jump = BuscarSpritesDoClip(assets, "jump", jump);
        fall = BuscarSpritesDoClip(assets, "fall", fall);
        attack = BuscarSpritesDoClip(assets, "attack", attack);
        hit = BuscarSpritesDoClip(assets, "hit", hit);
        death = BuscarSpritesDoClip(assets, "death", death);
    }

    private Sprite[] BuscarSpritesDoClip(Object[] assets, string nomeClip, Sprite[] valorAtual)
    {
        foreach (Object asset in assets)
        {
            AnimationClip clip = asset as AnimationClip;

            if (clip == null || clip.name != nomeClip)
                continue;

            Sprite[] sprites = ExtrairSpritesDoClip(clip);

            if (sprites.Length > 0)
                return sprites;
        }

        return valorAtual;
    }

    private Sprite[] ExtrairSpritesDoClip(AnimationClip clip)
    {
        UnityEditor.EditorCurveBinding[] bindings = UnityEditor.AnimationUtility.GetObjectReferenceCurveBindings(clip);

        foreach (UnityEditor.EditorCurveBinding binding in bindings)
        {
            if (binding.propertyName != "m_Sprite")
                continue;

            UnityEditor.ObjectReferenceKeyframe[] keyframes = UnityEditor.AnimationUtility.GetObjectReferenceCurve(clip, binding);
            Sprite[] sprites = new Sprite[keyframes.Length];

            for (int i = 0; i < keyframes.Length; i++)
                sprites[i] = keyframes[i].value as Sprite;

            return sprites;
        }

        return new Sprite[0];
    }
#endif
}

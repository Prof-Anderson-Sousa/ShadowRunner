using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MorcegoSpawner : MonoBehaviour
{
    [SerializeField] private GameObject morcegoPrefab;

    [Header("Tela de Missao Completa")]
    [SerializeField] private GameObject painelMissionComplete;
    [SerializeField] private bool pausarJogoAoCompletar = true;
    [SerializeField] private string textoMissionComplete = "MISSION ONE\nCOMPLETE";
    [SerializeField] private float duracaoAnimacaoMensagem = 1.2f;
    [SerializeField] private Vector2 posicaoInicialMensagem = new Vector2(0f, -220f);
    [SerializeField] private Vector2 posicaoFinalMensagem = new Vector2(0f, 80f);

    [Header("Posicoes dos Morcegos")]
    [SerializeField] private Vector2 posicaoMorcego1 = new Vector2(4f, -0.2f);
    [SerializeField] private Vector2 posicaoMorcego2 = new Vector2(9f, -0.2f);
    [SerializeField] private Vector2 posicaoMorcego3 = new Vector2(14f, -0.2f);
    [SerializeField] private Vector2 posicaoMorcego4 = new Vector2(19f, -0.2f);
    [SerializeField] private Vector2 posicaoMorcego5 = new Vector2(24f, -0.2f);
    [SerializeField] private Vector2 posicaoMorcego6 = new Vector2(29f, -0.2f);
    [SerializeField] private Vector2 posicaoMorcego7 = new Vector2(34f, -0.2f);
    [SerializeField] private Vector2 posicaoMorcego8 = new Vector2(39f, -0.2f);
    [SerializeField] private Vector2 posicaoMorcego9 = new Vector2(44f, -0.2f);
    [SerializeField] private Vector2 posicaoMorcego10 = new Vector2(49f, -0.2f);

    [SerializeField] private Vector2 posicaoMorcego11 = new Vector2(54f, -0.2f);
    [SerializeField] private Vector2 posicaoMorcego12 = new Vector2(59f, -0.2f);
    [SerializeField] private Vector2 posicaoMorcego13 = new Vector2(64f, -0.2f);
    [SerializeField] private Vector2 posicaoMorcego14 = new Vector2(69f, -0.2f);
    [SerializeField] private Vector2 posicaoMorcego15 = new Vector2(74f, -0.2f);
    [SerializeField] private Vector2 posicaoMorcego16 = new Vector2(79f, -0.2f);
    [SerializeField] private Vector2 posicaoMorcego17 = new Vector2(84f, -0.2f);
    [SerializeField] private Vector2 posicaoMorcego18 = new Vector2(89f, -0.2f);
    [SerializeField] private Vector2 posicaoMorcego19 = new Vector2(94f, -0.2f);
    [SerializeField] private Vector2 posicaoMorcego20 = new Vector2(99f, -0.2f);

    [SerializeField] private Vector2 posicaoMorcego21 = new Vector2(104f, -0.2f);
    [SerializeField] private Vector2 posicaoMorcego22 = new Vector2(109f, -0.2f);
    [SerializeField] private Vector2 posicaoMorcego23 = new Vector2(114f, -0.2f);

    private readonly List<GameObject> morcegosCriados = new List<GameObject>();
    private bool missaoCompleta = false;
    private RectTransform mensagemMissionCompleteRect;

    void Start()
    {
        Time.timeScale = 1f;

        if (painelMissionComplete != null)
            painelMissionComplete.SetActive(false);

        GameManager.Instance?.RegistrarNovaOnda(23);

        if (morcegoPrefab == null)
        {
            Debug.LogWarning("[MorcegoSpawner] Prefab do morcego nao configurado.");
            return;
        }

        CriarMorcego(posicaoMorcego1);
        CriarMorcego(posicaoMorcego2);
        CriarMorcego(posicaoMorcego3);
        CriarMorcego(posicaoMorcego4);
        CriarMorcego(posicaoMorcego5);
        CriarMorcego(posicaoMorcego6);
        CriarMorcego(posicaoMorcego7);
        CriarMorcego(posicaoMorcego8);
        CriarMorcego(posicaoMorcego9);
        CriarMorcego(posicaoMorcego10);

        CriarMorcego(posicaoMorcego11);
        CriarMorcego(posicaoMorcego12);
        CriarMorcego(posicaoMorcego13);
        CriarMorcego(posicaoMorcego14);
        CriarMorcego(posicaoMorcego15);
        CriarMorcego(posicaoMorcego16);
        CriarMorcego(posicaoMorcego17);
        CriarMorcego(posicaoMorcego18);
        CriarMorcego(posicaoMorcego19);
        CriarMorcego(posicaoMorcego20);

        CriarMorcego(posicaoMorcego21);
        CriarMorcego(posicaoMorcego22);
        CriarMorcego(posicaoMorcego23);
    }

    void Update()
    {
        if (missaoCompleta)
            return;

        morcegosCriados.RemoveAll(morcego => morcego == null);

        if (morcegosCriados.Count == 0)
        {
            MostrarMissionComplete();
        }
    }

    private void CriarMorcego(Vector2 posicao)
    {
        GameObject novoMorcego = Instantiate(morcegoPrefab, posicao, Quaternion.identity);
        novoMorcego.name = "Morcego_Inimigo";
        morcegosCriados.Add(novoMorcego);
    }

    private void MostrarMissionComplete()
    {
        missaoCompleta = true;

        Debug.Log("[MorcegoSpawner] Mission Complete!");

        if (painelMissionComplete != null)
        {
            painelMissionComplete.SetActive(true);
            ConfigurarPainelMissionCompleteExistente();
            mensagemMissionCompleteRect = painelMissionComplete.GetComponent<RectTransform>();
        }
        else
        {
            mensagemMissionCompleteRect = CriarMensagemMissionComplete();
        }

        StartCoroutine(SubirMensagemMissionComplete());

        if (pausarJogoAoCompletar)
            Time.timeScale = 0f;
    }

    private void ConfigurarPainelMissionCompleteExistente()
    {
        TMP_Text texto = painelMissionComplete.GetComponentInChildren<TMP_Text>(true);

        if (texto == null)
            return;

        texto.text = textoMissionComplete;
        texto.alignment = TextAlignmentOptions.Center;
    }

    private RectTransform CriarMensagemMissionComplete()
    {
        GameObject canvasObj = new GameObject(
            "MissionCompleteCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 120;

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject textoObj = new GameObject(
            "MissionOneComplete",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI)
        );

        textoObj.transform.SetParent(canvasObj.transform, false);

        RectTransform rect = textoObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(900f, 240f);
        rect.anchoredPosition = posicaoInicialMensagem;

        TMP_Text texto = textoObj.GetComponent<TMP_Text>();
        texto.text = textoMissionComplete;
        texto.fontSize = 86f;
        texto.fontStyle = FontStyles.Bold;
        texto.alignment = TextAlignmentOptions.Center;
        texto.color = new Color(1f, 0.95f, 0.35f, 1f);
        texto.raycastTarget = false;

        return rect;
    }

    private IEnumerator SubirMensagemMissionComplete()
    {
        if (mensagemMissionCompleteRect == null)
            yield break;

        float tempo = 0f;
        mensagemMissionCompleteRect.anchoredPosition = posicaoInicialMensagem;

        while (tempo < duracaoAnimacaoMensagem)
        {
            tempo += Time.unscaledDeltaTime;
            float progresso = Mathf.Clamp01(tempo / duracaoAnimacaoMensagem);
            float suavizado = Mathf.SmoothStep(0f, 1f, progresso);

            mensagemMissionCompleteRect.anchoredPosition = Vector2.Lerp(
                posicaoInicialMensagem,
                posicaoFinalMensagem,
                suavizado
            );

            yield return null;
        }

        mensagemMissionCompleteRect.anchoredPosition = posicaoFinalMensagem;
    }
}

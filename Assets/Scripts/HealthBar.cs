using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    private RectTransform fillRect;
    private TMP_Text countText;

    private static readonly Color PanelColor  = new Color(0.05f, 0.04f, 0.08f, 0.90f);
    private static readonly Color BorderColor = new Color(0.60f, 0.30f, 0.85f, 0.75f);
    private static readonly Color BarBgColor  = new Color(0.12f, 0.04f, 0.04f, 1f);
    private static readonly Color BarColor    = new Color(0.85f, 0.12f, 0.12f, 1f);

    private GameObject root;
    private PlayerCombat playerCombat;
    private int lastHealth = -1;

    private void Awake() => BuildUI();

    private void Update()
    {
        // Encontra o PlayerCombat do jogador atual quando disponível
        if (playerCombat == null && GameManager.Instance?.JogadorAtual != null)
            playerCombat = GameManager.Instance.JogadorAtual.GetComponent<PlayerCombat>();

        if (playerCombat != null && playerCombat.CurrentHealth != lastHealth)
        {
            lastHealth = playerCombat.CurrentHealth;
            UpdateHealth(playerCombat.CurrentHealth, playerCombat.MaxHealth);
        }

        // Esconde quando qualquer menu estiver aberto
        if (root == null) return;
        bool menuAberto = MenuController.Instance != null &&
                          (!MenuController.Instance.JogoIniciado ||
                           MenuController.Instance.JogoPausado  ||
                           MenuController.Instance.JogoTerminou);
        root.SetActive(!menuAberto);
    }

    public void UpdateHealth(int current, int max)
    {
        if (fillRect == null || max <= 0) return;

        // Ajusta anchorMax.x de 0 a 1 para controlar a largura da barra
        fillRect.anchorMax = new Vector2((float)current / max, 1f);

        if (countText != null)
            countText.text = $"{current}/{max}";
    }

    private void BuildUI()
    {
        GameObject canvasGO = new GameObject("[HUD] HealthBar");
        DontDestroyOnLoad(canvasGO);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Outer container = purple border, anchored bottom-left
        var outer = MakeRect(canvasGO.transform, "HealthBarOuter");
        root = outer.gameObject;
        SetAnchors(outer, Vector2.zero, Vector2.zero, Vector2.zero,
            new Vector2(230, 48), new Vector2(16, 16));
        MakeImage(outer, BorderColor);

        // Inner panel = child of outer, 3px inset → border is always perfectly aligned
        var inner = MakeRect(outer, "HealthBarInner");
        inner.anchorMin = Vector2.zero;
        inner.anchorMax = Vector2.one;
        inner.offsetMin = new Vector2(3, 3);
        inner.offsetMax = new Vector2(-3, -3);
        MakeImage(inner, PanelColor);

        // Bar background — fills the inner panel
        var barBg = MakeRect(inner, "BarBg");
        barBg.anchorMin = new Vector2(0, 0);
        barBg.anchorMax = new Vector2(1, 1);
        barBg.offsetMin = new Vector2(8, 8);
        barBg.offsetMax = new Vector2(-8, -8);
        MakeImage(barBg, BarBgColor);

        // Fill — âncora esquerda fixa (0,0), direita (1,1) começa cheia
        // anchorMax.x será ajustado em UpdateHealth para controlar a largura
        var fill = MakeRect(barBg, "Fill");
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = Vector2.one;
        fill.offsetMin = Vector2.zero;
        fill.offsetMax = Vector2.zero;
        MakeImage(fill, BarColor);
        fillRect = fill;

        // Count text drawn on top of the bar
        var count = MakeRect(barBg, "Count");
        count.anchorMin = Vector2.zero;
        count.anchorMax = Vector2.one;
        count.offsetMin = Vector2.zero;
        count.offsetMax = Vector2.zero;
        countText = count.gameObject.AddComponent<TextMeshProUGUI>();
        countText.text = "5/5";
        countText.fontSize = 16f;
        countText.fontStyle = FontStyles.Bold;
        countText.color = Color.white;
        countText.outlineWidth = 0.3f;
        countText.outlineColor = new Color32(0, 0, 0, 220);
        countText.alignment = TextAlignmentOptions.Center;
        countText.enableWordWrapping = false;
        countText.raycastTarget = false;
    }

    private static RectTransform MakeRect(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.AddComponent<RectTransform>();
    }

    private static RectTransform MakeRect(RectTransform parent, string name)
        => MakeRect(parent.transform, name);

    private static void SetAnchors(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPos)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.sizeDelta = sizeDelta;
        rt.anchoredPosition = anchoredPos;
    }

    private static void MakeImage(RectTransform rt, Color color)
    {
        var img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
    }
}

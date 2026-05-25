using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ShadowRunnerMenuSetup
{
    private const string MenuName = "MenuInicial";
    private const string SetupMarkerName = "ShadowRunnerMenuSetupDone";
    private const string MenuBackgroundResource = "back_menu";
    private const string GameFontResource = "Fonts/KiwiSoda SDF";
    private const string MenuButtonTextStyleName = "C1";

    private static readonly Vector2 MenuButtonSize = new Vector2(304f, 48f);
    private static readonly Vector2 MenuButtonStart = new Vector2(0f, 4f);
    private const float MenuButtonSpacing = 64f;
    private const float MenuButtonBorderSize = 2f;
    private const float MenuButtonGlowSize = 5f;
    private const float MenuButtonFontSize = 22f;
    private const float MenuButtonTracking = 4f;

    private static readonly Color DeepBlack = new Color32(0x0A, 0x0A, 0x0F, 210);
    private static readonly Color NeonBlue = new Color32(0x4A, 0x90, 0xD9, 255);
    private static readonly Color NeonBlueGlow = new Color32(0x4A, 0x90, 0xD9, 90);
    private static readonly Color CreamWhite = new Color32(0xF0, 0xED, 0xE8, 255);

    private struct MenuButtonFrame
    {
        public Image[] BorderLines;
        public Image[] GlowLines;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnSceneLoaded()
    {
        SceneManager.sceneLoaded += (_, __) => BuildMenu();
        BuildMenu();
    }

    private static void BuildMenu()
    {
        GameObject menu = GameObject.Find(MenuName);

        if (menu == null || menu.transform.Find(SetupMarkerName) != null)
            return;

        ApplyGameFontToAllTexts();
        StyleExistingSceneButtons();
        CreateMarker(menu.transform);
        StyleMenuBackground(menu);

        RectTransform menuRect = menu.GetComponent<RectTransform>();
        CreateTitle(menuRect);
        CreateSubtitle(menuRect);

        Button newGameButton = menu.GetComponentInChildren<Button>(true);

        if (newGameButton != null)
        {
            StyleButton(newGameButton, "NEW GAME", MenuButtonStart, true);
            ConfigureHudVisibility(menu, newGameButton);
        }

        GameObject helpPanel = CreateHelpPanel(menuRect);
        CreateButton(menuRect, "HELP", MenuButtonStart + Vector2.down * MenuButtonSpacing, () => ShowHelpPanel(menuRect, helpPanel));
    }

    private static void CreateMarker(Transform parent)
    {
        GameObject marker = new GameObject(SetupMarkerName);
        marker.transform.SetParent(parent, false);
        marker.hideFlags = HideFlags.HideInHierarchy;
    }

    private static void StyleMenuBackground(GameObject menu)
    {
        Image menuImage = menu.GetComponent<Image>();

        if (menuImage != null)
            menuImage.color = new Color(0.015f, 0.018f, 0.024f, 0.35f);

        Texture2D backgroundTexture = Resources.Load<Texture2D>(MenuBackgroundResource);

        if (backgroundTexture != null)
        {
            GameObject background = new GameObject("ShadowRunnerBackgroundImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            background.transform.SetParent(menu.transform, false);
            background.transform.SetAsFirstSibling();

            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            RawImage rawImage = background.GetComponent<RawImage>();
            rawImage.texture = backgroundTexture;
            rawImage.color = Color.white;
            rawImage.raycastTarget = false;
        }

        GameObject backdrop = new GameObject("ShadowRunnerBackdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        backdrop.transform.SetParent(menu.transform, false);
        backdrop.transform.SetSiblingIndex(backgroundTexture != null ? 1 : 0);

        RectTransform rect = backdrop.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = backdrop.GetComponent<Image>();
        image.color = new Color(0.01f, 0.015f, 0.02f, backgroundTexture != null ? 0.38f : 0.78f);
        image.raycastTarget = false;
    }

    private static void CreateTitle(RectTransform parent)
    {
        TMP_Text title = CreateText(parent, "SHADOW RUNNER", new Vector2(0f, 152f), new Vector2(760f, 86f));
        title.fontSize = 56f;
        title.fontStyle = FontStyles.Bold;
        title.characterSpacing = 6f;
        title.color = new Color(0.83f, 0.88f, 0.9f, 1f);
    }

    private static void CreateSubtitle(RectTransform parent)
    {
        TMP_Text subtitle = CreateText(parent, "corra entre ruinas, sobreviva a noite", new Vector2(0f, 96f), new Vector2(620f, 36f));
        subtitle.fontSize = 21f;
        subtitle.fontStyle = FontStyles.Italic;
        subtitle.color = new Color(0.48f, 0.7f, 0.78f, 0.9f);
    }

    private static void ConfigureHudVisibility(GameObject menu, Button newGameButton)
    {
        ShadowRunnerHudVisibility hudVisibility = menu.AddComponent<ShadowRunnerHudVisibility>();
        hudVisibility.CacheHudObjects();
        hudVisibility.HideHud();
        newGameButton.onClick.AddListener(hudVisibility.ShowHud);
    }

    private static GameObject CreateHelpPanel(RectTransform parent)
    {
        GameObject panel = new GameObject("PainelHelp", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -8f);
        rect.sizeDelta = new Vector2(560f, 300f);

        Image image = panel.GetComponent<Image>();
        image.color = new Color(0.035f, 0.045f, 0.058f, 0.96f);

        TMP_Text header = CreateText(rect, "COMANDOS", new Vector2(0f, 106f), new Vector2(500f, 42f));
        header.fontSize = 30f;
        header.fontStyle = FontStyles.Bold;
        header.color = new Color(0.86f, 0.92f, 0.94f, 1f);

        TMP_Text commands = CreateText(rect,
            "A / D ou Setas: mover\n" +
            "Espaco: pular\n" +
            "Left Shift: acao especial\n" +
            "ESC: pausar\n" +
            "Pise nos inimigos ou use essencia para sobreviver.",
            new Vector2(0f, 18f),
            new Vector2(500f, 150f));
        commands.fontSize = 22f;
        commands.alignment = TextAlignmentOptions.Left;
        commands.color = new Color(0.73f, 0.82f, 0.84f, 1f);

        CreateButton(rect, "VOLTAR", new Vector2(0f, -112f), () => HideHelpPanel(parent, panel));

        panel.SetActive(false);
        return panel;
    }

    private static void ShowHelpPanel(RectTransform menuRect, GameObject helpPanel)
    {
        SetMainMenuButtonsVisible(menuRect, helpPanel.transform, false);
        helpPanel.SetActive(true);
        helpPanel.transform.SetAsLastSibling();
    }

    private static void HideHelpPanel(RectTransform menuRect, GameObject helpPanel)
    {
        helpPanel.SetActive(false);
        SetMainMenuButtonsVisible(menuRect, helpPanel.transform, true);
    }

    private static void SetMainMenuButtonsVisible(RectTransform menuRect, Transform helpPanel, bool visible)
    {
        Button[] buttons = menuRect.GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            if (button.transform.IsChildOf(helpPanel))
                continue;

            button.gameObject.SetActive(visible);
        }
    }

    private static Button CreateButton(RectTransform parent, string label, Vector2 position, UnityAction action)
    {
        GameObject buttonObject = new GameObject("Botao" + label.Replace(" ", ""), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        Button button = buttonObject.GetComponent<Button>();
        StyleButton(button, label, position, true);
        button.onClick.AddListener(action);
        return button;
    }

    private static void StyleExistingSceneButtons()
    {
        Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Button button in buttons)
            StyleButton(button, null, Vector2.zero, false);
    }

    private static void StyleButton(Button button, string label, Vector2 position, bool reposition)
    {
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);

        if (reposition)
            rect.anchoredPosition = position;

        rect.sizeDelta = MenuButtonSize;

        Image image = button.GetComponent<Image>();

        if (image != null)
        {
            image.color = DeepBlack;
            image.raycastTarget = true;
        }

        MenuButtonFrame frame = ApplyButtonFrame(button.transform);
        button.transition = Selectable.Transition.None;

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.white;
        colors.pressedColor = Color.white;
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.04f, 0.04f, 0.04f, 0.35f);
        colors.fadeDuration = 0f;
        button.colors = colors;

        TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);

        if (text == null)
        {
            text = CreateText(rect, label, Vector2.zero, Vector2.zero);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
        }

        if (!string.IsNullOrEmpty(label))
            text.text = label;

        text.fontSize = MenuButtonFontSize;
        text.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
        text.characterSpacing = MenuButtonTracking;
        text.alignment = TextAlignmentOptions.Center;
        text.color = CreamWhite;
        text.enableAutoSizing = false;
        text.raycastTarget = false;
        ApplyGameFont(text);
        ApplyButtonTextStyle(text);

        ShadowRunnerMenuButton buttonFx = button.gameObject.GetComponent<ShadowRunnerMenuButton>();

        if (buttonFx == null)
            buttonFx = button.gameObject.AddComponent<ShadowRunnerMenuButton>();

        buttonFx.Setup(text, image, frame.BorderLines, frame.GlowLines);
    }

    private static void ApplyGameFont(TMP_Text text)
    {
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>(GameFontResource);

        if (font != null)
            text.font = font;
    }

    private static void ApplyGameFontToAllTexts()
    {
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>(GameFontResource);

        if (font == null)
            return;

        TMP_Text[] texts = Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (TMP_Text text in texts)
            text.font = font;
    }

    private static void ApplyButtonTextStyle(TMP_Text text)
    {
        TMP_Style style = TMP_Settings.defaultStyleSheet != null
            ? TMP_Settings.defaultStyleSheet.GetStyle(MenuButtonTextStyleName)
            : null;

        if (style != null)
            text.textStyle = style;

        text.overrideColorTags = false;
    }

    private static MenuButtonFrame ApplyButtonFrame(Transform buttonTransform)
    {
        Transform oldFrame = buttonTransform.Find("PixelBorder");

        if (oldFrame != null)
            Object.Destroy(oldFrame.gameObject);

        GameObject border = new GameObject("PixelBorder", typeof(RectTransform));
        border.transform.SetParent(buttonTransform, false);
        border.transform.SetAsFirstSibling();

        RectTransform borderRect = border.GetComponent<RectTransform>();
        borderRect.anchorMin = Vector2.zero;
        borderRect.anchorMax = Vector2.one;
        borderRect.offsetMin = Vector2.zero;
        borderRect.offsetMax = Vector2.zero;

        Image[] glowLines =
        {
            CreateBorderLine(border.transform, "GlowTop", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(-MenuButtonGlowSize, -MenuButtonGlowSize), new Vector2(MenuButtonGlowSize, 0f), NeonBlueGlow),
            CreateBorderLine(border.transform, "GlowBottom", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(-MenuButtonGlowSize, 0f), new Vector2(MenuButtonGlowSize, MenuButtonGlowSize), NeonBlueGlow),
            CreateBorderLine(border.transform, "GlowLeft", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(-MenuButtonGlowSize, -MenuButtonGlowSize), new Vector2(0f, MenuButtonGlowSize), NeonBlueGlow),
            CreateBorderLine(border.transform, "GlowRight", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, -MenuButtonGlowSize), new Vector2(MenuButtonGlowSize, MenuButtonGlowSize), NeonBlueGlow)
        };

        Image[] borderLines =
        {
            CreateBorderLine(border.transform, "Top", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -MenuButtonBorderSize), new Vector2(0f, 0f), NeonBlue),
            CreateBorderLine(border.transform, "Bottom", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, MenuButtonBorderSize), NeonBlue),
            CreateBorderLine(border.transform, "Left", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(MenuButtonBorderSize, 0f), NeonBlue),
            CreateBorderLine(border.transform, "Right", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-MenuButtonBorderSize, 0f), new Vector2(0f, 0f), NeonBlue)
        };

        return new MenuButtonFrame
        {
            BorderLines = borderLines,
            GlowLines = glowLines
        };
    }

    private static Image CreateBorderLine(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        GameObject line = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        line.transform.SetParent(parent, false);

        RectTransform rect = line.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        Image image = line.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TMP_Text CreateText(RectTransform parent, string content, Vector2 position, Vector2 size)
    {
        GameObject textObject = new GameObject("Texto" + content.Replace(" ", ""), typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.text = content;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        ApplyGameFont(text);
        return text;
    }
}

public class ShadowRunnerHudVisibility : MonoBehaviour
{
    private GameObject[] hudObjects;

    private readonly string[] hudObjectNames =
    {
        "TextoPontuacao",
        "TextoEssencia",
        "TextoOndas"
    };

    public void CacheHudObjects()
    {
        hudObjects = new GameObject[hudObjectNames.Length];

        for (int i = 0; i < hudObjectNames.Length; i++)
            hudObjects[i] = GameObject.Find(hudObjectNames[i]);
    }

    public void HideHud()
    {
        SetHudVisible(false);
    }

    public void ShowHud()
    {
        SetHudVisible(true);
    }

    private void SetHudVisible(bool visible)
    {
        if (hudObjects == null)
            CacheHudObjects();

        foreach (GameObject hudObject in hudObjects)
        {
            if (hudObject != null)
                hudObject.SetActive(visible);
        }
    }
}

public class ShadowRunnerMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    private static readonly Color NormalBackground = new Color32(0x0A, 0x0A, 0x0F, 210);
    private static readonly Color HoverBackground = new Color32(0x2D, 0x1B, 0x4E, 235);
    private static readonly Color PressedBackground = new Color32(0x12, 0x10, 0x1D, 245);
    private static readonly Color NeonBlue = new Color32(0x4A, 0x90, 0xD9, 255);
    private static readonly Color NeonBlueGlow = new Color32(0x4A, 0x90, 0xD9, 120);
    private static readonly Color VibrantPurple = new Color32(0x7B, 0x4F, 0xBF, 255);
    private static readonly Color VibrantPurpleGlow = new Color32(0x7B, 0x4F, 0xBF, 135);
    private static readonly Color CreamWhite = new Color32(0xF0, 0xED, 0xE8, 255);

    private const float HoverScale = 1.025f;
    private const float PressedScale = 0.985f;
    private const float TransitionSpeed = 14f;

    private TMP_Text label;
    private Image background;
    private Image[] borderLines;
    private Image[] glowLines;
    private Vector3 baseScale;
    private Color targetBackground;
    private Color targetText;
    private Color targetBorder;
    private Color targetGlow;
    private Vector3 targetScale;
    private bool pointerInside;
    private bool selected;

    public void Setup(TMP_Text labelText, Image backgroundImage, Image[] borders, Image[] glows)
    {
        label = labelText;
        background = backgroundImage;
        borderLines = borders;
        glowLines = glows;
        baseScale = transform.localScale;
        SetNormalState(true);
    }

    private void Update()
    {
        if (background != null)
            background.color = Color.Lerp(background.color, targetBackground, Time.unscaledDeltaTime * TransitionSpeed);

        if (label != null)
            label.color = Color.Lerp(label.color, targetText, Time.unscaledDeltaTime * TransitionSpeed);

        ApplyLineColor(borderLines, targetBorder);
        ApplyLineColor(glowLines, targetGlow);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * TransitionSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;
        SetHoverState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;

        if (selected)
            SetFocusedState();
        else
            SetNormalState(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SetPressedState();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (pointerInside)
            SetHoverState();
        else if (selected)
            SetFocusedState();
        else
            SetNormalState(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        selected = true;

        if (!pointerInside)
            SetFocusedState();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selected = false;

        if (!pointerInside)
            SetNormalState(false);
    }

    private void SetNormalState(bool immediate)
    {
        targetBackground = NormalBackground;
        targetText = CreamWhite;
        targetBorder = NeonBlue;
        targetGlow = new Color(NeonBlueGlow.r, NeonBlueGlow.g, NeonBlueGlow.b, 0.28f);
        targetScale = baseScale == Vector3.zero ? Vector3.one : baseScale;

        if (immediate)
            ApplyTargetsImmediately();
    }

    private void SetHoverState()
    {
        targetBackground = HoverBackground;
        targetText = CreamWhite;
        targetBorder = VibrantPurple;
        targetGlow = VibrantPurpleGlow;
        targetScale = baseScale * HoverScale;
    }

    private void SetPressedState()
    {
        targetBackground = PressedBackground;
        targetText = NeonBlue;
        targetBorder = NeonBlue;
        targetGlow = new Color(NeonBlueGlow.r, NeonBlueGlow.g, NeonBlueGlow.b, 0.42f);
        targetScale = baseScale * PressedScale;
    }

    private void SetFocusedState()
    {
        targetBackground = NormalBackground;
        targetText = CreamWhite;
        targetBorder = NeonBlue;
        targetGlow = NeonBlueGlow;
        targetScale = baseScale == Vector3.zero ? Vector3.one : baseScale;
    }

    private void ApplyTargetsImmediately()
    {
        if (background != null)
            background.color = targetBackground;

        if (label != null)
            label.color = targetText;

        SetLineColor(borderLines, targetBorder);
        SetLineColor(glowLines, targetGlow);
        transform.localScale = targetScale;
    }

    private void ApplyLineColor(Image[] lines, Color color)
    {
        if (lines == null)
            return;

        foreach (Image line in lines)
        {
            if (line != null)
                line.color = Color.Lerp(line.color, color, Time.unscaledDeltaTime * TransitionSpeed);
        }
    }

    private void SetLineColor(Image[] lines, Color color)
    {
        if (lines == null)
            return;

        foreach (Image line in lines)
        {
            if (line != null)
                line.color = color;
        }
    }
}

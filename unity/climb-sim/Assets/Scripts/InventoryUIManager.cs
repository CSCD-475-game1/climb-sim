using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;

    [Header("Inventory Layout")]
    [SerializeField] private int totalSlotCount = 16;
    [SerializeField] private int hotbarSlotCount = 4;
    [SerializeField] private int inventoryColumns = 4;

    [Header("Debug")]
    [SerializeField] private KeyCode debugFillKey = KeyCode.LeftBracket;
    [SerializeField] private KeyCode debugClearKey = KeyCode.RightBracket;
    [SerializeField] private ItemDefinition[] allItems = { };

    [Header("Visuals")]
    [SerializeField] private Vector2 hotbarSlotSize = new Vector2(100f, 100f);
    [SerializeField] private Vector2 inventorySlotSize = new Vector2(100f, 100f);
    [SerializeField] private Color panelColor = new Color(0f, 0f, 0f, 0.40f);
    [SerializeField] private Color slotColor = new Color(1f, 1f, 1f, 0.10f);
    [SerializeField] private Color slotBorderColor = new Color(1f, 1f, 1f, 0.18f);
    [SerializeField] private Color selectedSlotColor = new Color(1f, 1f, 1f, 0.22f);
    [SerializeField] private Color selectedBorderColor = new Color(1f, 1f, 1f, 0.70f);
    [SerializeField] private Color textColor = new Color(1f, 1f, 1f, 0.85f);

    private readonly List<SlotVisual> _hotbarViews = new();
    private readonly List<SlotVisual> _inventoryViews = new();

    private Canvas _canvas;
    private RectTransform _canvas2;
    private GameObject _inventoryPanel;
    private bool _inventoryOpen;
    private Font _uiFont;
    private CursorLockMode _previousLockState;
    private bool _previousCursorVisible;
    private bool _hasStoredCursorState;
    private GameObject _itemTooltip;
    private RectTransform _tooltipBackgrd;
    private Text _tooltipTitle;
    private Text _tooltipDesc;

    private int _dragSource = -1;
    private GameObject _dragGhost;

    private const char xMark = 'x';

    private void Awake()
    {
        ApplyConstraints();
        BuildUI();
        SetInventoryOpen(false);
        RefreshAllViews();
    }

    private void Update()
    {
        if (_itemTooltip != null && _itemTooltip.activeSelf)
            UpdateTooltipPosition();

        if (GameManager.Instance != null && !GameManager.Instance.IsGameplayMode)
            return;

        if (Input.GetKeyDown(debugFillKey)) DebugFillInventory();
        if (Input.GetKeyDown(debugClearKey)) DebugClearInventory();
    }

    public void DebugFillInventory()
    {
        if (playerInventory == null) return;
        foreach (ItemDefinition item in allItems)
            playerInventory.AddItem(item);
    }

    public void DebugClearInventory()
    {
        if (playerInventory == null) return;
        playerInventory.ClearAll();
    }

    private void ApplyConstraints()
    {
        if (hotbarSlotCount < 1) hotbarSlotCount = 1;
        if (totalSlotCount < hotbarSlotCount) totalSlotCount = hotbarSlotCount;
        if (inventoryColumns < 1) inventoryColumns = 1;
    }

    private void BuildUI()
    {
        _uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (FindObjectOfType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        GameObject canvasObject = new GameObject("InventoryCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        _canvas = canvasObject.GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100;

        _canvas2 = canvasObject.GetComponent<RectTransform>();

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        CreateHotbar(canvasObject.transform);
        CreateInventoryPanel(canvasObject.transform);
        CreateTooltip(canvasObject.transform);
    }

    private void CreateHotbar(Transform parent)
    {
        GameObject hotbarRoot = CreateUIObject("Hotbar", parent);
        RectTransform hotbarRect = hotbarRoot.AddComponent<RectTransform>();
        hotbarRect.anchorMin = new Vector2(0.5f, 0f);
        hotbarRect.anchorMax = new Vector2(0.5f, 0f);
        hotbarRect.pivot = new Vector2(0.5f, 0.5f);
        hotbarRect.anchoredPosition = new Vector2(0f, 70f);
        hotbarRect.sizeDelta = new Vector2((hotbarSlotSize.x * hotbarSlotCount) + (12f * (hotbarSlotCount - 1)), hotbarSlotSize.y);

        HorizontalLayoutGroup layout = hotbarRoot.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.spacing = 12f;
        layout.padding = new RectOffset(0, 0, 0, 0);

        for (int i = 0; i < hotbarSlotCount; i++)
        {
            SlotVisual slot = CreateSlot(hotbarRoot.transform, i, hotbarSlotSize, true);
            _hotbarViews.Add(slot);
        }
    }

    private void CreateInventoryPanel(Transform parent)
    {
        _inventoryPanel = CreateUIObject("InventoryPanel", parent);
        RectTransform panelRect = _inventoryPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(466f, 360f);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = _inventoryPanel.AddComponent<Image>();
        panelImage.color = panelColor;
        panelImage.raycastTarget = false;

        Outline panelOutline = _inventoryPanel.AddComponent<Outline>();
        panelOutline.effectColor = new Color(1f, 1f, 1f, 0.12f);
        panelOutline.effectDistance = new Vector2(1f, -1f);

        VerticalLayoutGroup layout = _inventoryPanel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(18, 18, 18, 18);
        layout.spacing = 16f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlHeight = false;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        ContentSizeFitter fitter = _inventoryPanel.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        Text title = CreateText("Title", _inventoryPanel.transform, "Inventory", 30, FontStyle.Bold, TextAnchor.MiddleCenter);
        title.color = textColor;
        LayoutElement titleLayout = title.gameObject.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 30f;

        GameObject gridObject = CreateUIObject("InventoryGrid", _inventoryPanel.transform);
        RectTransform gridRect = gridObject.AddComponent<RectTransform>();
        gridRect.sizeDelta = new Vector2(0f, 0f);

        GridLayoutGroup grid = gridObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = inventorySlotSize;
        grid.spacing = new Vector2(10f, 10f);
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = inventoryColumns;

        ContentSizeFitter gridFitter = gridObject.AddComponent<ContentSizeFitter>();
        gridFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        gridFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        for (int i = 0; i < totalSlotCount; i++)
        {
            SlotVisual slot = CreateSlot(gridObject.transform, i, inventorySlotSize, false);
            _inventoryViews.Add(slot);
        }
    }

    private void CreateTooltip(Transform parent)
    {
        _itemTooltip = CreateUIObject("ItemTooltip", parent);
        _tooltipBackgrd = _itemTooltip.AddComponent<RectTransform>();
        _tooltipBackgrd.anchorMin = new Vector2(0.5f, 0.5f);
        _tooltipBackgrd.anchorMax = new Vector2(0.5f, 0.5f);
        _tooltipBackgrd.pivot = new Vector2(0f, 1f);
        _tooltipBackgrd.sizeDelta = new Vector2(220f, 0f);

        Image bg = _itemTooltip.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.1f, 0.93f);
        bg.raycastTarget = false;

        Outline outline = _itemTooltip.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.30f);
        outline.effectDistance = new Vector2(1f, -1f);

        VerticalLayoutGroup layout = _itemTooltip.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 8, 8);
        layout.spacing = 3f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        ContentSizeFitter fitter = _itemTooltip.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _tooltipTitle = CreateText("TooltipName", _itemTooltip.transform, "", 14, FontStyle.Bold, TextAnchor.UpperLeft);
        _tooltipTitle.color = Color.white;

        _tooltipDesc = CreateText("TooltipDesc", _itemTooltip.transform, "", 12, FontStyle.Normal, TextAnchor.UpperLeft);
        _tooltipDesc.color = new Color(0.78f, 0.78f, 0.78f, 1f);
        _tooltipDesc.horizontalOverflow = HorizontalWrapMode.Wrap;
        _tooltipDesc.verticalOverflow = VerticalWrapMode.Overflow;

        _itemTooltip.SetActive(false);
    }

    private SlotVisual CreateSlot(Transform parent, int slotIndex, Vector2 size, bool isHotbarVisual)
    {
        GameObject slotObject = CreateUIObject(isHotbarVisual ? $"HotbarSlot_{slotIndex}" : $"InventorySlot_{slotIndex}", parent);
        RectTransform slotRect = slotObject.AddComponent<RectTransform>();
        slotRect.sizeDelta = size;

        Image background = slotObject.AddComponent<Image>();
        background.color = slotColor;
        background.raycastTarget = true;

        Outline outline = slotObject.AddComponent<Outline>();
        outline.effectColor = slotBorderColor;
        outline.effectDistance = new Vector2(1f, -1f);

        InventorySlotInteractor interactor = slotObject.AddComponent<InventorySlotInteractor>();
        interactor.SlotIndex = slotIndex;
        interactor.Manager = this;

        GameObject iconObject = CreateUIObject("Icon", slotObject.transform);
        RectTransform iconRect = iconObject.AddComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.offsetMin = new Vector2(8f, 8f);
        iconRect.offsetMax = new Vector2(-8f, -8f);
        Image iconImage = iconObject.AddComponent<Image>();
        iconImage.raycastTarget = false;
        iconImage.preserveAspect = true;
        iconImage.enabled = false;

        Text keyLabel = CreateText("KeyLabel", slotObject.transform, GetKeyLabel(slotIndex), 16, FontStyle.Bold, TextAnchor.UpperLeft);
        RectTransform keyRect = keyLabel.rectTransform;
        keyRect.anchorMin = new Vector2(0f, 1f);
        keyRect.anchorMax = new Vector2(0f, 1f);
        keyRect.pivot = new Vector2(0f, 1f);
        keyRect.anchoredPosition = new Vector2(8f, -6f);
        keyRect.sizeDelta = new Vector2(30f, 20f);
        keyLabel.color = new Color(textColor.r, textColor.g, textColor.b, 0.95f);

        Text itemLabel = CreateText("ItemLabel", slotObject.transform, string.Empty, 13, FontStyle.Bold, TextAnchor.LowerRight);
        RectTransform itemRect = itemLabel.rectTransform;
        itemRect.anchorMin = new Vector2(1f, 0f);
        itemRect.anchorMax = new Vector2(1f, 0f);
        itemRect.pivot = new Vector2(1f, 0f);
        itemRect.anchoredPosition = new Vector2(-5f, 5f);
        itemRect.sizeDelta = new Vector2(50f, 18f);
        itemLabel.color = new Color(1f, 0.95f, 0.2f, 1f);

        Outline textOutline = itemLabel.gameObject.AddComponent<Outline>();
        textOutline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        textOutline.effectDistance = new Vector2(1f, -1f);

        return new SlotVisual
        {
            slotIndex = slotIndex,
            background = background,
            icon = iconImage,
            outline = outline,
            keyLabel = keyLabel,
            itemLabel = itemLabel,
            isHotbarVisual = isHotbarVisual
        };
    }

    // Item tooltip

    public void OnSlotHoverEnter(int slotIndex)
    {
        if (!_inventoryOpen || _dragSource >= 0) return;
        if (playerInventory == null) return;

        PlayerInventory.InventorySlotData data = playerInventory.GetSlotData(slotIndex);
        if (data == null || data.item == null) return;

        _tooltipTitle.text = data.item.displayName;
        _tooltipDesc.text = data.item.description;
        _itemTooltip.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipBackgrd);
        UpdateTooltipPosition();
    }

    public void OnSlotHoverExit()
    {
        if (_itemTooltip != null)
            _itemTooltip.SetActive(false);
    }

    private void UpdateTooltipPosition()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas2, Input.mousePosition, null, out Vector2 localPos);

        localPos += new Vector2(15f, -15f);

        Rect canvas = _canvas2.rect;
        Vector2 size = _tooltipBackgrd.sizeDelta;
        localPos.x = Mathf.Clamp(localPos.x, canvas.xMin, canvas.xMax - size.x);
        localPos.y = Mathf.Clamp(localPos.y, canvas.yMin + size.y, canvas.yMax);

        _tooltipBackgrd.anchoredPosition = localPos;
    }

    // Item DragDrop

    public void OnSlotBeginDrag(int slotIndex, PointerEventData eventData)
    {
        if (!_inventoryOpen || playerInventory == null) return;

        PlayerInventory.InventorySlotData data = playerInventory.GetSlotData(slotIndex);
        if (data == null || data.item == null) return;

        _dragSource = slotIndex;
        OnSlotHoverExit();

        _dragGhost = CreateUIObject("DragGhost", _canvas2);
        RectTransform ghostRect = _dragGhost.AddComponent<RectTransform>();
        ghostRect.anchorMin = new Vector2(0.5f, 0.5f);
        ghostRect.anchorMax = new Vector2(0.5f, 0.5f);
        ghostRect.pivot = new Vector2(0.5f, 0.5f);
        ghostRect.sizeDelta = new Vector2(80f, 80f);

        Image ghostImage = _dragGhost.AddComponent<Image>();
        ghostImage.raycastTarget = false;
        ghostImage.preserveAspect = true;
        ghostImage.color = new Color(1f, 1f, 1f, 0.8f);

        if (data.item.icon != null)
            ghostImage.sprite = data.item.icon;

        UpdateDragGhostPosition(eventData.position, eventData.pressEventCamera);
    }

    public void OnSlotDrag(PointerEventData eventData)
    {
        if (_dragGhost == null) return;
        UpdateDragGhostPosition(eventData.position, eventData.pressEventCamera);
    }

    public void OnSlotEndDrag()
    {
        if (_dragGhost != null)
        {
            Destroy(_dragGhost);
            _dragGhost = null;
        }
        _dragSource = -1;
    }

    public void OnSlotDrop(int targetIndex)
    {
        if (_dragSource < 0 || _dragSource == targetIndex) return;
        playerInventory.SwapSlots(_dragSource, targetIndex);
    }

    private void UpdateDragGhostPosition(Vector2 screenPos, Camera cam)
    {
        if (_dragGhost == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas2, screenPos, cam, out Vector2 localPos);
        _dragGhost.GetComponent<RectTransform>().anchoredPosition = localPos;
    }

    private Text CreateText(string name, Transform parent, string value, int fontSize, FontStyle fontStyle, TextAnchor alignment)
    {
        GameObject textObject = CreateUIObject(name, parent);
        Text text = textObject.AddComponent<Text>();
        text.font = _uiFont;
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = textColor;
        text.raycastTarget = false;

        RectTransform rect = text.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;

        return text;
    }

    private GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent, false);

        int uiLayer = LayerMask.NameToLayer("UI");
        gameObject.layer = uiLayer >= 0 ? uiLayer : 0;

        return gameObject;
    }

    private void SetInventoryOpen(bool open)
    {
        if (!open)
        {
            if (_dragGhost != null)
            {
                Destroy(_dragGhost);
                _dragGhost = null;
                _dragSource = -1;
            }
            if (_itemTooltip != null)
                _itemTooltip.SetActive(false);
        }

        _inventoryOpen = open;

        if (_inventoryPanel != null)
            _inventoryPanel.SetActive(open);

        if (open)
        {
            _previousLockState = Cursor.lockState;
            _previousCursorVisible = Cursor.visible;
            _hasStoredCursorState = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (_hasStoredCursorState)
        {
            Cursor.lockState = _previousLockState;
            Cursor.visible = _previousCursorVisible;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void RefreshAllViews()
    {
        foreach (SlotVisual view in _hotbarViews) RefreshSlotView(view);
        foreach (SlotVisual view in _inventoryViews) RefreshSlotView(view);
    }

    private void RefreshSlotView(SlotVisual view)
    {
        if (playerInventory == null)
        {
            view.background.color = slotColor;
            view.outline.effectColor = slotBorderColor;
            view.icon.enabled = false;
            view.itemLabel.text = string.Empty;
            view.keyLabel.text = GetKeyLabel(view.slotIndex);
            return;
        }

        bool isSelected = view.slotIndex == playerInventory.GetSelectedHotbarIndex() && view.slotIndex < hotbarSlotCount;
        view.background.color = isSelected ? selectedSlotColor : slotColor;
        view.outline.effectColor = isSelected ? selectedBorderColor : slotBorderColor;
        view.keyLabel.text = GetKeyLabel(view.slotIndex);

        PlayerInventory.InventorySlotData data = playerInventory.GetSlotData(view.slotIndex);

        if (data == null || data.item == null)
        {
            view.icon.enabled = false;
            view.itemLabel.text = string.Empty;
        }
        else if (data.item.icon != null)
        {
            view.icon.sprite = data.item.icon;
            view.icon.enabled = true;
            if (data.usesRemaining > 0)
                view.itemLabel.text = $"{data.usesRemaining}";
            else if (data.amount > 1)
                view.itemLabel.text = $"{xMark}{data.amount}";
            else
                view.itemLabel.text = string.Empty;
        }
        else
        {
            view.icon.enabled = false;
            view.itemLabel.text = data.amount > 1
                ? $"{data.item.displayName}\n{xMark}{data.amount}"
                : data.item.displayName;
        }
    }

    private string GetKeyLabel(int slotIndex)
    {
        return slotIndex < hotbarSlotCount ? (slotIndex + 1).ToString() : string.Empty;
    }

    private class SlotVisual
    {
        public int slotIndex;
        public bool isHotbarVisual;
        public Image background;
        public Image icon;
        public Outline outline;
        public Text keyLabel;
        public Text itemLabel;
    }

    public void OpenInventory() => SetInventoryOpen(true);
    public void CloseInventory() => SetInventoryOpen(false);
    public bool IsOpen => _inventoryOpen;
    public void RefreshFromInventory() => RefreshAllViews();
}

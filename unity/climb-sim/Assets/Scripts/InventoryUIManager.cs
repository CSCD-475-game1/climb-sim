using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
{
    [Header("Inventory Layout")]
    [SerializeField] private KeyCode toggleKey = KeyCode.I;
    [SerializeField] private int totalSlotCount = 16;
    [SerializeField] private int hotbarSlotCount = 4;
    [SerializeField] private int inventoryColumns = 4;

    [Header("Debug")]
    [SerializeField] private KeyCode debugFillKey = KeyCode.LeftBracket;
    [SerializeField] private KeyCode debugClearKey = KeyCode.RightBracket;
    [SerializeField] private ItemDefinition[] allItems = {};

    [Header("Visuals")]
    [SerializeField] private Vector2 hotbarSlotSize = new Vector2(100f, 100f);
    [SerializeField] private Vector2 inventorySlotSize = new Vector2(100f, 100f);
    [SerializeField] private Color panelColor = new Color(0f, 0f, 0f, 0.40f);
    [SerializeField] private Color slotColor = new Color(1f, 1f, 1f, 0.10f);
    [SerializeField] private Color slotBorderColor = new Color(1f, 1f, 1f, 0.18f);
    [SerializeField] private Color selectedSlotColor = new Color(1f, 1f, 1f, 0.22f);
    [SerializeField] private Color selectedBorderColor = new Color(1f, 1f, 1f, 0.70f);
    [SerializeField] private Color textColor = new Color(1f, 1f, 1f, 0.85f);

    private readonly List<InventorySlotData> _slotData = new();
    private readonly List<SlotVisual> _hotbarViews = new();
    private readonly List<SlotVisual> _inventoryViews = new();

    private Canvas _canvas;
    private GameObject _inventoryPanel;
    private bool _inventoryOpen;
    private int _selectedHotbarIndex;
    private Font _uiFont;
    private CursorLockMode _previousLockState;
    private bool _previousCursorVisible;
    private bool _hasStoredCursorState;

    private void Awake()
    {
        ApplyConstraints();
        EnsureSlotData();
        BuildUI();
        SetInventoryOpen(false);
        RefreshAllViews();
    }

    private void Update()
    {

        if (GameManager.Instance != null && !GameManager.Instance.IsGameplayMode)
            return; 
        if (Input.GetKeyDown(toggleKey))
            SetInventoryOpen(!_inventoryOpen);

        if (_inventoryOpen && Input.GetKeyDown(KeyCode.Escape))
            SetInventoryOpen(false);

        HandleHotbarSelection();

        if (Input.GetKeyDown(debugFillKey)) DebugFillInventory();
        if (Input.GetKeyDown(debugClearKey)) DebugClearInventory();
    }

    // --- Debug ---

    private void DebugFillInventory()
    {
        foreach (ItemDefinition item in allItems)
            AddItem(item);
    }

    private void DebugClearInventory()
    {
        foreach (InventorySlotData slot in _slotData)
        {
            slot.item = null;
            slot.amount = 0;
        }
        RefreshAllViews();
    }

    // --- Public API ---

    public bool AddItem(ItemDefinition itemDef, int amount = 1)
    {
        if (itemDef == null || amount < 1) return false;

        for (int i = 0; i < _slotData.Count; i++)
        {
            if (_slotData[i].item == itemDef)
            {
                _slotData[i].amount += amount;
                RefreshAllViews();
                return true;
            }
        }

        for (int i = 0; i < _slotData.Count; i++)
        {
            if (_slotData[i].item == null)
            {
                _slotData[i].item = itemDef;
                _slotData[i].amount = amount;
                RefreshAllViews();
                return true;
            }
        }

        return false;
    }

    public bool RemoveItem(ItemDefinition itemDef, int amount = 1)
    {
        if (itemDef == null || amount < 1) return false;

        for (int i = 0; i < _slotData.Count; i++)
        {
            if (_slotData[i].item == itemDef)
            {
                _slotData[i].amount -= amount;
                if (_slotData[i].amount <= 0)
                {
                    _slotData[i].item = null;
                    _slotData[i].amount = 0;
                }
                RefreshAllViews();
                return true;
            }
        }

        return false;
    }

    public ItemDefinition GetSelectedItem() => _slotData[_selectedHotbarIndex].item;

    public ItemDefinition GetSlotItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _slotData.Count) return null;
        return _slotData[slotIndex].item;
    }

    // --- Private ---

    private void ApplyConstraints()
    {
        if (hotbarSlotCount < 1) hotbarSlotCount = 1;
        if (totalSlotCount < hotbarSlotCount) totalSlotCount = hotbarSlotCount;
        if (inventoryColumns < 1) inventoryColumns = 1;
        _selectedHotbarIndex = Mathf.Clamp(_selectedHotbarIndex, 0, hotbarSlotCount - 1);
    }

    private void EnsureSlotData()
    {
        while (_slotData.Count < totalSlotCount)
            _slotData.Add(new InventorySlotData());
        while (_slotData.Count > totalSlotCount)
            _slotData.RemoveAt(_slotData.Count - 1);
    }

    private void BuildUI()
    {
        _uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject canvasObject = new GameObject("InventoryCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        _canvas = canvasObject.GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        CreateHotbar(canvasObject.transform);
        CreateInventoryPanel(canvasObject.transform);
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
        // Width: 2*18 padding + 4*100 slots + 3*10 spacing = 466
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

    private SlotVisual CreateSlot(Transform parent, int slotIndex, Vector2 size, bool isHotbarVisual)
    {
        GameObject slotObject = CreateUIObject(isHotbarVisual ? $"HotbarSlot_{slotIndex}" : $"InventorySlot_{slotIndex}", parent);
        RectTransform slotRect = slotObject.AddComponent<RectTransform>();
        slotRect.sizeDelta = size;

        Image background = slotObject.AddComponent<Image>();
        background.color = slotColor;
        background.raycastTarget = false;

        Outline outline = slotObject.AddComponent<Outline>();
        outline.effectColor = slotBorderColor;
        outline.effectDistance = new Vector2(1f, -1f);

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

        Text itemLabel = CreateText("ItemLabel", slotObject.transform, string.Empty, 14, FontStyle.Normal, TextAnchor.MiddleCenter);
        RectTransform itemRect = itemLabel.rectTransform;
        itemRect.anchorMin = new Vector2(0.5f, 0.5f);
        itemRect.anchorMax = new Vector2(0.5f, 0.5f);
        itemRect.pivot = new Vector2(0.5f, 0.5f);
        itemRect.anchoredPosition = new Vector2(0f, 4f);
        itemRect.sizeDelta = new Vector2(size.x - 16f, size.y - 16f);
        itemLabel.color = new Color(textColor.r, textColor.g, textColor.b, 0.75f);
        itemLabel.horizontalOverflow = HorizontalWrapMode.Wrap;
        itemLabel.verticalOverflow = VerticalWrapMode.Overflow;

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

    private void HandleHotbarSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) HotbarSelect(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2) && hotbarSlotCount >= 2) HotbarSelect(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3) && hotbarSlotCount >= 3) HotbarSelect(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4) && hotbarSlotCount >= 4) HotbarSelect(3);
    }

    private void HotbarSelect(int index)
    {
        if (index < 0 || index >= hotbarSlotCount) return;
        _selectedHotbarIndex = index;
        RefreshAllViews();
    }

    private void SetInventoryOpen(bool open)
    {
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
        bool isSelected = view.slotIndex == _selectedHotbarIndex && view.slotIndex < hotbarSlotCount;
        view.background.color = isSelected ? selectedSlotColor : slotColor;
        view.outline.effectColor = isSelected ? selectedBorderColor : slotBorderColor;
        view.keyLabel.text = GetKeyLabel(view.slotIndex);

        InventorySlotData data = _slotData[view.slotIndex];

        if (data.item == null)
        {
            view.icon.enabled = false;
            view.itemLabel.text = string.Empty;
        }
        else if (data.item.icon != null)
        {
            view.icon.sprite = data.item.icon;
            view.icon.enabled = true;
            view.itemLabel.text = data.amount > 1 ? $"{xMark}{data.amount}" : string.Empty;
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

    private const char xMark = 'x';

    private class InventorySlotData
    {
        public ItemDefinition item = null;
        public int amount = 0;
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
}

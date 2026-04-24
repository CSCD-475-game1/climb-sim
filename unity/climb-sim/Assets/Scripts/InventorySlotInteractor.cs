using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotInteractor : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public int SlotIndex;
    public InventoryUIManager Manager;

    public void OnPointerEnter(PointerEventData eventData) => Manager.OnSlotHoverEnter(SlotIndex);
    public void OnPointerExit(PointerEventData eventData) => Manager.OnSlotHoverExit();
    public void OnBeginDrag(PointerEventData eventData) => Manager.OnSlotBeginDrag(SlotIndex, eventData);
    public void OnDrag(PointerEventData eventData) => Manager.OnSlotDrag(eventData);
    public void OnEndDrag(PointerEventData eventData) => Manager.OnSlotEndDrag();
    public void OnDrop(PointerEventData eventData) => Manager.OnSlotDrop(SlotIndex);
}

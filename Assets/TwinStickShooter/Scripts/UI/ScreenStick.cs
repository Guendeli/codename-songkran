namespace TwinStickShooter
{
  using UnityEngine.EventSystems;
  using UnityEngine.Serialization;
  using UnityEngine;
  using UnityEngine.InputSystem.Layouts;

  public class ScreenStick : ScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
  {
    public float MovementRange
    {
      get => _movementRange;
      set => _movementRange = value;
    }

    [FormerlySerializedAs("movementRange")] [SerializeField]
    private float _movementRange = 50;

    [InputControl(layout = "Vector2")] [SerializeField]
    private string _controlPath;

    private Vector3 _startPos;
    private Vector2 _pointerDownPos;

    protected override string controlPathInternal
    {
      get => _controlPath;
      set => _controlPath = value;
    }

    private void Start()
    {
      _startPos = ((RectTransform)transform).anchoredPosition;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
      if (eventData == null)
      {
        throw new System.ArgumentNullException(nameof(eventData));
      }

      RectTransformUtility.ScreenPointToLocalPointInRectangle
      (transform.parent.GetComponentInParent<RectTransform>(), eventData.position, eventData.pressEventCamera,
        out _pointerDownPos);
    }

    public override void OnDrag(PointerEventData eventData)
    {
      if (eventData == null)
      {
        throw new System.ArgumentNullException(nameof(eventData));
      }

      RectTransformUtility.ScreenPointToLocalPointInRectangle
      (transform.parent.GetComponentInParent<RectTransform>(), eventData.position, eventData.pressEventCamera,
        out var position);
      var delta = position - _pointerDownPos;

      delta = Vector2.ClampMagnitude(delta, MovementRange);
      ((RectTransform)transform).anchoredPosition = _startPos + (Vector3)delta;

      var newPos = new Vector2(delta.x / MovementRange, delta.y / MovementRange);
      SendValueToControl(newPos);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
      ((RectTransform)transform).anchoredPosition = _startPos;
      SendValueToControl(Vector2.zero);
    }
  }
}
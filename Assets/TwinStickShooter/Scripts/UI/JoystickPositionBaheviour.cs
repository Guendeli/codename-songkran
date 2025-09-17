namespace TwinStickShooter
{
  using UnityEngine;
  using UnityEngine.UI;
  using UnityEngine.EventSystems;

  public class JoystickPositionBaheviour : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
  {
    public ScreenStick ScreenStick;
    public GameObject ControlsParent;

    [SerializeField] private RectTransform _joystick;
    private Vector3 _defaultPos;
    private ScreenControl[] _controls;

    void Start()
    {
      _defaultPos = _joystick.position;
      Image image = GetComponent<Image>();
      image.color = new Color(image.color.r, image.color.g, image.color.g, 0);
      _controls = ControlsParent.GetComponents<ScreenControl>();
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
      foreach (var item in _controls)
      {
        item.OnDrag(eventData);
      }
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
      if (eventData == null)
        throw new System.ArgumentNullException(nameof(eventData));

      RectTransformUtility.ScreenPointToLocalPointInRectangle
      (transform.parent.GetComponentInParent<RectTransform>(), eventData.position, eventData.pressEventCamera,
        out var position);
      _joystick.localPosition = position;

      foreach (var item in _controls)
      {
        item.OnPointerDown(eventData);
      }
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
      foreach (var item in _controls)
      {
        item.OnPointerUp(eventData);
      }

      _joystick.position = _defaultPos;
    }
  }
}
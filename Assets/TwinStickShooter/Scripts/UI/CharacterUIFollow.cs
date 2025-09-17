namespace TwinStickShooter
{
  using Quantum;
  using System.Collections.Generic;
  using UnityEngine;

  public class CharacterUIFollow : QuantumEntityViewComponent<CustomViewContext>
  {
    public Vector2 Offset = new Vector2(0, 0);
    public bool SelfDestroy = false;
    public bool DeactivatesOnBecameInvisible = true;
    public List<GameObject> ContainedObjects;

    private Transform _entityViewTransform;
    private RectTransform _targetCanvas;
    private RectTransform _transform;

    public override void OnActivate(Frame frame)
    {
      base.OnActivate(frame);
      _entityViewTransform = EntityView.transform;

      _transform = GetComponent<RectTransform>();
      _targetCanvas = GameObject.Find("GameplayCanvas").GetComponent<RectTransform>();
      _transform.SetParent(_targetCanvas, false);

      RepositionUIElement();

      CharacterInvisibilityController.OnCharacterVisibilityChange += OnCharacterVisibilityChange;
    }

    private void OnCharacterVisibilityChange(bool value, EntityRef target)
    {
      if (DeactivatesOnBecameInvisible == false)
      {
        return;
      }

      if (target != EntityRef)
      {
        return;
      }

      for (int i = 0; i < ContainedObjects.Count; i++)
      {
        if (ContainedObjects[i] == null) continue;
        ContainedObjects[i].SetActive(value);
      }
    }

    void LateUpdate()
    {
      if (_entityViewTransform == null || _entityViewTransform.gameObject.activeInHierarchy == false)
      {
        if (SelfDestroy)
          Destroy(gameObject);
        return;
      }

      RepositionUIElement();
    }

    private void RepositionUIElement()
    {
      Vector2 viewportPosition = Camera.main.WorldToViewportPoint(_entityViewTransform.position);
      Vector2 worldObjectScreenPosition = new Vector2(
        ((viewportPosition.x * _targetCanvas.sizeDelta.x) - (_targetCanvas.sizeDelta.x * 0.5f)),
        ((viewportPosition.y * _targetCanvas.sizeDelta.y) - (_targetCanvas.sizeDelta.y * 0.5f)));
      _transform.anchoredPosition = worldObjectScreenPosition + Offset;
    }
  }
}
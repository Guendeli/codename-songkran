using UnityEngine;

public class MobileButtonsManager : MonoBehaviour
{
  public bool ShowOnEditor = false;

  void Start()
  {
#if UNITY_STANDALONE || UNITY_WEBGL
#if UNITY_EDITOR
    if (ShowOnEditor == false)
    {
      gameObject.SetActive(false);
    }
    return;
#endif
#pragma warning disable CS0162
    gameObject.SetActive(false);
#pragma warning restore CS0162
#endif
  }
}

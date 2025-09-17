namespace TwinStickShooter
{
  using UnityEngine;
  using TMPro;
  using Quantum;

  public class CoinGrabUI : MonoBehaviour
  {
    public GameObject GameOverContainer;
    public TextMeshProUGUI WinnerText;

    private void OnEnable()
    {
      QuantumEvent.Subscribe<EventGameOver>(this, OnGameOver);
    }

    private void OnDisable()
    {
      QuantumEvent.UnsubscribeListener(this);
    }

    private void OnGameOver(EventGameOver ev)
    {
      GameOverContainer.SetActive(true);
      string winnerName = ev.WinnerTeam == 0 ? "Left Team" : "Right Team";
      WinnerText.text = winnerName + " wins!";
    }
  }
}
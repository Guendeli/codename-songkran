namespace TwinStickShooter
{
  using Quantum;
  using UnityEngine;
  using TMPro;

  public class NameUI : QuantumEntityViewComponent<CustomViewContext>
  {
    public TextMeshProUGUI NameText;
    public TextAsset DefaultTextAsset;

    private CharacterUIFollow _characterFollow;

    private void Awake()
    {
      _characterFollow = GetComponent<CharacterUIFollow>();
    }

    public override void OnActivate(Frame frame)
    {
      base.OnActivate(frame);
      if (PredictedFrame.TryGet<PlayerLink>(_characterFollow.EntityView.EntityRef, out var playerLink))
      {
        int playerId = playerLink.PlayerRef;
        RuntimePlayer playerData = PredictedFrame.GetPlayerData(playerId);
        if (playerData != null)
        {
          NameText.text = playerData.PlayerNickname;
        }
        else
        {
          NameText.text = GetNameFromFile(playerLink.PlayerRef - 1);
        }
      }
    }

    private string GetNameFromFile(int id)
    {
      TextAsset asset = Resources.Load("BotsNames") as TextAsset;
      if (asset == null)
        asset = DefaultTextAsset;
      
      string[] names = asset.text.Split(';');
      return names[id];
    }
  }
}
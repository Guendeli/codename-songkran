namespace TwinStickShooter
{
  using System.Collections.Generic;
  using Quantum;
  using Quantum.Menu;
  using UnityEngine;

  public class CharacterSelectionUI : QuantumSceneViewComponent<CustomViewContext>
  {
    public GameObject StartButton;
    
    public AssetRef<EntityPrototype> PlaceHolderPrototype;
    public List<CharacterSelectionTeamInfo> Teams;

    private QuantumMenuConnectionBehaviour _connection;
    private UnityEngine.UI.Button _selectedButton;

    public override void OnActivate(Frame frame)
    {
      base.OnActivate(frame);
      StartButton.SetActive(false);
      _connection = FindObjectOfType<QuantumMenuConnectionBehaviour>();
      QuantumEvent.Subscribe<EventArenaPresentation>(this, OnArenaPreparation);

      RuntimePlayer playerData = new RuntimePlayer();
      playerData.PlayerAvatar = PlaceHolderPrototype;

      if (_connection != null)
      {
        playerData.PlayerNickname = _connection.Client.LocalPlayer.NickName;
      }

      Game.AddPlayer(playerData);
    }

    public override void OnUpdateView()
    {
      base.OnUpdateView();
      
      for (int i = 0; i < Teams.Count; i++)
      {
        CleanTeamSlots(i);
        UpdatePlayerByTeam(i);
      }

      if (_connection == null)
      {
        StartButton.SetActive(true);
        return;
      }

      StartButton.SetActive(_connection.Client.LocalPlayer.IsMasterClient);
    }

    private void CleanTeamSlots(int teamIndex)
    {
      for (int i = 0; i < Teams[teamIndex].TeamPlayers.Length; i++)
      {
        Teams[teamIndex].TeamPlayers[i].text = "--";
        Teams[teamIndex].TeamCharacters[i].text = "";
      }
    }

    private unsafe void UpdatePlayerByTeam(int teamIndex)
    {
      var characterIndex = 0;
      var characterFilter = VerifiedFrame.Filter<PlayerLink, Character, TeamInfo>();
      while (characterFilter.Next(out var entityRef, out var playerLink, out var character, out var team))
      {
        if (team.Index != teamIndex)
        {
          continue;
        }

        var playerData = VerifiedFrame.GetPlayerData(playerLink.PlayerRef);
        var nickname = "none";
        if (playerData != null && playerLink.PlayerRef != PlayerRef.None)
        {
          nickname = playerData.PlayerNickname;
        }

        Teams[team.Index].TeamPlayers[characterIndex].text = nickname;
        var characterInfo = VerifiedFrame.FindAsset<Quantum.CharacterInfo>(character.Info.Id);
        Teams[team.Index].TeamCharacters[characterIndex].text = characterInfo.Name;
        Teams[team.Index].TeamCharacters[characterIndex].color = characterInfo.NameColor;

        characterIndex++;
      }
    }

    public void SelectCharacterClicked(AssetRef<EntityPrototype> CharacterPrototype, UnityEngine.UI.Button button)
    {
      UpdateSelectedButtonState(button);
      
      var selectCommand = new SelectCharacterCommands.SelectCharacterCommand();
      selectCommand.CharacterSelected = CharacterPrototype;
      Game.SendCommand(selectCommand);
    }

    public void ChangeTeamClicked()
    {
      var changeTeamCommand = new SelectCharacterCommands.ChangePlayerTeamCommand();
      Game.SendCommand(changeTeamCommand);
    }

    private void UpdateSelectedButtonState(UnityEngine.UI.Button button)
    {
      if (_selectedButton != null)
      {
        _selectedButton.interactable = true;
      }
      _selectedButton = button;
      _selectedButton.interactable = false;
    }

    public void OnStartClicked()
    {
      var startCommand = new SelectCharacterCommands.FinishCharacterSelectionCommand();
      Game.SendCommand(startCommand);
    }

    private void OnArenaPreparation(EventArenaPresentation e)
    {
      var objects = GetComponentsInChildren<Transform>();
      foreach (Transform obj in objects)
      {
        obj.gameObject.SetActive(false);
      }
    }
  }

  
}
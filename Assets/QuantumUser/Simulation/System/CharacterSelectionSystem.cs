namespace Quantum
{
  using Photon.Deterministic;
  using UnityEngine.Scripting;

  [Preserve]
  public unsafe class CharacterSelectionSystem : SystemSignalsOnly,
    ISignalOnCharacterSelectionStart,
    ISignalOnCommandFinishCharacterSelection,
    ISignalOnCreatePlayerCharacter,
    ISignalOnChangePlayerTeam,
    ISignalOnDestroyPlayerCharacter
  {
    public void OnCharacterSelectionStart(Frame frame)
    {
      frame.Events.StartCharacterSelection();
    }

    public void OnCommandFinishCharacterSelection(Frame frame)
    {
      HFSMManager.TriggerEvent(frame, &frame.Global->GameManagerHFSM, default, "OnFinishCharacterSelection");
      frame.Events.FinishCharacterSelection();
      frame.SystemDisable<CharacterSelectionSystem>();
    }

    public void OnDestroyPlayerCharacter(Frame frame, PlayerRef player)
    {
      var players = frame.GetComponentIterator<PlayerLink>();
      foreach (var playerLink in players)
      {
        if (playerLink.Component.PlayerRef == player)
        {
          frame.Destroy(playerLink.Entity);
        }
      }
    }

    public void OnChangePlayerTeam(Frame frame, PlayerRef player)
    {
      var characterFilter = frame.Filter<PlayerLink, TeamInfo>();
      while (characterFilter.NextUnsafe(out var entity, out var playerLink, out var team))
      {
        if (playerLink->PlayerRef == player)
        {
          var targetTeam = team->Index == 0 ? 1 : 0;
          var playersInTargetTeam = GetTeamPlayerCount(frame, targetTeam);

          if (playersInTargetTeam > Constants.MAX_TEAM_SIZE) return;

          team->Index = targetTeam;
          frame.Signals.OnRespawnCharacter(entity, true);
        }
      }
    }

    public void OnCreatePlayerCharacter(Frame frame, PlayerRef player, AssetRef<EntityPrototype> prototype)
    {
      var currentTeamIndex = GetTeamInfo(frame, player);
      frame.Signals.OnDestroyPlayerCharacter(player);

      EntityRef playerCharacter = frame.Create(prototype);

      // Store it's PlayerRef so we can later use it for input polling
      PlayerLink* playerLink = frame.Unsafe.GetPointer<PlayerLink>(playerCharacter);
      playerLink->PlayerRef = player;

      TeamInfo* teamInfo = frame.Unsafe.GetPointer<TeamInfo>(playerCharacter);
      teamInfo->Index = currentTeamIndex == null ? FindValidTeam(frame) : currentTeamIndex->Index;

      // Spawn the character
      frame.Signals.OnRespawnCharacter(playerCharacter, true);

      // If, from the player data, we want to force this character to be controlled by AI,
      // we already setup the components needed
      if (frame.GetPlayerData(player).ForceAI == true)
      {
        AISetupHelper.Botify(frame, playerCharacter);
      }
    }

    private int FindValidTeam(Frame frame)
    {
      var teamAPlayersCount = GetTeamPlayerCount(frame, 0);
      return teamAPlayersCount <= Constants.MAX_TEAM_SIZE ? 0 : 1;
    }

    private TeamInfo* GetTeamInfo(Frame frame, PlayerRef player)
    {
      var characterFilter = frame.Filter<PlayerLink, TeamInfo>();
      while (characterFilter.NextUnsafe(out var entity, out var playerLink, out var team))
      {
        if (playerLink->PlayerRef == player)
        {
          return team;
        }
      }

      return null;
    }

    private int GetTeamPlayerCount(Frame frame, int teamIndex)
    {
      var count = 0;
      var teams = frame.GetComponentIterator<TeamInfo>();
      foreach (var team in teams)
      {
        if (team.Component.Index == teamIndex)
        {
          count++;
        }
      }

      return count;
    }
  }
}
using System;

namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebVirtualClientContextSnapshot
    {
        public WebVirtualClientContextSnapshot(
            string steamId,
            bool isAuthenticated,
            bool isMatchedPlayer,
            bool isActivePlayer,
            string playerId,
            string playerName,
            string accessLevel,
            bool trainCrewMembershipRequired,
            string[] trainCrewIds,
            bool canControlEquipment,
            DateTimeOffset? capturedAtUtc = null)
        {
            SteamId = steamId ?? string.Empty;
            IsAuthenticated = isAuthenticated;
            IsMatchedPlayer = isMatchedPlayer;
            IsActivePlayer = isActivePlayer;
            PlayerId = playerId ?? string.Empty;
            PlayerName = playerName ?? string.Empty;
            AccessLevel = accessLevel ?? string.Empty;
            TrainCrewMembershipRequired = trainCrewMembershipRequired;
            TrainCrewIds = trainCrewIds ?? Array.Empty<string>();
            CanControlEquipment = canControlEquipment;
            CapturedAtUtc = capturedAtUtc ?? DateTimeOffset.UtcNow;
        }

        public string SteamId { get; }

        public bool IsAuthenticated { get; }

        public bool IsMatchedPlayer { get; }

        public bool IsActivePlayer { get; }

        public string PlayerId { get; }

        public string PlayerName { get; }

        public string AccessLevel { get; }

        public bool TrainCrewMembershipRequired { get; }

        public string[] TrainCrewIds { get; }

        public bool CanControlEquipment { get; }

        public DateTimeOffset CapturedAtUtc { get; }
    }
}

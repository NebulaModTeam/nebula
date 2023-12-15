#region

using System.Collections.Generic;
using System.Text;

#endregion

namespace Discord
{
    public partial class LobbyManager
    {
        public IEnumerable<User> GetMemberUsers(long lobbyID)
        {
            var memberCount = MemberCount(lobbyID);
            var members = new List<User>();
            for (var i = 0; i < memberCount; i++)
            {
                members.Add(GetMemberUser(lobbyID, GetMemberUserId(lobbyID, i)));
            }
            return members;
        }

        public void SendLobbyMessage(long lobbyID, string data, SendLobbyMessageHandler handler)
        {
            SendLobbyMessage(lobbyID, Encoding.UTF8.GetBytes(data), handler);
        }
    }
}

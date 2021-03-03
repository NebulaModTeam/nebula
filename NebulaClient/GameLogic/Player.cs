using UnityEngine;
using NebulaModel.DataStructures;
using NebulaClient.MonoBehaviours;
using NebulaModel.Logger;
using NebulaModel.Packets.Players;

namespace NebulaClient.GameLogic
{
    public class Player
    {
        public ushort PlayerId { get; protected set; }
        public Float4 PlayerColor { get; protected set; }

        public Player(ushort playerId)
        {
            PlayerId = playerId;
            //Set default player color
            PlayerColor = new Float4(1.0f, 0.6846404f, 0.243137181f, 1.0f);
        }

        public void UpdateColor(Float4 newColor)
        {
            PlayerManager pm = MultiplayerSession.instance.PlayerManager;
            Transform transformToColor;

            // Find the Transform of the mecha, either the local player or a remote player
            if (pm.LocalPlayer?.PlayerId == this.PlayerId)
            {
                transformToColor = pm.LocalPlayerModel.Transform;
            }
            else
            {
                transformToColor = pm.GetPlayerModelById(this.PlayerId).PlayerTransform;
            }

            if (transformToColor == null)
            {
                Log.Warn($"Couldn't change color of {this.PlayerId} due to missing Transform.");
                return;
            }


            // Apply new color to each part of the mecha
            Renderer[] componentsInChildren = transformToColor.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
            foreach (Renderer r in componentsInChildren)
            {
                if (r.material?.name == "icarus-armor (Instance)")
                {
                    r.material.SetColor("_Color", newColor.ToColor());
                }
            }

            // If we are the local player, we need to let other clients know we changed color
            if (pm.LocalPlayer?.PlayerId == this.PlayerId)
            {
                MultiplayerSession.instance.Client.SendPacket(new PlayerColorChanged(this.PlayerId, newColor));
            }

            this.PlayerColor = newColor;
            Log.Info($"Changed color of player {this.PlayerId} to {newColor}");
        }
    }
}

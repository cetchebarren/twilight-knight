using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class SceneTeleportWaypointsManager : MonoBehaviour
    {
        public List<TeleportWaypoint> teleportWaypointsInScene;

        void Awake()
        {
            WorldStateManager.instance.currentTeleportWaypointsManager = this;
        }

        public void RestoreScaleForAllTeleportWaypoints()
        {
            foreach(TeleportWaypoint tw in teleportWaypointsInScene)
            {
                tw?.RestoreIconScale();
            }

            foreach(WorldMapTeleportButton wmtb in WorldStateManager.instance.allWorldMapTeleportWaypoints)
            {
                wmtb?.RestoreIconScale();
            }

            
        }

        public void SetScaleForAllTeleportWaypoints(float multiplier)
        {
            foreach (TeleportWaypoint tw in teleportWaypointsInScene)
            {
                tw?.SetIconScale(multiplier);
            }

            foreach (WorldMapTeleportButton wmtb in WorldStateManager.instance.allWorldMapTeleportWaypoints)
            {
                wmtb?.SetIconScale(multiplier);
            }
        }
    }
}

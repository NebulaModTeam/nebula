using NebulaAPI;
using NebulaModel.Packets.Players;
using NebulaWorld.MonoBehaviours.Local;
using System;
using UnityEngine;

namespace NebulaWorld.MonoBehaviours.Remote
{
    public class RemotePlayerMovement : MonoBehaviour
    {
        private const int BUFFERED_SNAPSHOT_COUNT = 4;
        private const double INTERPOLATION_TIME = (1000 / (double)LocalPlayerMovement.SEND_RATE) * (BUFFERED_SNAPSHOT_COUNT - 1);

        public struct Snapshot
        {
            public long Timestamp { get; set; }
            public int LocalPlanetId { get; set; }
            public Float3 LocalPlanetPosition { get; set; }
            public Double3 UPosition { get; set; }
            public Quaternion Rotation { get; set; }
            public Quaternion BodyRotation { get; set; }
        }

        private Transform rootTransform;
        private Transform bodyTransform;

        public int localPlanetId;
        public VectorLF3 absolutePosition;

        private GameObject playerDot;
        private GameObject playerName;
        public String Username { get; set; }
        public ushort PlayerID { get; set; }

#if DEBUG
        private GameObject positionDebugger;
#endif

        // To have a smooth transition between position updates, we keep a buffer of states received 
        // and once the buffer is full, we start replaying the states from the oldest to the newest state.
        // This will make sure player movement is still smooth in high latency cases and even if there are dropped packets.
        private readonly Snapshot[] snapshotBuffer = new Snapshot[BUFFERED_SNAPSHOT_COUNT];

        private void Awake()
        {
            rootTransform = GetComponent<Transform>();
            bodyTransform = rootTransform.Find("Model");

            localPlanetId = -1;
            absolutePosition = Vector3.zero;
#if DEBUG
            positionDebugger = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject.Destroy(positionDebugger.GetComponent<SphereCollider>());
            positionDebugger.transform.SetParent(rootTransform, false);
            positionDebugger.transform.localScale = Vector3.one * 30;
            positionDebugger.GetComponent<MeshRenderer>().material = null;
            positionDebugger.SetActive(false);
#endif
        }

        private void OnEnable()
        {
            GameObject origPlayerDot = UIRoot.instance.uiGame.planetGlobe.minimapControl.playerDot.gameObject;
            TextMesh uiSailIndicator_targetText = UIRoot.instance.uiGame.sailIndicator.targetText;
            if (origPlayerDot != null && uiSailIndicator_targetText != null)
            {
                playerDot = Instantiate(origPlayerDot, origPlayerDot.transform.parent, false);
                playerName = Instantiate(origPlayerDot, origPlayerDot.transform.parent, false);
                playerName.name = "playerName(Clone)";

                Destroy(playerName.GetComponent<MeshFilter>());

                MeshRenderer meshRenderer = playerName.GetComponent<MeshRenderer>();
                playerName.AddComponent<TextMesh>();

                meshRenderer.sharedMaterial = uiSailIndicator_targetText.gameObject.GetComponent<MeshRenderer>().sharedMaterial;

                playerName.SetActive(true);
            }
        }

        private void OnDisable()
        {
            if(playerDot != null)
            {
                Destroy(playerDot);
            }
            if(playerName != null)
            {
                Destroy(playerName);
            }
        }

        private void Update()
        {
#if DEBUG
            if (Input.GetKeyDown(KeyCode.F10))
            {
                positionDebugger.SetActive(!positionDebugger.activeSelf);
            }
#endif

            // Wait for the entire buffer to be full before starting to interpolate the player position
            if (snapshotBuffer[0].Timestamp == 0)
            {
                return;
            }

            // update navigation indicator if requested
            if(GameMain.mainPlayer.navigation.indicatorAstroId > 100000)
            {
                UpdateNavigationGizmo();
            }

            // update player dot on minimap if on same planet
            if(playerDot != null && playerName != null && localPlanetId == GameMain.mainPlayer.planetId)
            {
                TextMesh textMesh = playerName.GetComponent<TextMesh>();

                playerDot.SetActive(true);
                playerName.SetActive(true);

                if (textMesh != null)
                {
                    bool isFront = Vector3.Dot(UIRoot.instance.uiGame.planetGlobe.minimapControl.cam.transform.localPosition, rootTransform.localPosition) > 0;
                    textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, isFront ? 1f : 0.2f);
                }

                playerDot.transform.localPosition = rootTransform.position * (float)(0.5 / (double)GameMain.localPlanet.realRadius);
                playerDot.transform.localScale = 0.02f * Vector3.one;

                playerName.transform.localPosition = playerDot.transform.localPosition;
                playerName.transform.rotation = UIRoot.instance.uiGame.planetGlobe.minimapControl.cam.transform.rotation;

                if (textMesh != null && textMesh.text != Username)
                {
                    TextMesh uiSailIndicator_targetText = UIRoot.instance.uiGame.sailIndicator.targetText;

                    textMesh.font = uiSailIndicator_targetText.font;
                    textMesh.text = Username;
                    textMesh.fontSize = 20;
                }
                else if(textMesh == null)
                {
                    // may be reached if the destroy in Awake() did not happen fast enough preventing us from adding a TextMesh
                    playerName.AddComponent<TextMesh>();
                }
            }
            else if(playerDot != null && playerName != null)
            {
                playerDot.SetActive(false);
                playerName.SetActive(false);
            }

            double renderTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - INTERPOLATION_TIME;

            for (int i = 0; i < snapshotBuffer.Length - 1; ++i)
            {
                long t1 = snapshotBuffer[i].Timestamp;
                long t2 = snapshotBuffer[i + 1].Timestamp;

                if (renderTime <= t2 && renderTime >= t1)
                {
                    long total = t2 - t1;
                    double reminder = renderTime - t1;
                    double ratio = total > 0 ? reminder / total : 1;

                    // We interpolate to the appropriate position between our 2 known snapshot
                    MoveInterpolated(snapshotBuffer[i], snapshotBuffer[i + 1], (float)ratio);
                    break;
                }

                if (i == snapshotBuffer.Length - 2 && renderTime > t2)
                {
                    // This will skip interpolation and will snap to the most recent position.
                    MoveInterpolated(snapshotBuffer[i], snapshotBuffer[i + 1], 1);
                }
            }
        }

        private void UpdateNavigationGizmo()
        {
            if(PlayerID == GameMain.mainPlayer.navigation.indicatorAstroId - 100000)
            {
                PlayerControlGizmo gizmo = GameMain.mainPlayer.gizmo;
                UIStarmap starmap = UIRoot.instance.uiGame.starmap;

                if (gizmo.naviIndicatorGizmo == null)
                {
                    gizmo.naviIndicatorGizmo = LineGizmo.Create(1, gizmo.player.position, rootTransform.position);
                    gizmo.naviIndicatorGizmo.autoRefresh = true;
                    gizmo.naviIndicatorGizmo.multiplier = 1.5f;
                    gizmo.naviIndicatorGizmo.alphaMultiplier = 0.6f;
                    gizmo.naviIndicatorGizmo.width = 1.8f;
                    gizmo.naviIndicatorGizmo.color = Configs.builtin.gizmoColors[4];
                    gizmo.naviIndicatorGizmo.spherical = gizmo.player.planetId == localPlanetId;
                    gizmo.naviIndicatorGizmo.Open();
                }
                if (gizmo.naviIndicatorGizmoStarmap == null)
                {
                    gizmo.naviIndicatorGizmoStarmap = LineGizmo.Create(1, gizmo.player.position, rootTransform.position);
                    gizmo.naviIndicatorGizmoStarmap.autoRefresh = true;
                    gizmo.naviIndicatorGizmoStarmap.multiplier = 1.5f;
                    gizmo.naviIndicatorGizmoStarmap.alphaMultiplier = 0.3f;
                    gizmo.naviIndicatorGizmoStarmap.width = 0.01f;
                    gizmo.naviIndicatorGizmoStarmap.color = Configs.builtin.gizmoColors[4];
                    gizmo.naviIndicatorGizmoStarmap.spherical = false;
                    gizmo.naviIndicatorGizmoStarmap.Open();
                }

                gizmo.naviIndicatorGizmo.startPoint = gizmo.player.position;
                gizmo.naviIndicatorGizmo.endPoint = rootTransform.position;
                gizmo.naviIndicatorGizmoStarmap.startPoint = (gizmo.player.uPosition - starmap.viewTargetUPos) * 0.00025;
                gizmo.naviIndicatorGizmoStarmap.endPoint = (absolutePosition - starmap.viewTargetUPos) * 0.00025;
                gizmo.naviIndicatorGizmoStarmap.gameObject.layer = 20;
            }
        }

        public void UpdatePosition(PlayerMovement movement)
        {
            if (!rootTransform)
            {
                return;
            }

            for (int i = 0; i < snapshotBuffer.Length - 1; ++i)
            {
                snapshotBuffer[i] = snapshotBuffer[i + 1];
            }

            snapshotBuffer[snapshotBuffer.Length - 1] = new Snapshot()
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                LocalPlanetId = movement.LocalPlanetId,
                LocalPlanetPosition = movement.LocalPlanetPosition,
                UPosition = movement.UPosition,
                Rotation = Quaternion.Euler(movement.Rotation.ToVector3()),
                BodyRotation = Quaternion.Euler(movement.BodyRotation.ToVector3()),
            };
        }

        private void MoveInterpolated(Snapshot previous, Snapshot current, float ratio)
        {
            Vector3 previousRelativePosition = GetRelativePosition(previous);
            Vector3 currentRelativePosition = GetRelativePosition(current);
            Vector3 previousAbsolutePosition = GetAbsolutePosition(previous);
            Vector3 currentAbsolutePosition = GetAbsolutePosition(current);

            localPlanetId = current.LocalPlanetId;

            rootTransform.position = Vector3.Lerp(previousRelativePosition, currentRelativePosition, ratio);
            rootTransform.rotation = Quaternion.Slerp(previous.Rotation, current.Rotation, ratio);
            bodyTransform.rotation = Quaternion.Slerp(previous.BodyRotation, current.BodyRotation, ratio);

            absolutePosition = Vector3.Lerp(previousAbsolutePosition, currentAbsolutePosition, ratio);
        }

        private Vector3 GetRelativePosition(Snapshot snapshot)
        {
            if (GameMain.data == null)
            {
                return Vector3.zero;
            }

            // If we are on a local planet and the remote player is on the same local planet, we use the LocalPlanetPosition that is more precise.
            if (GameMain.localPlanet != null && GameMain.localPlanet.id == snapshot.LocalPlanetId)
            {
                return snapshot.LocalPlanetPosition.ToVector3();
            }

            // If the remote player is in space, we need to calculate the remote player relative position from our local player position.
            VectorLF3 uPosition = new VectorLF3(snapshot.UPosition.x, snapshot.UPosition.y, snapshot.UPosition.z);
            return Maths.QInvRotateLF(GameMain.data.relativeRot, uPosition - GameMain.data.relativePos);
        }

        private Vector3 GetAbsolutePosition(Snapshot snapshot)
        {
            // We only need to bother with uPos for this part for now
            return new VectorLF3(snapshot.UPosition.x, snapshot.UPosition.y, snapshot.UPosition.z);
        }

        public Snapshot GetLastPosition()
        {
            return snapshotBuffer[snapshotBuffer.Length - 1];
        }
    }
}

#region

using System;
using NebulaAPI.DataStructures;
using NebulaModel.Packets.Players;
using NebulaWorld.MonoBehaviours.Local;
using UnityEngine;

#endregion

namespace NebulaWorld.MonoBehaviours.Remote;

public class RemotePlayerMovement : MonoBehaviour
{
    private const int BUFFERED_SNAPSHOT_COUNT = 3;
    private const double INTERPOLATION_TIME = 1000 / (double)LocalPlayerMovement.SEND_RATE * (BUFFERED_SNAPSHOT_COUNT - 1);

    public int localPlanetId;
    public VectorLF3 absolutePosition;

    // To have a smooth transition between position updates, we keep a buffer of states received 
    // and once the buffer is full, we start replaying the states from the oldest to the newest state.
    // This will make sure player movement is still smooth in high latency cases and even if there are dropped packets.
    private readonly Snapshot[] snapshotBuffer = new Snapshot[BUFFERED_SNAPSHOT_COUNT];
    private Transform bodyTransform;

    private GameObject playerDot;
    private GameObject playerName;

#if DEBUG
    private GameObject positionDebugger;
#endif

    private Transform rootTransform;
    public string Username { get; set; }
    public ushort PlayerID { get; set; }
    public int LocalStarId { get; set; }

    private void Awake()
    {
        rootTransform = GetComponent<Transform>();
        bodyTransform = rootTransform.Find("Model");

        localPlanetId = -1;
        absolutePosition = Vector3.zero;
#if DEBUG
        positionDebugger = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(positionDebugger.GetComponent<SphereCollider>());
        positionDebugger.transform.SetParent(rootTransform, false);
        positionDebugger.transform.localScale = Vector3.one * 30;
        positionDebugger.GetComponent<MeshRenderer>().material = null;
        positionDebugger.SetActive(false);
#endif
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

        // update player dot on minimap if on same planet
        if (playerDot != null && playerName != null && localPlanetId == GameMain.mainPlayer.planetId)
        {
            var textMesh = playerName.GetComponent<TextMesh>();

            playerDot.SetActive(true);
            playerName.SetActive(true);

            if (textMesh != null)
            {
                var isFront = Vector3.Dot(UIRoot.instance.uiGame.planetGlobe.minimapControl.cam.transform.localPosition,
                    rootTransform.localPosition) > 0;
                var color = textMesh.color;
                textMesh.color = new Color(color.r, color.g, color.b, isFront ? 1f : 0.2f);
            }

            playerDot.transform.localPosition = rootTransform.position * (float)(0.5 / GameMain.localPlanet.realRadius);
            playerDot.transform.localScale = 0.02f * Vector3.one;

            playerName.transform.localPosition = playerDot.transform.localPosition;
            playerName.transform.rotation = UIRoot.instance.uiGame.planetGlobe.minimapControl.cam.transform.rotation;

            if (textMesh != null && textMesh.text != Username)
            {
                var uiSailIndicator_targetText = UIRoot.instance.uiGame.sailIndicator.targetText;

                textMesh.font = uiSailIndicator_targetText.font;
                textMesh.text = Username;
                textMesh.fontSize = 20;
            }
            else if (textMesh == null)
            {
                // may be reached if the destroy in Awake() did not happen fast enough preventing us from adding a TextMesh
                playerName.AddComponent<TextMesh>();
            }
        }
        else if (playerDot != null && playerName != null)
        {
            playerDot.SetActive(false);
            playerName.SetActive(false);
        }

        var renderTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - INTERPOLATION_TIME;

        for (var i = 0; i < snapshotBuffer.Length - 1; ++i)
        {
            var t1 = snapshotBuffer[i].Timestamp;
            var t2 = snapshotBuffer[i + 1].Timestamp;

            if (renderTime <= t2 && renderTime >= t1)
            {
                var total = t2 - t1;
                var reminder = renderTime - t1;
                var ratio = total > 0 ? reminder / total : 1;

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

    private void OnEnable()
    {
        var origPlayerDot = UIRoot.instance.uiGame.planetGlobe.minimapControl.playerDot.gameObject;
        var uiSailIndicator_targetText = UIRoot.instance.uiGame.sailIndicator.targetText;
        if (origPlayerDot == null || uiSailIndicator_targetText == null)
        {
            return;
        }
        var parent = origPlayerDot.transform.parent;
        playerDot = Instantiate(origPlayerDot, parent, false);
        playerName = Instantiate(origPlayerDot, parent, false);
        playerName.name = "playerName(Clone)";

        Destroy(playerName.GetComponent<MeshFilter>());

        var meshRenderer = playerName.GetComponent<MeshRenderer>();
        playerName.AddComponent<TextMesh>();

        meshRenderer.sharedMaterial = uiSailIndicator_targetText.gameObject.GetComponent<MeshRenderer>().sharedMaterial;

        playerName.SetActive(true);
    }

    private void OnDisable()
    {
        if (playerDot != null)
        {
            Destroy(playerDot);
        }
        if (playerName != null)
        {
            Destroy(playerName);
        }
    }

    public void UpdatePosition(PlayerMovement movement)
    {
        if (!rootTransform)
        {
            return;
        }

        for (var i = 0; i < snapshotBuffer.Length - 1; ++i)
        {
            snapshotBuffer[i] = snapshotBuffer[i + 1];
        }

        snapshotBuffer[snapshotBuffer.Length - 1] = new Snapshot
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            LocalPlanetId = movement.LocalPlanetId,
            LocalPlanetPosition = movement.LocalPlanetPosition,
            UPosition = movement.UPosition,
            Rotation = Quaternion.Euler(movement.Rotation.ToVector3()),
            BodyRotation = Quaternion.Euler(movement.BodyRotation.ToVector3())
        };
    }

    private void MoveInterpolated(Snapshot previous, Snapshot current, float ratio)
    {
        var previousRelativePosition = GetRelativePosition(previous);
        var currentRelativePosition = GetRelativePosition(current);
        var previousAbsolutePosition = GetAbsolutePosition(previous);
        var currentAbsolutePosition = GetAbsolutePosition(current);

        localPlanetId = current.LocalPlanetId;
        if (current.LocalPlanetId > 0) LocalStarId = current.LocalPlanetId / 100;

        rootTransform.SetPositionAndRotation(Vector3.Lerp(previousRelativePosition, currentRelativePosition, ratio),
            Quaternion.Slerp(previous.Rotation, current.Rotation, ratio));
        bodyTransform.rotation = Quaternion.Slerp(previous.BodyRotation, current.BodyRotation, ratio);

        absolutePosition = Vector3.Lerp(previousAbsolutePosition, currentAbsolutePosition, ratio);
    }

    private static Vector3 GetRelativePosition(Snapshot snapshot)
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
        var uPosition = new VectorLF3(snapshot.UPosition.x, snapshot.UPosition.y, snapshot.UPosition.z);
        return Maths.QInvRotateLF(GameMain.data.relativeRot, uPosition - GameMain.data.relativePos);
    }

    private static Vector3 GetAbsolutePosition(Snapshot snapshot)
    {
        // We only need to bother with uPos for this part for now
        return new VectorLF3(snapshot.UPosition.x, snapshot.UPosition.y, snapshot.UPosition.z);
    }

    public Snapshot GetLastPosition()
    {
        return snapshotBuffer[snapshotBuffer.Length - 1];
    }

    public struct Snapshot
    {
        public long Timestamp { get; set; }
        public int LocalPlanetId { get; set; }
        public Float3 LocalPlanetPosition { get; set; }
        public Double3 UPosition { get; set; }
        public Quaternion Rotation { get; set; }
        public Quaternion BodyRotation { get; set; }
    }
}

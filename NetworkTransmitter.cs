// Simple networking script to transmit avatar data over the network.
// Data such as headset or controller positions can also be sent during time intervals and not every frame.
// These values can be Lerped when received by the other application to make movement smoother.

using Azury;
using Azury.Chips;
using Azury.Notifications;
using Mirror;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField]
    private AvatarData avatarData;

    private AvatarData.AvatarMode avatarMode;

    private const byte POINT_CLOUD = 0;
    private const byte VIDEO_STREAM = 1;
    private const byte AVATAR = 2;

    [Server]
    void Start()
    {
        avatarMode = avatarData.avatarMode;
        RpcSetupClient(avatarData.kinectWidth, avatarData.kinectHeight);
    }

    [ClientRpc]
    private void RpcSetupClient(int kinWidth, int kinHeight)
    {
        avatarData.isClient = true;
        avatarData.Initialize(kinWidth, kinHeight);
    }

    [Client]
    private void LateUpdate()
    {
        CmdSendClientPosition(avatarData.clientHeadsetPosition, avatarData.clientHeadsetRotation, avatarData.clientRightHandPosition, avatarData.clientRightHandRotation);
    }

    [Command]
    private void CmdSendClientPosition(Vector3 headPos, Vector3 headRot, Vector3 handPos, Vector3 handRot)
    {
        avatarData.clientHeadsetPosition = headPos;
        avatarData.clientHeadsetRotation = headRot;
        avatarData.clientRightHandPosition = handPos;
        avatarData.clientRightHandRotation = handRot;
    }

    [Server]
    private void Update()
    {
        if (avatarData.avatarMode == AvatarData.AvatarMode.PointCloud)
        {
            RpcSendBytes(avatarData.colorData, avatarData.vertexData);
        }
        if (avatarData.avatarMode == AvatarData.AvatarMode.VideoStream)
        {
            RpcSendBytes(avatarData.colorData, null);
        }
        if (avatarData.avatarMode == AvatarData.AvatarMode.Avatar)
        {
            RpcSendAvatarPositions(avatarData.headPosition, avatarData.headRotation, avatarData.leftHandPosition, avatarData.leftRotation, avatarData.rightHandPosition, avatarData.rightRotation, avatarData.isTalking, avatarData.isLeftPointing, avatarData.isRightPointing);
        }
        
        RpcUpdatePointer(avatarData.pointerPosition);
        RpcUpdateAgentPosition(avatarData.agentPosition, avatarData.arePositionsEnabled);

        if (avatarMode != avatarData.avatarMode)
        {
            avatarMode = avatarData.avatarMode;
            switch (avatarMode)
            {
                case AvatarData.AvatarMode.PointCloud:
                    RpcChangeMode(POINT_CLOUD);
                    break;

                case AvatarData.AvatarMode.VideoStream:
                    RpcChangeMode(VIDEO_STREAM);
                    break;

                case AvatarData.AvatarMode.Avatar:
                    RpcChangeMode(AVATAR);
                    break;

                default:
                    break;
            }
        }
    }

    [ClientRpc]
    private void RpcUpdatePointer(Vector3 pos)
    {
        avatarData.pointerPosition = pos;
    }

    [ClientRpc]
    private void RpcUpdateAgentPosition(Vector3 pos, bool areposen)
    {
        avatarData.agentPosition = pos;
        avatarData.arePositionsEnabled = areposen;
    }

    [ClientRpc]
    private void RpcSendBytes(byte[] colorbytes, byte[] vertexbytes)
    {
        avatarData.colorData = colorbytes;
        avatarData.vertexData = vertexbytes;
    }

    [ClientRpc]
    private void RpcSendAvatarPositions(Vector3 headpos, Vector3 headrot, Vector3 leftpos, Vector3 leftrot, Vector3 rightpos, Vector3 rightrot, bool istalking, bool isleftpointing, bool isrightpointing)
    {
        avatarData.headPosition = headpos;
        avatarData.headRotation = headrot;
        avatarData.leftHandPosition = leftpos;
        avatarData.leftRotation = leftrot;
        avatarData.rightHandPosition = rightpos;
        avatarData.rightRotation = rightrot;
        avatarData.isTalking = istalking;
        avatarData.isLeftPointing = isleftpointing;
        avatarData.isRightPointing = isrightpointing;
    }

    [ClientRpc]
    private void RpcChangeMode(byte index)
    {
        switch (index)
        {
            case POINT_CLOUD:
                avatarData.avatarMode = AvatarData.AvatarMode.PointCloud;
                break;

            case VIDEO_STREAM:
                avatarData.avatarMode = AvatarData.AvatarMode.VideoStream;
                break;

            case AVATAR:
                avatarData.avatarMode = AvatarData.AvatarMode.Avatar;
                break;

            default:
                break;
        }
    }
}

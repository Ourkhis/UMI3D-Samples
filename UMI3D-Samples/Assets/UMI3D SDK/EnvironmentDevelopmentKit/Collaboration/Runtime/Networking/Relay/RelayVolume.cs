﻿/*
Copyright 2019 - 2021 Inetum

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using umi3d.common.collaboration;
using umi3d.edk.userCapture;
using UnityEngine;

namespace umi3d.edk.collaboration
{
    /// <summary>
    /// Relay for a collaboration room defined by a volume. 
    /// </summary>
    public class RelayVolume : MonoBehaviour, ICollaborationRoom
    {
        public static Dictionary<ulong, RelayVolume> relaysVolumes = new Dictionary<ulong, RelayVolume>();

        [Serializable]
        public struct RelayAssociation
        {
            public DataChannelTypes channel;
            public RelayDescription relay;
        }

        [SerializeField]
        private RelayAssociation[] relays = null;

        protected Dictionary<DataChannelTypes, RelayDescription> DicoRelays = new Dictionary<DataChannelTypes, RelayDescription>();

        /// <summary>
        /// The objects's unique id. 
        /// </summary>
        protected ulong volumeId = 0;

        protected Dictionary<ulong, Dictionary<ulong, ulong>> relayDataMemory = new Dictionary<ulong, Dictionary<ulong, ulong>>();
        protected Dictionary<ulong, Dictionary<ulong, ulong>> relayTrackingMemory = new Dictionary<ulong, Dictionary<ulong, ulong>>();
        protected Dictionary<ulong, Dictionary<ulong, ulong>> relayVoIPMemory = new Dictionary<ulong, Dictionary<ulong, ulong>>();
        protected Dictionary<ulong, Dictionary<ulong, ulong>> relayVideoMemory = new Dictionary<ulong, Dictionary<ulong, ulong>>();

        private void Awake()
        {
            RelayVolume.relaysVolumes.Add(this.Id(), this);
            DicoRelays = relays.ToDictionary(p => p.channel, p => p.relay);
        }

        /// <summary>
        /// The public getter for volumeId
        /// </summary>
        /// <returns></returns>
        public ulong Id()
        {
            if (volumeId == 0 && UMI3DEnvironment.Exists)
            {
                volumeId = UMI3DEnvironment.Register(this);
            }

            return volumeId;
        }



        public RelayDescription RelayDescription(DataChannelTypes channel)
        {
            return DicoRelays[channel];
        }

        /// <summary>
        /// Handle relay for channel Data
        /// </summary>
        /// <param name="sender">Node associated to the request</param>
        /// <param name="data">Data to send</param>
        /// <param name="target"></param>
        /// <param name="receiverSetting"></param>
        /// <param name="isReliable"></param>
        public List<UMI3DUser> RelayDataRequest(UMI3DAbstractNode sender, object data, UMI3DUser target, Receivers receiverSetting, bool isReliable = false)
        {
            return RelayRequest(sender, data, target, receiverSetting, isReliable, DataChannelTypes.Data);
        }

        /// <summary>
        /// Handle relay for channel Tracking
        /// </summary>
        /// <param name="sender">Node associated to the request</param>
        /// <param name="data">Data to send</param>
        /// <param name="target"></param>
        /// <param name="receiverSetting"></param>
        /// <param name="isReliable"></param>
        public List<UMI3DUser> RelayTrackingRequest(UMI3DAbstractNode sender, object data, UMI3DUser target, Receivers receiverSetting, bool isReliable = false)
        {
            return RelayRequest(sender, data, target, receiverSetting, isReliable, DataChannelTypes.Tracking);
        }

        /// <summary>
        /// Handle relay for channel VoIP
        /// </summary>
        /// <param name="sender">Node associated to the request</param>
        /// <param name="data">Data to send</param>
        /// <param name="target"></param>
        /// <param name="receiverSetting"></param>
        /// <param name="isReliable"></param>
        public List<UMI3DUser> RelayVoIPRequest(UMI3DAbstractNode sender, object data, UMI3DUser target, Receivers receiverSetting, bool isReliable = false)
        {
            return RelayRequest(sender, data, target, receiverSetting, isReliable, DataChannelTypes.VoIP);
        }


        /// <summary>
        /// Handle relay for channel Video
        /// </summary>
        /// <param name="sender">Node associated to the request</param>
        /// <param name="data">Data to send</param>
        /// <param name="target"></param>
        /// <param name="receiverSetting"></param>
        /// <param name="isReliable"></param>
        public List<UMI3DUser> RelayVideoRequest(UMI3DAbstractNode sender, object data, UMI3DUser target, Receivers receiverSetting, bool isReliable = false)
        {
            return RelayRequest(sender, data, target, receiverSetting, isReliable, DataChannelTypes.Video);
        }

        /// <summary>
        /// Create a new HashSet with receivers
        /// </summary>
        /// <param name="target"></param>
        /// <param name="receiverSetting"></param>
        /// <returns></returns>
        protected List<UMI3DCollaborationUser> GetTargetHashSet(UMI3DUser target, Receivers receiverSetting)
        {
            switch (receiverSetting)
            {
                case Receivers.All:
                    return new List<UMI3DCollaborationUser>(UMI3DCollaborationServer.Collaboration.Users);
                case Receivers.Others:
                    return new List<UMI3DCollaborationUser>(UMI3DCollaborationServer.Collaboration.Users.Where(u => u.Id() != target.Id()));
                case Receivers.Target:
                    return new List<UMI3DCollaborationUser>() { target as UMI3DCollaborationUser };
                default:
                    return null;
            }
        }

        protected List<UMI3DUser> RelayRequest(UMI3DAbstractNode sender, object data, UMI3DUser target, Receivers receiverSetting, bool isReliable, DataChannelTypes dataChannel)
        {
            ulong now = UMI3DCollaborationServer.ForgeServer.time;

            List<UMI3DCollaborationUser> targetHashSet = GetTargetHashSet(target, receiverSetting);
            List<UMI3DUser> result = targetHashSet?.Select(p => p as UMI3DUser).ToList();

            if (targetHashSet != null)
            {
                foreach (UMI3DCollaborationUser user in targetHashSet)
                {
                    if (ShouldRelay(sender, user, dataChannel, now))
                    {
                        RememberRelay(sender, user, dataChannel, now);
                    }
                    else
                        result.Remove(user);
                }
            }
            return result;
        }

        /// <summary>
        /// Determine if data have to be relayed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="to"></param>
        /// <param name="channel"></param>
        /// <param name="now"></param>
        /// <returns></returns>
        protected bool ShouldRelay(UMI3DAbstractNode sender, UMI3DCollaborationUser to, DataChannelTypes channel, ulong now)
        {
            if (to.status != common.StatusType.ACTIVE)
                return false;

            RelayDescription relay = DicoRelays[channel];
            RelayDescription.Strategy strategy;

            if (to.Avatar.RelayRoom == null)
                strategy = relay.OutsideVolume;
            else
                strategy = to.Avatar.RelayRoom.Equals(this) ? relay.InsideVolume : relay.OutsideVolume;

            if (strategy.sendData)
            {
                Dictionary<ulong, Dictionary<ulong, ulong>> relayMemory;

                switch (strategy.sendingStrategy)
                {
                    case collaboration.RelayDescription.SendingStrategy.AlwaysSend:
                        return true;

                    case collaboration.RelayDescription.SendingStrategy.Fixed:
                        relayMemory = GetRelayMemory(channel);

                        if (relayMemory != null)
                        {
                            if (!relayMemory.ContainsKey(sender.Id()) || !relayMemory[sender.Id()].ContainsKey(to.Id()))
                            {
                                return true;
                            }
                            else
                            {
                                float StrategyDelay = 1 / strategy.constantFPS;
                                float CurrentDelay = (now - relayMemory[sender.Id()][to.Id()]) / 1000f;

                                return StrategyDelay <= CurrentDelay;
                            }
                        }
                        return false;

                    case collaboration.RelayDescription.SendingStrategy.Proximity:
                        relayMemory = GetRelayMemory(channel);

                        if (relayMemory != null)
                        {
                            if (!relayMemory.ContainsKey(sender.Id()) || !relayMemory[sender.Id()].ContainsKey(to.Id()))
                            {
                                return true;
                            }
                            else
                            {
                                float dist = 0f;
                                if (channel == DataChannelTypes.Tracking)
                                {
                                    UMI3DCollaborationUser userSender = UMI3DCollaborationServer.Collaboration.GetUser((sender as UMI3DAvatarNode).userId);
                                    dist = Vector3.Distance(to.Avatar.objectPosition.GetValue(userSender), sender.objectPosition.GetValue(to));
                                }
                                else
                                {
                                    dist = Vector3.Distance(to.Avatar.objectPosition.GetValue(), sender.objectPosition.GetValue(to));
                                }

                                float coeff = 0f;
                                if (dist > strategy.startingProximityDistance && dist < strategy.stoppingProximityDistance)
                                {
                                    coeff = (dist - strategy.startingProximityDistance) / (strategy.stoppingProximityDistance - strategy.startingProximityDistance);
                                }
                                else if (dist >= strategy.stoppingProximityDistance)
                                {
                                    coeff = 1f;
                                }

                                float StrategyDelay = ((1f - coeff) * (1 / strategy.maxProximityFPS)) + (coeff * (1 / strategy.minProximityFPS));
                                float CurrentDelay = (now - relayMemory[sender.Id()][to.Id()]) / 1000f;

                                return StrategyDelay <= CurrentDelay;
                            }
                        }
                        return false;

                    default:
                        return true;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Handle Relay Memory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="to"></param>
        /// <param name="channel"></param>
        /// <param name="now"></param>
        protected void RememberRelay(UMI3DAbstractNode sender, UMI3DCollaborationUser to, DataChannelTypes channel, ulong now)
        {
            Dictionary<ulong, Dictionary<ulong, ulong>> relayMemory = GetRelayMemory(channel);

            if (relayMemory != null)
            {
                if (!relayMemory.ContainsKey(sender.Id()))
                    relayMemory.Add(sender.Id(), new Dictionary<ulong, ulong>());

                if (!relayMemory[sender.Id()].ContainsKey(to.Id()))
                    relayMemory[sender.Id()].Add(to.Id(), now);
                else
                    relayMemory[sender.Id()][to.Id()] = now;
            }
        }

        protected Dictionary<ulong, Dictionary<ulong, ulong>> GetRelayMemory(DataChannelTypes channel)
        {
            switch (channel)
            {
                case DataChannelTypes.Tracking:
                    return relayTrackingMemory;
                case DataChannelTypes.Data:
                    return relayDataMemory;
                case DataChannelTypes.Video:
                    return relayVideoMemory;
                case DataChannelTypes.VoIP:
                    return relayVoIPMemory;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Checks if this relay defines a strategy for <paramref name="channelType"/>.
        /// </summary>
        /// <param name="channelType"></param>
        /// <returns></returns>
        public bool HasStrategyFor(DataChannelTypes channelType)
        {
            return DicoRelays.ContainsKey(channelType);
        }
    }
}
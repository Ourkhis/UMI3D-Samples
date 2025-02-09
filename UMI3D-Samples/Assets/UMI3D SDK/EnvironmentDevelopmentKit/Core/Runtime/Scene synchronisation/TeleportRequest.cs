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

using umi3d.common;
using UnityEngine;

namespace umi3d.edk
{
    /// <summary>
    /// Request to teleport a user. Adds rotation compared to a <see cref="NavigationRequest"/>.
    /// </summary>
    public class TeleportRequest : NavigationRequest
    {
        /// <summary>
        /// Rotation of th user as a quaternion;
        /// </summary>
        public SerializableVector4 rotation;

        public TeleportRequest(Vector3 position, Quaternion rotation, bool reliable) : base(position, reliable)
        {
            this.rotation = rotation;
        }

        /// <inheritdoc/>
        protected override uint GetOperationKey()
        {
            return UMI3DOperationKeys.TeleportationRequest;
        }

        /// <inheritdoc/>
        protected override Bytable ToBytable()
        {
            if (rotation == null) rotation = new SerializableVector4();
            return base.ToBytable()
                + UMI3DNetworkingHelper.Write(rotation);
        }

        /// <inheritdoc/>
        protected override NavigateDto CreateDto() { return new TeleportDto(); }

        /// <inheritdoc/>
        protected override void WriteProperties(NavigateDto dto)
        {
            base.WriteProperties(dto);
            if (dto is TeleportDto tpDto)
                tpDto.rotation = rotation;
        }
    }
}
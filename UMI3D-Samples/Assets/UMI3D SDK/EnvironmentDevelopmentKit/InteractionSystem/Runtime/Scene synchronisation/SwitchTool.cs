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
using umi3d.common.interaction;


namespace umi3d.edk.interaction
{
    /// <summary>
    /// UMI3D <see cref="Operation"/> forcing the release of the current projected tool and the projection of another tool on clients.
    /// </summary>
    public class SwitchTool : ProjectTool
    {
        /// <summary>
        /// Tool to replace.
        /// </summary>
        public AbstractTool toolToReplace;

        /// <inheritdoc/>
        public override Bytable ToBytable(UMI3DUser user)
        {
            return UMI3DNetworkingHelper.Write(UMI3DOperationKeys.SwitchTool)
                + UMI3DNetworkingHelper.Write(tool.Id())
                + UMI3DNetworkingHelper.Write(toolToReplace.Id())
                + UMI3DNetworkingHelper.Write(releasable);
        }

        /// <inheritdoc/>
        public override AbstractOperationDto ToOperationDto(UMI3DUser user)
        {
            return new SwitchToolDto() { toolId = tool.Id(), replacedToolId = toolToReplace.Id(), releasable = releasable };
        }
    }
}
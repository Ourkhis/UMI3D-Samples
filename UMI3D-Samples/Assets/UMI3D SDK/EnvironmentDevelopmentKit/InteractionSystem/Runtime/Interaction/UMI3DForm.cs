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

using System.Collections.Generic;
using System.Linq;
using umi3d.common;
using umi3d.common.interaction;
using UnityEngine.Events;

namespace umi3d.edk.interaction
{
    /// <summary>
    /// Interaction representing a form.
    /// </summary>
    public class UMI3DForm : AbstractInteraction
    {
        /// <summary>
        /// Form fields as a list of <see cref="AbstractParameter"/>.
        /// </summary>
        public List<AbstractParameter> Fields = new List<AbstractParameter>();

        [System.Serializable]
        public class FormListener : UnityEvent<FormEventContent> { }

        public class FormEventContent : InteractionEventContent
        {
            public FormEventContent(UMI3DUser user, FormAnswerDto dto) : base(user, dto)
            {
            }

            public FormEventContent(UMI3DUser user, ulong toolId, ulong id, ulong hoveredObjectId, uint boneType) : base(user, toolId, id, hoveredObjectId, boneType)
            {
            }
        }


        /// <summary>
        /// Event raised on value change.
        /// </summary>
        public FormListener onFormCompleted = new FormListener();

        /// <inheritdoc/>
        protected override AbstractInteractionDto CreateDto()
        {
            return new FormDto();
        }

        /// <inheritdoc/>
        protected override void WriteProperties(AbstractInteractionDto dto_, UMI3DUser user)
        {
            base.WriteProperties(dto_, user);
            var dto = dto_ as FormDto;
            if (dto == null)
                return;
            dto.fields = Fields.Select(f => f.ToDto(user) as AbstractParameterDto).Where(f => f != null).ToList();
        }

        /// <inheritdoc/>
        protected override byte GetInteractionKey()
        {
            return UMI3DInteractionKeys.Form;
        }

        /// <inheritdoc/>
        public override Bytable ToByte(UMI3DUser user)
        {
            return base.ToByte(user)
                + UMI3DNetworkingHelper.WriteIBytableCollection(Fields);
        }

        /// <inheritdoc/>
        public override void OnUserInteraction(UMI3DUser user, InteractionRequestDto interactionRequest)
        {
            switch (interactionRequest)
            {
                case FormAnswerDto formAnswer:
                    formAnswer.answers.ForEach(a => UMI3DBrowserRequestDispatcher.DispatchBrowserRequest(user, a));
                    onFormCompleted.Invoke(new FormEventContent(user, formAnswer));
                    break;
                default:
                    throw new System.Exception("User interaction not supported (ParameterSettingRequestDto) ");
            }
        }

        /// <inheritdoc/>
        public override void OnUserInteraction(UMI3DUser user, ulong operationId, ulong toolId, ulong interactionId, ulong hoverredId, uint boneType, ByteContainer container)
        {
            switch (interactionId)
            {
                case UMI3DOperationKeys.FormAnswer:
                    UMI3DBrowserRequestDispatcher.DispatchBrowserRequest(user, UMI3DOperationKeys.ParameterSettingRequest, container);
                    onFormCompleted.Invoke(new FormEventContent(user, toolId, interactionId, hoverredId, boneType));
                    break;
                default:
                    throw new System.Exception("User interaction not supported (ParameterSettingRequestDto) ");
            }
        }
    }
}
﻿/*
Copyright 2019 Gfi Informatique

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
using umi3d.edk;
using umi3d.edk.collaboration;
using UnityEngine;
using inetum.unityUtils;

[RequireComponent(typeof(UMI3DModel))]
public class ChangeColorOnTrigger : MonoBehaviour
{
    UMI3DModel model;
    public List<MaterialSO> materials;
    int i = 0;

    private void Start()
    {
        model = GetComponent<UMI3DModel>();
        model.objectMaterialsOverrided.SetValue(true);
        if (model.objectMaterialOverriders.GetValue().Count > 0)
            model.objectMaterialOverriders.SetValue(0, new MaterialOverrider() { overrideAllMaterial = true, newMaterial = materials[i] });
        else
            model.objectMaterialOverriders.Add(new MaterialOverrider() { overrideAllMaterial = true, newMaterial = materials[i] });
    }

    public void OnTrigger(umi3d.edk.interaction.AbstractInteraction.InteractionEventContent content)
    {
        updateColor();
    }

    public void updateColor()
    {
        i++;
        if (i >= materials.Count) i = 0;
        var t = new Transaction()
        {
            reliable = true,
        };
        t.AddIfNotNull(model.objectMaterialOverriders.SetValue(0, new MaterialOverrider() { overrideAllMaterial = true, newMaterial = materials[i] }));
        t.Dispatch();
    }

}

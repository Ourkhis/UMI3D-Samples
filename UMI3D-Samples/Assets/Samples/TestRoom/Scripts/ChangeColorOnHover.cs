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
using umi3d.edk;
using umi3d.edk.collaboration;
using UnityEngine;

public class ChangeColorOnHover : MonoBehaviour
{
    List<string> trackers = new List<string>();
    bool lastState = false;

    UMI3DModel model;

    public MaterialSO HoverMaterial;
    public MaterialSO defaultMaterial;

    private void Start()
    {
        model = GetComponent<UMI3DModel>();
        model.objectMaterialsOverrided.SetValue(true);
        if (model.objectMaterialOverriders.GetValue().Count > 0)
            model.objectMaterialOverriders.SetValue(0, new MaterialOverrider() { overrideAllMaterial = true, newMaterial = defaultMaterial });
        else
            model.objectMaterialOverriders.Add(new MaterialOverrider() { overrideAllMaterial = true, newMaterial = defaultMaterial });
    }


    string ToName(UMI3DUser user, uint boneType)
    {
        return $"{user.Id()}:{boneType}";
    }

    public void OnHoverEnter(umi3d.edk.interaction.AbstractInteraction.InteractionEventContent content)
    {
        var name = ToName(content.user, content.boneType);
        if (!trackers.Contains(name))
            trackers.Add(name);
        updateColor();
    }
    public void OnHoverExit(umi3d.edk.interaction.AbstractInteraction.InteractionEventContent content)
    {
        var name = ToName(content.user, content.boneType);
        if (trackers.Contains(name))
        {
            trackers.Remove(ToName(content.user, content.boneType));
        }
        updateColor();
    }

    public void updateColor()
    {
        if (lastState == trackers.Count > 0) return;
        lastState = !lastState;
        var t = new Transaction()
        {
            reliable = true,
        };
        t.AddIfNotNull(model.objectMaterialOverriders.SetValue(0, new MaterialOverrider() { overrideAllMaterial = true, newMaterial = lastState ? HoverMaterial : defaultMaterial }));
        t.Dispatch();
    }

}

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
using umi3d.common;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace umi3d.edk
{
    /// <summary>
    /// Obsolete simple way to make transaction automatic on UMI3D Entities.
    /// This class isn't meant to be used in production.
    /// </summary>
    [Obsolete("This class isn't meant to be used in production", false)]
    public partial class SimpleModificationListener : MonoBehaviour
    {
        private const DebugScope scope = DebugScope.EDK | DebugScope.Core | DebugScope.Editor;

        private UMI3DNode[] nodes;
        private UMI3DScene[] scenes;
        public float time = 0f;
        private float timeTmp = 0;
        public int max = 0;
        private Dictionary<ulong, Dictionary<ulong, SetEntityProperty>> sets;

        public UnityEvent SetNodes = new UnityEvent();

        // Start is called before the first frame update
        private void Start()
        {
            nodes = GetComponentsInChildren<UMI3DNode>();
            scenes = GetComponentsInChildren<UMI3DScene>();

            SetNodes.Invoke();
        }

        // Update is called once per frame
        private void Update()
        {
            if (UMI3DServer.Exists)
            {
                foreach (UMI3DNode node in nodes)
                    Update(node);

                foreach (UMI3DScene scene in scenes)
                    MaterialUpdate(scene);

                Dispatch();
            }
        }

        private void Dispatch()
        {
            if (checkTime() || checkMax())
            {

                var transaction = new Transaction();
                transaction.AddIfNotNull(sets.SelectMany(p => p.Value).Select(p => (Operation)p.Value));
                if (transaction.Count() > 0)
                {
                    transaction.reliable = false;
                    UMI3DServer.Dispatch(transaction);
                    sets = new Dictionary<ulong, Dictionary<ulong, SetEntityProperty>>();
                }
                nodes = GetComponentsInChildren<UMI3DNode>();
                scenes = GetComponentsInChildren<UMI3DScene>();

                SetNodes.Invoke();
            }
        }

        private bool checkTime()
        {
            timeTmp -= Time.deltaTime;
            if (time == 0 || timeTmp <= 0)
            {
                timeTmp = time;
                return true;
            }
            return false;
        }

        private bool checkMax()
        {
            return (max != 0) && (sets.SelectMany(p => p.Value).Count() > max);
        }

        private void Update(UMI3DNode obj)
        {
            if (obj == null) return;
            if (sets == null) sets = new Dictionary<ulong, Dictionary<ulong, SetEntityProperty>>();
            if (!sets.ContainsKey(obj.Id())) sets[obj.Id()] = new Dictionary<ulong, SetEntityProperty>();

            setOperation(obj.objectPosition.SetValue(obj.transform.localPosition));
            setOperation(obj.objectRotation.SetValue(obj.transform.localRotation));
            setOperation(obj.objectScale.SetValue(obj.transform.localScale));
            //setOperation(obj.objectXBillboard.SetValue(obj.xBillboard));
            //setOperation(obj.objectYBillboard.SetValue(obj.yBillboard));

            UIUpdate(obj as UIRect);

            ModelUpdate(obj);

        }

        private void MaterialUpdate(UMI3DScene scene)
        {
            if (scene == null) return;
            if (sets == null) sets = new Dictionary<ulong, Dictionary<ulong, SetEntityProperty>>();


            foreach (MaterialSO mat in scene.materialSOs)
            {
                if (!sets.ContainsKey(mat.Id())) sets[mat.Id()] = new Dictionary<ulong, SetEntityProperty>();
                switch (mat)
                {
                    case PBRMaterial pbrmat:
                        setOperation(pbrmat.objectBaseColorFactor.SetValue(pbrmat.baseColorFactor));
                        setOperation(pbrmat.objectEmissiveFactor.SetValue(pbrmat.emissive));
                        setOperation(pbrmat.objectEmissiveTexture.SetValue(pbrmat.textures.emissiveTexture));
                        setOperation(pbrmat.objectHeightTexture.SetValue(pbrmat.textures.heightTexture));
                        setOperation(pbrmat.objectHeightTextureScale.SetValue(pbrmat.textures.heightTexture.scale));
                        setOperation(pbrmat.objectMaintexture.SetValue(pbrmat.textures.baseColorTexture));
                        setOperation(pbrmat.objectMetallicFactor.SetValue(pbrmat.metallicFactor));
                        setOperation(pbrmat.objectMetallicRoughnessTexture.SetValue(pbrmat.textures.metallicRoughnessTexture));
                        setOperation(pbrmat.objectMetallicTexture.SetValue(pbrmat.textures.metallicTexture));
                        setOperation(pbrmat.objectNormalTexture.SetValue(pbrmat.textures.normalTexture));
                        setOperation(pbrmat.objectNormalTextureScale.SetValue(pbrmat.textures.normalTexture.scale));
                        setOperation(pbrmat.objectOcclusionTexture.SetValue(pbrmat.textures.occlusionTexture));
                        setOperation(pbrmat.objectRoughnessFactor.SetValue(pbrmat.roughnessFactor));
                        setOperation(pbrmat.objectRoughnessTexture.SetValue(pbrmat.textures.roughnessTexture));
                        setOperation(pbrmat.objectShaderProperties.SetValue(pbrmat.shaderProperties));
                        setOperation(pbrmat.objectTextureTilingOffset.SetValue(pbrmat.tilingOffset));
                        setOperation(pbrmat.objectTextureTilingScale.SetValue(pbrmat.tilingScale));
                        break;
                    case ExternalResourceMaterial extmat:
                        break;
                    case OriginalMaterial extmat:
                        break;
                    default:
                        UMI3DLogger.LogWarning("unsupported material type", scope);
                        break;
                }
            }
        }

        private void ModelUpdate(UMI3DNode obj)
        {
            ////setOperation(obj.hasCollider)
            //// setOperation(obj.objectColliderBoxSize.SetValue(o)
            //setOperation(obj.objectColliderRadius.SetValue(obj.colliderRadius));
            //setOperation(obj.objectColliderCenter.SetValue(obj.colliderCenter));
            //setOperation(obj.objectColliderBoxSize.SetValue(obj.colliderBoxSize));
            //setOperation(obj.objectColliderDirection.SetValue(obj.colliderDirection));
            //setOperation(obj.objectColliderHeight.SetValue(obj.colliderHeight));
            //setOperation(obj.objectColliderType.SetValue(obj.colliderType));
            //setOperation(obj.objectCustomMeshCollider.SetValue(obj.customMeshCollider));
            //setOperation(obj.objectHasCollider.SetValue(obj.hasCollider));
            //setOperation(obj.objectIsConvexe.SetValue(obj.convex));
            //setOperation(obj.objectIsMeshCustom.SetValue(obj.isMeshCustom));
            //if (obj as AbstractRenderedNode)
            //{
            //    //setOperation(((AbstractRenderedNode)obj).objectMaterialsOverrided.SetValue(((AbstractRenderedNode)obj).overrideModelMaterials));
            //    //setOperation(((AbstractRenderedNode)obj).objectMaterialOverriders.SetValue(((AbstractRenderedNode)obj).materialsOverrider));
            //}
            //if (obj as UMI3DSubModel)
            //{
            //    setOperation(((UMI3DSubModel)obj).objectIgnoreModelMaterialOverride.SetValue(((UMI3DSubModel)obj).ignoreModelMaterialOverride));
            //}
        }

        private void UIUpdate(UIRect obj)
        {
            if (obj != null)
            {
                RectTransform rectTransform = obj.GetComponent<RectTransform>();
                setOperation(obj.AnchoredPosition.SetValue(rectTransform.anchoredPosition));
                setOperation(obj.AnchoredPosition3D.SetValue(rectTransform.anchoredPosition3D));
                setOperation(obj.AnchorMax.SetValue(rectTransform.anchorMax));
                setOperation(obj.AnchorMin.SetValue(rectTransform.anchorMin));
                setOperation(obj.OffsetMax.SetValue(rectTransform.offsetMax));
                setOperation(obj.OffsetMin.SetValue(rectTransform.offsetMin));
                setOperation(obj.Pivot.SetValue(rectTransform.pivot));
                setOperation(obj.SizeDelta.SetValue(rectTransform.sizeDelta));
                setOperation(obj.RectMask.SetValue(obj.GetComponent<RectMask2D>() != null));
                if (obj is UICanvas)
                {
                    var canvas = obj as UICanvas;
                    CanvasScaler canvasScaler = obj.GetComponent<CanvasScaler>();
                    Canvas Canvas = obj.GetComponent<Canvas>();
                    setOperation(canvas.DynamicPixelPerUnit.SetValue(canvasScaler.dynamicPixelsPerUnit));
                    setOperation(canvas.ReferencePixelPerUnit.SetValue(canvasScaler.referencePixelsPerUnit));
                    setOperation(canvas.OrderInLayer.SetValue(Canvas.sortingOrder));
                }
                if (obj is UIImage)
                {
                    var image = obj as UIImage;
                    Image Image = obj.GetComponent<Image>();
                    setOperation(image.Color.SetValue(Image.color));
                    setOperation(image.ImageType.SetValue(Image.type));
                    //update sprite
                }

                if (obj is UIText)
                {
                    var text = obj as UIText;
                    Text Text = obj.GetComponent<Text>();
                    setOperation(text.Alignment.SetValue(Text.alignment));
                    setOperation(text.AlignByGeometry.SetValue(Text.alignByGeometry));
                    setOperation(text.TextColor.SetValue(Text.color));
                    setOperation(text.TextFont.SetValue(Text.font));
                    setOperation(text.FontSize.SetValue(Text.fontSize));
                    setOperation(text.FontStyle.SetValue(Text.fontStyle));
                    setOperation(text.HorizontalOverflow.SetValue(Text.horizontalOverflow));
                    setOperation(text.VerticalOverflow.SetValue(Text.verticalOverflow));
                    setOperation(text.LineSpacing.SetValue(Text.lineSpacing));
                    setOperation(text.ResizeTextForBestFit.SetValue(Text.resizeTextForBestFit));
                    setOperation(text.ResizeTextMaxSize.SetValue(Text.resizeTextMaxSize));
                    setOperation(text.ResizeTextMinSize.SetValue(Text.resizeTextMinSize));
                    setOperation(text.SupportRichText.SetValue(Text.supportRichText));
                    setOperation(text.Text.SetValue(Text.text));
                }
            }
        }

        private void setOperation(SetEntityProperty operation)
        {
            if (operation != null)
            {

                try
                {
                    sets[operation.entityId][operation.property] = operation;
                }
                catch (Exception e)
                {
                    UMI3DLogger.LogError(e, scope);
                    UMI3DLogger.Log("entityid = " + operation.entityId, scope);
                    UMI3DLogger.Log("property = " + operation.property, scope);

                }
            }
        }
        public void RemoveNode(UMI3DNode node)
        {
            nodes = nodes.Where(n => !n.Equals(node)).ToArray();
        }
    }
}
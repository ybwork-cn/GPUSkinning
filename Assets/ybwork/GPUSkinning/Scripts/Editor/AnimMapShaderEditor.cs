﻿// Created by 月北(ybwork-cn) https://github.com/ybwork-cn/

using System;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

public class GPUSkinningShaderEditor : BaseShaderGUI
{
    private static readonly string[] workflowModeNames = Enum.GetNames(typeof(LitGUI.WorkflowMode));
    private LitGUI.LitProperties litProperties;
    private LitDetailGUI.DetailLitProperties litDetailProperties;

    //public static readonly GUIContent animMapTitle = EditorGUIUtility.TrTextContent("Anim Map");
    //public static readonly GUIContent animMapNormalTitle = EditorGUIUtility.TrTextContent("Anim Map Normal");
    //public static readonly string animLenTitle = "Anim Len";
    //public static readonly string loopPropTitle = "Loop";
    //public static readonly string currentTimeTitle = "Current Time";

    //protected MaterialProperty AnimMapProp { get; set; }
    //protected MaterialProperty AnimMapNormalProp { get; set; }
    //protected MaterialProperty AnimLenProp { get; set; }
    //protected MaterialProperty CurrentTimeProp { get; set; }
    //protected MaterialProperty LoopProp { get; set; }

    public override void FillAdditionalFoldouts(MaterialHeaderScopeList materialScopesList)
    {
        materialScopesList.RegisterHeaderScope(LitDetailGUI.Styles.detailInputs, Expandable.Details, delegate
        {
            LitDetailGUI.DoDetailArea(litDetailProperties, base.materialEditor);
        });
    }

    public override void FindProperties(MaterialProperty[] properties)
    {
        base.FindProperties(properties);
        litProperties = new LitGUI.LitProperties(properties);
        litDetailProperties = new LitDetailGUI.DetailLitProperties(properties);
        //AnimMapProp = FindProperty("_AnimMap", properties, propertyIsMandatory: false);
        //AnimMapNormalProp = FindProperty("_AnimMapNormal", properties, propertyIsMandatory: false);
        //AnimLenProp = FindProperty("_AnimLen", properties, propertyIsMandatory: false);
        //LoopProp = FindProperty("_Loop", properties, propertyIsMandatory: false);
        //CurrentTimeProp = FindProperty("_CurrentTime", properties, propertyIsMandatory: false);
    }

    public override void ValidateMaterial(Material material)
    {
        SetMaterialKeywords(material, LitGUI.SetMaterialKeywords, LitDetailGUI.SetMaterialKeywords);
    }

    public override void DrawSurfaceOptions(Material material)
    {
        EditorGUIUtility.labelWidth = 0f;
        if (litProperties.workflowMode != null)
        {
            DoPopup(LitGUI.Styles.workflowModeText, litProperties.workflowMode, workflowModeNames);
        }

        base.DrawSurfaceOptions(material);
    }

    public override void DrawSurfaceInputs(Material material)
    {
        //materialEditor.TexturePropertySingleLine(animMapTitle, AnimMapProp);
        //materialEditor.TexturePropertySingleLine(animMapNormalTitle, AnimMapNormalProp);
        //materialEditor.ShaderProperty(AnimLenProp, animLenTitle);
        //materialEditor.ShaderProperty(LoopProp, loopPropTitle);
        //materialEditor.ShaderProperty(CurrentTimeProp, currentTimeTitle);
        EditorGUILayout.Space();

        base.DrawSurfaceInputs(material);
        LitGUI.Inputs(litProperties, base.materialEditor, material);
        DrawEmissionProperties(material, keyword: true);
        BaseShaderGUI.DrawTileOffset(base.materialEditor, base.baseMapProp);
    }

    public override void DrawAdvancedOptions(Material material)
    {
        if (litProperties.reflections != null && litProperties.highlights != null)
        {
            base.materialEditor.ShaderProperty(litProperties.highlights, LitGUI.Styles.highlightsText);
            base.materialEditor.ShaderProperty(litProperties.reflections, LitGUI.Styles.reflectionsText);
        }

        base.DrawAdvancedOptions(material);
    }

    public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
    {
        if (material == null)
        {
            throw new ArgumentNullException("material");
        }

        if (material.HasProperty("_Emission"))
        {
            material.SetColor("_EmissionColor", material.GetColor("_Emission"));
        }

        base.AssignNewShaderToMaterial(material, oldShader, newShader);
        if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
        {
            BaseShaderGUI.SetupMaterialBlendMode(material);
            return;
        }

        SurfaceType surfaceType = SurfaceType.Opaque;
        BlendMode blendMode = BlendMode.Alpha;
        if (oldShader.name.Contains("/Transparent/Cutout/"))
        {
            surfaceType = SurfaceType.Opaque;
            material.SetFloat("_AlphaClip", 1f);
        }
        else if (oldShader.name.Contains("/Transparent/"))
        {
            surfaceType = SurfaceType.Transparent;
            blendMode = BlendMode.Alpha;
        }

        material.SetFloat("_Blend", (float)blendMode);
        material.SetFloat("_Surface", (float)surfaceType);
        if (surfaceType == SurfaceType.Opaque)
        {
            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }
        else
        {
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }

        if (oldShader.name.Equals("Standard (Specular setup)"))
        {
            material.SetFloat("_WorkflowMode", 0f);
            Texture texture = material.GetTexture("_SpecGlossMap");
            if (texture != null)
            {
                material.SetTexture("_MetallicSpecGlossMap", texture);
            }
        }
        else
        {
            material.SetFloat("_WorkflowMode", 1f);
            Texture texture2 = material.GetTexture("_MetallicGlossMap");
            if (texture2 != null)
            {
                material.SetTexture("_MetallicSpecGlossMap", texture2);
            }
        }
    }
}

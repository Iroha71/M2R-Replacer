using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class MRReplacer : EditorWindow
{
    private DefaultAsset vrmMaterialFolder = null;
    private const float M_OPAQUE = 0f, M_CUTOUT = 1f, M_TRANSPARENT = 2f, M_TRANSPARENT_Z = 3f;
    private const float CULL_OFF = 0f, CULL_FRONT = 1f, CULL_BACK = 2f;
    private const string REALTOON_SHADER_PATH_URP = "Universal Render Pipeline/RealToon/Version 5/Default/Default";
    private const string REALTOON_SHADER_PATH_BUILTIN = "RealToon/Version 5/Default/";
    private Color outlineColor;
    private enum RenderPipeline { BuiltIn, URP, HDRP };
    [MenuItem("Tools/M2R-MaterialReplacer")]
    private static void Create() {
        CreateWindow<MRReplacer>("M2R-MaterialReplacer");
    }

    private void OnGUI() {
        vrmMaterialFolder = (DefaultAsset)EditorGUILayout.ObjectField("VRM Material Folder", vrmMaterialFolder, typeof(DefaultAsset), false);
        
        outlineColor = EditorGUILayout.ColorField("Outline Color", outlineColor);

        if (GUILayout.Button("マテリアルを変換")) {
            List<Material> vrmMaterials = GetMtoons(vrmMaterialFolder);
            ReplaceRealToon(vrmMaterials);
        }
    }

    private void ReplaceRealToonBuiltIn(List<Material> vrmMaterials) {
        foreach (Material vrmMaterial in vrmMaterials) {
            float renderMode = vrmMaterial.GetFloat("_BlendMode");
            float cullMode = vrmMaterial.GetFloat("_CullMode");
            Color litColor = vrmMaterial.GetColor("_Color");
            Color shadeColor = vrmMaterial.GetColor("_ShadeColor");
            int renderQueue = vrmMaterial.renderQueue;
            string shaderType = renderMode == M_TRANSPARENT || renderMode == M_TRANSPARENT_Z ? "Fade Transparency" : "Default";
            vrmMaterial.shader = Shader.Find(REALTOON_SHADER_PATH_BUILTIN + shaderType);
            if (renderMode == M_CUTOUT) {
                vrmMaterial.SetFloat("_N_F_CO", 1f);
                vrmMaterial.SetFloat("_Cutout", 0.5f);
            }
            vrmMaterial.SetColor("_OutlineColor", outlineColor);
            vrmMaterial.SetFloat("_Culling", cullMode);
            vrmMaterial.renderQueue = renderQueue;
            vrmMaterial.SetColor("_MainColor", litColor);
            vrmMaterial.SetColor("_OverallShadowColor", shadeColor);
        }
    }

    private void ReplaceRealToon(List<Material> vrmMaterials) {
        if (vrmMaterialFolder == null || vrmMaterials.Count <= 0)
            return;

        if (!GraphicsSettings.renderPipelineAsset) {
            ReplaceRealToonBuiltIn(vrmMaterials);
            return;
        }

        foreach (Material vrmMaterial in vrmMaterials) {
            float renderMode = vrmMaterial.GetFloat("_BlendMode");
            float cullMode = vrmMaterial.GetFloat("_CullMode");
            Color litColor = vrmMaterial.GetColor("_Color");
            Color shadeColor = vrmMaterial.GetColor("_ShadeColor");
            int renderQueue = vrmMaterial.renderQueue;
            vrmMaterial.shader = Shader.Find(REALTOON_SHADER_PATH_URP);
            if (renderMode == M_CUTOUT) {
                vrmMaterial.SetFloat("_TRANSMODE", 1f);
                vrmMaterial.SetFloat("_N_F_CO", 1f);
                vrmMaterial.SetFloat("_Cutout", 0.5f);
            } else if (renderMode == M_TRANSPARENT) {
                vrmMaterial.SetFloat("_TRANSMODE", 1f);
                vrmMaterial.SetInt("_BleModSour", 5);
                vrmMaterial.SetInt("_BleModDest", 10);
            }
            vrmMaterial.SetColor("_OutlineColor", outlineColor);
            vrmMaterial.SetFloat("_Culling", cullMode);
            Undo.RecordObject(vrmMaterial, "Change material");
            vrmMaterial.renderQueue = renderQueue;
            vrmMaterial.SetColor("_MainColor", litColor);
            vrmMaterial.SetColor("_OverallShadowColor", shadeColor);
        }
    }

    private List<Material> GetMtoons(DefaultAsset vrmMaterialFolder) {
        string path = AssetDatabase.GetAssetOrScenePath(vrmMaterialFolder);
        string[] foundGuids = AssetDatabase.FindAssets("t:material", new string[] { path });

        return ConvertGuid2Material(foundGuids);
    }

    private List<Material> ConvertGuid2Material(string[] guids) {
        List<Material> foundMaterials = new List<Material>();
        if (guids.Length <= 0) {
            Debug.LogWarning("変換できるMtoonマテリアルがありませんでした");

            return foundMaterials;
        }

        foreach (string guid in guids) {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            foundMaterials.Add(material);
        }

        return foundMaterials;
    }
}

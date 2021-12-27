using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MRReplacer : EditorWindow
{
    private DefaultAsset vrmMaterialFolder = null;
    private const float M_OPAQUE = 0f, M_CUTOUT = 1f, M_TRANSPARENT = 2f, M_TRANSPARENT_Z = 3f;
    private const float CULL_OFF = 0f, CULL_FRONT = 1f, CULL_BACK = 2f;
    private const string REALTOON_SHADER_PATH_URP = "Universal Render Pipeline/RealToon/Version 5/Default/Default";
    private Color outlineColor;
    [MenuItem("Tools/M2R-MaterialReplacer")]
    private static void Create() {
        CreateWindow<MRReplacer>("M2R-MaterialReplacer");
    }

    private void OnGUI() {
        EditorGUI.BeginChangeCheck();
        vrmMaterialFolder = (DefaultAsset)EditorGUILayout.ObjectField("VRM Material Folder", vrmMaterialFolder, typeof(DefaultAsset), false);
        
        outlineColor = EditorGUILayout.ColorField("Outline Color", outlineColor);

        if (GUILayout.Button("マテリアルを変換")) {
            List<Material> vrmMaterials = GetMtoons(vrmMaterialFolder);
            ReplaceRealToon(vrmMaterials);
        }
    }

    private void ReplaceRealToon(List<Material> vrmMaterials) {
        if (vrmMaterialFolder == null || vrmMaterials.Count <= 0)
            return;

        foreach (Material vrmMaterial in vrmMaterials) {
            float renderMode = vrmMaterial.GetFloat("_BlendMode");
            float cullMode = vrmMaterial.GetFloat("_CullMode");
            vrmMaterial.shader = Shader.Find(REALTOON_SHADER_PATH_URP);
            switch (renderMode) {
                case M_CUTOUT:
                    vrmMaterial.SetFloat("_TRANSMODE", 1f);
                    vrmMaterial.SetFloat("_N_F_CO", 1f);
                    vrmMaterial.SetFloat("_Cutout", 0.5f);
                    break;
                case M_TRANSPARENT:
                    vrmMaterial.SetFloat("_TRANSMODE", 1f);
                    break;
            }
            vrmMaterial.SetColor("_OutlineColor", outlineColor);
            vrmMaterial.SetFloat("_Culling", cullMode);
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

namespace BuProduction.StylizedWater.Editor
{
    using UnityEditor;

    public class StylizedWaterEditor : ShaderGUI
    {
        bool showBaseSection = true;
        bool showMovementSection = true;
        bool showColorSection = true;
        bool showFoamSection = true;
     
     
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            EditorGUILayout.LabelField("Base", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            DrawBaseSettings(materialEditor, properties);
            
            EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            DrawMovementSettings(materialEditor, properties);
            
            EditorGUILayout.LabelField("Color", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            DrawColorSettings(materialEditor, properties);
            
            EditorGUILayout.LabelField("Foam", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            DrawFoamSettings(materialEditor, properties);
        }
        
        private void DrawBaseSettings(MaterialEditor editor, MaterialProperty[] properties)
        {
            MaterialProperty angle = FindProperty("_WindAngle", properties);
            MaterialProperty smoothness = FindProperty("_Smoothness", properties);
            MaterialProperty metallic = FindProperty("_Metallic", properties);
            MaterialProperty transparency = FindProperty("_Transparency", properties);
            MaterialProperty enableTransparency = FindProperty("_ENABLEMAXBYTRANSPARENCY", properties);
            MaterialProperty maxTransparencyByDistance = FindProperty("_MaxTransparencyByDistance", properties);
            
            editor.ShaderProperty(angle, "WindAngle");
            editor.ShaderProperty(smoothness, "Smoothness");
            editor.ShaderProperty(metallic, "Metallic");
            editor.ShaderProperty(transparency, "Transparency");
            editor.ShaderProperty(enableTransparency, "Enable Transparency By Distance");
            if (enableTransparency.floatValue == 1)
            {
                editor.ShaderProperty(maxTransparencyByDistance, "Max Transparency Distance");
            }
            
            EditorGUILayout.Separator();
        }

        private void DrawMovementSettings(MaterialEditor editor, MaterialProperty[] properties)
        {
            MaterialProperty hightFrequency = FindProperty("_HightFrequency", properties);
            MaterialProperty waveNoiseScale = FindProperty("_WaveNoiseScale", properties);
            MaterialProperty waveSpeed = FindProperty("_WaveSpeed", properties);
            MaterialProperty waveAmplitude = FindProperty("_WaveAmplitude", properties);
            
            editor.ShaderProperty(hightFrequency, "HightFrequency");
            editor.ShaderProperty(waveNoiseScale, "WaveNoiseScale");
            editor.ShaderProperty(waveSpeed, "WaveSpeed");
            editor.ShaderProperty(waveAmplitude, "WaveAmplitude");
            
            EditorGUILayout.Separator();
        }
        
        private void DrawColorSettings(MaterialEditor editor, MaterialProperty[] properties)
        {
            MaterialProperty enableDepth = FindProperty("_EnableDepth", properties);
            MaterialProperty shallowColor = FindProperty("_ShallowColor", properties);
            MaterialProperty deepColor = FindProperty("_DeepColor", properties);
            MaterialProperty deepColorFade = FindProperty("_DepthColorFade", properties);
            
            editor.ShaderProperty(enableDepth, "Enable Depth");
            editor.ShaderProperty(shallowColor, "ShallowColor");
            if (enableDepth.floatValue == 1)
            {
                editor.ShaderProperty(deepColor, "DeepColor");
                editor.ShaderProperty(deepColorFade, "DepthColorFade");
            }
            
            EditorGUILayout.Separator();
        }
        
        private void DrawFoamSettings(MaterialEditor editor, MaterialProperty[] properties)
        {
            MaterialProperty foam = FindProperty("_Foam", properties);
            MaterialProperty enableDepth = FindProperty("_EnableDepth", properties);
            MaterialProperty foamColor = FindProperty("_FoamColor", properties);
            MaterialProperty foamSpeed = FindProperty("_FoamSpeed", properties);
            MaterialProperty foamScale = FindProperty("_FoamScale", properties);
            MaterialProperty foamCutoff = FindProperty("_FoamCutoff", properties);
            MaterialProperty foamDeepStart = FindProperty("_FoamDeepStart", properties);
            
            editor.ShaderProperty(foam, "Foam");
            editor.ShaderProperty(foamColor, "FoamColor");
            editor.ShaderProperty(foamSpeed, "FoamSpeed");
            editor.ShaderProperty(foamScale, "FoamScale");
            if (enableDepth.floatValue == 1)
            {
                editor.ShaderProperty(foamCutoff, "FoamCutoff");
                editor.ShaderProperty(foamDeepStart, "FoamDeepStart");
            }
            
            EditorGUILayout.Separator();
        }
    }
}

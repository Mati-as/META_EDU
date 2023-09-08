using UnityEditor;

namespace BuProduction.StylizedWater.Editor
{
    public class StylizedWaterEditor : ShaderGUI
    {
        private bool showBaseSection = true;
        private bool showColorSection = true;
        private bool showFoamSection = true;
        private bool showMovementSection = true;


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
            var angle = FindProperty("_WindAngle", properties);
            var smoothness = FindProperty("_Smoothness", properties);
            var metallic = FindProperty("_Metallic", properties);
            var transparency = FindProperty("_Transparency", properties);
            var enableTransparency = FindProperty("_ENABLEMAXBYTRANSPARENCY", properties);
            var maxTransparencyByDistance = FindProperty("_MaxTransparencyByDistance", properties);

            editor.ShaderProperty(angle, "WindAngle");
            editor.ShaderProperty(smoothness, "Smoothness");
            editor.ShaderProperty(metallic, "Metallic");
            editor.ShaderProperty(transparency, "Transparency");
            editor.ShaderProperty(enableTransparency, "Enable Transparency By Distance");
            if (enableTransparency.floatValue == 1)
                editor.ShaderProperty(maxTransparencyByDistance, "Max Transparency Distance");

            EditorGUILayout.Separator();
        }

        private void DrawMovementSettings(MaterialEditor editor, MaterialProperty[] properties)
        {
            var hightFrequency = FindProperty("_HightFrequency", properties);
            var waveNoiseScale = FindProperty("_WaveNoiseScale", properties);
            var waveSpeed = FindProperty("_WaveSpeed", properties);
            var waveAmplitude = FindProperty("_WaveAmplitude", properties);

            editor.ShaderProperty(hightFrequency, "HightFrequency");
            editor.ShaderProperty(waveNoiseScale, "WaveNoiseScale");
            editor.ShaderProperty(waveSpeed, "WaveSpeed");
            editor.ShaderProperty(waveAmplitude, "WaveAmplitude");

            EditorGUILayout.Separator();
        }

        private void DrawColorSettings(MaterialEditor editor, MaterialProperty[] properties)
        {
            var enableDepth = FindProperty("_EnableDepth", properties);
            var shallowColor = FindProperty("_ShallowColor", properties);
            var deepColor = FindProperty("_DeepColor", properties);
            var deepColorFade = FindProperty("_DepthColorFade", properties);

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
            var foam = FindProperty("_Foam", properties);
            var enableDepth = FindProperty("_EnableDepth", properties);
            var foamColor = FindProperty("_FoamColor", properties);
            var foamSpeed = FindProperty("_FoamSpeed", properties);
            var foamScale = FindProperty("_FoamScale", properties);
            var foamCutoff = FindProperty("_FoamCutoff", properties);
            var foamDeepStart = FindProperty("_FoamDeepStart", properties);

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
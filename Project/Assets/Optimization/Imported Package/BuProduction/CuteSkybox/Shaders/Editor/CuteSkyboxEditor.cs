using System;
using UnityEditor;
using UnityEngine;

namespace BuProduction.CuteSkybox.Editor
{
    public class CuteSkyboxEditor : ShaderGUI
    {
        private static readonly string ENABLE_BUTTON = "Enable";
        private static readonly string DISABLE_BUTTON = "Disable";
        private static readonly string BUTTON_DESCRIPTION = "moon direction handle";

        private static bool _isGizmoEnabled;
        private static CuteSkyboxEditor _instance;

        private Quaternion _cachedRotation;
        private MaterialEditor _currentMaterialEditor;
        private MaterialProperty[] _currentMaterialProperties;
        private static Action<SceneView> _cachedSceneGUI => OnSceneGUIDelegate;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            _instance = this;
            _currentMaterialEditor = materialEditor;
            _currentMaterialProperties = properties;
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Cubemap Properties", EditorStyles.boldLabel);
            var cubemapToggle = FindProperty("_CUBEMAP", properties);
            materialEditor.ShaderProperty(cubemapToggle, "Cubemap");
            if (cubemapToggle.floatValue == 1)
            {
                var alphaCubemap = FindProperty("_EnableAlphaCubemap", properties);
                var cloudsOverAlphaCubemap = FindProperty("_CloudsOverAlphaCubemap", properties);
                var cubemapTexture = FindProperty("_CubemapTexture", properties);
                var cubemapOffset = FindProperty("_CubeMapOffset", properties);
                var cubemapRotation = FindProperty("_CubeMapRotation", properties);

                materialEditor.ShaderProperty(alphaCubemap, "Enable Alpha Cubemap");
                materialEditor.ShaderProperty(cloudsOverAlphaCubemap, "Clouds Over Alpha Cubemap");
                materialEditor.ShaderProperty(cubemapTexture, "CubemapTexture");
                materialEditor.ShaderProperty(cubemapOffset, "Cube Map Offset");
                materialEditor.ShaderProperty(cubemapRotation, "Cube Map Rotation");
            }

            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Global Color Properties", EditorStyles.boldLabel);
            var skyColor = FindProperty("_SkyColor", properties);
            var skyPower = FindProperty("_SkyPower", properties);
            materialEditor.ShaderProperty(skyColor, "Sky Color");
            materialEditor.ShaderProperty(skyPower, "Sky Power");

            var horizonColor = FindProperty("_HorizonColor", properties);
            var horizonOffset = FindProperty("_HorizonOffset", properties);
            materialEditor.ShaderProperty(horizonColor, "Horizon Color");
            materialEditor.ShaderProperty(horizonOffset, "Horizon Offset");

            var groundColor = FindProperty("_GroundColor", properties);
            var groundPower = FindProperty("_GroundPower", properties);
            materialEditor.ShaderProperty(groundColor, "Ground Color");
            materialEditor.ShaderProperty(groundPower, "Ground Power");
            EditorGUILayout.EndVertical();

            // Sun properties
            var sunToggle = FindProperty("_SUN", properties);
            var enableSunTextureToggle = FindProperty("_EnableSunTexture", properties);
            var sunTexture = FindProperty("_SunTexture", properties);
            var sunTextureRotation = FindProperty("_SunTextureRotation", properties);
            var sunColor = FindProperty("_SunColor", properties);
            var sunRadius = FindProperty("_SunRadius", properties);
            var sunHardness = FindProperty("_SunHardness", properties);
            var sunHeightOffsetDirection = FindProperty("_SunHeightOffsetDirection", properties);


            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Sun Properties", EditorStyles.boldLabel);
            materialEditor.ShaderProperty(sunToggle, "Sun");
            if (sunToggle.floatValue == 1)
            {
                materialEditor.ShaderProperty(enableSunTextureToggle, "Enable Sun Texture");
                if (enableSunTextureToggle.floatValue == 1)
                {
                    materialEditor.ShaderProperty(sunTexture, "Sun Texture");
                    materialEditor.ShaderProperty(sunTextureRotation, "Sun Texture Rotation");
                }

                materialEditor.ShaderProperty(sunColor, "Sun Color");
                materialEditor.ShaderProperty(sunHeightOffsetDirection, "Sun Height Offset Direction");
                materialEditor.ShaderProperty(sunRadius, "Sun Radius");
                materialEditor.ShaderProperty(sunHardness, "Sun Hardness");
            }

            EditorGUILayout.EndVertical();


            // Moon properties
            var moonToggle = FindProperty("_MOON", properties);
            var enableMoonTextureToggle = FindProperty("_EnableMoonTexture", properties);
            var moonTexture = FindProperty("_MoonTexture", properties);
            var moonTextureRotation = FindProperty("_MoonTextureRotation", properties);
            var moonDirection = FindProperty("_MoonDirection", properties);
            var moonColor = FindProperty("_MoonColor", properties);
            var moonRadius = FindProperty("_MoonRadius", properties);
            var moonHardness = FindProperty("_MoonHardness", properties);

            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Moon Properties", EditorStyles.boldLabel);
            materialEditor.ShaderProperty(moonToggle, "Moon");
            if (moonToggle.floatValue == 1)
            {
                materialEditor.ShaderProperty(enableMoonTextureToggle, "Enable Moon Texture");
                if (enableMoonTextureToggle.floatValue == 1)
                {
                    materialEditor.ShaderProperty(moonTexture, "Moon Texture");
                    materialEditor.ShaderProperty(moonTextureRotation, "Moon Texture Rotation");
                }

                GUI.enabled = false;
                materialEditor.ShaderProperty(moonDirection, "Moon Direction");
                GUI.enabled = true;
                if (GUILayout.Button(_isGizmoEnabled
                        ? $"{DISABLE_BUTTON} {BUTTON_DESCRIPTION}"
                        : $"{ENABLE_BUTTON} {BUTTON_DESCRIPTION}"))
                {
                    if (_isGizmoEnabled)
                        SceneView.duringSceneGui -= _cachedSceneGUI;
                    else
                        SceneView.duringSceneGui += _cachedSceneGUI;
                    _isGizmoEnabled = !_isGizmoEnabled;

                    SceneView.RepaintAll();
                    //moonPosition.vectorValue = 
                }

                materialEditor.ShaderProperty(moonColor, "Moon Color");
                materialEditor.ShaderProperty(moonRadius, "Moon Radius");
                materialEditor.ShaderProperty(moonHardness, "Moon Hardness");
            }
            else if (_isGizmoEnabled)
            {
                SceneView.duringSceneGui -= _cachedSceneGUI;
                _isGizmoEnabled = !_isGizmoEnabled;
            }


            EditorGUILayout.EndVertical();

            // Star properties
            var starToggle = FindProperty("_STARS", properties);
            var starRotationToggle = FindProperty("_STARROTATION", properties);
            var starTexture = FindProperty("_StarTexture", properties);
            var starColor = FindProperty("_StarColor", properties);
            var starSize = FindProperty("_StarSize", properties);
            var starOffset = FindProperty("_StarOffset", properties);
            var starDensity = FindProperty("_StarDensity", properties);
            var starRotationSpeed = FindProperty("_StarRotationSpeed", properties);
            var starBlinkSpeed = FindProperty("_StarBlinkSpeed", properties);
            var starBlinkContrast = FindProperty("_StarBlinkContrast", properties);


            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Stars Properties", EditorStyles.boldLabel);
            materialEditor.ShaderProperty(starToggle, "Stars");
            if (starToggle.floatValue == 1)
            {
                materialEditor.ShaderProperty(starTexture, "Star Texture");
                materialEditor.ShaderProperty(starColor, "Star Color");
                materialEditor.ShaderProperty(starSize, "Star Size");
                materialEditor.ShaderProperty(starOffset, "Star Offset");
                materialEditor.ShaderProperty(starDensity, "Star Density");

                materialEditor.ShaderProperty(starRotationToggle, "Stars Rotation");

                if (starRotationToggle.floatValue == 1)
                    materialEditor.ShaderProperty(starRotationSpeed, "Star Rotation Speed");

                materialEditor.ShaderProperty(starBlinkSpeed, "Star Blink Speed");
                materialEditor.ShaderProperty(starBlinkContrast, "Star Blink Contrast");
            }

            EditorGUILayout.EndVertical();


            var cloudState = FindProperty("_CLOUDSTATE", properties);


            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Clouds Properties", EditorStyles.boldLabel);
            materialEditor.ShaderProperty(cloudState, "Clouds");
            if (cloudState.floatValue != 2)
            {
                var cloudcolor = FindProperty("_CloudColor", properties);
                var cloudTexture1 = FindProperty("_CloudTexture1", properties);
                var cloudSpeed1 = FindProperty("_CloudSpeed1", properties);
                var cloudScale1 = FindProperty("_CloudScale1", properties);
                var cloudHorizonGradient = FindProperty("_CloudHorizonGradient", properties);
                var cloudHorizonOffset = FindProperty("_CloudHorizonOffset", properties);
                var cloudMoveAngle = FindProperty("_CloudMoveAngle", properties);

                materialEditor.ShaderProperty(cloudcolor, "Cloud Color");
                materialEditor.ShaderProperty(cloudTexture1, "Cloud Texture 1");
                materialEditor.ShaderProperty(cloudSpeed1, "Cloud Speed 1");
                materialEditor.ShaderProperty(cloudScale1, "Cloud Scale 1");

                if (cloudState.floatValue == 1)
                {
                    var cloudTexture2 = FindProperty("_CloudTexture2", properties);
                    var cloudSpeed2 = FindProperty("_CloudSpeed2", properties);
                    var cloudScale2 = FindProperty("_CloudScale2", properties);

                    materialEditor.ShaderProperty(cloudTexture2, "Cloud Texture 2");
                    materialEditor.ShaderProperty(cloudSpeed2, "Cloud Speed 2");
                    materialEditor.ShaderProperty(cloudScale2, "Cloud Scale 2");
                }

                materialEditor.ShaderProperty(cloudHorizonGradient, "Cloud Horizon Gradient");
                materialEditor.ShaderProperty(cloudHorizonOffset, "Cloud Horizon Offset");
                materialEditor.ShaderProperty(cloudMoveAngle, "Cloud Move Angle");
            }

            EditorGUILayout.EndVertical();
        }

        public override void OnClosed(Material material)
        {
            base.OnClosed(material);
            _isGizmoEnabled = false;
            SceneView.duringSceneGui -= _cachedSceneGUI;
        }

        private static void OnSceneGUIDelegate(SceneView sceneview)
        {
            if (_instance == null)
            {
                SceneView.duringSceneGui -= _cachedSceneGUI;
                _isGizmoEnabled = false;
                return;
            }


            EditorGUI.BeginChangeCheck();

            var moonPosition = FindProperty("_MoonDirection", _instance._currentMaterialProperties);
            //MaterialProperty moonRotation = FindProperty("_MoonRotation", _currentMaterialProperties);


            // if (moonPosition.vectorValue.magnitude <= 0f)
            //     moonPosition.vectorValue = Vector3.up;
            // else
            moonPosition.vectorValue = moonPosition.vectorValue.normalized;

            _instance._cachedRotation =
                Quaternion.LookRotation(moonPosition.vectorValue, _instance._cachedRotation * Vector3.up);
            _instance._cachedRotation = Handles.RotationHandle(_instance._cachedRotation, Vector3.zero);
            Handles.PositionHandle(Vector3.zero, _instance._cachedRotation);


            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_instance._currentMaterialEditor.target, "Test");
                moonPosition.vectorValue = _instance._cachedRotation * Vector3.forward;
            }

            // Vector4 tmpMoonPosition = moonPosition.vectorValue;
            // tmpMoonPosition.w = 0f;
            // moonPosition.vectorValue = tmpMoonPosition;
        }
    }
}
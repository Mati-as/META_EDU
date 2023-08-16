namespace BuProduction.CuteSkybox.Editor
{
    using System;
    using UnityEngine;
    using UnityEditor;

    public class CuteSkyboxEditor : ShaderGUI
    {
        private static string ENABLE_BUTTON = "Enable";
        private static string DISABLE_BUTTON = "Disable";
        private static string BUTTON_DESCRIPTION = "moon direction handle";

        private static bool _isGizmoEnabled;
        private static CuteSkyboxEditor _instance;
        private static Action<SceneView> _cachedSceneGUI => OnSceneGUIDelegate;
        private MaterialEditor _currentMaterialEditor;
        private MaterialProperty[] _currentMaterialProperties;

        private Quaternion _cachedRotation;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            _instance = this;
            _currentMaterialEditor = materialEditor;
            _currentMaterialProperties = properties;
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Cubemap Properties", EditorStyles.boldLabel);
            MaterialProperty cubemapToggle = FindProperty("_CUBEMAP", properties);
            materialEditor.ShaderProperty(cubemapToggle, "Cubemap");
            if (cubemapToggle.floatValue == 1)
            {
                MaterialProperty alphaCubemap = FindProperty("_EnableAlphaCubemap", properties);
                MaterialProperty cloudsOverAlphaCubemap = FindProperty("_CloudsOverAlphaCubemap", properties);
                MaterialProperty cubemapTexture = FindProperty("_CubemapTexture", properties);
                MaterialProperty cubemapOffset = FindProperty("_CubeMapOffset", properties);
                MaterialProperty cubemapRotation = FindProperty("_CubeMapRotation", properties);
                
                materialEditor.ShaderProperty(alphaCubemap, "Enable Alpha Cubemap");
                materialEditor.ShaderProperty(cloudsOverAlphaCubemap, "Clouds Over Alpha Cubemap");
                materialEditor.ShaderProperty(cubemapTexture, "CubemapTexture");
                materialEditor.ShaderProperty(cubemapOffset, "Cube Map Offset");
                materialEditor.ShaderProperty(cubemapRotation, "Cube Map Rotation");
            }

            EditorGUILayout.EndVertical();


            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Global Color Properties", EditorStyles.boldLabel);
            MaterialProperty skyColor = FindProperty("_SkyColor", properties);
            MaterialProperty skyPower = FindProperty("_SkyPower", properties);
            materialEditor.ShaderProperty(skyColor, "Sky Color");
            materialEditor.ShaderProperty(skyPower, "Sky Power");

            MaterialProperty horizonColor = FindProperty("_HorizonColor", properties);
            MaterialProperty horizonOffset = FindProperty("_HorizonOffset", properties);
            materialEditor.ShaderProperty(horizonColor, "Horizon Color");
            materialEditor.ShaderProperty(horizonOffset, "Horizon Offset");

            MaterialProperty groundColor = FindProperty("_GroundColor", properties);
            MaterialProperty groundPower = FindProperty("_GroundPower", properties);
            materialEditor.ShaderProperty(groundColor, "Ground Color");
            materialEditor.ShaderProperty(groundPower, "Ground Power");
            EditorGUILayout.EndVertical();

            // Sun properties
            MaterialProperty sunToggle = FindProperty("_SUN", properties);
            MaterialProperty enableSunTextureToggle = FindProperty("_EnableSunTexture", properties);
            MaterialProperty sunTexture = FindProperty("_SunTexture", properties);
            MaterialProperty sunTextureRotation = FindProperty("_SunTextureRotation", properties);
            MaterialProperty sunColor = FindProperty("_SunColor", properties);
            MaterialProperty sunRadius = FindProperty("_SunRadius", properties);
            MaterialProperty sunHardness = FindProperty("_SunHardness", properties);
            MaterialProperty sunHeightOffsetDirection = FindProperty("_SunHeightOffsetDirection", properties);


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
            MaterialProperty moonToggle = FindProperty("_MOON", properties);
            MaterialProperty enableMoonTextureToggle = FindProperty("_EnableMoonTexture", properties);
            MaterialProperty moonTexture = FindProperty("_MoonTexture", properties);
            MaterialProperty moonTextureRotation = FindProperty("_MoonTextureRotation", properties);
            MaterialProperty moonDirection = FindProperty("_MoonDirection", properties);
            MaterialProperty moonColor = FindProperty("_MoonColor", properties);
            MaterialProperty moonRadius = FindProperty("_MoonRadius", properties);
            MaterialProperty moonHardness = FindProperty("_MoonHardness", properties);

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
            MaterialProperty starToggle = FindProperty("_STARS", properties);
            MaterialProperty starRotationToggle = FindProperty("_STARROTATION", properties);
            MaterialProperty starTexture = FindProperty("_StarTexture", properties);
            MaterialProperty starColor = FindProperty("_StarColor", properties);
            MaterialProperty starSize = FindProperty("_StarSize", properties);
            MaterialProperty starOffset = FindProperty("_StarOffset", properties);
            MaterialProperty starDensity = FindProperty("_StarDensity", properties);
            MaterialProperty starRotationSpeed = FindProperty("_StarRotationSpeed", properties);
            MaterialProperty starBlinkSpeed = FindProperty("_StarBlinkSpeed", properties);
            MaterialProperty starBlinkContrast = FindProperty("_StarBlinkContrast", properties);


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


            MaterialProperty cloudState = FindProperty("_CLOUDSTATE", properties);


            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("Clouds Properties", EditorStyles.boldLabel);
            materialEditor.ShaderProperty(cloudState, "Clouds");
            if (cloudState.floatValue != 2)
            {
                MaterialProperty cloudcolor = FindProperty("_CloudColor", properties);
                MaterialProperty cloudTexture1 = FindProperty("_CloudTexture1", properties);
                MaterialProperty cloudSpeed1 = FindProperty("_CloudSpeed1", properties);
                MaterialProperty cloudScale1 = FindProperty("_CloudScale1", properties);
                MaterialProperty cloudHorizonGradient = FindProperty("_CloudHorizonGradient", properties);
                MaterialProperty cloudHorizonOffset = FindProperty("_CloudHorizonOffset", properties);
                MaterialProperty cloudMoveAngle = FindProperty("_CloudMoveAngle", properties);

                materialEditor.ShaderProperty(cloudcolor, "Cloud Color");
                materialEditor.ShaderProperty(cloudTexture1, "Cloud Texture 1");
                materialEditor.ShaderProperty(cloudSpeed1, "Cloud Speed 1");
                materialEditor.ShaderProperty(cloudScale1, "Cloud Scale 1");

                if (cloudState.floatValue == 1)
                {
                    MaterialProperty cloudTexture2 = FindProperty("_CloudTexture2", properties);
                    MaterialProperty cloudSpeed2 = FindProperty("_CloudSpeed2", properties);
                    MaterialProperty cloudScale2 = FindProperty("_CloudScale2", properties);

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

            MaterialProperty moonPosition = FindProperty("_MoonDirection", _instance._currentMaterialProperties);
            //MaterialProperty moonRotation = FindProperty("_MoonRotation", _currentMaterialProperties);


            // if (moonPosition.vectorValue.magnitude <= 0f)
            //     moonPosition.vectorValue = Vector3.up;
            // else
                moonPosition.vectorValue = moonPosition.vectorValue.normalized;

            _instance._cachedRotation = Quaternion.LookRotation(moonPosition.vectorValue, _instance._cachedRotation * Vector3.up);
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

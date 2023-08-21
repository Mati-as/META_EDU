using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesSpawner : MonoBehaviour {

    [System.Serializable]
    public struct ParticleEffect
    {
        public string name;
        public GameObject prefab;
        public Transform spawnPosition;
    }

    public ParticleEffect[] effects;
    public CameraRotation cameraScript;
    public GUIStyle guiStyle;



    GameObject loadedEffect;
    int loadedEffectID;

    private void Start()
    {
        LoadEffect(effects[0]);
    }


    private void OnGUI()
    {
        GUI.Label(new Rect(Screen.width / 2 - 75, 30, 150, 30), effects[loadedEffectID].name + " (" + (loadedEffectID + 1) + "/" + effects.Length + ")", guiStyle);
        if (GUI.Button(new Rect(Screen.width / 2 - 290, 30, 70, 30), "<"))
            LoadPrev();

        if (GUI.Button(new Rect(Screen.width / 2 + 220, 30, 70, 30), ">"))
            LoadNext();

        if (cameraScript != null)
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height - 70, 200, 50), "Camera distance", guiStyle);
            cameraScript.distance = GUI.HorizontalSlider(new Rect(Screen.width / 2 - 100, Screen.height - 20, 200, 50), cameraScript.distance, 2, 20);
        }
    }




    public void LoadNext()
    {
        if (loadedEffectID >= effects.Length - 1)
            return;

        loadedEffectID++;
        LoadEffect(effects[loadedEffectID]);
    }

    public void LoadPrev()
    {
        if (loadedEffectID < 1 )
            return;

        loadedEffectID--;
        LoadEffect(effects[loadedEffectID]);
    }


    void LoadEffect(ParticleEffect effect)
    {
        if (loadedEffect != null)
            DestroyImmediate(loadedEffect);

        loadedEffect = Instantiate(effect.prefab, effect.spawnPosition.position, effect.spawnPosition.rotation);
    }





}

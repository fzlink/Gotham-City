using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    float avgFrameRate;


    void Start()
    {
        #if UNITY_EDITOR
            UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
        #endif
    }

    private void Update()
    {
        avgFrameRate = Time.frameCount / Time.time;
        Debug.Log(avgFrameRate);
    }


}

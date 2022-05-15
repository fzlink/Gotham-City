using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightsSystem : MonoBehaviour
{
    private TrafficLightTrigger[] triggers;
    private int currentGreenIndex;
    [SerializeField] private float toggleWaitTime = 5f;

    private void Start()
    {
        triggers = GetComponentsInChildren<TrafficLightTrigger>();
        StartCoroutine(WaitForToggle());
    }

    private IEnumerator WaitForToggle()
    {
        while (true)
        {
            if (triggers.Length == 1)
                yield return new WaitForSeconds(toggleWaitTime);
            else
                yield return new WaitForSeconds(toggleWaitTime / 4f);
            triggers[currentGreenIndex].Toggle(false);
            int prevGreen = currentGreenIndex;
            currentGreenIndex = (currentGreenIndex + 1) % triggers.Length;
            yield return new WaitForSeconds(toggleWaitTime);
            triggers[prevGreen].Toggle(true);
        }
    }

}

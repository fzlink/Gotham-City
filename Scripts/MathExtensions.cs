using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class MathExtensions
{

    public static float[] GetDigitsNormalized(int number, int count)
    {
        float[] normalizedDigits = new float[count];
        string numberString = number.ToString();
        int digitCount;
        do
        {
            digitCount = numberString.Length;
            if (digitCount >= count)
                break;
            numberString = numberString.Insert(0, "0");
        } while (true);

        for (int i = 0; i < count; i++)
        {
            int digit = numberString[i] - '0';
            normalizedDigits[i] = ((float)digit) / 10f;
        }
        return normalizedDigits;
    }
    public static List<float> GetAngleDeltas(int amount, List<CheckpointSingle> checkpoints, int currentTargetCheckpoint, Transform carTransform)
    {
        List<float> deltas = new List<float>(amount);
        bool isLastBeforePark = false;
        for (int i = 0; i < amount; i++)
        {
            CheckpointSingle firstCheckpoint = null, secondCheckpoint = null;
            int index = currentTargetCheckpoint + i;
            if (index < checkpoints.Count)
                firstCheckpoint = checkpoints[index];
            /*if (index + 1 < checkpoints.Count)
                secondCheckpoint = checkpoints[index + 1];*/
            if (firstCheckpoint != null && !isLastBeforePark)
            {
                Vector3 fCP = firstCheckpoint.transform.position;
                //Vector3 sCP = secondCheckpoint.transform.position;
                Vector3 carPos = carTransform.position;
                fCP.y = carPos.y = 0f;
                Vector3 dir = fCP - carPos;
                deltas.Add(Vector3.SignedAngle(carTransform.forward, dir, Vector3.up) / 180f);
                //deltas.Add(Mathf.DeltaAngle(firstCheckpoint.transform.eulerAngles.y, secondCheckpoint.transform.eulerAngles.y));
                isLastBeforePark = firstCheckpoint.GetIsLastBeforeParking();
            }
            else
            {
                if (i > 0)
                    deltas.Add(deltas[i - 1]);
                else
                    deltas.Add(0f);
            }
        }
        return deltas;
    }
}


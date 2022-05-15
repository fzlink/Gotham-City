using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ParkingSpotManager : MonoBehaviour
{
    [SerializeField] private List<ParkingSpot> spots;
    private int prevRandomIndex = -1;

    public ParkingSpot GetRandomFreeParkingSpot()
    {
        //return spots[0];
        var shuffledSpots = spots.OrderBy(x => UnityEngine.Random.value).ToList();
        return shuffledSpots.Where(x => !x.IsOccupied).FirstOrDefault();
    }

    public ParkingSpot GetFreeParkingSpot()
    {
        return spots.Where(x => !x.IsOccupied).FirstOrDefault();
    }

    public void FillRandomly()
    {
        if(UnityEngine.Random.value < 0.5f)
        {
            EmptySpots();
            return;
        }
        int randomSpotIndex;
        while (true)
        {
            randomSpotIndex = UnityEngine.Random.Range(0, spots.Count);
            if (prevRandomIndex == -1 || randomSpotIndex != prevRandomIndex)
            {
                prevRandomIndex = randomSpotIndex;
                break;
            }
        }
        for (int i = 0; i < spots.Count; i++)
        {
            if (i == randomSpotIndex)
            {
                spots[i].ToggleFill(false);
            }
            else
            {
                spots[i].ToggleFill(true);
            }
        }
    }

    private void EmptySpots()
    {
        for (int i = 0; i < spots.Count; i++)
        {
            spots[i].ToggleFill(false);
        }
    }
}

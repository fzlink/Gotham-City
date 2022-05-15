using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class CarComponentBase : MonoBehaviour
{
    protected CarAgent carAgent;

    protected virtual void Awake()
    {
        carAgent = GetComponent<CarAgent>();
        carAgent.OnCarEpisodeBegin += OnCarEpisodeBegin;
    }

    /*private void OnEnable()
    {
        carAgent.OnCarEpisodeBegin += OnCarEpisodeBegin;
    }*/

    private void OnDestroy()
    {
        carAgent.OnCarEpisodeBegin -= OnCarEpisodeBegin;
    }

    protected abstract void OnCarEpisodeBegin();
}


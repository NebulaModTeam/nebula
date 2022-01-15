using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitDSPCode : MonoBehaviour
{
    private void Awake()
    {
        VFInput.Init();
    }
    
    private void Update()
    {
        VFInput.OnUpdate();
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowManager : MonoBehaviour
{
    public Material glow, nonglow;
    bool isGlowing = true;

    public void EnableGlow()
    {
        gameObject.GetComponent<MeshRenderer>().material = glow;
        isGlowing = false;
    }

    public void DisableGlow()
    {
        gameObject.GetComponent<MeshRenderer>().material = nonglow;
        isGlowing = true;
    }    
}

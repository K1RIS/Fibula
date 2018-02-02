using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicWall : MonoBehaviour
{
    private float time;

    void Update()
    {
        time += Time.deltaTime;

        if (time >= 20f)
        {
            Destroy(gameObject);
        }
    }
}

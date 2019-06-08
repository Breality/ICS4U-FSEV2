using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapOffsetPositioning : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private Transform cam;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = cam.localPosition;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSpin : MonoBehaviour
{
    IEnumerator spin()
    {
        while (true)
        {
            transform.Rotate(new Vector3(0, 0.15f, 0));
            yield return new WaitForSeconds(0.01f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(spin());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

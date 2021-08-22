using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waiter : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject This;
    void Start()
    {
        StartCoroutine(waiter());


    }

    // Update is called once per frame
    void Update()
    {
        
    }


    IEnumerator waiter()
    {

        yield return new WaitForSecondsRealtime(4);

        This.SetActive(true);
    }
}

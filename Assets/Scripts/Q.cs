using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Q : MonoBehaviour
{
    [SerializeField] float lifeTime;

    public string side;

    // Update is called once per frame
    void Update()
    {
        transform.position += gameObject.transform.forward * 0.08f;

        if (lifeTime > 0)
        {
            lifeTime -= Time.deltaTime;
        }
        
        if (lifeTime <= 0)
        {
            Destroy(gameObject);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != side)
        {
            TestDummy test;
            if (other.gameObject.TryGetComponent<TestDummy>(out test))
            {
                
            }
        }
    }
}

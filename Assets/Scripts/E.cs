using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public class E : MonoBehaviour
{
    [SerializeField] float lifeTime;
    public string side;
    public PlayerController host;

    void Update()
    {
        transform.position += gameObject.transform.forward * 0.05f;

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
        if (other.tag != side)
        {
            // TODO TRAP ENEMY

            TestDummy test;
            if (other.gameObject.TryGetComponent<TestDummy>(out test))
            {
                Destroy(this.gameObject);
                test.Netted();
                host.attackTimer = 0;
            }

        }
    }
}

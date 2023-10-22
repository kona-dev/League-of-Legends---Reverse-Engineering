using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Trap : MonoBehaviour
{
    public string side;

    private float setupTimer;
    private bool isPrimed;

    public PlayerController host;

    // Start is called before the first frame update
    void Start()
    {
        setupTimer = 1f;
        isPrimed = false;
    }

    private void Update()
    {
        if (setupTimer > 0) { setupTimer -= Time.deltaTime; }
        if (setupTimer <= 0 && !isPrimed) isPrimed = true;
    }

    private void OnTriggerEnter(Collider other)
    {
       if (other.tag != side && isPrimed)
        {
            // TODO TRAP ENEMY

            TestDummy test;
            if (other.gameObject.TryGetComponent<TestDummy>(out test))
            {
                Destroy(this.gameObject);
                test.trapped = true;
                GameObject.Find("AudioManager").GetComponent<AudioManager>().PlayUnique("Trapped");
                
            }

        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag != side && isPrimed)
        {
            // TODO TRAP ENEMY

            TestDummy test;
            if (other.gameObject.TryGetComponent<TestDummy>(out test))
            {
                Destroy(this.gameObject);
                test.Trapped();
                GameObject.Find("AudioManager").GetComponent<AudioManager>().PlayUnique("Trapped");

            }

        }
    }

}

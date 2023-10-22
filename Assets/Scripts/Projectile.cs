using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject target;

    public float speed = 15;

    private void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        Vector2 direction = target.transform.position - transform.position;
        transform.position += (target.transform.position - transform.position).normalized * Time.deltaTime * speed;
        transform.LookAt(target.transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == target) Destroy(this.gameObject);
    }
}

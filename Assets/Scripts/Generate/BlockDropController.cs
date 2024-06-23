using System.Collections;
using UnityEngine;

public class BlockDropController : MonoBehaviour
{
    public ItemClass item;
    private void OnTriggerEnter2D(Collider2D colli)
    {
        if (colli.gameObject.CompareTag("Player"))
        {
            if (colli.GetComponent<Inventory>().Add(item))
            {
                 Destroy(this.gameObject);
            }
        }
    }
}

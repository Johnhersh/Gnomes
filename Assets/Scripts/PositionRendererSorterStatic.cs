using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionRendererSorterStatic : MonoBehaviour
{
    [SerializeField]
    private int sortingOrderBase = 5000;
    [SerializeField]
    private int offset = 0;

    private Renderer myRenderer;

    // Start is called before the first frame update
    void Start()
    {
        myRenderer = gameObject.GetComponent<Renderer>();
        myRenderer.sortingOrder = (int)( sortingOrderBase - transform.position.y - offset );
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

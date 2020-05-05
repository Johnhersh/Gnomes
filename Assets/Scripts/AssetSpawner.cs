using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AssetSpawner : MonoBehaviour
{
    public GameObject Top;
    public GameObject Middle;
    public GameObject Bottom;

    public List<Sprite> TopSprites;

    // Start is called before the first frame update
    void Start()
    {
        ChangeTopSprites();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void ChangeTopSprites()
    {
        Top.GetComponent<SpriteRenderer>().sprite = TopSprites[Random.Range(0, TopSprites.Count)];
        if (Random.value < 0.5f)
        {
            Top.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            Top.transform.localScale = new Vector3(1, 1, 1);
        }
    }
}

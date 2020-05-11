using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [ExecuteInEditMode]
public class AssetRandomizer : MonoBehaviour
{
    public List<GameObject> SourceObjects;

    public List<Sprite> TopSprites;
    public List<Sprite> MidSprites;
    public List<Sprite> BotSprites;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < SourceObjects.Count; i++)
        {
            switch (i)
            {
                case 0:
                    SourceObjects[i].GetComponent<SpriteRenderer>().sprite = TopSprites[Random.Range(0, TopSprites.Count)];
                    break;
                case 1:
                    SourceObjects[i].GetComponent<SpriteRenderer>().sprite = MidSprites[Random.Range(0, MidSprites.Count)];
                    break;
                case 2:
                    SourceObjects[i].GetComponent<SpriteRenderer>().sprite = BotSprites[Random.Range(0, BotSprites.Count)];
                    break;
            }
        }
    }
}

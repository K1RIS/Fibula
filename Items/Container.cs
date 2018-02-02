using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Container : MonoBehaviour
{
    public Image windowImage;
    public Image cellImage;
    public Image backgroundImage;
    private int size = 16;
    public List<GameObject> items;
    public List<Image> images;

    private string containerName;

    private void Awake()
    {
        items = new List<GameObject>();
        containerName = "bp";
    }

    public void open()
    {
        Image window = Instantiate(windowImage);
        window.transform.SetParent(backgroundImage.transform);
        window.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -100f);
        containerName = "bp" + (backgroundImage.transform.childCount - 1);
        window.name = containerName;

        Vector3[] corners = new Vector3[4];
        window.GetComponent<RectTransform>().GetWorldCorners(corners);

        Vector2 cellTransform = new Vector2(corners[1].x + 30, corners[1].y - 25);

        Transform background = backgroundImage.transform.GetChild(backgroundImage.transform.childCount - 1);

        for (int i = 1; i < (size + 1); i++)
        {
            Image cell = Instantiate(cellImage);

            cell.name = "cell " + i;
            cell.GetComponent<RectTransform>().anchoredPosition = cellTransform;
            cell.transform.SetParent(background);
            images.Add(cellImage);
            cellTransform.x += 45f;
            if (i % 4 == 0)
            {
                cellTransform.x -= 180f;
                cellTransform.y -= 45f;
            }
        }
    }

    public void close()
    {
        Destroy(backgroundImage.transform.Find(containerName).gameObject);
    }

    public void put(GameObject item)
    {
        if (items.Count < 16)
        {
            images[items.Count].color = Color.green;
            items.Insert(0, item);
        }
    }

    public void remove(int i)
    {
        items.RemoveAt(i - 1);
        images[items.Count].color = Color.red;
    }

    public GameObject get(string s)
    {
        int asd = int.Parse(s.Split(' ')[1]);

        if (asd <= items.Count)
        {
            return items[asd - 1];
        }
        return null;
    }

    public bool contains(GameObject a)
    {
        if (items.Contains(a))
        {
            return true;
        }
        return false;
    }
}

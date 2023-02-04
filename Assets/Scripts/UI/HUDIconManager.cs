using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDIconManager : MonoBehaviour
{

    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private Transform  iconHolder;

    static HUDIconManager instance;

    class IconObject
    {
        public GameObject       icon;
        public Color            color;
        public bool             blink;
        public bool             displayOnScreen;
        public SpriteRenderer   spriteRenderer;
        public Transform        sourceTransform;
    }

        List<IconObject> icons = new List<IconObject>();
    new Camera           camera;
        Vector2          halfSize;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        camera = GetComponent<Camera>();
        halfSize = new Vector2();
        halfSize.y = camera.orthographicSize;
        halfSize.x = camera.aspect * halfSize.y;
    }

    void Update()
    {
        foreach (var icon in icons)
        {
            if (icon.sourceTransform == null)
            {
                // This one was removed
                Destroy(icon.icon);
            }
            else
            {
                if (icon.blink)
                {
                    icon.spriteRenderer.color = (icon.color + icon.color * 0.25f * Mathf.Cos(Time.time * 10.0f)).Clamp().ChangeAlpha(icon.color.a);
                }
                UpdateIconPosition(icon);
            }
        }

        icons.RemoveAll((icon) => icon.sourceTransform == null);
    }

    GameObject _AddIcon(Sprite image, Color color, Transform pos, float scale, bool blink, bool displayOnScreen)
    {
        GameObject go = Instantiate(iconPrefab, pos.position, Quaternion.identity);
        go.transform.SetParent(iconHolder);
        go.transform.localScale *= scale;
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        sr.color = color;
        sr.sprite = image;        

        var tmp = new IconObject { icon = go, color = color, blink = blink, displayOnScreen = displayOnScreen, spriteRenderer = sr, sourceTransform = pos };
        icons.Add(tmp);

        UpdateIconPosition(tmp);

        return go;
    }

    void UpdateIconPosition(IconObject icon)
    {
        // Check if inside the screen
        Vector3 cameraViewportPos = camera.WorldToViewportPoint(icon.sourceTransform.position);
        if ((cameraViewportPos.x >= 0) && (cameraViewportPos.x <= 1) && (cameraViewportPos.y >= 0) && (cameraViewportPos.y <= 1))
        {
            if (icon.displayOnScreen)
            {
                // Inside the screen
                icon.icon.transform.position = icon.sourceTransform.position;
                icon.icon.SetActive(true);
            }
            else
            {
                icon.icon.SetActive(false);
            }
        }
        else
        {
            icon.icon.SetActive(true);

            Vector3 cp = transform.position;
            // Project to edges
            Vector3 dir = (icon.sourceTransform.position - transform.position);
            Vector3 pos = Vector3.zero;
            float minDist = float.MaxValue;
            Vector3 intersection = Vector3.zero;
            if (dir.x > 0)
            {
                if (Line.Intersect2d(transform.position, icon.sourceTransform.position, new Vector3(cp.x + halfSize.x, cp.y - halfSize.y, 0.0f), new Vector3(cp.x + halfSize.x, cp.y + halfSize.y, 0.0f), out intersection))
                {
                    float d = Vector3.Distance(transform.position, intersection);
                    if (d < minDist)
                    {
                        minDist = d;
                        pos = intersection;
                    }
                }
            }
            else
            {
                if (Line.Intersect2d(transform.position, icon.sourceTransform.position, new Vector3(cp.x - halfSize.x, cp.y - halfSize.y, 0.0f), new Vector3(cp.x - halfSize.x, cp.y + halfSize.y, 0.0f), out intersection))
                {
                    float d = Vector3.Distance(transform.position, intersection);
                    if (d < minDist)
                    {
                        minDist = d;
                        pos = intersection;
                    }
                }
            }
            if (dir.y > 0)
            {
                if (Line.Intersect2d(transform.position, icon.sourceTransform.position, new Vector3(cp.x - halfSize.x, cp.y + halfSize.y, 0.0f), new Vector3(cp.x + halfSize.x, cp.y + halfSize.y, 0.0f), out intersection))
                {
                    float d = Vector3.Distance(transform.position, intersection);
                    if (d < minDist)
                    {
                        minDist = d;
                        pos = intersection;
                    }
                }
            }
            else
            {
                if (Line.Intersect2d(transform.position, icon.sourceTransform.position, new Vector3(cp.x - halfSize.x, cp.y - halfSize.y, 0.0f), new Vector3(cp.x + halfSize.x, cp.y - halfSize.y, 0.0f), out intersection))
                {
                    float d = Vector3.Distance(transform.position, intersection);
                    if (d < minDist)
                    {
                        minDist = d;
                        pos = intersection;
                    }
                }
            }
            if (minDist != float.MaxValue)
            {
                icon.icon.transform.position = pos + (transform.position - pos).normalized * 32.0f;
            }
        }
    }

    void _RemoveIcon(GameObject icon)
    {
        for (int i = 0; i < icons.Count; i++) 
        {
            if (icons[i].icon == icon)
            {
                Destroy(icons[i].icon);
                icons.RemoveAt(i);
                return;
            }
        }
    }

    void _SetPos(GameObject icon, Transform pos)
    {
        for (int i = 0; i < icons.Count; i++)
        {
            if (icons[i].icon == icon)
            {
                icons[i].sourceTransform = pos;
                return;
            }
        }
    }

    public static GameObject AddIcon(Sprite image, Color color, Transform pos, float scale = 1.0f, bool blink = false, bool displayOnScreen = true)
    {
        return instance._AddIcon(image, color, pos, scale, blink, displayOnScreen);
    }

    public static void RemoveIcon(GameObject icon)
    {
        instance._RemoveIcon(icon);
    }

    public static void SetPos(GameObject icon, Transform pos)
    {
        instance._SetPos(icon, pos);
    }
}

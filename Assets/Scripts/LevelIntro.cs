using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelIntro : MonoBehaviour
{
        public bool         runIntro;
        public Transform    startPos;
        public Transform    endPos;
        public float        initialSize;
        public float        zoomAcceleration = 5.0f;
        public float        maxZoomSpeed = 400.0f;
        public float        travellingSpeed = 100.0f;
    new public Camera       camera;
        public Player       player;


    // Start is called before the first frame update
    void Start()
    {
        if (runIntro)
        {
            StartCoroutine(LevelIntroCR());
        }
        else
        {
            player.transform.parent.gameObject.SetActive(true);
            Destroy(gameObject);
        }
    }

    IEnumerator LevelIntroCR()
    {
        float prevSize = camera.orthographicSize;
        float z = camera.transform.position.z;
        camera.transform.position = new Vector3(startPos.position.x, startPos.position.y, z);

        camera.orthographicSize = initialSize;

        CameraFollow cf = camera.GetComponent<CameraFollow>();
        cf.enabled = false;

        yield return new WaitForSeconds(0.25f);

        float zoomSpeed = 0.0f;
        float targetSize = prevSize + (initialSize - prevSize) * 0.5f;

        while (camera.orthographicSize > targetSize)
        {
            camera.orthographicSize = camera.orthographicSize - Time.deltaTime * zoomSpeed;

            zoomSpeed = Mathf.Clamp(zoomSpeed + zoomAcceleration * Time.deltaTime, 0.0f, maxZoomSpeed);

            yield return null;
        }

        while (camera.transform.position.y > endPos.transform.position.y)
        {
            if (camera.orthographicSize > prevSize)
            {
                camera.orthographicSize = camera.orthographicSize - Time.deltaTime * zoomSpeed;
            }
            else
            {
                camera.orthographicSize = prevSize;
            }

            camera.transform.position = Vector3.MoveTowards(camera.transform.position, endPos.position, Time.deltaTime * travellingSpeed);

            yield return null;
        }

        cf.enabled = true;
        cf.targetObject = player.transform;
        camera.orthographicSize = prevSize;
        player.transform.parent.gameObject.SetActive(true);
        Destroy(gameObject);
    }
}

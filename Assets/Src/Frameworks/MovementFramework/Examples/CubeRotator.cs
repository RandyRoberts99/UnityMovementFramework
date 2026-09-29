using UnityEngine;

public class CubeRotator : MonoBehaviour
{
    Transform cubeTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cubeTransform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void FixedUpdate()
    {
        cubeTransform.Rotate(Vector3.up * Time.fixedDeltaTime * 50f);
    }
}

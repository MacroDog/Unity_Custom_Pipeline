using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanceColor : MonoBehaviour
{
    static MaterialPropertyBlock propertyBlock;
    static int colorID = Shader.PropertyToID("_Color");
    [SerializeField]
    Color color;
    // Start is called before the first frame update

    void OnValidate()
    {
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
        propertyBlock.SetColor(colorID, color);
        GetComponent<MeshRenderer>().SetPropertyBlock(propertyBlock);
    }
    void Awake()
    {
        OnValidate();
    }
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }
}

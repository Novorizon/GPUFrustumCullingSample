using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class FrustumCull
{
    public TextAsset TextAsset;
    public Mesh mesh;
    public Material material;

}
[Serializable]
public class FCObject
{
    public string name;
    public List<Vector3> positions;
}
public class FrustumCulling : MonoBehaviour
{
    public ComputeShader shader;
    ComputeBuffer bufferWithArgs;
    private uint[] args;
    private int ShaderId;
    private Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 10000);
    ComputeBuffer posBuffer;


    public TextAsset TextAsset;
    public Mesh mesh;
    public Material material;
    int count;

    void Start()
    {
        FCObject data = JsonUtility.FromJson<FCObject>(TextAsset.text);
        count = data.positions.Count;

        args = new uint[] { mesh.GetIndexCount(0), 0, 0, 0, 0 };
        bufferWithArgs = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
        bufferWithArgs.SetData(args);
        ShaderId = shader.FindKernel("FrustumCulling");

        posBuffer = new ComputeBuffer(count, 4 * 3);
        posBuffer.SetData(data.positions);
        var posVisibleBuffer = new ComputeBuffer(count, 4 * 3);
        shader.SetBuffer(ShaderId, "bufferWithArgs", bufferWithArgs);
        shader.SetBuffer(ShaderId, "posAllBuffer", posBuffer);
        shader.SetBuffer(ShaderId, "posVisibleBuffer", posVisibleBuffer);
        material.SetBuffer("posVisibleBuffer", posVisibleBuffer);

    }


    void Update()
    {
        args[1] = 0;
        bufferWithArgs.SetData(args);

        shader.SetVector("cmrPos", Camera.main.transform.position);
        shader.SetVector("cmrDir", Camera.main.transform.forward);
        shader.SetFloat("cmrHalfFov", Camera.main.fieldOfView / 2);
        var m = GL.GetGPUProjectionMatrix(Camera.main.projectionMatrix, false) * Camera.main.worldToCameraMatrix;
        shader.SetMatrix("matrix_VP", m);
        shader.Dispatch(ShaderId, count / 64, 1, 1);

        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, bufferWithArgs, 0, null, ShadowCastingMode.Off, false);
    }

}

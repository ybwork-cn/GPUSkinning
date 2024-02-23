﻿using UnityEngine;

[System.Serializable]
public class GPUSkinningData
{
    public bool Loop;
    public int AnimIndex;
    public float CurrentTime;

    public bool LastAnimLoop;
    public int LastAnimIndex;
    public float LastAnimExitTime;

    private readonly Material _material;

    public GPUSkinningData(Material material)
    {
        this._material = material;
    }

    public void SwitchState(int state, bool loop)
    {
        LastAnimLoop = Loop;
        LastAnimIndex = AnimIndex;
        LastAnimExitTime = CurrentTime;
        Loop = loop;
        AnimIndex = state;
        CurrentTime = 0;

        _material.SetFloat("_Loop", Loop ? 1 : 0);
        _material.SetFloat("_AnimIndex", AnimIndex);
        _material.SetFloat("_CurrentTime", CurrentTime);

        _material.SetFloat("_LastAnimLoop", LastAnimLoop ? 1 : 0);
        _material.SetFloat("_LastAnimIndex", LastAnimIndex);
        _material.SetFloat("_LastAnimExitTime", LastAnimExitTime);
    }

    public void Update(float deltaTime)
    {
        CurrentTime += deltaTime;
        _material.SetFloat("_CurrentTime", CurrentTime);
    }
}

public class GPUSkinningComponent : MonoBehaviour
{
    Material _material;

    [SerializeField] GPUSkinningData _gpuSkinningData;
    public GPUSkinningData GpuSkinningData => _gpuSkinningData;

    private void Awake()
    {
        _material = GetComponent<MeshRenderer>().material;
        _gpuSkinningData = new GPUSkinningData(_material);
        GpuSkinningData.SwitchState(0, true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
            GpuSkinningData.SwitchState(0, true);
        if (Input.GetKeyDown(KeyCode.Keypad2))
            GpuSkinningData.SwitchState(1, true);
        if (Input.GetKeyDown(KeyCode.Keypad3))
            GpuSkinningData.SwitchState(2, false);

        GpuSkinningData.Update(Time.deltaTime);
    }
}
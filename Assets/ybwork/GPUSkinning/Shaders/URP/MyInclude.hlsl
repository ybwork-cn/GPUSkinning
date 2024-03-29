// Created by 月北(ybwork) https://github.com/ybwork-cn/

// 用于普通渲染
// #define GetProp(t) t
// 用于GPU合批
#define GetProp(t) UNITY_ACCESS_INSTANCED_PROP(Props, t)

float GetLoopTime(float t, float duration)
{
    return frac(t / duration) * duration;
}

float GetClampTime(float t, float duration)
{
    return clamp(t / duration, 0, 1) * duration;
}

float GetCurrentTime()
{
    float startTime = tex2Dlod(_AnimInfosMap, float4((GetProp(_AnimIndex)  + 0.5) * _AnimInfosMap_TexelSize.x, 0.25, 0, 0)).r;
    float duration = tex2Dlod(_AnimInfosMap, float4((GetProp(_AnimIndex)  + 0.5) * _AnimInfosMap_TexelSize.x, 0.75, 0, 0)).r;

    float t = GetProp(_CurrentTime);
    if(GetProp(_Loop))
        return startTime + GetLoopTime(t, duration);
    else
        return startTime + GetClampTime(t, duration);
}

float GetExitTime(out float progress)
{
    if(GetProp(_LastAnimIndex) < 0)
    {
        progress = 1;
        return 0;
    }

    float startTime = tex2Dlod(_AnimInfosMap, float4((GetProp(_LastAnimIndex)  + 0.5) * _AnimInfosMap_TexelSize.x, 0.25, 0, 0)).r;
    float duration = tex2Dlod(_AnimInfosMap, float4((GetProp(_LastAnimIndex)  + 0.5) * _AnimInfosMap_TexelSize.x, 0.75, 0, 0)).r;

    float lastAnimExitTime;
    if(GetProp(_LastAnimLoop))
        lastAnimExitTime = GetLoopTime(GetProp(_LastAnimExitTime), duration);
    else
        lastAnimExitTime = GetClampTime(GetProp(_LastAnimExitTime), duration);
    progress = GetProp(_CurrentTime) / 0.3;
    progress = clamp(progress, 0, 1);
    float t = lastAnimExitTime + GetProp(_CurrentTime);
    return startTime + GetClampTime(t, duration);
}

float4x4 GetMatrix(uint matrixIndex)
{
    float fullDuration = 1.0 / _BoneMap_TexelSize.y / 30;
    float t_now = GetCurrentTime() / fullDuration;
    float4x4 mat_now = float4x4(
        tex2Dlod(_BoneMap, float4((matrixIndex * 4 + 0 + 0.5) * _BoneMap_TexelSize.x, t_now, 0, 0)),
        tex2Dlod(_BoneMap, float4((matrixIndex * 4 + 1 + 0.5) * _BoneMap_TexelSize.x, t_now, 0, 0)),
        tex2Dlod(_BoneMap, float4((matrixIndex * 4 + 2 + 0.5) * _BoneMap_TexelSize.x, t_now, 0, 0)),
        tex2Dlod(_BoneMap, float4((matrixIndex * 4 + 3 + 0.5) * _BoneMap_TexelSize.x, t_now, 0, 0)));

    float progress;
    float t_last = GetExitTime(progress) / fullDuration;
    float4x4 mat_last = float4x4(
        tex2Dlod(_BoneMap, float4((matrixIndex * 4 + 0 + 0.5) * _BoneMap_TexelSize.x, t_last, 0, 0)),
        tex2Dlod(_BoneMap, float4((matrixIndex * 4 + 1 + 0.5) * _BoneMap_TexelSize.x, t_last, 0, 0)),
        tex2Dlod(_BoneMap, float4((matrixIndex * 4 + 2 + 0.5) * _BoneMap_TexelSize.x, t_last, 0, 0)),
        tex2Dlod(_BoneMap, float4((matrixIndex * 4 + 3 + 0.5) * _BoneMap_TexelSize.x, t_last, 0, 0)));

    return lerp(mat_last, mat_now, progress);
}

float4x4 GetBindpos(uint matrixIndex)
{
    return float4x4(
        tex2Dlod(_BindposMap, float4((matrixIndex + 0.5) * _BindposMap_TexelSize.x, 0.125, 0, 0)),
        tex2Dlod(_BindposMap, float4((matrixIndex + 0.5) * _BindposMap_TexelSize.x, 0.375, 0, 0)),
        tex2Dlod(_BindposMap, float4((matrixIndex + 0.5) * _BindposMap_TexelSize.x, 0.625, 0, 0)),
        tex2Dlod(_BindposMap, float4((matrixIndex + 0.5) * _BindposMap_TexelSize.x, 0.875, 0, 0)));
}

void GetPositionAndNormal(inout float4 positionOS, inout float3 normalOS, uint4 blendIndices, float4 blendWeights)
{
    // 将顶点从对象空间变换到骨骼空间
    float4x4 matrix0 = mul(GetMatrix(blendIndices[0]), GetBindpos(blendIndices[0]));
    float4x4 matrix1 = mul(GetMatrix(blendIndices[1]), GetBindpos(blendIndices[1]));
    float4x4 matrix2 = mul(GetMatrix(blendIndices[2]), GetBindpos(blendIndices[2]));
    float4x4 matrix3 = mul(GetMatrix(blendIndices[3]), GetBindpos(blendIndices[3]));
    float4 p0 = mul(matrix0, positionOS) * blendWeights.x;
    float4 p1 = mul(matrix1, positionOS) * blendWeights.y;
    float4 p2 = mul(matrix2, positionOS) * blendWeights.z;
    float4 p3 = mul(matrix3, positionOS) * blendWeights.w;
    positionOS = p0 + p1 + p2 + p3;

    float3 n0 = mul((float3x3)matrix0, normalOS) * blendWeights.x;
    float3 n1 = mul((float3x3)matrix1, normalOS) * blendWeights.y;
    float3 n2 = mul((float3x3)matrix2, normalOS) * blendWeights.z;
    float3 n3 = mul((float3x3)matrix3, normalOS) * blendWeights.w;
    normalOS = n0 + n1 + n2 + n3;
}

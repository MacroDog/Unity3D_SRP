
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Render Pipeline/Lit")]
public class MDLitPipelineAsset : RenderPipelineAsset
{
    protected override RenderPipeline CreatePipeline()
    {
        return new MDLightPipeline();
    }
}

public class MDLightPipeline : RenderPipeline
{

    public readonly ShaderTagId m_ShaderTagId = new ShaderTagId("03LitPipeline");
    public int MAX_VISABLE_COUNT = 4;
    static int _LightColorId = Shader.PropertyToID("_LightColors");
    static int _LightDirectionsID = Shader.PropertyToID("_LightDirections");
    static int _LightAttenuationID = Shader.PropertyToID("_LightAttenuations");
    static int _LightSpotDirectionID = Shader.PropertyToID("_LightSpotDirections");
    private Vector4[] LightColors;
    private Vector4[] LightDirections;
    private Vector4[] LightAttenuations;
    private Vector4[] LightSpotDirections;

    private RenderTexture shadowMap;
    public CommandBuffer cameraBuffer
    {
        get;
        private set;
    }

    public CommandBuffer shadowBuffer
    {
        get;
        private set;
    }
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        BeginFrameRendering(cameras);
        foreach (var camera in cameras)
        {
            Render(context, camera);
        }
    }

    public MDLightPipeline()
    {
        cameraBuffer = new CommandBuffer()
        {
            name = "Camera Buffer"
        };
        shadowBuffer = new CommandBuffer()
        {
            name = "Shadow Buffer"
        };
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposed);
        cameraBuffer.Release();
    }

    private void Render(ScriptableRenderContext context, Camera camera)
    {
        BeginCameraRendering(camera);
        ScriptableCullingParameters parameters;
        if (!camera.TryGetCullingParameters(out parameters))
        {
            return;
        }
        CullingResults cullingResults = context.Cull(ref parameters);

        context.SetupCameraProperties(camera);


        cameraBuffer.Clear();
        cameraBuffer.ClearRenderTarget((camera.clearFlags & CameraClearFlags.Depth) != 0, (camera.clearFlags & CameraClearFlags.Color) != 0, camera.backgroundColor);
        context.ExecuteCommandBuffer(cameraBuffer);

        //init

        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.all);
        SortingSettings sortingSettings = new SortingSettings();
        DrawingSettings drawingSettings = new DrawingSettings(m_ShaderTagId, sortingSettings);
        RenderShawder(context);

        //  cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives();


        //drawSkybox
        context.DrawSkybox(camera);

        #region light
        var lightcount = Mathf.Min(cullingResults.visibleLights.Length, MAX_VISABLE_COUNT);
        LightColors = new Vector4[MAX_VISABLE_COUNT];
        LightDirections = new Vector4[MAX_VISABLE_COUNT];
        LightAttenuations = new Vector4[MAX_VISABLE_COUNT];
        LightSpotDirections = new Vector4[MAX_VISABLE_COUNT];
        for (int i = 0; i < cullingResults.visibleLights.Length; i++)
        {
            if (i == MAX_VISABLE_COUNT)
            {
                break;
            }
            LightSpotDirections[i] = Vector4.zero;
            LightDirections[i] = Vector4.zero;
            LightAttenuations[i] = Vector4.zero;
            LightColors[i] = Vector4.zero;
            var light = cullingResults.visibleLights[i];
            LightSpotDirections[i] = Vector4.zero;
            LightAttenuations[i].w = 1f;
            if (light.lightType == LightType.Directional)
            {
                var v = light.localToWorldMatrix.GetColumn(2);
                v.x = -v.x;
                v.y = -v.y;
                v.z = -v.z;
                v.w = 0;// 
                LightDirections[i] = v;
            }
            else
            {
                LightDirections[i] = cullingResults.visibleLights[i].localToWorldMatrix.GetColumn(3);
                LightDirections[i].w = 1;

                //衰减 (1-(dir^2/range^2)^2)^2
                LightAttenuations[i].x = 1f / Mathf.Max(Mathf.Sqrt(light.range), 0.000001f);

                //spot fade (lightPos*lightDir)-cos(r_out)/cos(r_in)-cos(r_out)
                if (light.lightType == LightType.Spot)
                {
                    var dir = light.localToWorldMatrix.GetColumn(2);
                    dir.x = -dir.x;
                    dir.y = -dir.y;
                    dir.z = -dir.z;
                    LightSpotDirections[i] = dir;
                    var outerRad = Mathf.Deg2Rad * 0.5f * light.spotAngle;
                    var outerCos = Mathf.Cos(outerRad);
                    //tan(r_in) = 46/64*tan(r_out)
                    var outerTan = Mathf.Tan(outerRad);
                    var innerCos = Mathf.Cos(Mathf.Atan(23 / 32 * outerTan));
                    LightAttenuations[i].z = 1 / Mathf.Max(innerCos - outerCos, 0.0001f);
                    LightAttenuations[i].w = -outerCos * LightAttenuations[i].z;
                }
            }
            LightColors[i] = light.finalColor;
        }
        if (cullingResults.visibleLights.Length > MAX_VISABLE_COUNT)
        {
            NativeArray<int> lightMap = cullingResults.GetLightIndexMap(Allocator.Invalid);
            for (int i = MAX_VISABLE_COUNT; i < cullingResults.visibleLights.Length; i++)
            {
                lightMap[i] = -1;
            }
            cullingResults.SetLightIndexMap(lightMap);
            lightMap.Dispose();
        }
        cameraBuffer.Clear();
        cameraBuffer.SetGlobalVectorArray(_LightDirectionsID, LightDirections);
        cameraBuffer.SetGlobalVectorArray(_LightColorId, LightColors);
        cameraBuffer.SetGlobalVectorArray(_LightAttenuationID, LightAttenuations);
        cameraBuffer.SetGlobalVectorArray(_LightSpotDirectionID, LightSpotDirections);
        context.ExecuteCommandBuffer(cameraBuffer);
        #endregion
        //qaueue
        filteringSettings.renderQueueRange = RenderQueueRange.opaque;
        sortingSettings.criteria = SortingCriteria.CommonOpaque;
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        context.Submit();
        if (shadowMap)
        {
            RenderTexture.ReleaseTemporary(shadowMap);
            shadowMap = null;
        }
    }

    private void RenderShawder(ScriptableRenderContext context)
    {
        shadowMap = RenderTexture.GetTemporary(512, 512, 32, RenderTextureFormat.Shadowmap);
        shadowMap.filterMode = FilterMode.Bilinear;
        shadowMap.wrapMode = TextureWrapMode.Clamp;
        CoreUtils.SetRenderTarget(shadowBuffer, shadowMap);
        shadowBuffer.BeginSample("Render Shadow");
        context.ExecuteCommandBuffer(shadowBuffer);
        shadowBuffer.Clear();
        shadowBuffer.EndSample("Rneder Shadow");
        context.ExecuteCommandBuffer(shadowBuffer);
        shadowBuffer.Clear();

    }
}

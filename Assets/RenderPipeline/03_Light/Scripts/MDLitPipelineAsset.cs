
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
    public int MRP_VISABLE_COUNT = 4;
    static int _LightColorID = Shader.PropertyToID("_LightColors");
    static int _LightDirectionID = Shader.PropertyToID("_LightDirections");
    static int _LightAttenuationID = Shader.PropertyToID("_LightAttenuations");
    private Vector4[] LightColors;
    private Vector4[] LightDirections;
    private Vector4[] LightAttenuations;
    public CommandBuffer commandBuffer
    {
        get;
        private set;
    }
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {

        foreach (var camera in cameras)
        {
            Render(context, camera);
        }
    }

    public MDLightPipeline()
    {
        commandBuffer = new CommandBuffer();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposed);
        commandBuffer.Release();
    }

    private void Render(ScriptableRenderContext context, Camera camera)
    {
        ScriptableCullingParameters parameters;
        if (!camera.TryGetCullingParameters(out parameters))
        {
            return;
        }

#if UNITY_EDITOR
        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }
#endif
        context.SetupCameraProperties(camera);


        commandBuffer.Clear();
        commandBuffer.ClearRenderTarget((camera.clearFlags & CameraClearFlags.Depth) != 0, (camera.clearFlags & CameraClearFlags.Color) != 0, camera.backgroundColor, camera.depth);
        context.ExecuteCommandBuffer(commandBuffer);

        //init
        CullingResults cullingResults = context.Cull(ref parameters);
        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.all);
        SortingSettings sortingSettings = new SortingSettings();
        DrawingSettings drawingSettings = new DrawingSettings(m_ShaderTagId, sortingSettings);

        //drawSkybox
        context.DrawSkybox(camera);

        //light
        var lightcount = Mathf.Min(cullingResults.lightIndexCount, MRP_VISABLE_COUNT);
        LightColors = new Vector4[MRP_VISABLE_COUNT];
        LightDirections = new Vector4[MRP_VISABLE_COUNT];
        LightAttenuations = new Vector4[MRP_VISABLE_COUNT];
        int i = 0;
        for (; i < lightcount; i++)
        {
            var light = cullingResults.visibleLights[i];
            LightColors[i] = light.finalColor;
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
                LightDirections[i] = light.localToWorldMatrix.GetColumn(3);
                LightDirections[i].w = 1;
                LightAttenuations[i].x = Mathf.Max(1f / Mathf.Sqrt(light.range), 0.000001f);
            }
        }
        for (; i < MRP_VISABLE_COUNT; i++)
        {
            LightColors[i] = Vector4.zero;
            LightDirections[i] = Vector4.zero;
            LightDirections[i] = Vector4.zero;
        }
        commandBuffer.Clear();
        commandBuffer.SetGlobalVectorArray(_LightDirectionID, LightDirections);
        commandBuffer.SetGlobalVectorArray(_LightColorID, LightColors);
        commandBuffer.SetGlobalVectorArray(_LightAttenuationID, LightAttenuations);
        context.ExecuteCommandBuffer(commandBuffer);
        //qaueue
        filteringSettings.renderQueueRange = RenderQueueRange.opaque;
        sortingSettings.criteria = SortingCriteria.CommonOpaque;
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);


        context.Submit();
    }


}

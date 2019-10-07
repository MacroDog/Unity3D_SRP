/*
 * -----
 * Created Date: Tuesday, October 1st 2019, 3:07:23 pm
 * Author: XieYiFeng
 * -----
 */

using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[CreateAssetMenu(menuName = "Render Pipeline/Batch")]
public class McBatchPipelineAsset : RenderPipelineAsset
{
    protected override RenderPipeline CreatePipeline()
    {
        return new McBatchPipeline();
    }
}

public class McBatchPipeline : RenderPipeline
{
    public readonly ShaderTagId m_ShaderTagId = new ShaderTagId("02BatchPipeline");

    public CommandBuffer commandBuffer;
    bool enableDynamicBatch = true;
    bool enableGPUInstance = true;
    public McBatchPipeline()
    {
        commandBuffer = new CommandBuffer();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        commandBuffer.Release();
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        BeginFrameRendering(cameras);
        foreach (var camera in cameras)
        {
            BeginCameraRendering(camera);
            Render(context, camera);
        }
    }

    private void Render(ScriptableRenderContext context, Camera camera)
    {
        ScriptableCullingParameters cullingParameters;
        if (!camera.TryGetCullingParameters(out cullingParameters))
        {
            return;
        }

        CullingResults cullingResults = context.Cull(ref cullingParameters);

        context.SetupCameraProperties(camera);

        // commandBuff
        commandBuffer.Clear();
        commandBuffer.ClearRenderTarget((CameraClearFlags.Depth & camera.clearFlags) != 0, (CameraClearFlags.Color & camera.clearFlags) != 0, camera.backgroundColor, camera.depth);
        context.ExecuteCommandBuffer(commandBuffer);

        //init
        SortingSettings sortingSettings = new SortingSettings(camera);
        DrawingSettings drawSettings = new DrawingSettings(m_ShaderTagId, sortingSettings);
        FilteringSettings filterSettings = new FilteringSettings(RenderQueueRange.all);
        drawSettings.enableDynamicBatching = enableDynamicBatch;
        drawSettings.enableInstancing = enableGPUInstance;


        //Draw Skybox
        context.DrawSkybox(camera);

        // Draw opaque
        sortingSettings.criteria = SortingCriteria.None;
        drawSettings.sortingSettings = sortingSettings;
        filterSettings.renderQueueRange = RenderQueueRange.opaque;
        context.DrawRenderers(cullingResults, ref drawSettings, ref filterSettings);

        // Draw Transparent
        sortingSettings.criteria = SortingCriteria.CommonTransparent;

        drawSettings.sortingSettings = sortingSettings;
        filterSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(cullingResults, ref drawSettings, ref filterSettings);
        context.Submit();
    }
}

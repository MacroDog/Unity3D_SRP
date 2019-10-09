using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Render Pipeline/Base")]
public class MDBasePipelineAsset : RenderPipelineAsset
{
    protected override RenderPipeline CreatePipeline()
    {
        return new MDBasePipeline();
    }
}

public class MDBasePipeline : RenderPipeline
{
    public readonly ShaderTagId m_ShaderTagId = new ShaderTagId("01BasePipeline");
    public CommandBuffer commandBuffer;

    public MDBasePipeline()
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

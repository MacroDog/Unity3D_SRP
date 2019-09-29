using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/FristPipeline")]
public class FristPipelineAssetCreater : RenderPipelineAsset
{
    protected override RenderPipeline CreatePipeline()
    {
        return new FristPipeline();
    }
}

public class FristPipeline : RenderPipeline
{
    private CommandBuffer commandBuffer;
    private ScriptableCullingParameters cullingParameters;
    private CullingResults cullingResults;
    private ShaderTagId shaderTagId;

    private SortingSettings sortingSettings;
    public FristPipeline()
    {
        commandBuffer = new CommandBuffer();
        shaderTagId = new ShaderTagId("SRPDefaultUnlit");

    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        commandBuffer.Release();
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            Render(context, camera);
        }
    }



    private void Render(ScriptableRenderContext context, Camera camera)
    {
        commandBuffer.Clear();
        commandBuffer.name = camera.name;
        var flags = camera.clearFlags;
        commandBuffer.ClearRenderTarget((flags & CameraClearFlags.Depth) != 0, (flags & CameraClearFlags.Color) != 0, camera.backgroundColor);
        if (!camera.TryGetCullingParameters(out cullingParameters))
        {
            return;
        }
        cullingResults = context.Cull(ref cullingParameters);
        context.SetupCameraProperties(camera);
        context.ExecuteCommandBuffer(commandBuffer);
        sortingSettings = new SortingSettings(camera);
        //绘制不透明物品
        sortingSettings.criteria = SortingCriteria.CommonOpaque;

        var filterSetting = new FilteringSettings();
        filterSetting.renderQueueRange = RenderQueueRange.opaque;
        var drawsetting = new DrawingSettings(shaderTagId, sortingSettings);
        drawsetting.sortingSettings = sortingSettings;
        context.DrawSkybox(camera);
        context.DrawRenderers(cullingResults, ref drawsetting, ref filterSetting);
        //Debug.Log(cullingResults.visibleReflectionProbes.Length);

        context.Submit();
    }
}

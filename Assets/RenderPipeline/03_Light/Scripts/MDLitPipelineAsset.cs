
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName="Render Pipeline/Lit")]
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
    static int _LightColorId = Shader.PropertyToID("_LightColors");
    static int _LightDirectionsID = Shader.PropertyToID("_LightDirections");
    private Vector4[] LightColors;
    private Vector4[] LightDirections;
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
        LightColors = new Vector4[lightcount];
        LightDirections = new Vector4[lightcount];
        int i = 0;
        for (; i < lightcount; i++)
        {
            if (cullingResults.visibleLights[i].lightType == LightType.Directional)
            {
                var v = cullingResults.visibleLights[i].localToWorldMatrix.GetColumn(2);
                v.x = -v.x;
                v.y = -v.y;
                v.z = -v.z;
                v.w = 1;// 
                LightDirections[i] = v;
                LightColors[i] = cullingResults.visibleLights[i].finalColor;
            }else{
                 LightDirections[i] = cullingResults.visibleLights[i].localToWorldMatrix.GetColumn(3);
            }
        }
        commandBuffer.Clear();
        commandBuffer.SetGlobalVectorArray(_LightDirectionsID,LightDirections);
        commandBuffer.SetGlobalVectorArray(_LightColorId,LightColors);
        context.ExecuteCommandBuffer(commandBuffer);    
        //qaueue
        filteringSettings.renderQueueRange = RenderQueueRange.opaque;
        sortingSettings.criteria = SortingCriteria.CommonOpaque;
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);


        context.Submit();
    }


}

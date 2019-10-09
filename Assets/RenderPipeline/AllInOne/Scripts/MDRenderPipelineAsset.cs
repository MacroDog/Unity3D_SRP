using UnityEngine.Rendering;
using UnityEngine;
namespace McPipeline
{
    [CreateAssetMenu(menuName = "Render Pipeline/McPipeline")]
    public class MDRenderPipelineAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline()
        {
            throw new System.NotImplementedException();
        }
    }

    public class MDRenderPipeline : RenderPipeline
    {
        public CommandBuffer commandBuffer;
        public readonly ShaderTagId[] shaderTagIds = new ShaderTagId[]{
            new ShaderTagId("McRenderPipeline")
        };
        public MDRenderPipeline()
        {
            commandBuffer = new CommandBuffer();
        }

        private void Render(ScriptableRenderContext context, Camera camera)
        {
            ScriptableCullingParameters parameters;
            if (!camera.TryGetCullingParameters(out parameters))
            {
                return;
            }
            commandBuffer.Clear();
            commandBuffer.ClearRenderTarget((camera.clearFlags & CameraClearFlags.Depth) != 0, (camera.clearFlags & CameraClearFlags.Color) != 0, camera.backgroundColor, camera.depth);
            context.ExecuteCommandBuffer(commandBuffer);

            //init
            SortingSettings sortingSettings = new SortingSettings(camera);
            CullingResults cullingResults = context.Cull(ref parameters);
            DrawingSettings drawingSettings = new DrawingSettings(shaderTagIds[0], sortingSettings);
            FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.all);
            for (int i = 1; i < shaderTagIds.Length; i++)
            {
                drawingSettings.SetShaderPassName(i, shaderTagIds[i]);
            }
            context.DrawSkybox(camera);


            //opaue
            sortingSettings.criteria = SortingCriteria.RenderQueue;
            drawingSettings.sortingSettings = sortingSettings;
            filteringSettings.renderQueueRange = RenderQueueRange.opaque;
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
            context.Submit();

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


    }
}

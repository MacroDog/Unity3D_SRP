/*
 * -----
 * Created Date: Tuesday, October 1st 2019, 2:59:52 pm
 * Author: XieYiFeng
 * -----
 */
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class SetPipeline : MonoBehaviour
{
   public RenderPipelineAsset renderPipelineAsset;

    void OnEnable()
    {
        GraphicsSettings.renderPipelineAsset = renderPipelineAsset;
    }

    void OnValidate()
    {
        GraphicsSettings.renderPipelineAsset = renderPipelineAsset;
    }
}

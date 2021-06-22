using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;
public class MyPipeline : RenderPipeline
{
    DrawRendererFlags drawRendererFlags;
    CullResults cull;
    const int maxVisibleLight = 4;
    static int visibleLightColorId = Shader.PropertyToID("_VisibleLightColors");
    static int visibleLightDirectionsID = Shader.PropertyToID("_VisibleLightDirections");
    Vector4[] visibleLightColors = new Vector4[maxVisibleLight];
    Vector4[] visibleLightDirections = new Vector4[maxVisibleLight];

    CommandBuffer camera_buffer = new CommandBuffer
    {
        name = "Render Camera"
    };

    public MyPipeline(bool dynamicBatching, bool instancing)
    {
        GraphicsSettings.lightsUseLinearIntensity = true;
        if (dynamicBatching)
        {
            drawRendererFlags = DrawRendererFlags.EnableDynamicBatching;
        }
        if (instancing)
        {
            drawRendererFlags |= DrawRendererFlags.EnableInstancing;
        }
    }
    Material error_mat;
    public override void Render(ScriptableRenderContext renderContext, Camera[] cameras)
    {
        base.Render(renderContext, cameras);

        foreach (var camera in cameras)
        {
            Render(renderContext, camera);
        }
    }

    void Render(ScriptableRenderContext context, Camera camera)
    {
        ScriptableCullingParameters cullingParameters;
        if (!CullResults.GetCullingParameters(camera, out cullingParameters))
        {
            return;
        }
#if UNITY_EDITOR
        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }
#endif
        CullResults.Cull(ref cullingParameters, context, ref cull);
        context.SetupCameraProperties(camera);
        CameraClearFlags clearFlags = camera.clearFlags;
        var cleardathp = ((int)clearFlags & (int)CameraClearFlags.Depth) == 1;
        var clearcolor = ((int)clearFlags & (int)CameraClearFlags.Color) == 1;
        camera_buffer.ClearRenderTarget(cleardathp, clearcolor, camera.backgroundColor);
        camera_buffer.BeginSample("Render Camera");
        ConfigureLights();
        camera_buffer.SetGlobalVectorArray(visibleLightColorId,visibleLightColors);
        camera_buffer.SetGlobalVectorArray(visibleLightDirectionsID,visibleLightDirections);
        context.ExecuteCommandBuffer(camera_buffer);
        camera_buffer.Clear();
        var filterSetting = new FilterRenderersSettings(true);
        var drawSetting = new DrawRendererSettings(camera, new ShaderPassName("SRPDefaultUnlit"));
        drawSetting.flags = drawRendererFlags;
        filterSetting.renderQueueRange = RenderQueueRange.opaque;
        drawSetting.sorting.flags = SortFlags.CommonOpaque;
        context.DrawRenderers(cull.visibleRenderers, ref drawSetting, filterSetting);
        context.DrawSkybox(camera);
        filterSetting.renderQueueRange = RenderQueueRange.transparent;
        drawSetting.sorting.flags = SortFlags.CommonTransparent;
        context.DrawRenderers(cull.visibleRenderers, ref drawSetting, filterSetting);
        DrawDefaultPipeline(context, camera);
        camera_buffer.EndSample("Render Camera");
        context.ExecuteCommandBuffer(camera_buffer);
        camera_buffer.Clear();

        context.Submit();
    }

    void ConfigureLights()
    {
        for (int i = 0; i < cull.visibleLights.Count; i++)
        {
            if (i == maxVisibleLight) {
				break;
			}
            VisibleLight light = cull.visibleLights[i];
            visibleLightColors[i] = light.finalColor;
            var v =  light.localToWorld.GetColumn(2);
            v.x = -v.x;
            v.y = -v.y;
            v.z = -v.z;
            visibleLightDirections[i] = v;
            v.w = cull.visibleLights[i].light.type == LightType.Point ? 1 : 0;
        }
    }

    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
    void DrawDefaultPipeline(ScriptableRenderContext context, Camera camera)
    {
        var drawSettings = new DrawRendererSettings(camera, new ShaderPassName("ForwardBase"));
        var filterSettings = new FilterRenderersSettings(true);
        ScriptableCullingParameters cullingParameters;
        if (error_mat != null)
        {
            Shader error_shader = Shader.Find("Hidden/InternalErrorShader");
            error_mat = new Material(error_shader);
        }
        drawSettings.SetOverrideMaterial(error_mat, 0);
        drawSettings.SetShaderPassName(1, new ShaderPassName("PrepassBase"));
        drawSettings.SetShaderPassName(2, new ShaderPassName("Always"));
        drawSettings.SetShaderPassName(3, new ShaderPassName("Vertex"));
        drawSettings.SetShaderPassName(4, new ShaderPassName("VertexLMRGBM"));
        drawSettings.SetShaderPassName(5, new ShaderPassName("VertexLM"));
        if (!CullResults.GetCullingParameters(camera, out cullingParameters))
        {
            return;
        }
        CullResults cull = CullResults.Cull(ref cullingParameters, context);
        context.DrawRenderers(cull.visibleRenderers, ref drawSettings, filterSettings);
    }
}
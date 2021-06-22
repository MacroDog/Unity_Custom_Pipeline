using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;
namespace CustomPipeline
{
    public class MyPipeline : RenderPipeline
    {
        DrawRendererFlags drawRendererFlags;
        CullResults cull;


        CommandBuffer cameraBuffer = new CommandBuffer
        {
            name = "Render Camera"
        };

        CommandBuffer shadowBuffer = new CommandBuffer
        {
            name = "Render Shadows"
        };

        RenderTexture shadowMap;

        ShadowSetting shadowSettings;

        public MyPipeline(bool dynamicBatching, bool instancing, ShadowSetting draw)
        {
            GraphicsSettings.lightsUseLinearIntensity = true;
            this.shadowSettings = draw;
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

        Lighting light  = new Lighting();

        void RenderShadows(ScriptableRenderContext context)
        {
            // shadowMap = RenderTexture.GetTemporary(512, 512, 16, RenderTextureFormat.Shadowmap);
            // shadowMap.filterMode = FilterMode.Bilinear;
            // shadowMap.wrapMode = TextureWrapMode.Clamp;
            // CoreUtils.SetRenderTarget(shadowBuffer, shadowMap, RenderBufferLoadAction.DontCare,
            // RenderBufferStoreAction.Store, ClearFlag.Depth);
            // shadowBuffer.BeginSample("Shadow");
            // context.ExecuteCommandBuffer(shadowBuffer);
            // shadowBuffer.Clear();
            // shadowBuffer.EndSample("Shadow");
            // context.ExecuteCommandBuffer(shadowBuffer);
            // shadowBuffer.Clear();
            // Matrix4x4 viewMatrix, projectMatrix;
            // ShadowSplitData shadowSplitData;
            // cull.ComputeSpotShadowMatricesAndCullingPrimitives(0, out viewMatrix, out projectMatrix, out shadowSplitData);
            // var shawdowSetting = new DrawShadowsSettings(cull, 0);
            // context.DrawShadows(ref shawdowSetting);
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
            RenderShadows(context);
            context.SetupCameraProperties(camera);
            CameraClearFlags clearFlags = camera.clearFlags;
            var cleardathp = ((int)clearFlags & (int)CameraClearFlags.Depth) == 1;
            var clearcolor = ((int)clearFlags & (int)CameraClearFlags.Color) == 1;
            cameraBuffer.ClearRenderTarget(cleardathp, clearcolor, camera.backgroundColor);
            cameraBuffer.BeginSample("Render Camera");
            light.SetUp(context,cull);
            context.ExecuteCommandBuffer(cameraBuffer);
            cameraBuffer.Clear();
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
            cameraBuffer.EndSample("Render Camera");
            context.ExecuteCommandBuffer(cameraBuffer);
            cameraBuffer.Clear();
            context.Submit();
            if (shadowMap)
            {
                RenderTexture.ReleaseTemporary(shadowMap);
                shadowMap = null;
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

    [System.Serializable]
    public class ShadowSetting
    {
        //阴影最大距离
        public float MaxDistence = 100;
        public enum TextureSize
        {
            _256 = 256,
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048,
            _4096 = 4096,
            _8192 = 8192
        }

        [System.Serializable]
        public struct Directional
        {
            public TextureSize TextureSize;
        }

        public Directional DirectionSetting = new Directional
        {
            TextureSize = TextureSize._1024
        };
    }
}


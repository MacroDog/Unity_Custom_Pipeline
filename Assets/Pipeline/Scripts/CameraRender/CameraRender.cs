using UnityEngine;
using UnityEngine.Rendering;
namespace CustomPipeline
{
    public partial class CameraRender
    {
        ScriptableRenderContext context;
        Camera camera;

        static ShaderTagId LitPass = new ShaderTagId("SRPDefaultLit");
        static ShaderTagId ShadowPass = new ShaderTagId("ShadowCaster");

        static ShaderTagId UnlitPass = new ShaderTagId("SRPDefaultUnlit");

        CommandBuffer cameraBuffer = new CommandBuffer
        {
            name = "Render Camera"
        };
        bool enableDynamicBatching;
        bool enableInstancing;
        Lighting lighting;
        ShadowSetting setting;
        public CameraRender(bool enableDynamicBatching, bool enableInstancing, ShadowSetting setting)
        {
            this.enableDynamicBatching = enableDynamicBatching;
            this.enableInstancing = enableInstancing;
            lighting = new Lighting();
            this.setting = setting;
        }
        public void Render(ScriptableRenderContext context, Camera camera)
        {
            this.camera = camera;
            this.context = context;
            PrepareBuffer();
            PrepareForSceneWindow();
            if (!Cull(setting.MaxDistence))
            {
                return;
            }
            cameraBuffer.BeginSample(cameraBuffer.name);
            ExecuteBuffer();
            lighting.SetUp(context, cullResults, setting);
            cameraBuffer.EndSample(cameraBuffer.name);
            SetUp();
            //绘制几何体
            DrawVisiableGemoerty();
            //绘制不支持的shader
            DrawUnsupportedShader();
            lighting.ClearUp();
            Submit();
        }
        partial void PrepareBuffer();
        partial void DrawUnsupportedShader();
        partial void PrepareForSceneWindow();

        void DrawVisiableGemoerty()
        {
            var SortingSettings = new SortingSettings(camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };
            var drawSetting = new DrawingSettings(UnlitPass, SortingSettings)
            {
                enableInstancing = enableInstancing,
                enableDynamicBatching = enableDynamicBatching
            };
            drawSetting.SetShaderPassName(1, LitPass);
            drawSetting.SetShaderPassName(2, ShadowPass);

            var filterSetting = new FilteringSettings(RenderQueueRange.opaque);
            //绘制不透明
            context.DrawRenderers(cullResults, ref drawSetting, ref filterSetting);
            //绘制天空盒
            context.DrawSkybox(camera);
            //绘制半透明
            SortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawSetting.sortingSettings = SortingSettings;
            filterSetting.renderQueueRange = RenderQueueRange.transparent;
            context.DrawRenderers(cullResults, ref drawSetting, ref filterSetting);
        }

        CullingResults cullResults;
        ScriptableCullingParameters cullingParam;

        void SetUp()
        {
            cameraBuffer.BeginSample(cameraBuffer.name);
            context.SetupCameraProperties(camera);
            //得到相机的clear flags
            CameraClearFlags flags = camera.clearFlags;
            //设置相机清除状态
            cameraBuffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, flags == CameraClearFlags.Color,
                flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear);
            ExecuteBuffer();
        }

        void Submit()
        {
            cameraBuffer.EndSample(cameraBuffer.name);
            ExecuteBuffer();
            context.Submit();
        }

        void ExecuteBuffer()
        {
            context.ExecuteCommandBuffer(cameraBuffer);
            cameraBuffer.Clear();
        }

        /// <summary>
        /// 剔除
        /// </summary>
        /// <returns></returns>
        bool Cull(float maxShadowDistance)
        {
            ScriptableCullingParameters p;

            if (camera.TryGetCullingParameters(out p))
            {
                //得到最大阴影距离,和相机远裁剪面距离作比较，取最小的那个作为阴影距离
                p.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
                cullResults = context.Cull(ref p);
                return true;
            }
            return false;
        }
    }
}

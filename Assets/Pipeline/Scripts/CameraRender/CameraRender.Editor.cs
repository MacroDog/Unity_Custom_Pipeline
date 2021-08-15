#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Profiling;
namespace CustomPipeline
{
    public partial class CameraRender
    {
        static ShaderTagId[] legacyShaderIds =
        {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")

    };
        Material errorMater;
        partial void DrawUnsupportedShader()
        {
            if (errorMater == null)
            {
                errorMater = new Material(Shader.Find("Hidden/InternalErrorShader"));
            }
            var drawSetting = new DrawingSettings()
            {
                overrideMaterial = errorMater,
                sortingSettings = new SortingSettings(camera)
            };
            for (int i = 0; i < legacyShaderIds.Length; i++)
            {
                drawSetting.SetShaderPassName(1, legacyShaderIds[i]);
            }
            var filterSetting = FilteringSettings.defaultValue;
            context.DrawRenderers(cullResults, ref drawSetting, ref filterSetting);
        }
        partial void PrepareBuffer()
        {
            Profiler.BeginSample("Editor.Only");
            ExecuteBuffer();
            cameraBuffer.name = camera.name;
            Profiler.EndSample();
            ExecuteBuffer();
        }

        partial void PrepareForSceneWindow()
        {
            if (camera.cameraType == CameraType.SceneView)
            {
                //如果切换到了Scene视图，调用此方法完成绘制
                ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
            }
        }
#endif
    }
}

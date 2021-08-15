using UnityEngine;
using UnityEngine.Rendering;
namespace CustomPipeline
{
    public class MyPipeline : RenderPipeline
    {
        DrawingSettings drawingSetting;

        ShadowSetting shadowSettings;

        CameraRender camererender;

        public MyPipeline(bool dynamicBatching, bool instancing, ShadowSetting shadowSetting)
        {
            GraphicsSettings.lightsUseLinearIntensity = true;
            this.shadowSettings = shadowSetting;
            camererender = new CameraRender(dynamicBatching, instancing, shadowSetting);
        }

        Material error_mat;
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach (var camera in cameras)
            {
                Render(context, camera);
            }
        }
        Lighting light = new Lighting();
        void Render(ScriptableRenderContext context, Camera camera)
        {
            camererender.Render(context, camera);
        }
    }
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
    public class ShadowSetting
    {
        //阴影最大距离
        public float MaxDistence = 100;

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


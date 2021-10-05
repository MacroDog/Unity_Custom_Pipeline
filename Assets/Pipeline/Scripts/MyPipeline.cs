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

        public enum FilterMode
        {
            PCF2x2 = 0,
            PCF3x3 = 1,
            PCF5x5 = 2,
            PCF7x7 = 3,
        }
        //阴影最大距离
        [Min(0.001f)]
        public float MaxDistence = 100;

        [Min(0.001f)]
        public float DistanceFade = .1f;
        [System.Serializable]
        public struct Directional
        {
            public TextureSize TextureSize;
            [Range(0, 4)]
            public int CascadeCount;
            [Range(0, 1)]
            public float CascadeRatio1, CascadeRatio2, CascadeRatio3;
            public float CascadeFade;
            public FilterMode FilterMode;
        }

        public Directional DirectionSetting = new Directional
        {
            TextureSize = TextureSize._1024
        };
    }
}


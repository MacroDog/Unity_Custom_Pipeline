using UnityEngine;
using UnityEngine.Rendering;
namespace CustomPipeline
{
    [CreateAssetMenu(menuName = "Rendering/My Pipeline")]
    public class MyPipelineAsset : RenderPipelineAsset
    {
        [SerializeField]
        bool dynamicBatching;
        [SerializeField]
        bool instancing;

        [SerializeField]
        ShadowSetting drawSetting = default;
        protected override RenderPipeline  CreatePipeline()
        {
            return new MyPipeline(dynamicBatching, instancing,drawSetting);
        }
    }
}

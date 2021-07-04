using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
namespace CustomPipeline
{
    public class Lighting
    {
        static int MAX_VISIBLE_LIGHT = 4;
        Vector4[] visibleLightColors = new Vector4[MAX_VISIBLE_LIGHT];
        Vector4[] visibleLightDirections = new Vector4[MAX_VISIBLE_LIGHT];
        static int LightColorId = Shader.PropertyToID("_VisibleLightColors");
        static int LightCountID = Shader.PropertyToID("_VisibleLightCount");
        static int LightDirectionsID = Shader.PropertyToID("_VisibleLightDirections");
        static string buffname = "LightBuffer";
        CommandBuffer buffer = new CommandBuffer
        {
            name = buffname
        };
        CullResults cullResults;
        void SetUpDirectionalLight(int index, ref VisibleLight light)
        {
            
            visibleLightColors[index] = light.finalColor;
            if (light.light.type == LightType.Directional)
            {
                var v = light.localToWorld.GetColumn(2);
                v.x = -v.x;
                v.y = -v.y;
                v.z = -v.z;
                v.w = 0;
                visibleLightDirections[index] = v;
            }
            // else
            // {
            //     visibleLightDirections[index] =
            //         light.localToWorld.GetColumn(3);
            //     visibleLightDirections[index].z = 1;
            // }
        }

        public void SetUp(ScriptableRenderContext context, CullResults cullResults)
        {
            this.cullResults = cullResults;
            buffer.BeginSample(buffname);
            SetUpLight();
            buffer.EndSample(buffname);
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        void SetUpLight()
        {
            for (int i = 0; i >= cullResults.visibleLights.Count; i++)
            {
                var lg = cullResults.visibleLights[i];
                if (i >= MAX_VISIBLE_LIGHT)
            {
                break;
            }
                if (lg.lightType == LightType.Directional)
                {
                    SetUpDirectionalLight(i, ref lg);
                }
            }

        }
    }
}
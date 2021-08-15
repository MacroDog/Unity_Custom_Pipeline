using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
namespace CustomPipeline
{
    public class Lighting
    {
        static int MAX_VISIBLE_LIGHT = 4;
        //
        Vector4[] visibleLightColors = new Vector4[MAX_VISIBLE_LIGHT];
        Vector4[] visibleLightDirections = new Vector4[MAX_VISIBLE_LIGHT];
        Vector4[] dirLightShadowData = new Vector4[MAX_VISIBLE_LIGHT];

        static int LightCountID = Shader.PropertyToID("_VisibleLightCount");
        static int LightColorId = Shader.PropertyToID("_VisibleLightColors");
        static int LightDirectionsID = Shader.PropertyToID("_VisibleLightDirections");
        static int DirectionLightShadowID = Shader.PropertyToID("_DirectionalLightShadowData");

        static string buffname = "LightBuffer";
        CommandBuffer buffer = new CommandBuffer
        {
            name = buffname
        };
        CullingResults cullResults;

        Shadows shadow  = new Shadows();
        void SetUpDirectionalLight(int index, ref VisibleLight light)
        {
            if (light.light.type == LightType.Directional)
            {
                visibleLightColors[index] = light.light.color;
                visibleLightDirections[index] = -light.localToWorldMatrix.GetColumn(2);
                dirLightShadowData[index] = shadow.ReserveDirectionalShadow(light.light, index);
            }
            // else
            // {
            //     visibleLightDirections[index] =
            //         light.localToWorld.GetColumn(3);
            //     visibleLightDirections[index].z = 1;
            // }
        }

        public void SetUp( ScriptableRenderContext context,  CullingResults cullResults,  ShadowSetting shadowSetting)
        {
            this.cullResults = cullResults;
            buffer.BeginSample(buffname);
            shadow.SetUp(context,cullResults,shadowSetting);
            SetUpLight();
            shadow.Render();
            buffer.EndSample(buffname);
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        void SetUpLight()
        {
            for (int i = 0; i < cullResults.visibleLights.Length; i++)
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
            buffer.SetGlobalInt(LightCountID, cullResults.visibleLights.Length);
            buffer.SetGlobalVectorArray(LightDirectionsID, visibleLightDirections);
            buffer.SetGlobalVectorArray(LightColorId, visibleLightColors);
            buffer.SetGlobalVectorArray(DirectionLightShadowID, dirLightShadowData);
        }

        public void ClearUp()
        {
            shadow.CleanUp();
        }
    }
}
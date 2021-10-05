using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
namespace CustomPipeline
{
    public class Shadows
    {

        public struct Directional
        {
            public TextureSize atlasSize;
        }

        static int MAX_SHADOW_LIGHT = 4;
        static int MAX_CASCADE_COUNT = 4;

        static string buffname = "Shadows";
        ShadowSetting shadowSetting;
        Matrix4x4[] dirShadowVPMatrix = new Matrix4x4[MAX_SHADOW_LIGHT * MAX_CASCADE_COUNT];
        Vector3 splitRatio = Vector3.zero;
        CommandBuffer buffer = new CommandBuffer
        {
            name = buffname
        };
        CullingResults cullResults;
        static int cascadeCountID = Shader.PropertyToID("_CascadeCount");
        static int cascadeCullingSpheresID = Shader.PropertyToID("_CascadeCullingSpheres");
        static int dirShadowAtlasID = Shader.PropertyToID("_DirectionalShadowAtlas");
        static int dirShadowVPMatrixID = Shader.PropertyToID("_DirShadowVPMatrix");
        static int shadowDistanceFadeID = Shader.PropertyToID("_ShadowDistanceFade");
        static int cascadeDataID = Shader.PropertyToID("_CascadeData");
        static int shadowAtlasSizeID = Shader.PropertyToID("_ShadowAtlasSize");


        static string[] directionalFilterKeyWord = {
            "_DIRECTIONAL_PCF3",
            "_DIRECTIONAL_PCF5",
            "_DIRECTIONAL_PCF7",
        };
        static Vector4[] cascadeCullingSpheres = new Vector4[MAX_CASCADE_COUNT];
        static Vector4[] cascadeData = new Vector4[MAX_CASCADE_COUNT];
        void SetKeyWords()
        {
            int enablekey = (int)shadowSetting.DirectionSetting.FilterMode - 1;
            for (int i = 0; i < directionalFilterKeyWord.Length; i++)
            {
                if (enablekey == i)
                {
                    buffer.EnableShaderKeyword(directionalFilterKeyWord[i]);
                }
                else
                {
                    buffer.DisableShaderKeyword(directionalFilterKeyWord[i]);
                }
            }
        }

        struct ShadowDirctionalLight
        {
            public int visiableLightIndex;
            public float slopeScaleBias;
            public float shadowNearPlane;
        }

        //用于生成阴影的可见光索引
        ShadowDirctionalLight[] ShadowDirctionalLights = new ShadowDirctionalLight[MAX_SHADOW_LIGHT];
        ScriptableRenderContext context;
        int shadowDirctionalLightCount;
        int shadowCasadeCount = 0;
        public void SetUp(ScriptableRenderContext context, CullingResults cullResults, ShadowSetting shadowSetting)
        {
            this.context = context;
            this.shadowSetting = shadowSetting;
            this.cullResults = cullResults;
            this.shadowDirctionalLightCount = 0;
            shadowCasadeCount = shadowSetting.DirectionSetting.CascadeCount == 0 ? 1 : shadowSetting.DirectionSetting.CascadeCount;
            splitRatio.x = shadowSetting.DirectionSetting.CascadeRatio1;
            splitRatio.y = shadowSetting.DirectionSetting.CascadeRatio2;
            splitRatio.z = shadowSetting.DirectionSetting.CascadeRatio3;
        }

        public Vector4 ReserveDirectionalShadow(Light light, int visiableLightIndex)
        {
            if (visiableLightIndex < MAX_SHADOW_LIGHT
            && light.shadows != LightShadows.None
            && light.shadowStrength > 0
            && cullResults.GetShadowCasterBounds(visiableLightIndex, out Bounds bound))
            {
                ShadowDirctionalLights[shadowDirctionalLightCount] = new ShadowDirctionalLight
                {
                    visiableLightIndex = visiableLightIndex,
                    slopeScaleBias = light.shadowBias,
                    shadowNearPlane = light.shadowNearPlane
                };
                var temp = new Vector4(light.shadowStrength, shadowDirctionalLightCount * shadowCasadeCount, light.shadowNormalBias);
                shadowDirctionalLightCount++;
                return temp;
            }
            return Vector4.zero;
        }

        void SetCasadeData(int index, Vector4 cullingShpere, float tilesize)
        {
            //这里是计算通过级联阴影包围shpere的直径除以tilesize
            float text_size = cullingShpere.w * 2 / tilesize;
            cascadeCullingSpheres[index] = cullingShpere;
            cascadeCullingSpheres[index].w *= cascadeCullingSpheres[index].w;
            //这里乘以根号2是应为 text_size是tilesize对角线
            cascadeData[index] = new Vector4(1f / cullingShpere.w, text_size * 1.414213f);
        }

        void ExecuteBuffer()
        {
            context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }

        public void CleanUp()
        {
            buffer.ReleaseTemporaryRT(dirShadowAtlasID);
            ExecuteBuffer();
        }

        public void Render()
        {
            if (shadowDirctionalLightCount > 0)
            {
                RenderDirctionalShadows();
            }

        }

        void RenderDirctionalShadows()
        {
            buffer.BeginSample(buffname);
            ExecuteBuffer();
            int atlassize = (int)shadowSetting.DirectionSetting.TextureSize;
            buffer.GetTemporaryRT(dirShadowAtlasID, atlassize, atlassize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
            buffer.SetRenderTarget(dirShadowAtlasID, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            buffer.ClearRenderTarget(true, false, Color.clear);
            int tiles = shadowDirctionalLightCount * shadowSetting.DirectionSetting.CascadeCount;
            var spile = Mathf.CeilToInt(Mathf.Sqrt(tiles));
            var tilesize = atlassize / spile;
            for (int i = 0; i < shadowDirctionalLightCount; i++)
            {
                RenderDirctionalShadows(i, spile, tilesize);
            }
            buffer.SetGlobalMatrixArray(dirShadowVPMatrixID, dirShadowVPMatrix);
            buffer.SetGlobalInt(cascadeCountID, shadowSetting.DirectionSetting.CascadeCount);
            buffer.SetGlobalVectorArray(cascadeCullingSpheresID, cascadeCullingSpheres);
            buffer.SetGlobalVectorArray(cascadeDataID, cascadeData);
            buffer.SetGlobalVector(shadowAtlasSizeID, new Vector4(atlassize, 1f / atlassize));
            float f = 1f - shadowSetting.DirectionSetting.CascadeFade;
            buffer.SetGlobalVector(shadowDistanceFadeID, new Vector4(1f / shadowSetting.MaxDistence, 1f / shadowSetting.DistanceFade, 1f / (1f - f * f)));
            SetKeyWords();
            buffer.EndSample(buffname);
            ExecuteBuffer();
        }

        void RenderDirctionalShadows(int index, int split, int tilesize)
        {
            ShadowDirctionalLight light = ShadowDirctionalLights[index];
            ShadowDrawingSettings shadowDrawingSettings = new ShadowDrawingSettings(cullResults, light.visiableLightIndex);
            var temp = cullResults.visibleLights[light.visiableLightIndex];
            var casesplit = Mathf.CeilToInt(Mathf.Sqrt(shadowCasadeCount));
            var casesize = tilesize / casesplit;
            int tileOffset = index * shadowCasadeCount;
            for (int i = 0; i < shadowCasadeCount; i++)
            {
                cullResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.visiableLightIndex, i, shadowCasadeCount, splitRatio, tilesize, light.shadowNearPlane
                , out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix, out ShadowSplitData shadowSplitData);
                if (index == 0)
                {
                    Vector4 cullingSphere = shadowSplitData.cullingSphere;
                    cullingSphere.w *= cullingSphere.w;
                    cascadeCullingSpheres[i] = cullingSphere;
                    SetCasadeData(i, shadowSplitData.cullingSphere, casesize);
                }
                shadowDrawingSettings.splitData = shadowSplitData;
                // var offset = setTileViewport(index % split * tilesize, index / split * tilesize, i, casesplit, casesize);
                int tileIndex = tileOffset + i;
                var offset = SetTileViewport(tileIndex, split, tilesize);
                buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
                dirShadowVPMatrix[index * shadowCasadeCount + i] = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, offset, split);
                buffer.SetGlobalDepthBias(0, light.slopeScaleBias);
                ExecuteBuffer();
                context.DrawShadows(ref shadowDrawingSettings);
            }
        }

        Vector2 SetTileViewport(int index, int split, float tileSize)
        {
            //计算索引图块的偏移位置
            Vector2 offset = new Vector2(index % split, index / split);
            //设置渲染视口，拆分成多个图块
            buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
            return offset;
        }

        Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split)
        {
            //如果使用了反向Zbuffer
            if (SystemInfo.usesReversedZBuffer)
            {
                m.m20 = -m.m20;
                m.m21 = -m.m21;
                m.m22 = -m.m22;
                m.m23 = -m.m23;
            }
            //设置矩阵坐标
            float scale = 1f / split;
            m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
            m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
            m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
            m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
            m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
            m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
            m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
            m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
            m.m20 = 0.5f * (m.m20 + m.m30);
            m.m21 = 0.5f * (m.m21 + m.m31);
            m.m22 = 0.5f * (m.m22 + m.m32);
            m.m23 = 0.5f * (m.m23 + m.m33);
            return m;
        }
    }
}
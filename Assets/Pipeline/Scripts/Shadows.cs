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
        static string buffname = "Shadows";
        ShadowSetting shadowSetting;
        Matrix4x4[] dirShadowVPMatrix = new Matrix4x4[MAX_SHADOW_LIGHT];
        CommandBuffer buffer = new CommandBuffer
        {
            name = buffname
        };
        CullingResults cullResults;
        static int dirShadowAtlasID = Shader.PropertyToID("_DirectionalShadowAtlas");

        static int dirShadowVPMatrixID = Shader.PropertyToID("_DirShadowVPMatrix");


        struct ShadowDirctionalLight
        {
            public int visiableLightIndex;
        }

        //用于生成阴影的可见光索引
        ShadowDirctionalLight[] ShadowDirctionalLights = new ShadowDirctionalLight[MAX_SHADOW_LIGHT];
        ScriptableRenderContext context;
        int shadowDirctionalLightCount;
        public void SetUp(ScriptableRenderContext context, CullingResults cullResults, ShadowSetting shadowSetting)
        {
            this.context = context;
            this.shadowSetting = shadowSetting;
            this.cullResults = cullResults;
            this.shadowDirctionalLightCount = 0;
        }

        public Vector2 ReserveDirectionalShadow(Light ligth, int visiableLightIndex)
        {
            if (visiableLightIndex < MAX_SHADOW_LIGHT
            && ligth.shadows != LightShadows.None
            && ligth.shadowStrength > 0
            && cullResults.GetShadowCasterBounds(visiableLightIndex, out Bounds bound))
            {
                ShadowDirctionalLights[shadowDirctionalLightCount] = new ShadowDirctionalLight { visiableLightIndex = visiableLightIndex };
                var temp = new Vector2(ligth.shadowStrength, shadowDirctionalLightCount);
                shadowDirctionalLightCount++;
                return temp;
            }
            return Vector2.zero;
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
            var spile = Mathf.CeilToInt(Mathf.Sqrt(shadowDirctionalLightCount));
            var tilesize = atlassize / spile;
            for (int i = 0; i < shadowDirctionalLightCount; i++)
            {
                RenderDirctionalShadows(i, spile, tilesize);
            }
            buffer.SetGlobalMatrixArray(dirShadowVPMatrixID, dirShadowVPMatrix);
            buffer.EndSample(buffname);
            ExecuteBuffer();
        }

        void RenderDirctionalShadows(int index, int split, int tilesize)
        {
            ShadowDirctionalLight light = ShadowDirctionalLights[index];
            ShadowDrawingSettings shadowDrawingSettings = new ShadowDrawingSettings(cullResults, light.visiableLightIndex);
            var temp = cullResults.visibleLights[light.visiableLightIndex];
            cullResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.visiableLightIndex, 0, 1, Vector3.zero, tilesize, 0f
            , out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix, out ShadowSplitData shadowSplitData);
            shadowDrawingSettings.splitData = shadowSplitData;
            var offset = setTileViewport(index, split, tilesize);
            buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            Debug.Log("[CTA] projectionMatrix"+projectionMatrix);
            Debug.Log("[CTA] viewMatrix"+viewMatrix);
            dirShadowVPMatrix[index] = convertToAtlasMatrix(projectionMatrix * viewMatrix, offset, split);
            ExecuteBuffer();
            context.DrawShadows(ref shadowDrawingSettings);
        }

        Vector2 setTileViewport(int index, int split, float size)
        {
            Vector2 offset = new Vector2(index % split, index / split);
            buffer.SetViewport(new Rect(offset.x * size, offset.y * size, size, size));
            return offset;
        }


        Matrix4x4 convertToAtlasMatrix(Matrix4x4 vpm, Vector2 offset, int split = 1)
        {
 //如果使用了反向Zbuffer
        if (SystemInfo.usesReversedZBuffer)
        {
            vpm.m20 = -vpm.m20;
            vpm.m21 = -vpm.m21;
            vpm.m22 = -vpm.m22;
            vpm.m23 = -vpm.m23;
        }
            Debug.Log("[CTA] offset" + offset);
        //设置矩阵坐标
        float scale = 1f / split;
        vpm.m00 = (0.5f * (vpm.m00 + vpm.m30) + offset.x * vpm.m30) * scale;
        vpm.m01 = (0.5f * (vpm.m01 + vpm.m31) + offset.x * vpm.m31) * scale;
        vpm.m02 = (0.5f * (vpm.m02 + vpm.m32) + offset.x * vpm.m32) * scale;
        vpm.m03 = (0.5f * (vpm.m03 + vpm.m33) + offset.x * vpm.m33) * scale;
        vpm.m10 = (0.5f * (vpm.m10 + vpm.m30) + offset.y * vpm.m30) * scale;
        vpm.m11 = (0.5f * (vpm.m11 + vpm.m31) + offset.y * vpm.m31) * scale;
        vpm.m12 = (0.5f * (vpm.m12 + vpm.m32) + offset.y * vpm.m32) * scale;
        vpm.m13 = (0.5f * (vpm.m13 + vpm.m33) + offset.y * vpm.m33) * scale;
        vpm.m20 = 0.5f * (vpm.m20 + vpm.m30);
        vpm.m21 = 0.5f * (vpm.m21 + vpm.m31);
        vpm.m22 = 0.5f * (vpm.m22 + vpm.m32);
        vpm.m23 = 0.5f * (vpm.m23 + vpm.m33);
        Debug.Log("[CTA] vpm" + vpm);
        return vpm;
        }
    }
}
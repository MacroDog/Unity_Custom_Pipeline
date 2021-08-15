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
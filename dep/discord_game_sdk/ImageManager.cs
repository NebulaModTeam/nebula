#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine;
#endif

namespace Discord
{
    public partial struct ImageHandle
    {
        public static ImageHandle User(long id)
        {
            return User(id, 128);
        }

        public static ImageHandle User(long id, uint size)
        {
            return new ImageHandle { Type = ImageType.User, Id = id, Size = size };
        }
    }

    public partial class ImageManager
    {
        public void Fetch(ImageHandle handle, FetchHandler callback)
        {
            Fetch(handle, false, callback);
        }

        public byte[] GetData(ImageHandle handle)
        {
            var dimensions = GetDimensions(handle);
            var data = new byte[dimensions.Width * dimensions.Height * 4];
            GetData(handle, data);
            return data;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        public Texture2D GetTexture(ImageHandle handle)
        {
            var dimensions = GetDimensions(handle);
            var texture = new Texture2D((int)dimensions.Width, (int)dimensions.Height, TextureFormat.RGBA32, false, true);
            texture.LoadRawTextureData(GetData(handle));
            texture.Apply();
            return texture;
        }
#endif
    }
}

using AssetStudioUtility;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.IO;

namespace AssetStudio
{
    public static class Texture2DExtensions
    {
        public static Image<Bgra32> ConvertToImage(this Texture2D m_Texture2D, bool flip)
        {
            var width = m_Texture2D.m_Width;
            var height = m_Texture2D.m_Height;

            var switchDeswizzle = false;
            if (m_Texture2D.reader.assetsFile.m_TargetPlatform == BuildTarget.Switch)
            {
                //unsure how to decode this, but it appears that only
                //texture2ds with this array non-empty are swizzled.
                if (m_Texture2D.m_PlatformBlob.Length != 0)
                {
                    switchDeswizzle = true;
                    height = ToNextNearestPo2(height);
                }
            }

            var converter = new Texture2DConverter(m_Texture2D, width, height);

            var buff = BigArrayPool<byte>.Shared.Rent(width * height * 4);
            try
            {
                if (converter.DecodeTexture2D(buff))
                {
                    var image = Image.LoadPixelData<Bgra32>(buff, width, height);
                    if (switchDeswizzle)
                    {
                        Size blockSize = Texture2DDeswizzler.TextureFormatToBlockSize(m_Texture2D.m_TextureFormat);
                        image = Texture2DDeswizzler.SwitchUnswizzle(image, blockSize);
                        image.Mutate(i => i.Crop(width, m_Texture2D.m_Height));
                    }
                    if (flip)
                    {
                        image.Mutate(x => x.Flip(FlipMode.Vertical));
                    }
                    return image;
                }
                return null;
            }
            finally
            {
                BigArrayPool<byte>.Shared.Return(buff);
            }
        }

        public static MemoryStream ConvertToStream(this Texture2D m_Texture2D, ImageFormat imageFormat, bool flip)
        {
            var image = ConvertToImage(m_Texture2D, flip);
            if (image != null)
            {
                using (image)
                {
                    return image.ConvertToStream(imageFormat);
                }
            }
            return null;
        }

        private static int ToNextNearestPo2(int x)
        {
            if (x < 0)
                return 0;

            --x;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            return x + 1;
        }
    }
}

using AssetStudioUtility;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
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
            var gobsPerBlock = 1;
            var blockSize = new Size(1, 1);
            if (m_Texture2D.reader.assetsFile.m_TargetPlatform == BuildTarget.Switch)
            {
                //unsure how to decode this, but it appears that only
                //texture2ds with this array non-empty are swizzled.
                if (m_Texture2D.m_PlatformBlob.Length != 0)
                {
                    switchDeswizzle = true;

                    var platBlob = m_Texture2D.m_PlatformBlob;
                    gobsPerBlock = 1 << BitConverter.ToInt32(platBlob, 8);
                    //apparently there is another value to worry about, but seeing as it's
                    //always 0 and I have nothing else to test against, this will probably
                    //work fine for now

                    //in older versions of unity, rgb24 has a platformblob which shouldn't
                    //be possible. it turns out in this case, the image is just rgba32.
                    //probably shouldn't be modifying the texture2d here, but eh, who cares
                    if (m_Texture2D.m_TextureFormat == TextureFormat.RGB24)
                    {
                        m_Texture2D.m_TextureFormat = TextureFormat.RGBA32;
                    }
                    else if (m_Texture2D.m_TextureFormat == TextureFormat.BGR24)
                    {
                        m_Texture2D.m_TextureFormat = TextureFormat.BGRA32;
                    }

                    blockSize = Texture2DDeswizzler.TextureFormatToBlockSize(m_Texture2D.m_TextureFormat);
                    var newSize = Texture2DDeswizzler.SwitchGetPaddedTextureSize(width, height, blockSize.Width, blockSize.Height, gobsPerBlock);
                    width = newSize.Width;
                    height = newSize.Height;
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
                        image = Texture2DDeswizzler.SwitchUnswizzle(image, blockSize, gobsPerBlock);
                        image.Mutate(i => i.Crop(m_Texture2D.m_Width, m_Texture2D.m_Height));
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

        private static int CeilDivide(int a, int b)
        {
            return (a + b - 1) / b;
        }
    }
}

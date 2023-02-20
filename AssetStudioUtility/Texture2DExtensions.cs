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
            var converter = new Texture2DConverter(m_Texture2D);
            Size uncroppedSize = converter.GetUncroppedSize();
            bool usesSwitchSwizzle = converter.UsesSwitchSwizzle();
            var buff = BigArrayPool<byte>.Shared.Rent(uncroppedSize.Width * uncroppedSize.Height * 4);
            try
            {
                if (converter.DecodeTexture2D(buff))
                {
                    Image<Bgra32> image;
                    if (usesSwitchSwizzle)
                    {
                        image = Image.LoadPixelData<Bgra32>(buff, uncroppedSize.Width, uncroppedSize.Height);
                        image.Mutate(x => x.Crop(m_Texture2D.m_Width, m_Texture2D.m_Height));
                    }
                    else
                    {
                        image = Image.LoadPixelData<Bgra32>(buff, m_Texture2D.m_Width, m_Texture2D.m_Height);
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

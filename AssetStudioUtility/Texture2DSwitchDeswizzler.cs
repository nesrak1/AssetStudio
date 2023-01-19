using AssetStudio;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AssetStudioUtility
{
    public class Texture2DDeswizzler
    {
        const int SECTOR_HEIGHT = 2;
        const int GOB_WIDTH = 4;
        const int GOB_HEIGHT = 8;
        const int SECTOR_LINES_IN_GOB = GOB_WIDTH * GOB_HEIGHT;

        private static void CopyBlock(Image<Bgra32> srcImage, Image<Bgra32> dstImage, int sbx, int sby, int dbx, int dby, int blockSizeW, int blockSizeH)
        {
            for (int i = 0; i < blockSizeW; i++)
            {
                for (int j = 0; j < blockSizeH; j++)
                {
                    dstImage[dbx * blockSizeW + i, dby * blockSizeH + j] = srcImage[sbx * blockSizeW + i, sby * blockSizeH + j];
                }
            }
        }

        private static int CeilDivide(int a, int b)
        {
            return (a + b - 1) / b;
        }

        internal static Image<Bgra32> SwitchUnswizzle(Image<Bgra32> srcImage, Size blockSize)
        {
            Image<Bgra32> dstImage = new Image<Bgra32>(srcImage.Width, srcImage.Height);

            int width = srcImage.Width;
            int height = srcImage.Height;

            int blockCountX = CeilDivide(width, blockSize.Width);
            int blockCountY = CeilDivide(height, blockSize.Height);

            int gobCountX = blockCountX / GOB_WIDTH;
            int gobCountY = blockCountY / GOB_HEIGHT;

            int srcX = 0;
            int srcY = 0;

            int blockCountYOr16 = Math.Min(gobCountY, GOB_HEIGHT * SECTOR_HEIGHT);

            for (int i = 0; i < gobCountY / blockCountYOr16; i++)
            {
                for (int j = 0; j < gobCountX; j++)
                {
                    for (int k = 0; k < blockCountYOr16; k++)
                    {
                        for (int l = 0; l < SECTOR_LINES_IN_GOB; l++)
                        {
                            int gobX = ((l >> 3) & 0b10) | ((l >> 1) & 0b1);
                            int gobY = ((l >> 1) & 0b110) | (l & 0b1);
                            int gobDstX = j * GOB_WIDTH + gobX;
                            int gobDstY = (i * blockCountYOr16 + k) * GOB_HEIGHT + gobY;
                            CopyBlock(srcImage, dstImage, srcX, srcY, gobDstX, gobDstY, blockSize.Width, blockSize.Height);

                            srcX++;
                            if (srcX >= blockCountX)
                            {
                                srcX = 0;
                                srcY++;
                            }
                        }
                    }
                }
            }

            return dstImage;
        }

        internal static Size TextureFormatToBlockSize(TextureFormat m_TextureFormat)
        {
            switch (m_TextureFormat)
            {
                case TextureFormat.Alpha8: return new Size(1, 1); //untested
                case TextureFormat.ARGB4444: return new Size(1, 1); //untested
                case TextureFormat.RGB24: return new Size(1, 1); //untested
                case TextureFormat.RGBA32: return new Size(1, 1); //untested
                case TextureFormat.ARGB32: return new Size(1, 1); //untested
                case TextureFormat.ARGBFloat: return new Size(1, 1); //untested
                case TextureFormat.RGB565: return new Size(1, 1); //untested
                case TextureFormat.BGR24: return new Size(1, 1); //untested
                case TextureFormat.R16: return new Size(1, 1); //untested
                case TextureFormat.DXT1: return new Size(8, 4); //8 bytes per block
                case TextureFormat.DXT3: return new Size(4, 4); //untested
                case TextureFormat.DXT5: return new Size(4, 4); //16 bytes per block
                case TextureFormat.RGBA4444: return new Size(1, 1);
                case TextureFormat.BGRA32: return new Size(1, 1);
                case TextureFormat.RHalf: throw new NotImplementedException();
                case TextureFormat.RGHalf: throw new NotImplementedException();
                case TextureFormat.RGBAHalf: throw new NotImplementedException();
                case TextureFormat.RFloat: throw new NotImplementedException();
                case TextureFormat.RGFloat: throw new NotImplementedException();
                case TextureFormat.RGBAFloat: throw new NotImplementedException();
                case TextureFormat.YUY2: throw new NotImplementedException();
                case TextureFormat.RGB9e5Float: throw new NotImplementedException();
                case TextureFormat.RGBFloat: throw new NotImplementedException();
                case TextureFormat.BC6H: throw new NotImplementedException();
                case TextureFormat.BC7: return new Size(4, 4); //untested
                case TextureFormat.BC4: throw new NotImplementedException(); //same as dxt1?
                case TextureFormat.BC5: return new Size(4, 4); //untested
                case TextureFormat.DXT1Crunched: throw new NotImplementedException();
                case TextureFormat.DXT5Crunched: throw new NotImplementedException();
                case TextureFormat.PVRTC_RGB2: throw new NotImplementedException();
                case TextureFormat.PVRTC_RGBA2: throw new NotImplementedException();
                case TextureFormat.PVRTC_RGB4: throw new NotImplementedException();
                case TextureFormat.PVRTC_RGBA4: throw new NotImplementedException();
                case TextureFormat.ETC_RGB4: throw new NotImplementedException();
                case TextureFormat.ATC_RGB4: throw new NotImplementedException();
                case TextureFormat.ATC_RGBA8: throw new NotImplementedException();
                case TextureFormat.EAC_R: throw new NotImplementedException();
                case TextureFormat.EAC_R_SIGNED: throw new NotImplementedException();
                case TextureFormat.EAC_RG: throw new NotImplementedException();
                case TextureFormat.EAC_RG_SIGNED: throw new NotImplementedException();
                case TextureFormat.ETC2_RGB: throw new NotImplementedException();
                case TextureFormat.ETC2_RGBA1: throw new NotImplementedException();
                case TextureFormat.ETC2_RGBA8: throw new NotImplementedException();
                case TextureFormat.ASTC_RGB_4x4: return new Size(4, 4); //untested
                case TextureFormat.ASTC_RGB_5x5: return new Size(5, 5); //untested
                case TextureFormat.ASTC_RGB_6x6: return new Size(6, 6); //untested
                case TextureFormat.ASTC_RGB_8x8: return new Size(8, 8); //untested
                case TextureFormat.ASTC_RGB_10x10: return new Size(10, 10); //untested
                case TextureFormat.ASTC_RGB_12x12: return new Size(12, 12); //untested
                case TextureFormat.ASTC_RGBA_4x4: return new Size(4, 4); //untested
                case TextureFormat.ASTC_RGBA_5x5: return new Size(5, 5); //untested
                case TextureFormat.ASTC_RGBA_6x6: return new Size(6, 6); //untested
                case TextureFormat.ASTC_RGBA_8x8: return new Size(8, 8); //untested
                case TextureFormat.ASTC_RGBA_10x10: return new Size(10, 10); //untested
                case TextureFormat.ASTC_RGBA_12x12: return new Size(12, 12); //untested
                case TextureFormat.ETC_RGB4_3DS: throw new NotImplementedException();
                case TextureFormat.ETC_RGBA8_3DS: throw new NotImplementedException();
                case TextureFormat.RG16: throw new NotImplementedException();
                case TextureFormat.R8: throw new NotImplementedException();
                case TextureFormat.ETC_RGB4Crunched: throw new NotImplementedException();
                case TextureFormat.ETC2_RGBA8Crunched: throw new NotImplementedException();
                case TextureFormat.ASTC_HDR_4x4: throw new NotImplementedException();
                case TextureFormat.ASTC_HDR_5x5: throw new NotImplementedException();
                case TextureFormat.ASTC_HDR_6x6: throw new NotImplementedException();
                case TextureFormat.ASTC_HDR_8x8: throw new NotImplementedException();
                case TextureFormat.ASTC_HDR_10x10: throw new NotImplementedException();
                case TextureFormat.ASTC_HDR_12x12: throw new NotImplementedException();
                case TextureFormat.RG32: throw new NotImplementedException();
                case TextureFormat.RGB48: throw new NotImplementedException();
                case TextureFormat.RGBA64: throw new NotImplementedException();
                default: throw new NotImplementedException();
            };
        }
    }
}

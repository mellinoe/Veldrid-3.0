﻿using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System;
using System.IO;

namespace Veldrid.Graphics
{
    /// <summary>
    /// A texture loaded by ImageSharp. This is a single, non-mipmapped image.
    /// To create a mipmap chain, construct an ImageSharpMipmapChain.
    /// </summary>
    public class ImageSharpTexture : TextureData
    {
        private static readonly PngEncoder s_pngEncoder = new PngEncoder();

        /// <summary>
        /// The ImageSharp image.
        /// </summary>
        public Image<Rgba32> ISImage { get; }

        /// <summary>
        /// The raw pixel data, stored in RGBA format, where each element is a byte (32 bits per pixel).
        /// </summary>
        public ref Rgba32 FirstPixel => ref ISImage.GetReferenceToFirstPixel();

        /// <summary>
        /// The width of the texture.
        /// </summary>
        public int Width => ISImage.Width;
        /// <summary>
        /// The height of the iamge.
        /// </summary>
        public int Height => ISImage.Height;

        /// <summary>
        /// The <see cref="PixelFormat"/> of the data.
        /// </summary>
        public PixelFormat Format => PixelFormat.R8_G8_B8_A8_UInt;

        /// <summary>
        /// The size of each pixel, in bytes.
        /// </summary>
        public int PixelSizeInBytes => sizeof(byte) * 4;

        public int MipLevels => 1;

        /// <summary>
        /// Loads and constructs a new ImageSharpTexture from the file at the given path.
        /// </summary>
        /// <param name="filePath">The path to the file on disk.</param>
        public ImageSharpTexture(string filePath)
        {
            using (FileStream fs = File.OpenRead(filePath))
            {
                ISImage = Image.Load(fs);
            }
        }

        /// <summary>
        /// Constructs an ImageSharpTexture from the existing ImageProcessor image.
        /// </summary>
        /// <param name="image">The existing image.</param>
        public ImageSharpTexture(Image<Rgba32> image)
        {
            ISImage = image;
        }
        
        /// <summary>
        /// Saves the image to disk.
        /// </summary>
        /// <param name="path">The target path on disk.</param>
        public void SaveToFile(string path)
        {
            using (FileStream fs = File.OpenWrite(path))
            {
                ISImage.Save(fs, s_pngEncoder);
            }
        }

        /// <summary>
        /// Constructs a DeviceTexture from this texture.
        /// </summary>
        /// <param name="factory">The resource factory responsible for constructing the DeviceTexture.</param>
        /// <returns>A new <see cref="DeviceTexture2D"/> containing this image's pixel data.</returns>
        public unsafe DeviceTexture2D CreateDeviceTexture(ResourceFactory factory)
        {
            fixed (Rgba32* pixelPtr = &FirstPixel)
            {
                return factory.CreateTexture(new IntPtr(pixelPtr), Width, Height, Format);
            }
        }
    }
}

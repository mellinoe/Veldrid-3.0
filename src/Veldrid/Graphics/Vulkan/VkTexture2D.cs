﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Vulkan;
using static Veldrid.Graphics.Vulkan.VulkanUtil;
using static Vulkan.VulkanNative;

namespace Veldrid.Graphics.Vulkan
{
    public unsafe class VkTexture2D : DeviceTexture2D
    {
        private VkImage _image;
        private VkDeviceMemory _memory;
        private VkDevice _device;
        private VkPhysicalDevice _physicalDevice;
        private PixelFormat _veldridFormat;
        private DeviceTextureCreateOptions _createOptions;

        public VkTexture2D(
            VkDevice device,
            VkPhysicalDevice physicalDevice,
            int mipLevels,
            int width,
            int height,
            PixelFormat veldridFormat,
            DeviceTextureCreateOptions createOptions)
        {
            _device = device;
            _physicalDevice = physicalDevice;
            MipLevels = mipLevels;
            Width = width;
            Height = height;
            _createOptions = createOptions;
            if (createOptions == DeviceTextureCreateOptions.DepthStencil)
            {
                Format = VkFormat.D16Unorm;
            }
            else
            {
                Format = VkFormats.VeldridToVkPixelFormat(veldridFormat);
            }

            _veldridFormat = veldridFormat;

            VkImageCreateInfo imageCI = VkImageCreateInfo.New();
            imageCI.mipLevels = (uint)mipLevels;
            imageCI.arrayLayers = 1;
            imageCI.imageType = VkImageType.Image2D;
            imageCI.extent.width = (uint)width;
            imageCI.extent.height = (uint)height;
            imageCI.extent.depth = 1;
            imageCI.initialLayout = VkImageLayout.Undefined; // TODO: Use proper VkImageLayout values and transitions.
            imageCI.usage = VkImageUsageFlags.TransferSrc;
            if (createOptions == DeviceTextureCreateOptions.RenderTarget)
            {
                imageCI.usage |= VkImageUsageFlags.ColorAttachment;
            }
            else if (createOptions == DeviceTextureCreateOptions.DepthStencil)
            {
                imageCI.usage |= VkImageUsageFlags.DepthStencilAttachment;
            }
            imageCI.tiling = createOptions == DeviceTextureCreateOptions.DepthStencil ? VkImageTiling.Optimal : VkImageTiling.Linear;
            imageCI.format = Format;

            imageCI.samples = VkSampleCountFlags.Count1;

            VkResult result = vkCreateImage(device, ref imageCI, null, out _image);
            CheckResult(result);

            vkGetImageMemoryRequirements(_device, _image, out VkMemoryRequirements memoryRequirements);

            VkMemoryAllocateInfo memoryAI = VkMemoryAllocateInfo.New();
            memoryAI.allocationSize = memoryRequirements.size;
            memoryAI.memoryTypeIndex = FindMemoryType(
                _physicalDevice,
                memoryRequirements.memoryTypeBits,
                VkMemoryPropertyFlags.DeviceLocal);
            vkAllocateMemory(_device, ref memoryAI, null, out _memory);
            vkBindImageMemory(_device, _image, _memory, 0);
        }

        public VkTexture2D(
            VkDevice device,
            int mipLevels,
            int width,
            int height,
            VkFormat vkFormat,
            VkImage existingImage)
        {
            _device = device;
            MipLevels = mipLevels;
            Width = width;
            Height = height;
            Format = vkFormat;
            _veldridFormat = VkFormats.VkToVeldridPixelFormat(vkFormat);
            _image = existingImage;
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int MipLevels { get; private set; }

        public VkFormat Format { get; }

        public VkImage DeviceImage => _image;

        public void GetTextureData(int mipLevel, IntPtr destination, int storageSizeInBytes)
        {
            throw new NotImplementedException();
        }

        public void GetTextureData<T>(int mipLevel, T[] destination) where T : struct
        {
            throw new NotImplementedException();
        }

        public void SetTextureData(int mipLevel, int x_unused, int y_unused, int width, int height, IntPtr data, int dataSizeInBytes)
        {
            Debug.Assert(x_unused == 0 && y_unused == 0);

            VkImageSubresource subresource = new VkImageSubresource();
            subresource.aspectMask = VkImageAspectFlags.Color;
            subresource.mipLevel = (uint)mipLevel;
            subresource.arrayLayer = 0;
            vkGetImageSubresourceLayout(_device, _image, ref subresource, out VkSubresourceLayout layout);
            ulong rowPitch = layout.rowPitch;

            void* mappedPtr;
            VkResult result = vkMapMemory(_device, _memory, 0, (ulong)dataSizeInBytes, 0, &mappedPtr);
            CheckResult(result);

            if (rowPitch == (ulong)width)
            {
                Buffer.MemoryCopy(data.ToPointer(), mappedPtr, dataSizeInBytes, dataSizeInBytes);
            }
            else
            {
                int pixelSizeInBytes = FormatHelpers.GetPixelSizeInBytes(_veldridFormat);
                for (uint y = 0; y < height; y++)
                {
                    byte* dstRowStart = ((byte*)mappedPtr) + (rowPitch * y);
                    byte* srcRowStart = ((byte*)data.ToPointer()) + (width * y * pixelSizeInBytes);
                    Unsafe.CopyBlock(dstRowStart, srcRowStart, (uint)(width * pixelSizeInBytes));
                }
            }

            vkUnmapMemory(_device, _memory);
        }

        public void Dispose()
        {
            vkDestroyImage(_device, _image, null);
            if (_memory != VkDeviceMemory.Null)
            {
                vkFreeMemory(_device, _memory, null);
            }
        }
    }
}

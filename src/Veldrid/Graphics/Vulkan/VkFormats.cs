﻿using System;
using Vulkan;

namespace Veldrid.Graphics.Vulkan
{
    public static class VkFormats
    {
        public static VkFormat VeldridToVkPixelFormat(PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.R32_G32_B32_A32_Float:
                    return VkFormat.R32g32b32a32Sfloat;
                case PixelFormat.R8_UInt:
                    return VkFormat.R8Uint;
                case PixelFormat.R16_UInt:
                    return VkFormat.R16Uint;
                case PixelFormat.R8_G8_B8_A8_UInt:
                    return VkFormat.R8g8b8a8Uint;
                case PixelFormat.B8_G8_R8_A8_UInt:
                    return VkFormat.B8g8r8a8Unorm;
                default:
                    throw Illegal.Value<PixelFormat>();
            }
        }

        public static PixelFormat VkToVeldridPixelFormat(VkFormat vkFormat)
        {
            switch (vkFormat)
            {
                case VkFormat.R32g32b32a32Sfloat:
                    return PixelFormat.R32_G32_B32_A32_Float;
                case VkFormat.R8Uint:
                    return PixelFormat.R8_UInt;
                case VkFormat.R16Uint:
                    return PixelFormat.R16_UInt;
                case VkFormat.R8g8b8a8Uint:
                    return PixelFormat.R8_G8_B8_A8_UInt;
                case VkFormat.B8g8r8a8Unorm:
                    return PixelFormat.B8_G8_R8_A8_UInt;
                default:
                    throw Illegal.Value<VkFormat>();
            }
        }


        internal static VkCullModeFlags VeldridToVkCullMode(FaceCullingMode cullMode)
        {
            switch (cullMode)
            {
                case FaceCullingMode.Back:
                    return VkCullModeFlags.Back;
                case FaceCullingMode.Front:
                    return VkCullModeFlags.Front;
                case FaceCullingMode.None:
                    return VkCullModeFlags.None;
                default:
                    throw Illegal.Value<FaceCullingMode>();
            }
        }

        internal static VkPolygonMode VeldridToVkFillMode(TriangleFillMode fillMode)
        {
            switch (fillMode)
            {
                case TriangleFillMode.Solid:
                    return VkPolygonMode.Fill;
                case TriangleFillMode.Wireframe:
                    return VkPolygonMode.Line;
                default:
                    throw Illegal.Value<TriangleFillMode>();
            }
        }

        public static VkIndexType VeldridToVkIndexFormat(IndexFormat format)
        {
            switch (format)
            {
                case IndexFormat.UInt16:
                    return VkIndexType.Uint16;
                case IndexFormat.UInt32:
                    return VkIndexType.Uint32;
                default:
                    throw Illegal.Value<IndexFormat>();
            }
        }
    }
}

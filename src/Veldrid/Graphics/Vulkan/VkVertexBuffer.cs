﻿using System;
using System.Runtime.CompilerServices;
using Vulkan;

namespace Veldrid.Graphics.Vulkan
{
    public class VkVertexBuffer : VkDeviceBuffer, VertexBuffer
    {
        public VkVertexBuffer(
            VkDevice device,
            VkPhysicalDevice physicalDevice,
            ulong size,
            VkMemoryPropertyFlags memoryProperties)
            : base(device, physicalDevice, size, VkBufferUsageFlags.VertexBuffer, memoryProperties)
        {
        }

        public void SetVertexData<T>(T[] vertexData, VertexDescriptor descriptor) where T : struct
        {
            SetData(vertexData);
        }

        public void SetVertexData<T>(T[] vertexData, VertexDescriptor descriptor, int destinationOffsetInVertices) where T : struct
        {
            int byteOffset = Unsafe.SizeOf<T>() * destinationOffsetInVertices;
            SetData(vertexData, byteOffset);
        }

        public void SetVertexData<T>(ArraySegment<T> vertexData, VertexDescriptor descriptor, int destinationOffsetInVertices) where T : struct
        {
            int byteOffset = Unsafe.SizeOf<T>() * destinationOffsetInVertices;
            SetData(vertexData, byteOffset);
        }

        public void SetVertexData(IntPtr vertexData, VertexDescriptor descriptor, int numVertices)
        {
            int dataSizeInBytes = numVertices * descriptor.VertexSizeInBytes;
            SetData(vertexData, dataSizeInBytes);
        }

        public void SetVertexData(IntPtr vertexData, VertexDescriptor descriptor, int numVertices, int destinationOffsetInVertices)
        {
            int dataSizeInBytes = numVertices * descriptor.VertexSizeInBytes;
            int destinationOffsetInBytes = destinationOffsetInVertices * descriptor.VertexSizeInBytes;
            SetData(vertexData, dataSizeInBytes, destinationOffsetInBytes);
        }
    }
}
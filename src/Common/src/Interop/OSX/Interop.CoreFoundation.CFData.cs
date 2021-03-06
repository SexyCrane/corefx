// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

using Microsoft.Win32.SafeHandles;

// Declared as signed long, which has sizeof(void*) on OSX.
using CFIndex=System.IntPtr;

internal static partial class Interop
{
    internal static partial class CoreFoundation
    {
        [DllImport(Libraries.CoreFoundationLibrary)]
        private static extern unsafe byte* CFDataGetBytePtr(SafeCFDataHandle cfData);

        [DllImport(Libraries.CoreFoundationLibrary)]
        private static extern CFIndex CFDataGetLength(SafeCFDataHandle cfData);

        internal static byte[] CFGetData(SafeCFDataHandle cfData)
        {
            bool addedRef = false;

            try
            {
                cfData.DangerousAddRef(ref addedRef);
                byte[] bytes = new byte[CFDataGetLength(cfData).ToInt64()];

                unsafe
                {
                    byte* dataBytes = CFDataGetBytePtr(cfData);
                    Marshal.Copy((IntPtr)dataBytes, bytes, 0, bytes.Length);
                }

                return bytes;

            }
            finally
            {
                if (addedRef)
                {
                    cfData.DangerousRelease();
                }
            }
        }
    }
}

namespace Microsoft.Win32.SafeHandles
{
    internal sealed class SafeCFDataHandle : SafeHandle
    {
        internal SafeCFDataHandle()
            : base(IntPtr.Zero, ownsHandle: true)
        {
        }

        internal SafeCFDataHandle(IntPtr handle, bool ownsHandle)
            : base(handle, ownsHandle)
        {
        }

        protected override bool ReleaseHandle()
        {
            Interop.CoreFoundation.CFRelease(handle);
            SetHandle(IntPtr.Zero);
            return true;
        }

        public override bool IsInvalid => handle == IntPtr.Zero;
    }
}

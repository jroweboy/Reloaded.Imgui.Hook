using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Imgui.Hook.Misc;
using CallingConventions = Reloaded.Hooks.Definitions.X86.CallingConventions;
using Debug = Reloaded.Imgui.Hook.Misc.Debug;

namespace Reloaded.Imgui.Hook.OpenGL3
{
    /// <summary>
    /// Provides access to OpenGL 3 functions.
    /// </summary>
    internal static class GL3Hook
    {

        /// <summary>
        /// Pointer to SwapBuffers
        /// </summary>
        public static IntPtr SwapBuffersPtr { get; private set; }

        static GL3Hook()
        {
            // Debugger.Launch();
            IntPtr libHandle = LoadLibrary("opengl32.dll");
            SwapBuffersPtr = GetProcAddress(libHandle, "wglSwapBuffers");
            Debug.DebugWriteLine($"[GL3 Imgui] SwapBuffers found at {SwapBuffersPtr.ToInt64()}");
        }

        /// <summary>
        /// Defines the gdi32 SwapBuffers function, allowing us to render ontop of the OpenGL scene.
        /// </summary>
        /// <param name="deviceContext">Handle to the deviceContext.</param>
        [Function(Reloaded.Hooks.Definitions.X64.CallingConventions.Microsoft)]
        [Reloaded.Hooks.Definitions.X86.Function(CallingConventions.Stdcall)]
        public delegate bool SwapBuffers(IntPtr deviceContext);

        ///// <summary>
        ///// Defines MakeCurrent allowing us to detect
        ///// </summary>
        ///// <param name="deviceContext">Handle to the deviceContext.</param>
        ///// <param name="glRenderingContext">Handle to the OpenGL Rendering Context</param>
        //[Function(Reloaded.Hooks.Definitions.X64.CallingConventions.Microsoft)]
        //[Reloaded.Hooks.Definitions.X86.Function(CallingConventions.Stdcall)]
        //public delegate bool MakeCurrent(IntPtr deviceContext, IntPtr glRenderingContext);

        [DllImport("kernel32", SetLastError=true, CharSet = CharSet.Ansi)]
        static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("user32.dll")]
        internal static extern IntPtr WindowFromDC(IntPtr hDeviceContext);
    }
}

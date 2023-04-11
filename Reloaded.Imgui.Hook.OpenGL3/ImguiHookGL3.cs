using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DearImguiSharp;
using Reloaded.Hooks.Definitions;
using Reloaded.Imgui.Hook.OpenGL3;
using static Reloaded.Imgui.Hook.Misc.Native;
using Debug = Reloaded.Imgui.Hook.Misc.Debug;

namespace Reloaded.Imgui.Hook.Implementations
{

    public class ImguiHookGL3 : IImguiHook
    {
        public static ImguiHookGL3 Instance { get; private set; }

        private IHook<GL3Hook.SwapBuffers> _swapBuffers;
        private bool _initialized;
        private IntPtr _windowHandle;
        private IntPtr _device;

        /*
            * In some cases (E.g. under DX9 + Viewports enabled), Dear ImGui might call
            * DirectX functions from within its internal logic.
            *
            * We put a lock on the current thread in order to prevent stack overflow.
            */
        private bool _swapBuffersRecursionLock;

        public void Initialize()
        {
            Instance = this;

            Debug.WriteLine($"[GL3 SwapBuffers] Intializing");
            _swapBuffers = SDK.Hooks.CreateHook<GL3Hook.SwapBuffers>(SwapBuffersImpl, (long)GL3Hook.SwapBuffersPtr).Activate();

        }

        ~ImguiHookGL3()
        {
            ReleaseUnmanagedResources();
        }

        public bool IsApiSupported() => GetModuleHandle("opengl32.dll") != IntPtr.Zero;

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        private void Shutdown()
        {
            Debug.WriteLine($"[GL3 Shutdown] Shutdown");
            ImGui.ImGuiImplOpenGL3Shutdown();
            _windowHandle = IntPtr.Zero;
            _device = IntPtr.Zero;
            _initialized = false;
            ImguiHook.Shutdown();
        }

        private void ReleaseUnmanagedResources()
        {
            if (_initialized)
                Shutdown();
        }

        private unsafe bool SwapBuffersImpl(IntPtr deviceContext)
        {
            if (_swapBuffersRecursionLock)
            {
                Debug.WriteLine($"[GL3 SwapBuffers] Discarding via Recursion Lock");
                return _swapBuffers.OriginalFunction.Invoke(deviceContext);
            }

            _swapBuffersRecursionLock = true;
            try
            {
                var windowHandle = GL3Hook.WindowFromDC(deviceContext);
                // Ignore windows which don't belong to us.
                if (!ImguiHook.CheckWindowHandle(windowHandle))
                {
                    Debug.WriteLine($"[GL3 SwapBuffers] Discarding Window Handle {(long)windowHandle:X}");
                    return _swapBuffers.OriginalFunction.Invoke(deviceContext);
                }

                if (!_initialized)
                {
                    _device = deviceContext;
                    _windowHandle = windowHandle;
                    if (_windowHandle == IntPtr.Zero)
                        return _swapBuffers.OriginalFunction.Invoke(deviceContext);

                    Debug.WriteLine($"[GL3 SwapBuffers] Init, Window Handle {(long)windowHandle:X}");
                    ImguiHook.InitializeWithHandle(windowHandle);
                    ImGui.ImGuiImplOpenGL3Init("#version 130"); // GL 3.0
                    _initialized = true;
                }
                ImGui.ImGuiImplOpenGL3NewFrame();
                ImguiHook.NewFrame();
                using var drawData = ImGui.GetDrawData();
                ImGui.ImGuiImplOpenGL3RenderDrawData(drawData);
                return _swapBuffers.OriginalFunction.Invoke(deviceContext);
            }
            finally
            {
                _swapBuffersRecursionLock = false;
            }
        }

        public void Disable()
        {
            Debug.WriteLine($"[GL3 SwapBuffers] Disabling");
            _swapBuffers.Disable();
        }

        public void Enable()
        {
            Debug.WriteLine($"[GL3 SwapBuffers] Enabling");
            _swapBuffers.Enable();
        }
    }
}
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace CrosshairY.Managers
{
    public class HotkeyManager : IDisposable
    {
        private IntPtr _hookId = IntPtr.Zero;
        private LowLevelKeyboardProc _proc;

        private HashSet<Key> _keysDown = new();
        private List<Hotkey> _hotkeys = new();

        public HotkeyManager()
        {
            _proc = HookCallback;
            _hookId = SetWindowsHookEx(13, _proc, IntPtr.Zero, 0); // WH_KEYBOARD_LL == 13

            if (_hookId == IntPtr.Zero)
                throw new Exception("Failed to install keyboard hook");
        }

        public void RegisterHotkey(Hotkey hotkey)
        {
            _hotkeys.Add(hotkey);
        }

        public void UnregisterHotkey(Hotkey hotkey)
        {
            _hotkeys.Remove(hotkey);
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            const int WM_KEYDOWN = 0x0100;
            const int WM_KEYUP = 0x0101;
            const int WM_SYSKEYDOWN = 0x0104;
            const int WM_SYSKEYUP = 0x0105;

            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Key key = KeyInterop.KeyFromVirtualKey(vkCode);

                bool isKeyDown = wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN;
                bool isKeyUp = wParam == WM_KEYUP || wParam == WM_SYSKEYUP;

                if (isKeyDown)
                {
                    _keysDown.Add(key);

                    foreach (Hotkey hotkey in _hotkeys)
                    {
                        if (!hotkey.IsTriggered && hotkey.IsPressed(_keysDown))
                        {
                            hotkey.IsTriggered = true;
                            hotkey.Action?.Invoke();
                        }
                    }
                }
                else if (isKeyUp)
                {
                    _keysDown.Remove(key);

                    foreach (Hotkey hotkey in _hotkeys)
                    {
                        if (hotkey.ContainsKey(key))
                        {
                            hotkey.IsTriggered = false;
                        }
                    }
                }
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }
        }

        // WinAPI
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        public class Hotkey
        {
            public Key Key { get; }
            public ModifierKeys Modifiers { get; }

            public Action? Action { get; set; }

            internal bool IsTriggered { get; set; }

            public Hotkey(Key key, ModifierKeys modifiers = ModifierKeys.None)
            {
                Key = key;
                Modifiers = modifiers;
            }

            internal bool IsPressed(HashSet<Key> keysDown)
            {
                // Main key must be pressed
                if (!keysDown.Contains(Key))
                    return false;

                // Check each modifier group — only one side needs to be pressed
                if (Modifiers.HasFlag(ModifierKeys.Control) &&
                    !HasEither(keysDown, Key.LeftCtrl, Key.RightCtrl))
                    return false;

                if (Modifiers.HasFlag(ModifierKeys.Shift) &&
                    !HasEither(keysDown, Key.LeftShift, Key.RightShift))
                    return false;

                if (Modifiers.HasFlag(ModifierKeys.Alt) &&
                    !HasEither(keysDown, Key.LeftAlt, Key.RightAlt))
                    return false;

                if (Modifiers.HasFlag(ModifierKeys.Windows) &&
                    !HasEither(keysDown, Key.LWin, Key.RWin))
                    return false;

                return true;
            }

            private static bool HasEither(HashSet<Key> keysDown, Key left, Key right)
            {
                return keysDown.Contains(left) || keysDown.Contains(right);
            }

            internal bool ContainsKey(Key key)
            {
                if (key == Key) return true;

                if (Modifiers.HasFlag(ModifierKeys.Control) && (key == Key.LeftCtrl || key == Key.RightCtrl)) return true;
                if (Modifiers.HasFlag(ModifierKeys.Shift) && (key == Key.LeftShift || key == Key.RightShift)) return true;
                if (Modifiers.HasFlag(ModifierKeys.Alt) && (key == Key.LeftAlt || key == Key.RightAlt)) return true;
                if (Modifiers.HasFlag(ModifierKeys.Windows) && (key == Key.LWin || key == Key.RWin)) return true;

                return false;
            }
        }
    }
}
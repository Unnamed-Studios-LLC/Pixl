﻿using System;

namespace Pixl.Mac;

public static class MacKeyHelper
{
    public static KeyCode GetKeyCode(ushort macKeyCode)
    {
        var casted = (MacKeyCode)macKeyCode;
        return casted switch
        {
            MacKeyCode.A => KeyCode.A,
            MacKeyCode.B => KeyCode.B,
            MacKeyCode.C => KeyCode.C,
            MacKeyCode.D => KeyCode.D,
            MacKeyCode.E => KeyCode.E,
            MacKeyCode.F => KeyCode.F,
            MacKeyCode.G => KeyCode.G,
            MacKeyCode.H => KeyCode.H,
            MacKeyCode.I => KeyCode.I,
            MacKeyCode.J => KeyCode.J,
            MacKeyCode.K => KeyCode.K,
            MacKeyCode.L => KeyCode.L,
            MacKeyCode.M => KeyCode.M,
            MacKeyCode.N => KeyCode.N,
            MacKeyCode.O => KeyCode.O,
            MacKeyCode.P => KeyCode.P,
            MacKeyCode.Q => KeyCode.Q,
            MacKeyCode.R => KeyCode.R,
            MacKeyCode.S => KeyCode.S,
            MacKeyCode.T => KeyCode.T,
            MacKeyCode.U => KeyCode.U,
            MacKeyCode.V => KeyCode.V,
            MacKeyCode.W => KeyCode.W,
            MacKeyCode.X => KeyCode.X,
            MacKeyCode.Y => KeyCode.Y,
            MacKeyCode.Z => KeyCode.Z,
            MacKeyCode.Alpha1 => KeyCode.Alpha1,
            MacKeyCode.Alpha2 => KeyCode.Alpha2,
            MacKeyCode.Alpha3 => KeyCode.Alpha3,
            MacKeyCode.Alpha4 => KeyCode.Alpha4,
            MacKeyCode.Alpha5 => KeyCode.Alpha5,
            MacKeyCode.Alpha6 => KeyCode.Alpha6,
            MacKeyCode.Alpha7 => KeyCode.Alpha7,
            MacKeyCode.Alpha8 => KeyCode.Alpha8,
            MacKeyCode.Alpha9 => KeyCode.Alpha9,
            MacKeyCode.Alpha0 => KeyCode.Alpha0,
            MacKeyCode.Keypad1 => KeyCode.Keypad1,
            MacKeyCode.Keypad2 => KeyCode.Keypad2,
            MacKeyCode.Keypad3 => KeyCode.Keypad3,
            MacKeyCode.Keypad4 => KeyCode.Keypad4,
            MacKeyCode.Keypad5 => KeyCode.Keypad5,
            MacKeyCode.Keypad6 => KeyCode.Keypad6,
            MacKeyCode.Keypad7 => KeyCode.Keypad7,
            MacKeyCode.Keypad8 => KeyCode.Keypad8,
            MacKeyCode.Keypad9 => KeyCode.Keypad9,
            MacKeyCode.Keypad0 => KeyCode.Keypad0,
            MacKeyCode.Delete => KeyCode.Backspace,
            MacKeyCode.Return => KeyCode.Enter,
            MacKeyCode.Shift => KeyCode.LeftShift,
            MacKeyCode.RightShift => KeyCode.RightShift,
            MacKeyCode.Control => KeyCode.LeftControl,
            MacKeyCode.RightControl => KeyCode.RightControl,
            MacKeyCode.Option => KeyCode.LeftAlt,
            MacKeyCode.RightOption => KeyCode.RightAlt,
            MacKeyCode.LeftArrow => KeyCode.LeftArrow,
            MacKeyCode.RightArrow => KeyCode.RightArrow,
            MacKeyCode.UpArrow => KeyCode.UpArrow,
            MacKeyCode.DownArrow => KeyCode.DownArrow,
            _ => KeyCode.None
        };
    }

    private enum MacKeyCode
    {
        A = 0x00,
        S = 0x01,
        D = 0x02,
        F = 0x03,
        H = 0x04,
        G = 0x05,
        Z = 0x06,
        X = 0x07,
        C = 0x08,
        V = 0x09,
        B = 0x0B,
        Q = 0x0C,
        W = 0x0D,
        E = 0x0E,
        R = 0x0F,
        Y = 0x10,
        T = 0x11,
        Alpha1 = 0x12,
        Alpha2 = 0x13,
        Alpha3 = 0x14,
        Alpha4 = 0x15,
        Alpha6 = 0x16,
        Alpha5 = 0x17,
        Equal = 0x18,
        Alpha9 = 0x19,
        Alpha7 = 0x1A,
        Minus = 0x1B,
        Alpha8 = 0x1C,
        Alpha0 = 0x1D,
        RightBracket = 0x1E,
        O = 0x1F,
        U = 0x20,
        LeftBracket = 0x21,
        I = 0x22,
        P = 0x23,
        L = 0x25,
        J = 0x26,
        Quote = 0x27,
        K = 0x28,
        Semicolon = 0x29,
        Backslash = 0x2A,
        Comma = 0x2B,
        Slash = 0x2C,
        N = 0x2D,
        M = 0x2E,
        Period = 0x2F,
        Grave = 0x32,
        KeypadDecimal = 0x41,
        KeypadMultiply = 0x43,
        KeypadPlus = 0x45,
        KeypadClear = 0x47,
        KeypadDivide = 0x4B,
        KeypadEnter = 0x4C,
        KeypadMinus = 0x4E,
        KeypadEquals = 0x51,
        Keypad0 = 0x52,
        Keypad1 = 0x53,
        Keypad2 = 0x54,
        Keypad3 = 0x55,
        Keypad4 = 0x56,
        Keypad5 = 0x57,
        Keypad6 = 0x58,
        Keypad7 = 0x59,
        Keypad8 = 0x5B,
        Keypad9 = 0x5C,
        Return = 0x24,
        Tab = 0x30,
        Space = 0x31,
        Delete = 0x33,
        Escape = 0x35,
        Command = 0x37,
        Shift = 0x38,
        CapsLock = 0x39,
        Option = 0x3A,
        Control = 0x3B,
        RightShift = 0x3C,
        RightOption = 0x3D,
        RightControl = 0x3E,
        Function = 0x3F,
        F17 = 0x40,
        VolumeUp = 0x48,
        VolumeDown = 0x49,
        Mute = 0x4A,
        F18 = 0x4F,
        F19 = 0x50,
        F20 = 0x5A,
        F5 = 0x60,
        F6 = 0x61,
        F7 = 0x62,
        F3 = 0x63,
        F8 = 0x64,
        F9 = 0x65,
        F11 = 0x67,
        F13 = 0x69,
        F16 = 0x6A,
        F14 = 0x6B,
        F10 = 0x6D,
        F12 = 0x6F,
        F15 = 0x71,
        Help = 0x72,
        Home = 0x73,
        PageUp = 0x74,
        ForwardDelete = 0x75,
        F4 = 0x76,
        End = 0x77,
        F2 = 0x78,
        PageDown = 0x79,
        F1 = 0x7A,
        LeftArrow = 0x7B,
        RightArrow = 0x7C,
        DownArrow = 0x7D,
        UpArrow = 0x7E
    }
}

/*
enum {
  kVK_ANSI_A                    = 0x00,
  kVK_ANSI_S                    = 0x01,
  kVK_ANSI_D                    = 0x02,
  kVK_ANSI_F                    = 0x03,
  kVK_ANSI_H                    = 0x04,
  kVK_ANSI_G                    = 0x05,
  kVK_ANSI_Z                    = 0x06,
  kVK_ANSI_X                    = 0x07,
  kVK_ANSI_C                    = 0x08,
  kVK_ANSI_V                    = 0x09,
  kVK_ANSI_B                    = 0x0B,
  kVK_ANSI_Q                    = 0x0C,
  kVK_ANSI_W                    = 0x0D,
  kVK_ANSI_E                    = 0x0E,
  kVK_ANSI_R                    = 0x0F,
  kVK_ANSI_Y                    = 0x10,
  kVK_ANSI_T                    = 0x11,
  kVK_ANSI_1                    = 0x12,
  kVK_ANSI_2                    = 0x13,
  kVK_ANSI_3                    = 0x14,
  kVK_ANSI_4                    = 0x15,
  kVK_ANSI_6                    = 0x16,
  kVK_ANSI_5                    = 0x17,
  kVK_ANSI_Equal                = 0x18,
  kVK_ANSI_9                    = 0x19,
  kVK_ANSI_7                    = 0x1A,
  kVK_ANSI_Minus                = 0x1B,
  kVK_ANSI_8                    = 0x1C,
  kVK_ANSI_0                    = 0x1D,
  kVK_ANSI_RightBracket         = 0x1E,
  kVK_ANSI_O                    = 0x1F,
  kVK_ANSI_U                    = 0x20,
  kVK_ANSI_LeftBracket          = 0x21,
  kVK_ANSI_I                    = 0x22,
  kVK_ANSI_P                    = 0x23,
  kVK_ANSI_L                    = 0x25,
  kVK_ANSI_J                    = 0x26,
  kVK_ANSI_Quote                = 0x27,
  kVK_ANSI_K                    = 0x28,
  kVK_ANSI_Semicolon            = 0x29,
  kVK_ANSI_Backslash            = 0x2A,
  kVK_ANSI_Comma                = 0x2B,
  kVK_ANSI_Slash                = 0x2C,
  kVK_ANSI_N                    = 0x2D,
  kVK_ANSI_M                    = 0x2E,
  kVK_ANSI_Period               = 0x2F,
  kVK_ANSI_Grave                = 0x32,
  kVK_ANSI_KeypadDecimal        = 0x41,
  kVK_ANSI_KeypadMultiply       = 0x43,
  kVK_ANSI_KeypadPlus           = 0x45,
  kVK_ANSI_KeypadClear          = 0x47,
  kVK_ANSI_KeypadDivide         = 0x4B,
  kVK_ANSI_KeypadEnter          = 0x4C,
  kVK_ANSI_KeypadMinus          = 0x4E,
  kVK_ANSI_KeypadEquals         = 0x51,
  kVK_ANSI_Keypad0              = 0x52,
  kVK_ANSI_Keypad1              = 0x53,
  kVK_ANSI_Keypad2              = 0x54,
  kVK_ANSI_Keypad3              = 0x55,
  kVK_ANSI_Keypad4              = 0x56,
  kVK_ANSI_Keypad5              = 0x57,
  kVK_ANSI_Keypad6              = 0x58,
  kVK_ANSI_Keypad7              = 0x59,
  kVK_ANSI_Keypad8              = 0x5B,
  kVK_ANSI_Keypad9              = 0x5C
};

keycodes for keys that are independent of keyboard layout
enum {
    kVK_Return = 0x24,
    kVK_Tab = 0x30,
    kVK_Space = 0x31,
    kVK_Delete = 0x33,
    kVK_Escape = 0x35,
    kVK_Command = 0x37,
    kVK_Shift = 0x38,
    kVK_CapsLock = 0x39,
    kVK_Option = 0x3A,
    kVK_Control = 0x3B,
    kVK_RightShift = 0x3C,
    kVK_RightOption = 0x3D,
    kVK_RightControl = 0x3E,
    kVK_Function = 0x3F,
    kVK_F17 = 0x40,
    kVK_VolumeUp = 0x48,
    kVK_VolumeDown = 0x49,
    kVK_Mute = 0x4A,
    kVK_F18 = 0x4F,
    kVK_F19 = 0x50,
    kVK_F20 = 0x5A,
    kVK_F5 = 0x60,
    kVK_F6 = 0x61,
    kVK_F7 = 0x62,
    kVK_F3 = 0x63,
    kVK_F8 = 0x64,
    kVK_F9 = 0x65,
    kVK_F11 = 0x67,
    kVK_F13 = 0x69,
    kVK_F16 = 0x6A,
    kVK_F14 = 0x6B,
    kVK_F10 = 0x6D,
    kVK_F12 = 0x6F,
    kVK_F15 = 0x71,
    kVK_Help = 0x72,
    kVK_Home = 0x73,
    kVK_PageUp = 0x74,
    kVK_ForwardDelete = 0x75,
    kVK_F4 = 0x76,
    kVK_End = 0x77,
    kVK_F2 = 0x78,
    kVK_PageDown = 0x79,
    kVK_F1 = 0x7A,
    kVK_LeftArrow = 0x7B,
    kVK_RightArrow = 0x7C,
    kVK_DownArrow = 0x7D,
    kVK_UpArrow = 0x7E
};
*/
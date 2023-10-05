// Decompiled with JetBrains decompiler
// Type: DiscordRpc
// Assembly: BetterDiscordPresence, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D0EBF762-3541-4927-847C-F459AB7AC2BE
// Assembly location: C:\Users\Suden\Downloads\dotpeek dlls\BetterDiscordPresence.dll

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

public class DiscordRpc
{
    [DllImport("discord-rpc", EntryPoint = "Discord_Initialize", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Initialize(
      string applicationId,
      ref DiscordRpc.EventHandlers handlers,
      bool autoRegister,
      string optionalSteamId);

    [DllImport("discord-rpc", EntryPoint = "Discord_Shutdown", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Shutdown();

    [DllImport("discord-rpc", EntryPoint = "Discord_RunCallbacks", CallingConvention = CallingConvention.Cdecl)]
    public static extern void RunCallbacks();

    [DllImport("discord-rpc", EntryPoint = "Discord_UpdatePresence", CallingConvention = CallingConvention.Cdecl)]
    private static extern void UpdatePresenceNative(ref DiscordRpc.RichPresenceStruct presence);

    [DllImport("discord-rpc", EntryPoint = "Discord_ClearPresence", CallingConvention = CallingConvention.Cdecl)]
    public static extern void ClearPresence();

    [DllImport("discord-rpc", EntryPoint = "Discord_Respond", CallingConvention = CallingConvention.Cdecl)]
    public static extern void Respond(string userId, DiscordRpc.Reply reply);

    public static void UpdatePresence(DiscordRpc.RichPresence presence)
    {
        DiscordRpc.RichPresenceStruct presence1 = presence.GetStruct();
        DiscordRpc.UpdatePresenceNative(ref presence1);
        presence.FreeMem();
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ReadyCallback();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DisconnectedCallback(int errorCode, string message);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void ErrorCallback(int errorCode, string message);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void JoinCallback(string secret);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SpectateCallback(string secret);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void RequestCallback(ref DiscordRpc.JoinRequest request);

    public struct EventHandlers
    {
        public DiscordRpc.ReadyCallback readyCallback;
        public DiscordRpc.DisconnectedCallback disconnectedCallback;
        public DiscordRpc.ErrorCallback errorCallback;
        public DiscordRpc.JoinCallback joinCallback;
        public DiscordRpc.SpectateCallback spectateCallback;
        public DiscordRpc.RequestCallback requestCallback;
    }

    [Serializable]
    public struct RichPresenceStruct
    {
        public IntPtr state;
        public IntPtr details;
        public long startTimestamp;
        public long endTimestamp;
        public IntPtr largeImageKey;
        public IntPtr largeImageText;
        public IntPtr smallImageKey;
        public IntPtr smallImageText;
        public IntPtr partyId;
        public int partySize;
        public int partyMax;
        public IntPtr matchSecret;
        public IntPtr joinSecret;
        public IntPtr spectateSecret;
        public bool instance;
    }

    [Serializable]
    public struct JoinRequest
    {
        public string userId;
        public string username;
        public string discriminator;
        public string avatar;
    }

    public enum Reply
    {
        No,
        Yes,
        Ignore,
    }

    public class RichPresence
    {
        private DiscordRpc.RichPresenceStruct _presence;
        private readonly List<IntPtr> _buffers = new List<IntPtr>(10);
        public string state;
        public string details;
        public long startTimestamp;
        public long endTimestamp;
        public string largeImageKey;
        public string largeImageText;
        public string smallImageKey;
        public string smallImageText;
        public string partyId;
        public int partySize;
        public int partyMax;
        public string matchSecret;
        public string joinSecret;
        public string spectateSecret;
        public bool instance;

        internal DiscordRpc.RichPresenceStruct GetStruct()
        {
            if (this._buffers.Count > 0)
                this.FreeMem();
            this._presence.state = this.StrToPtr(this.state, 128);
            this._presence.details = this.StrToPtr(this.details, 128);
            this._presence.startTimestamp = this.startTimestamp;
            this._presence.endTimestamp = this.endTimestamp;
            this._presence.largeImageKey = this.StrToPtr(this.largeImageKey, 32);
            this._presence.largeImageText = this.StrToPtr(this.largeImageText, 128);
            this._presence.smallImageKey = this.StrToPtr(this.smallImageKey, 32);
            this._presence.smallImageText = this.StrToPtr(this.smallImageText, 128);
            this._presence.partyId = this.StrToPtr(this.partyId, 128);
            this._presence.partySize = this.partySize;
            this._presence.partyMax = this.partyMax;
            this._presence.matchSecret = this.StrToPtr(this.matchSecret, 128);
            this._presence.joinSecret = this.StrToPtr(this.joinSecret, 128);
            this._presence.spectateSecret = this.StrToPtr(this.spectateSecret, 128);
            this._presence.instance = this.instance;
            return this._presence;
        }

        private IntPtr StrToPtr(string input, int maxbytes)
        {
            if (string.IsNullOrEmpty(input))
                return IntPtr.Zero;
            string s = DiscordRpc.RichPresence.StrClampBytes(input, maxbytes);
            int byteCount = Encoding.UTF8.GetByteCount(s);
            IntPtr destination = Marshal.AllocHGlobal(byteCount);
            this._buffers.Add(destination);
            Marshal.Copy(Encoding.UTF8.GetBytes(s), 0, destination, byteCount);
            return destination;
        }

        private static string StrToUtf8NullTerm(string toconv)
        {
            string s = toconv.Trim();
            byte[] bytes = Encoding.Default.GetBytes(s);
            if (bytes.Length != 0 && bytes[bytes.Length - 1] != (byte)0)
                s += "\0\0";
            return Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(s));
        }

        private static string StrClampBytes(string toclamp, int maxbytes)
        {
            string utf8NullTerm = DiscordRpc.RichPresence.StrToUtf8NullTerm(toclamp);
            byte[] bytes = Encoding.UTF8.GetBytes(utf8NullTerm);
            if (bytes.Length <= maxbytes)
                return utf8NullTerm;
            byte[] numArray = new byte[0];
            Array.Copy((Array)bytes, 0, (Array)numArray, 0, maxbytes - 1);
            numArray[numArray.Length - 1] = (byte)0;
            numArray[numArray.Length - 2] = (byte)0;
            return Encoding.UTF8.GetString(numArray);
        }

        internal void FreeMem()
        {
            for (int index = this._buffers.Count - 1; index >= 0; --index)
            {
                Marshal.FreeHGlobal(this._buffers[index]);
                this._buffers.RemoveAt(index);
            }
        }
    }
}

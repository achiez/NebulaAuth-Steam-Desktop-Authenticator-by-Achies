using ProtoBuf;
using SteamLib.ProtoCore.Interfaces;

namespace SteamLib.ProtoCore.Services;

[ProtoContract]
public class EmptyMessage : IProtoMsg
{
    public static readonly EmptyMessage Instance = new();
}
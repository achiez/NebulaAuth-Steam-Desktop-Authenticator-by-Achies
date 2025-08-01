﻿using ProtoBuf;
using SteamLib.ProtoCore.Interfaces;
using SteamLibForked.Models.Session;

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
#pragma warning disable CS8618

namespace SteamLib.ProtoCore.Services;

[ProtoContract]
public class AccountPhoneStatus_Response : IProtoMsg
{
    [ProtoMember(1)] public bool HasPhone { get; set; }
}

[ProtoContract]
public class AddAuthenticator_Request : IProtoMsg
{
    [ProtoMember(1, DataFormat = DataFormat.FixedSize)]
    public ulong SteamId { get; set; }

    [ProtoMember(4)] public int AuthenticatorType { get; set; }
    [ProtoMember(5)] public string DeviceIdentifier { get; set; }
    [ProtoMember(6)] public string SmsPhoneId { get; set; }
    [ProtoMember(8)] public int Version { get; set; }
}

[ProtoContract]
public class AddAuthenticator_Response : IProtoMsg
{
    [ProtoMember(1)] public byte[] SharedSecret { get; set; }
    [ProtoMember(2)] public ulong SerialNumber { get; set; }
    [ProtoMember(3)] public string RevocationCode { get; set; }
    [ProtoMember(4)] public string Uri { get; set; }
    [ProtoMember(5)] public long ServerTime { get; set; }
    [ProtoMember(6)] public string AccountName { get; set; }
    [ProtoMember(7)] public string TokenGid { get; set; }
    [ProtoMember(8)] public byte[] IdentitySecret { get; set; }
    [ProtoMember(9)] public byte[] Secret1 { get; set; }
    [ProtoMember(10)] public int Status { get; set; }
    [ProtoMember(11)] public string PhoneNumberHint { get; set; }

    /// <summary>
    ///     2 - PhoneNumber
    ///     3 - EmailCode
    /// </summary>
    [ProtoMember(12)]
    public int ConfirmType { get; set; }


    public MobileDataExtended ToMobileDataExtended(string deviceId, MobileSessionData? sessionData)
    {
        return new MobileDataExtended
        {
            SharedSecret = Convert.ToBase64String(SharedSecret),
            SerialNumber = SerialNumber,
            RevocationCode = RevocationCode,
            Uri = Uri,
            ServerTime = ServerTime,
            AccountName = AccountName,
            TokenGid = TokenGid,
            IdentitySecret = Convert.ToBase64String(IdentitySecret),
            Secret1 = Convert.ToBase64String(Secret1),
            SessionData = sessionData,
            DeviceId = deviceId
        };
    }
}

[ProtoContract]
public class SetAccountPhoneNumber_Request : IProtoMsg
{
    [ProtoMember(1)] public string PhoneNumber { get; set; }
    [ProtoMember(2)] public string CountryCode { get; set; }
}

[ProtoContract]
public class SetAccountPhoneNumber_Response : IProtoMsg
{
    [ProtoMember(1)] public string EmailHint { get; set; }
    [ProtoMember(2)] public string PhoneNumber { get; set; }
}

[ProtoContract]
public class IsAccountWaitingForEmailConfirmation_Response : IProtoMsg
{
    [ProtoMember(1)] public bool IsWaiting { get; set; }
    [ProtoMember(2)] public int SecondsToWait { get; set; }
}

[ProtoContract]
public class SendPhoneVerificationCode_Request : IProtoMsg
{
    [ProtoMember(1, IsRequired = true)] public int Language { get; set; }
}

[ProtoContract]
public class FinalizeAddAuthenticator_Request : IProtoMsg
{
    [ProtoMember(1, DataFormat = DataFormat.FixedSize)]
    public ulong SteamId { get; set; }

    [ProtoMember(2)] public string AuthenticatorCode { get; set; }
    [ProtoMember(3)] public ulong AuthenticatorTime { get; set; }
    [ProtoMember(4)] public string ConfirmationCode { get; set; }
    [ProtoMember(6)] public bool ValidateConfirmationCode { get; set; }
}

[ProtoContract]
public class FinalizeAddAuthenticator_Response : IProtoMsg
{
    [ProtoMember(1)] public bool Success { get; set; }

    [ProtoMember(2)] public bool WantMore { get; set; }
    [ProtoMember(3)] public ulong ServerTime { get; set; }
    [ProtoMember(4)] public int Status { get; set; }
}

[ProtoContract]
public class RemoveAuthenticator_Request : IProtoMsg
{
    [ProtoMember(2)] public string RevocationCode { get; set; }
    [ProtoMember(5)] public int RevocationReason { get; set; }
    [ProtoMember(6)] public int SteamGuardScheme { get; set; }
}

[ProtoContract]
public class RemoveAuthenticator_Response : IProtoMsg
{
    [ProtoMember(1)] public bool Success { get; set; }
    [ProtoMember(5)] public int RevocationAttemptsRemaining { get; set; }
}

[ProtoContract]
public class RemoveAuthenticatorViaChallengeContinue_Request : IProtoMsg
{
    [ProtoMember(1)] public string SmsCode { get; set; }
    [ProtoMember(2)] public bool GenerateNewToken { get; set; }
    [ProtoMember(3)] public uint Version { get; set; }
}

[ProtoContract]
public class RemoveAuthenticatorViaChallengeContinue_Replacement_Token : IProtoMsg
{
    [ProtoMember(1)] public byte[] SharedSecret { get; set; }
    [ProtoMember(2)] public ulong SerialNumber { get; set; }
    [ProtoMember(3)] public string RevocationCode { get; set; }
    [ProtoMember(4)] public string Uri { get; set; }
    [ProtoMember(5)] public long ServerTime { get; set; }
    [ProtoMember(6)] public string AccountName { get; set; }
    [ProtoMember(7)] public string TokenGid { get; set; }
    [ProtoMember(8)] public byte[] IdentitySecret { get; set; }
    [ProtoMember(9)] public byte[] Secret1 { get; set; }
    [ProtoMember(10)] public int Status { get; set; }
    [ProtoMember(11)] public uint SteamGuardScheme { get; set; }
    [ProtoMember(12)] public ulong SteamId { get; set; }

    public MobileDataExtended ToMobileDataExtended(string deviceId, MobileSessionData? sessionData)
    {
        return new MobileDataExtended
        {
            SharedSecret = Convert.ToBase64String(SharedSecret),
            SerialNumber = SerialNumber,
            RevocationCode = RevocationCode,
            Uri = Uri,
            ServerTime = ServerTime,
            AccountName = AccountName,
            TokenGid = TokenGid,
            IdentitySecret = Convert.ToBase64String(IdentitySecret),
            Secret1 = Convert.ToBase64String(Secret1),
            SessionData = sessionData,
            DeviceId = deviceId,
            SteamId = global::SteamId.FromSteam64(SteamId)
        };
    }
}

[ProtoContract]
public class RemoveAuthenticatorViaChallengeContinue_Response : IProtoMsg
{
    [ProtoMember(1)] public int Success { get; set; }
    [ProtoMember(2)] public RemoveAuthenticatorViaChallengeContinue_Replacement_Token ReplacementToken { get; set; }
}
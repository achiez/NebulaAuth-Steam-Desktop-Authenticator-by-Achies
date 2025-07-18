﻿using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;
using SteamLib.Authentication;
using SteamLibForked.Models.Session;

namespace SteamLib.Utility.MafileSerialization;

public partial class MafileSerializer //Validate
{
    private static class Validate
    {
        public static void NotNull(string name, [NotNull] object? o)
        {
            if (o == null)
            {
                throw new ArgumentNullException(name, $"{name} is null");
            }
        }

        public static void NotNull(string name, [NotNull] JToken? o)
        {
            if (o == null || o.Type == JTokenType.Null)
            {
                throw new ArgumentNullException(name, $"{name} is null");
            }
        }


        public static void NotNullOrEmpty(string name, [NotNull] string? s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentException($"{name} is null or empty");
            }
        }


        public static void IsValidBase64(string name, string base64)
        {
            var buffer = new Span<byte>(new byte[base64.Length]);
            if (Convert.TryFromBase64String(base64, buffer, out _) == false)
                throw new ArgumentException($"{name} is not valid base64 string");
        }

        public static MobileSessionData? ValidateMobileData(MobileData data,
            out DeserializedMafileSessionResult sessionResult)
        {
            NotNullOrEmpty(nameof(data.SharedSecret), data.SharedSecret);
            NotNullOrEmpty(nameof(data.IdentitySecret), data.IdentitySecret);
            NotNullOrEmpty(nameof(data.DeviceId), data.DeviceId);
            IsValidBase64(nameof(data.SharedSecret), data.SharedSecret);
            IsValidBase64(nameof(data.IdentitySecret), data.IdentitySecret);

            sessionResult = DeserializedMafileSessionResult.Missing;
            if (data is MobileDataExtended d)
            {
                NotNullOrEmpty(nameof(MobileDataExtended.AccountName), d.AccountName);
                NotNullOrEmpty(nameof(MobileDataExtended.RevocationCode), d.RevocationCode);
                if (d.SessionData == null) return null;

                sessionResult = DeserializedMafileSessionResult.Invalid;
                d.SessionData.IsValid = SessionDataValidator.Validate(null, d.SessionData).Succeeded;
                if (d.SessionData.IsValid == false) return null;

                sessionResult = DeserializedMafileSessionResult.Valid;
                return d.SessionData;
            }

            return null;
        }
    }
}
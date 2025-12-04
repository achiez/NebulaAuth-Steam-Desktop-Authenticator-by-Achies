using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using NebulaAuth.Model.Entities;
using NLog;

namespace NebulaAuth.Model.Services;

public class EmailCodeParser
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static readonly Regex SteamCodeRegex = new(@"\b([A-Z0-9]{5})\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static async Task<string?> GetSteamCodeFromEmailAsync(EmailAccount emailAccount, int timeoutSeconds = 60, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(emailAccount.Email) || string.IsNullOrWhiteSpace(emailAccount.Password))
        {
            Logger.Warn("Email account credentials are missing");
            return null;
        }

        try
        {
            using var client = new ImapClient();
            await client.ConnectAsync(emailAccount.ImapServer, emailAccount.ImapPort, emailAccount.UseSsl, cancellationToken);
            await client.AuthenticateAsync(emailAccount.Email, emailAccount.Password, cancellationToken);

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken);

            var startTime = DateTime.UtcNow;
            var timeout = TimeSpan.FromSeconds(timeoutSeconds);

            while (DateTime.UtcNow - startTime < timeout)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Ищем письма от Steam за последние 5 минут
                var searchQuery = SearchQuery.And(
                    SearchQuery.FromContains("noreply@steampowered.com"),
                    SearchQuery.DeliveredAfter(DateTime.UtcNow.AddMinutes(-5))
                );

                var uids = await inbox.SearchAsync(searchQuery, cancellationToken);

                foreach (var uid in uids.OrderByDescending(x => x))
                {
                    var message = await inbox.GetMessageAsync(uid, cancellationToken);
                    var code = ExtractSteamCode(message);

                    if (!string.IsNullOrEmpty(code))
                    {
                        Logger.Info($"Found Steam code {code} in email {emailAccount.Email}");
                        await client.DisconnectAsync(true, cancellationToken);
                        return code;
                    }
                }

                // Ждем 2 секунды перед следующей проверкой
                await Task.Delay(2000, cancellationToken);
            }

            await client.DisconnectAsync(true, cancellationToken);
            Logger.Warn($"Timeout waiting for Steam code in email {emailAccount.Email}");
            return null;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"Error retrieving Steam code from email {emailAccount.Email}");
            return null;
        }
    }

    private static string? ExtractSteamCode(MimeMessage message)
    {
        // Проверяем тему письма
        if (!string.IsNullOrEmpty(message.Subject))
        {
            var subjectMatch = SteamCodeRegex.Match(message.Subject);
            if (subjectMatch.Success)
            {
                return subjectMatch.Groups[1].Value.ToUpper();
            }
        }

        // Проверяем тело письма (текстовая и HTML версии)
        var textBody = message.TextBody;
        if (!string.IsNullOrEmpty(textBody))
        {
            var bodyMatch = SteamCodeRegex.Match(textBody);
            if (bodyMatch.Success)
            {
                return bodyMatch.Groups[1].Value.ToUpper();
            }
        }

        var htmlBody = message.HtmlBody;
        if (!string.IsNullOrEmpty(htmlBody))
        {
            // Удаляем HTML теги для поиска кода
            var plainText = Regex.Replace(htmlBody, "<[^>]+>", " ");
            var htmlMatch = SteamCodeRegex.Match(plainText);
            if (htmlMatch.Success)
            {
                return htmlMatch.Groups[1].Value.ToUpper();
            }
        }

        return null;
    }
}


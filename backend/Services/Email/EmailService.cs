using System.Text;
using System.Text.Json;
using InternshipPortal.API.Services.Interfaces;

namespace InternshipPortal.API.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        private readonly HttpClient _httpClient;

        public EmailService(
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _configuration = configuration;

            _httpClient = httpClient;
        }

        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string body)
        {
            var apiKey =
                _configuration["BrevoSettings:ApiKey"];

            var fromEmail =
                _configuration["BrevoSettings:FromEmail"];

            var fromName =
                _configuration["BrevoSettings:FromName"];

            var htmlBody = GetHtmlTemplate(subject, body);

            var emailData = new
            {
                sender = new { name = fromName, email = fromEmail },
                to = new[] { new { email = toEmail } },
                subject = subject,
                htmlContent = htmlBody
            };

            var json =
                JsonSerializer.Serialize(emailData);

            var content =
                new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json"
                );

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
            {
                Content = content
            };

            request.Headers.Add("api-key", apiKey);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content
                        .ReadAsStringAsync();

                throw new Exception(
                    $"Brevo Error: {error}"
                );
            }
        }

        private string GetHtmlTemplate(string title, string bodyText)
        {
            // Auto format standard labels
            var labels = new[] { "NEW STATUS:", "ADMIN REMARKS:", "Internship:", "Company:", "Deadline:", "Material:", "Description:", "Certificate:" };
            foreach (var label in labels)
            {
                bodyText = bodyText.Replace(label, $"<strong>{label}</strong>");
            }

            // Highlight OTP numbers
            var otpMatch = System.Text.RegularExpressions.Regex.Match(bodyText, @"\b\d{6}\b");
            if (otpMatch.Success && (bodyText.Contains("OTP") || bodyText.Contains("code")))
            {
                string otpCode = otpMatch.Value;
                bodyText = bodyText.Replace(otpCode, $"<div style=\"text-align: center; margin: 20px 0;\"><span class=\"badge-otp\">{otpCode}</span></div>");
            }

            // Format Status Badges
            bodyText = bodyText.Replace("<strong>NEW STATUS:</strong>\r\nAccepted", "<strong>NEW STATUS:</strong> <span class=\"badge badge-accepted\">Accepted</span>");
            bodyText = bodyText.Replace("<strong>NEW STATUS:</strong>\nAccepted", "<strong>NEW STATUS:</strong> <span class=\"badge badge-accepted\">Accepted</span>");
            bodyText = bodyText.Replace("<strong>NEW STATUS:</strong> Accepted", "<strong>NEW STATUS:</strong> <span class=\"badge badge-accepted\">Accepted</span>");

            bodyText = bodyText.Replace("<strong>NEW STATUS:</strong>\r\nCompleted", "<strong>NEW STATUS:</strong> <span class=\"badge badge-completed\">Completed</span>");
            bodyText = bodyText.Replace("<strong>NEW STATUS:</strong>\nCompleted", "<strong>NEW STATUS:</strong> <span class=\"badge badge-completed\">Completed</span>");
            bodyText = bodyText.Replace("<strong>NEW STATUS:</strong> Completed", "<strong>NEW STATUS:</strong> <span class=\"badge badge-completed\">Completed</span>");

            bodyText = bodyText.Replace("<strong>NEW STATUS:</strong>\r\nRejected", "<strong>NEW STATUS:</strong> <span class=\"badge badge-rejected\">Rejected</span>");
            bodyText = bodyText.Replace("<strong>NEW STATUS:</strong>\nRejected", "<strong>NEW STATUS:</strong> <span class=\"badge badge-rejected\">Rejected</span>");
            bodyText = bodyText.Replace("<strong>NEW STATUS:</strong> Rejected", "<strong>NEW STATUS:</strong> <span class=\"badge badge-rejected\">Rejected</span>");

            bodyText = bodyText.Replace("<strong>NEW STATUS:</strong>\r\nInProgress", "<strong>NEW STATUS:</strong> <span class=\"badge badge-in-progress\">In-Progress</span>");
            bodyText = bodyText.Replace("<strong>NEW STATUS:</strong>\nInProgress", "<strong>NEW STATUS:</strong> <span class=\"badge badge-in-progress\">In-Progress</span>");
            bodyText = bodyText.Replace("<strong>NEW STATUS:</strong> InProgress", "<strong>NEW STATUS:</strong> <span class=\"badge badge-in-progress\">In-Progress</span>");

            // Format line breaks and paragraphs
            string formattedBody = bodyText;
            if (!bodyText.Contains("<p>") && !bodyText.Contains("</div>") && !bodyText.Contains("html"))
            {
                var paragraphs = bodyText.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => $"<p style=\"margin: 0 0 16px; font-size: 16px; line-height: 1.6; color: #374151;\">{p.Replace("\n", "<br/>")}</p>");
                formattedBody = string.Join("\n", paragraphs);
            }

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{title}</title>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            background-color: #f3f4f6;
            margin: 0;
            padding: 0;
            -webkit-font-smoothing: antialiased;
        }}
        .wrapper {{
            width: 100%;
            background-color: #f3f4f6;
            padding: 40px 20px;
            box-sizing: border-box;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border-radius: 12px;
            overflow: hidden;
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
        }}
        .header {{
            background: linear-gradient(135deg, #4f46e5 0%, #3b82f6 100%);
            padding: 32px 24px;
            text-align: center;
        }}
        .header h1 {{
            color: #ffffff;
            margin: 0;
            font-size: 24px;
            font-weight: 700;
            letter-spacing: -0.025em;
        }}
        .content {{
            padding: 32px 24px;
        }}
        .footer {{
            background-color: #f9fafb;
            padding: 24px;
            text-align: center;
            border-top: 1px solid #f3f4f6;
        }}
        .footer p {{
            margin: 0;
            font-size: 13px;
            color: #9ca3af;
        }}
        .badge {{
            display: inline-block;
            padding: 4px 12px;
            font-size: 14px;
            font-weight: 600;
            border-radius: 9999px;
            margin-left: 2px;
        }}
        .badge-completed {{
            background-color: #dbeafe;
            color: #1e40af;
        }}
        .badge-accepted {{
            background-color: #dcfce7;
            color: #166534;
        }}
        .badge-in-progress {{
            background-color: #fef9c3;
            color: #854d0e;
        }}
        .badge-rejected {{
            background-color: #fee2e2;
            color: #991b1b;
        }}
        .badge-otp {{
            font-size: 28px;
            font-family: monospace;
            background-color: #f3f4f6;
            color: #1f2937;
            letter-spacing: 4px;
            padding: 8px 24px;
            border-radius: 8px;
            font-weight: bold;
            display: inline-block;
        }}
    </style>
</head>
<body>
    <div class=""wrapper"">
        <div class=""container"">
            <div class=""header"">
                <h1>{title}</h1>
            </div>
            <div class=""content"">
                {formattedBody}
            </div>
            <div class=""footer"">
                <p>&copy; {DateTime.UtcNow.Year} Internship Portal. All rights reserved.</p>
                <p style=""margin-top: 4px;"">This is an automated notification, please do not reply directly to this email.</p>
            </div>
        </div>
    </div>
</body>
</html>";
        }
    }
}
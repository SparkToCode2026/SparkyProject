namespace SparkyProject.API.Services;

// Self-study piece: basic email-sending service (MailKit / SMTP).
// Domain trigger: booking-confirmation email with booking summary, and an invoice email after checkout.
public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}

public class EmailService : IEmailService
{
    // TODO: read SMTP settings from appsettings.json (EmailSettings section) and implement with MailKit.
    public Task SendEmailAsync(string toEmail, string subject, string body)
    {
        throw new NotImplementedException();
    }
}

using System.Reflection;

using Amazon;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

namespace StyleWerk.NBB.AWS;

public class SimpleEmailService
{
    public static bool SendMail(string sender, string receiver, string subject, string htmlBody)
    {
        try
        {
            using AmazonSimpleEmailServiceClient client = new(RegionEndpoint.EUNorth1);

            SendEmailRequest sendRequest = new()
            {
                Source = sender,
                Destination = new Destination
                {
                    ToAddresses =
                    [receiver]
                },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = new Body
                    {
                        Html = new Content
                        {
                            Charset = "UTF-8",
                            Data = htmlBody
                        }
                    }
                },
            };

            Console.WriteLine("Sending email using Amazon SES...");

            SendEmailResponse response = client.SendEmailAsync(sendRequest).Result;
            Console.WriteLine("The email was sent successfully.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("The email was not sent.");
            Console.WriteLine("Error message: " + ex.Message);
            return false;
        }
    }

    public static string AccessEmailTemplate(string fileName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using Stream stream = assembly.GetManifestResourceStream($"StyleWerk.NBB.AWS.EmailTemplates.{fileName}") ?? throw new Exception();
        using StreamReader reader = new(stream);
        string fileContent = reader.ReadToEnd();
        return fileContent;
    }
}

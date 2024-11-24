using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class SpamFilter
{
    class EmailAnalysis
    {
        public int WordRepetition { get; set; }
        public int Length { get; set; }
        public int RecipientCount { get; set; }
        public int SubjectLength { get; set; }
        public bool HasHtmlContent { get; set; }
        public int LinkCount { get; set; }
        public int SpecialCharCount { get; set; }
        public int FormattingCount { get; set; }
        public bool HasAttachment { get; set; }
    }

    static void Main(string[] args)
    {
        // Előre definiált levelek elemzése
        var spamExamples = new List<EmailAnalysis>();
        var normalExamples = new List<EmailAnalysis>();

        for (int i = 1; i <= 30; i++)
        {
            string fileName = $"{i}{(i % 2 == 0 ? "n.eml" : "s.eml")}";
            if (File.Exists(fileName))
            {
                var analysis = AnalyzeEmail(fileName);
                if (fileName.Contains("s.eml"))
                    spamExamples.Add(analysis);
                else
                    normalExamples.Add(analysis);
            }
        }

        // Átlagértékek kiszámítása
        var spamAverages = CalculateAverages(spamExamples);
        var normalAverages = CalculateAverages(normalExamples);/*
        foreach (var item in spamAverages)
        {
            Console.WriteLine($"Kulcs: {item.Key}, Érték: {item.Value}");
        }
        Console.WriteLine();
        foreach (var item in normalAverages)
        {
            Console.WriteLine($"Kulcs: {item.Key}, Érték: {item.Value}");
        }*/
        // Új levél elemzése
        Console.WriteLine("Add meg a kielemezendő levél útvonalát:");
        string emailPath = Console.ReadLine();

        if (File.Exists(emailPath))
        {
            var emailAnalysis = AnalyzeEmail(emailPath);
            var isSpam = DetermineEmailType(emailAnalysis, spamAverages, normalAverages);/*
            Console.WriteLine(emailAnalysis.WordRepetition);
            Console.WriteLine(emailAnalysis.Length);
            Console.WriteLine(emailAnalysis.RecipientCount);
            Console.WriteLine(emailAnalysis.SubjectLength);
            Console.WriteLine(emailAnalysis.HasHtmlContent);
            Console.WriteLine(emailAnalysis.LinkCount);
            Console.WriteLine(emailAnalysis.SpecialCharCount);
            Console.WriteLine(emailAnalysis.FormattingCount);
            Console.WriteLine(emailAnalysis.HasAttachment);*/
            Console.WriteLine(isSpam
                ? "A levél SPAM."
                : "A levél NORMÁLIS.");
        }
        else
        {
            Console.WriteLine("A megadott fájl nem található.");
        }
        Console.ReadLine();
    }

    static EmailAnalysis AnalyzeEmail(string filePath)
    {
        var email = File.ReadAllText(filePath);

        return new EmailAnalysis
        {
            WordRepetition = CountWordRepetitions(email),
            Length = email.Length,
            RecipientCount = CountRecipients(email),
            SubjectLength = ExtractSubject(email).Length,
            HasHtmlContent = email.Contains("<html>") || email.Contains("</html>"),
            LinkCount = CountLinks(email),
            SpecialCharCount = CountSpecialCharacters(email),
            FormattingCount = CountFormattingTags(email),
            HasAttachment = email.Contains("Attachment:")
        };
    }

    static int CountWordRepetitions(string text)
    {
        var words = text.Split(new[] { ' ', '\n', '\r', '\t', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        var wordGroups = words.GroupBy(w => w);
        return wordGroups.Count(g => g.Count() > 1);
    }

    static int CountRecipients(string email) =>
        email.Split(new[] { "To:", "Cc:" }, StringSplitOptions.None).Length - 1;

    static string ExtractSubject(string email)
    {
        var lines = email.Split('\n');
        var subjectLine = lines.FirstOrDefault(line => line.StartsWith("Subject:"));
        return subjectLine?.Substring(8).Trim() ?? string.Empty;
    }

    static int CountLinks(string email) =>
        email.Split(new[] { "http://", "https://" }, StringSplitOptions.None).Length - 1;

    static int CountSpecialCharacters(string email) =>
        email.Count(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));

    static int CountFormattingTags(string email) =>
        email.Split(new[] { "<b>", "</b>", "<i>", "</i>", "<u>", "</u>" }, StringSplitOptions.None).Length - 1;

    static Dictionary<string, double> CalculateAverages(List<EmailAnalysis> emails)
    {
        return new Dictionary<string, double>
        {
            { "WordRepetition", emails.Average(e => e.WordRepetition) },
            { "Length", emails.Average(e => e.Length) },
            { "RecipientCount", emails.Average(e => e.RecipientCount) },
            { "SubjectLength", emails.Average(e => e.SubjectLength) },
            { "HasHtmlContent", emails.Average(e => e.HasHtmlContent ? 1 : 0) },
            { "LinkCount", emails.Average(e => e.LinkCount) },
            { "SpecialCharCount", emails.Average(e => e.SpecialCharCount) },
            { "FormattingCount", emails.Average(e => e.FormattingCount) },
            { "HasAttachment", emails.Average(e => e.HasAttachment ? 1 : 0) },
        };
    }

    static bool DetermineEmailType(EmailAnalysis email, Dictionary<string, double> spamAverages, Dictionary<string, double> normalAverages)
    {
        double spamScore = 0;
        double normalScore = 0;

        spamScore += Math.Abs(email.WordRepetition - spamAverages["WordRepetition"]);
        spamScore += Math.Abs(email.Length - spamAverages["Length"]);
        spamScore += Math.Abs(email.RecipientCount - spamAverages["RecipientCount"]);
        spamScore += Math.Abs(email.SubjectLength - spamAverages["SubjectLength"]);
        spamScore += Math.Abs((email.HasHtmlContent ? 1 : 0) - spamAverages["HasHtmlContent"]);
        spamScore += Math.Abs(email.LinkCount - spamAverages["LinkCount"]);
        spamScore += Math.Abs(email.SpecialCharCount - spamAverages["SpecialCharCount"]);
        spamScore += Math.Abs(email.FormattingCount - spamAverages["FormattingCount"]);
        spamScore += Math.Abs((email.HasAttachment ? 1 : 0) - spamAverages["HasAttachment"]);

        normalScore += Math.Abs(email.WordRepetition - normalAverages["WordRepetition"]);
        normalScore += Math.Abs(email.Length - normalAverages["Length"]);
        normalScore += Math.Abs(email.RecipientCount - normalAverages["RecipientCount"]);
        normalScore += Math.Abs(email.SubjectLength - normalAverages["SubjectLength"]);
        normalScore += Math.Abs((email.HasHtmlContent ? 1 : 0) - normalAverages["HasHtmlContent"]);
        normalScore += Math.Abs(email.LinkCount - normalAverages["LinkCount"]);
        normalScore += Math.Abs(email.SpecialCharCount - normalAverages["SpecialCharCount"]);
        normalScore += Math.Abs(email.FormattingCount - normalAverages["FormattingCount"]);
        normalScore += Math.Abs((email.HasAttachment ? 1 : 0) - normalAverages["HasAttachment"]);
        Console.WriteLine(spamScore);
        Console.WriteLine(normalScore);
        if (spamScore > normalScore)
        {
            return true;
        }
        else
        {
            return false;
        }

    }
}

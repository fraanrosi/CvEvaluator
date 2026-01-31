namespace CvEvaluator.Application.Prompts;

public static class CvEvaluationPrompt
{
    private static readonly string Template =
@"You are an AI assistant evaluating a candidate CV.

TARGET ROLE:
Backend Software Engineer (.NET)

REQUIREMENTS:
- Strong experience with C# and .NET
- Minimum 2 years professional experience
- Basic database knowledge

INSTRUCTIONS:
- Analyze the CV content
- The CV text may have lost visual formatting.
- Infer structure from headings, line breaks, and keywords.
- Produce a STRICT JSON response
- Do NOT include explanations outside JSON
- Do NOT include markdown
- Do NOT add comments

OUTPUT FORMAT:
{
  ""score"": 0-100,
  ""yearsExperience"": number,
  ""matchesRequirements"": boolean,
  ""strengths"": [""string""],
  ""weaknesses"": [""string""]
}

CV CONTENT:
===== CV START =====
""
{{CV_TEXT}}
""
===== CV END =====
";

public static string Build(string cvText)
    {
        return Template.Replace("{{CV_TEXT}}", cvText);
    }
}

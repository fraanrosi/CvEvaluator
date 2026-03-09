namespace CvEvaluator.Application.Prompts;

public static class CvEvaluationPrompt
{
    private static readonly string Template =
@"You are an AI assistant evaluating a candidate CV.

TARGET ROLE:
{{JOB_TITLE}}

JOB DESCRIPTION:
{{JOB_DESCRIPTION}}

INSTRUCTIONS:
- Analyze the CV content
- The CV text may have lost visual formatting.
- Infer structure from headings, line breaks, and keywords.
- Evaluate the candidate strictly against the provided job description.
- Produce a STRICT JSON response
- Do NOT include explanations outside JSON
- Do NOT include markdown
- Do NOT add comments
- Your response must be only the JSON

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

    public static string Build(
        string cvText,
        string jobTitle,
        string jobDescription)
    {
        return Template
            .Replace("{{JOB_TITLE}}", jobTitle)
            .Replace("{{JOB_DESCRIPTION}}", jobDescription)
            .Replace("{{CV_TEXT}}", cvText);
    }
}
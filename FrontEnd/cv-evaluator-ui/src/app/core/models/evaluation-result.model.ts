export interface EvaluationResult {
  fileName: string;
  decision: string;
  score: number;
  strengths: string[];
  weaknesses: string[];
  error?: string;
}

export interface JobPositionDetail {
  id: string;
  title: string;
  description: string;
  createdAt: string;
  evaluations: EvaluationSummary[];
}

export interface EvaluationSummary {
  id: string;
  candidateName: string;
  fileName: string;
  score: number | null;
  evaluatedAt: string;
}
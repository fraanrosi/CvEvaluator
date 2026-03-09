export interface EvaluationResult {
  id: string;
  originalFilename: string;
  status: 'Processing' | 'Completed' | 'Failed';
  overallScore?: number;
  errorMessage?: string;
  createdAt: string;
  evaluatedAt?: string;
}
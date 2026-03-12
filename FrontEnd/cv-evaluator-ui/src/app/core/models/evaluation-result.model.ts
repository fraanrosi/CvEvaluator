export interface EvaluationResult {
  id: string;
  jobPositionId: string;
  originalFilename: string;
  status: number | string;
  overallScore?: number;
  technicalScore?: number;
  experienceScore?: number;
  educationScore?: number;
  strengths?: string[];
  weaknesses?: string[];
  yearsExperience?: number;
  matchesRequirements?: boolean;
  modelUsed?: string;
  processingTimeMs?: number;
  errorMessage?: string;
  createdAt: string;
  evaluatedAt?: string;
}

export function isCompleted(status: number | string): boolean {
  return status === 1 || status === 'Completed';
}

export function isProcessing(status: number | string): boolean {
  return status === 0 || status === 'Processing';
}

export function isFailed(status: number | string): boolean {
  return status === 2 || status === 'Failed';
}

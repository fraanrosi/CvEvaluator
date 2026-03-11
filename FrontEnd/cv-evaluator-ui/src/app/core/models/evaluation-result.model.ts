export interface EvaluationResult {
  id: string;
  jobPositionId: string;
  originalFilename: string;
  status: 'Processing' | 'Completed' | 'Failed';
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

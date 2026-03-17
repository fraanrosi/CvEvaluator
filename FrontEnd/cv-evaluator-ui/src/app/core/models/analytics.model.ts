export interface AnalyticsOverview {
  totalEvaluations: number;
  averageScore: number;
  totalJobPositions: number;
  evaluationsThisMonth: number;
  scoreTrendPercent: number | null;
  topJobPositionTitle: string | null;
}

export interface ScoreTimeSeries {
  dataPoints: ScoreDataPoint[];
}

export interface ScoreDataPoint {
  date: string;
  averageScore: number;
  count: number;
}

export interface ScoreDistribution {
  buckets: ScoreBucket[];
}

export interface ScoreBucket {
  range: string;
  count: number;
}

export interface TopCandidate {
  evaluationId: string;
  filename: string;
  overallScore: number;
  technicalScore: number | null;
  experienceScore: number | null;
  evaluatedAt: string;
}

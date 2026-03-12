export interface Plan {
  id: string;
  name: string;
  maxEvaluationsPerMonth: number;
  maxJobPositions: number;
}

export interface UserSubscription {
  planId: string;
  planName: string;
  maxEvaluationsPerMonth: number;
  maxJobPositions: number;
  status: string;
  startDate: string;
  evaluationsUsedThisMonth: number;
  jobPositionsCount: number;
}

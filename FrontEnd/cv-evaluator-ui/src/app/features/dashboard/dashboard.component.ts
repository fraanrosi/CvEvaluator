import { Component, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { JobPositionsService } from '../job-positions/job-positions.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {

  totalPositions = signal(0);

  constructor(private jobPositionsService: JobPositionsService) {}

  ngOnInit(): void {
    this.loadStats();
  }

  loadStats() {
    this.jobPositionsService.getAll().subscribe({
      next: (positions) => {
        this.totalPositions.set(positions.length);
      }
    });
  }

}
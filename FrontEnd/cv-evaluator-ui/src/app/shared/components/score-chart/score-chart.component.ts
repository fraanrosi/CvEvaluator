import { Component, input, effect, viewChild, ElementRef } from '@angular/core';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { ScoreDataPoint } from '../../../core/models/analytics.model';
import {
  Chart,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Filler,
  Tooltip,
  LineController
} from 'chart.js';

Chart.register(CategoryScale, LinearScale, PointElement, LineElement, Filler, Tooltip, LineController);

@Component({
  selector: 'app-score-chart',
  standalone: true,
  imports: [BaseChartDirective],
  template: `
    <div class="w-full h-[280px]">
      <canvas baseChart
        [type]="'line'"
        [data]="chartData"
        [options]="chartOptions">
      </canvas>
    </div>
  `
})
export class ScoreChartComponent {
  dataPoints = input<ScoreDataPoint[]>([]);

  chartData: ChartConfiguration<'line'>['data'] = { labels: [], datasets: [] };

  chartOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      x: {
        grid: { color: 'rgba(28, 28, 40, 0.8)' },
        ticks: { color: '#8b8ba0', font: { size: 11 } }
      },
      y: {
        min: 0,
        max: 100,
        grid: { color: 'rgba(28, 28, 40, 0.8)' },
        ticks: { color: '#8b8ba0', font: { size: 11 } }
      }
    },
    plugins: {
      tooltip: {
        backgroundColor: '#16161f',
        titleColor: '#fff',
        bodyColor: '#8b8ba0',
        borderColor: '#2a2a3a',
        borderWidth: 1,
        padding: 10,
        cornerRadius: 8,
      }
    },
    elements: {
      point: { radius: 3, hoverRadius: 6, backgroundColor: '#7C3AED' },
      line: { tension: 0.4 }
    }
  };

  constructor() {
    effect(() => {
      const pts = this.dataPoints();
      this.chartData = {
        labels: pts.map(p => new Date(p.date).toLocaleDateString('en', { month: 'short', day: 'numeric' })),
        datasets: [{
          data: pts.map(p => p.averageScore),
          borderColor: '#7C3AED',
          backgroundColor: 'rgba(124, 58, 237, 0.1)',
          fill: true,
          borderWidth: 2,
        }]
      };
    });
  }
}

import { Component, input, effect } from '@angular/core';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { ScoreBucket } from '../../../core/models/analytics.model';
import {
  Chart,
  CategoryScale,
  LinearScale,
  BarElement,
  Tooltip,
  BarController
} from 'chart.js';

Chart.register(CategoryScale, LinearScale, BarElement, Tooltip, BarController);

@Component({
  selector: 'app-distribution-chart',
  standalone: true,
  imports: [BaseChartDirective],
  template: `
    <div class="w-full h-[200px]">
      <canvas baseChart
        [type]="'bar'"
        [data]="chartData"
        [options]="chartOptions">
      </canvas>
    </div>
  `
})
export class DistributionChartComponent {
  buckets = input<ScoreBucket[]>([]);

  chartData: ChartConfiguration<'bar'>['data'] = { labels: [], datasets: [] };

  chartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      x: {
        grid: { display: false },
        ticks: { color: '#8b8ba0', font: { size: 11 } }
      },
      y: {
        grid: { color: 'rgba(28, 28, 40, 0.8)' },
        ticks: { color: '#8b8ba0', font: { size: 11 }, stepSize: 1 }
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
    }
  };

  constructor() {
    effect(() => {
      const bkts = this.buckets();
      this.chartData = {
        labels: bkts.map(b => b.range),
        datasets: [{
          data: bkts.map(b => b.count),
          backgroundColor: 'rgba(124, 58, 237, 0.6)',
          borderColor: '#7C3AED',
          borderWidth: 1,
          borderRadius: 6,
        }]
      };
    });
  }
}

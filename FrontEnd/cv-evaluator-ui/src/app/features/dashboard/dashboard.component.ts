import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SessionService } from '../../core/services/session.service';

@Component({
  standalone: true,
  selector: 'app-dashboard',
  imports: [CommonModule, RouterModule],
  template: `
  <div class="min-h-screen bg-gray-100 p-8">

    <div class="max-w-6xl mx-auto">

      <!-- Header -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold">Dashboard</h1>
        <p class="text-gray-600 mt-2">
          Bienvenido {{ email }}
        </p>
      </div>

      <!-- Main Actions -->
      <div class="grid md:grid-cols-2 gap-6">

        <!-- Nueva Evaluación -->
        <div class="bg-white rounded-xl shadow p-6 flex flex-col justify-between">
          <div>
            <h2 class="text-xl font-semibold mb-2">
              Nueva Evaluación
            </h2>
            <p class="text-gray-600">
              Subí uno o más CVs en formato PDF para obtener una evaluación automática con IA.
            </p>
          </div>

          <a
            routerLink="/evaluations"
            class="mt-6 inline-block bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 text-center">
            Crear evaluación
          </a>
        </div>

        <!-- Historial -->
        <div class="bg-white rounded-xl shadow p-6 flex flex-col justify-between">
          <div>
            <h2 class="text-xl font-semibold mb-2">
              Historial de Evaluaciones
            </h2>
            <p class="text-gray-600">
              Revisá resultados anteriores, scores y feedback generado.
            </p>
          </div>

          <a
            routerLink="/evaluations"
            class="mt-6 inline-block bg-gray-800 text-white px-4 py-2 rounded-lg hover:bg-gray-900 text-center">
            Ver evaluaciones
          </a>
        </div>

      </div>

      <!-- Placeholder para métricas futuras -->
      <div class="mt-12">
        <h2 class="text-2xl font-semibold mb-4">Próximamente</h2>

        <div class="grid md:grid-cols-3 gap-6">
          <div class="bg-white rounded-xl shadow p-6 text-center">
            <p class="text-gray-400">Total evaluaciones</p>
            <p class="text-2xl font-bold mt-2">—</p>
          </div>

          <div class="bg-white rounded-xl shadow p-6 text-center">
            <p class="text-gray-400">Score promedio</p>
            <p class="text-2xl font-bold mt-2">—</p>
          </div>

          <div class="bg-white rounded-xl shadow p-6 text-center">
            <p class="text-gray-400">Última evaluación</p>
            <p class="text-2xl font-bold mt-2">—</p>
          </div>
        </div>
      </div>

    </div>
  </div>
  `
})
export class DashboardComponent {

  private session = inject(SessionService);

  email = this.session.getEmail();
}
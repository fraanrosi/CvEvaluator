import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { CvEvaluationService } from './cv-evaluation.service';
import { environment } from '../../../environments/environment';

describe('CvEvaluationService', () => {
  let service: CvEvaluationService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(CvEvaluationService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('uploadPdf posts FormData to correct endpoint', () => {
    const file = new File(['pdf'], 'cv.pdf', { type: 'application/pdf' });
    service.uploadPdf(file, 'job-123').subscribe();
    const req = httpMock.expectOne(`${environment.apiBaseUrl}/cvEvaluations/evaluate-pdf`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body instanceof FormData).toBe(true);
    req.flush([]);
  });

  it('uploadPdf FormData contains file and jobPositionId', () => {
    const file = new File(['pdf'], 'cv.pdf', { type: 'application/pdf' });
    service.uploadPdf(file, 'job-456').subscribe();
    const req = httpMock.expectOne(`${environment.apiBaseUrl}/cvEvaluations/evaluate-pdf`);
    const formData = req.request.body as FormData;
    expect(formData.get('files')).toBeTruthy();
    expect(formData.get('jobPositionId')).toBe('job-456');
    req.flush([]);
  });

  it('getById gets from correct endpoint', () => {
    service.getById('eval-123').subscribe();
    const req = httpMock.expectOne(`${environment.apiBaseUrl}/cvEvaluations/eval-123`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });
});

import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { JobPositionsService } from './job-positions.service';
import { environment } from '../../../environments/environment';

describe('JobPositionsService', () => {
  let service: JobPositionsService;
  let httpMock: HttpTestingController;
  const base = `${environment.apiBaseUrl}/jobpositions`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(JobPositionsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('getAll gets from correct endpoint', () => {
    service.getAll().subscribe();
    const req = httpMock.expectOne(base);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('getById gets with id in url', () => {
    service.getById('abc').subscribe();
    const req = httpMock.expectOne(`${base}/abc`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('create posts with correct payload', () => {
    const data = { title: 'Dev', description: 'Desc' };
    service.create(data).subscribe();
    const req = httpMock.expectOne(base);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(data);
    req.flush({});
  });

  it('update puts with id and payload', () => {
    const data = { title: 'Updated', description: 'New' };
    service.update('xyz', data).subscribe();
    const req = httpMock.expectOne(`${base}/xyz`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(data);
    req.flush({});
  });

  it('delete deletes with id', () => {
    service.delete('xyz').subscribe();
    const req = httpMock.expectOne(`${base}/xyz`);
    expect(req.request.method).toBe('DELETE');
    req.flush({});
  });

  it('all methods use api/jobpositions base', () => {
    service.getAll().subscribe();
    const req = httpMock.expectOne(base);
    expect(req.request.url).toContain('/jobpositions');
    req.flush([]);
  });
});

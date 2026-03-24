import { ToastService } from './toast.service';

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    service = new ToastService();
  });

  it('success adds toast with success type', () => {
    service.success('Done');
    expect(service.toasts().length).toBe(1);
    expect(service.toasts()[0].type).toBe('success');
    expect(service.toasts()[0].message).toBe('Done');
  });

  it('error adds toast with error type', () => {
    service.error('Fail');
    expect(service.toasts()[0].type).toBe('error');
  });

  it('warning adds toast with warning type', () => {
    service.warning('Watch out');
    expect(service.toasts()[0].type).toBe('warning');
  });

  it('info adds toast with info type', () => {
    service.info('FYI');
    expect(service.toasts()[0].type).toBe('info');
  });

  it('auto-dismisses after timeout', async () => {
    service.success('Bye');
    expect(service.toasts().length).toBe(1);
    await new Promise((resolve) => setTimeout(resolve, 4100));
    expect(service.toasts().length).toBe(0);
  });

  it('dismiss removes toast by id', () => {
    service.success('A');
    service.success('B');
    const idToRemove = service.toasts()[0].id;
    service.dismiss(idToRemove);
    expect(service.toasts().length).toBe(1);
    expect(service.toasts()[0].message).toBe('B');
  });

  it('multiple toasts increment ids', () => {
    service.success('A');
    service.success('B');
    service.success('C');
    const ids = service.toasts().map((t) => t.id);
    expect(ids[0]).toBeLessThan(ids[1]);
    expect(ids[1]).toBeLessThan(ids[2]);
  });
});

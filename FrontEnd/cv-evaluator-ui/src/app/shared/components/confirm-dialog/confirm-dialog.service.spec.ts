import { ConfirmDialogService } from './confirm-dialog.service';

describe('ConfirmDialogService', () => {
  let service: ConfirmDialogService;

  beforeEach(() => {
    service = new ConfirmDialogService();
  });

  it('confirm sets visible to true', () => {
    service.confirm('Are you sure?');
    expect(service.visible()).toBe(true);
  });

  it('confirm sets request with message', () => {
    service.confirm('Delete this?');
    expect(service.request()?.message).toBe('Delete this?');
  });

  it('confirm uses default button texts', () => {
    service.confirm('Sure?');
    expect(service.request()?.confirmText).toBe('Confirm');
    expect(service.request()?.cancelText).toBe('Cancel');
  });

  it('accept sets visible to false and emits true', () => {
    let result: boolean | undefined;
    service.confirm('Sure?').subscribe((v) => (result = v));
    service.accept();
    expect(service.visible()).toBe(false);
    expect(result).toBe(true);
  });

  it('cancel sets visible to false and emits false', () => {
    let result: boolean | undefined;
    service.confirm('Sure?').subscribe((v) => (result = v));
    service.cancel();
    expect(service.visible()).toBe(false);
    expect(result).toBe(false);
  });

  it('confirm returns observable', () => {
    const obs = service.confirm('Sure?');
    expect(obs.subscribe).toBeDefined();
  });
});

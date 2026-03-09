import { chromium } from 'playwright';

const USER = {
  email: 'pepe.gomez@cvevaluator.com',
  password: 'PepeG0mez!',
};

(async () => {
  const browser = await chromium.launch({ headless: false, slowMo: 600 });
  const page = await browser.newPage();

  await page.goto('http://localhost:4200/login');
  await page.waitForLoadState('networkidle');
  console.log('✓ Navegando a /login — URL actual:', page.url());
  await page.screenshot({ path: 'login-01-initial.png' });

  const emailInput = await page.waitForSelector('input[type="email"]', { timeout: 8000 });
  await emailInput.click();
  await emailInput.fill(USER.email);
  console.log('✓ email:', USER.email);

  const passwordInput = await page.waitForSelector('input[type="password"]');
  await passwordInput.click();
  await passwordInput.fill(USER.password);
  console.log('✓ password:', USER.password);

  await page.screenshot({ path: 'login-02-filled.png' });

  const submitBtn = await page.waitForSelector('button[type="submit"]');
  await submitBtn.click();
  console.log('✓ Click en submit');

  try {
    await page.waitForURL('**/dashboard', { timeout: 10000 });
    console.log('✅ LOGIN EXITOSO — redirigió a /dashboard');
  } catch {
    const errorEl = await page.$('p.text-red-500, .error, [class*="error"]');
    const errorText = errorEl ? await errorEl.textContent() : 'sin mensaje de error visible';
    console.log('❌ NO redirigió a /dashboard. URL actual:', page.url());
    console.log('   Error en pantalla:', errorText);
  }

  await page.screenshot({ path: 'login-03-result.png' });
  console.log('Screenshots: login-01-initial.png, login-02-filled.png, login-03-result.png');

  // Browser queda abierto indefinidamente
  await new Promise(() => {});
})();

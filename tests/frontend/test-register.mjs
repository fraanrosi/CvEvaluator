import { chromium } from 'playwright';

const USER = {
  fullName: 'Pepe Gómez',
  email: 'pepe.gomez@cvevaluator.com',
  password: 'PepeG0mez!',
};

(async () => {
  const browser = await chromium.launch({ headless: false, slowMo: 600 });
  const page = await browser.newPage();

  // Wait for frontend to be ready
  let ready = false;
  for (let i = 0; i < 20; i++) {
    try {
      const res = await page.goto('http://localhost:4200/register', { timeout: 5000 });
      if (res && res.ok()) { ready = true; break; }
    } catch { await new Promise(r => setTimeout(r, 2000)); }
  }

  if (!ready) { console.error('Frontend not reachable'); await browser.close(); process.exit(1); }

  await page.waitForLoadState('networkidle');
  console.log('✓ Navegando a /register — URL actual:', page.url());
  await page.screenshot({ path: 'reg-01-initial.png' });

  // Fill fullName
  const fullNameInput = await page.waitForSelector('input[formcontrolname="fullName"]', { timeout: 8000 });
  await fullNameInput.click();
  await fullNameInput.fill(USER.fullName);
  console.log('✓ fullName:', USER.fullName);

  // Fill email
  const emailInput = await page.waitForSelector('input[formcontrolname="email"]');
  await emailInput.click();
  await emailInput.fill(USER.email);
  console.log('✓ email:', USER.email);

  // Fill password
  const inputs = await page.$$('input[type="password"]');
  await inputs[0].click();
  await inputs[0].fill(USER.password);
  console.log('✓ password:', USER.password);

  // Fill confirmPassword
  await inputs[1].click();
  await inputs[1].fill(USER.password);
  console.log('✓ confirmPassword: (mismo)');

  await page.screenshot({ path: 'reg-02-filled.png' });

  // Submit
  const submitBtn = await page.waitForSelector('button[type="submit"]');
  await submitBtn.click();
  console.log('✓ Click en "Crear cuenta"');

  // Wait for result: redirect to /login OR error message
  try {
    await page.waitForURL('**/login', { timeout: 10000 });
    console.log('✅ REGISTRO EXITOSO — redirigió a /login');
  } catch {
    const errorEl = await page.$('p.text-red-500');
    const errorText = errorEl ? await errorEl.textContent() : 'sin mensaje de error visible';
    console.log('❌ NO redirigió a /login. Error en pantalla:', errorText);
  }

  await page.screenshot({ path: 'reg-03-result.png' });
  console.log('Screenshots guardados: reg-01-initial.png, reg-02-filled.png, reg-03-result.png');

  await page.waitForTimeout(15000);
  await browser.close();
})();

import { test, expect, Page } from '@playwright/test';

const uniqueId = Date.now();
const TEST_USER = {
  email: `pw_jobs_${uniqueId}@test.com`,
  password: 'Test@123456',
  fullName: 'Playwright Jobs User',
};
const JOB_TITLE = `QA Engineer ${uniqueId}`;

async function registerIfNeeded(page: Page) {
  await page.request.post('http://localhost:8080/api/auth/register', {
    data: {
      email: TEST_USER.email,
      password: TEST_USER.password,
      fullName: TEST_USER.fullName,
    },
  });
}

async function login(page: Page) {
  await page.goto('/login');
  await page.locator('input[formcontrolname="email"]').fill(TEST_USER.email);
  await page.locator('input[formcontrolname="password"]').fill(TEST_USER.password);
  await page.locator('button[type="submit"]').click();
  await expect(page).toHaveURL(/\/dashboard/, { timeout: 10_000 });
}

test.describe('Job Positions flows', () => {
  test.beforeAll(async ({ browser }) => {
    const page = await browser.newPage();
    await registerIfNeeded(page);
    await page.close();
  });

  test('create a job position', async ({ page }) => {
    await login(page);

    await page.locator('nav a[href="/job-positions/create"]').click();
    await expect(page).toHaveURL(/\/job-positions\/create/);

    await page.locator('input[formcontrolname="title"]').fill(JOB_TITLE);
    await page.locator('textarea[formcontrolname="description"]').fill(
      'Looking for a QA engineer with 3+ years in test automation, Playwright, and CI/CD.'
    );

    await page.locator('button[type="submit"]').click();

    await expect(page).toHaveURL(/\/job-positions$/, { timeout: 10_000 });
    await expect(page.locator('text=Job position created')).toBeVisible({ timeout: 5_000 });
    await expect(page.locator(`text=${JOB_TITLE}`).first()).toBeVisible();
  });

  test('view job position detail', async ({ page }) => {
    await login(page);

    await page.locator('nav a:text("Job Positions")').click();
    await expect(page).toHaveURL(/\/job-positions$/);

    await page.locator(`text=${JOB_TITLE}`).first().click();

    await expect(page.locator('h2').first()).toBeVisible({ timeout: 5_000 });
  });

  test('navigate through navbar links', async ({ page }) => {
    await login(page);

    await page.locator('nav a:text("Job Positions")').click();
    await expect(page).toHaveURL(/\/job-positions$/);

    await page.locator('nav a:text("My Plan")').click();
    await expect(page).toHaveURL(/\/subscription$/);

    await page.locator('nav a:text("CvEvaluator")').click();
    await expect(page).toHaveURL(/\/dashboard$/);
  });

  test('logout redirects to login', async ({ page }) => {
    await login(page);

    await page.locator('nav button:text("Logout")').click();
    await expect(page).toHaveURL(/\/login/, { timeout: 5_000 });
  });
});

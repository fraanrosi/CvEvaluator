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

    await page.locator('a[href="/job-positions/create"]').first().click();
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

    await page.locator('a:text("Job Positions")').first().click();
    await expect(page).toHaveURL(/\/job-positions$/);

    await page.locator(`text=${JOB_TITLE}`).first().click();

    await expect(page.locator('h2').first()).toBeVisible({ timeout: 5_000 });
    await expect(page.locator('text=Evaluations')).toBeVisible();
    await expect(page.locator('text=Upload PDF')).toBeVisible();
  });

  test('empty state shows in job positions list for new user', async ({ page }) => {
    const freshUser = {
      email: `pw_empty_${Date.now()}@test.com`,
      password: 'Test@123456',
      fullName: 'Empty User',
    };

    const regResponse = await page.request.post('http://localhost:8080/api/auth/register', {
      data: freshUser,
    });
    expect(regResponse.ok(), `Registration failed: ${regResponse.status()}`).toBeTruthy();

    await page.goto('/login');
    await page.locator('input[formcontrolname="email"]').fill(freshUser.email);
    await page.locator('input[formcontrolname="password"]').fill(freshUser.password);
    await page.locator('button[type="submit"]').click();
    await expect(page).toHaveURL(/\/dashboard/, { timeout: 10_000 });

    await page.locator('a:text("Job Positions")').first().click();
    await expect(page.locator('text=No job positions yet')).toBeVisible({ timeout: 5_000 });
  });

  test('navigate through navbar links', async ({ page }) => {
    await login(page);

    await page.locator('a:text("Job Positions")').first().click();
    await expect(page).toHaveURL(/\/job-positions$/);

    await page.locator('a:text("My Plan")').first().click();
    await expect(page).toHaveURL(/\/subscription$/);

    await page.locator('a:text("Dashboard")').first().click();
    await expect(page).toHaveURL(/\/dashboard$/);
  });

  test('logout redirects to login', async ({ page }) => {
    await login(page);

    await page.locator('button:text("Logout")').first().click();
    await expect(page).toHaveURL(/\/login/, { timeout: 5_000 });
  });

  test('dashboard shows stats and action buttons', async ({ page }) => {
    await login(page);

    await expect(page.locator('h1:text("Dashboard")')).toBeVisible();
    await expect(page.locator('h3:text("Job Positions")')).toBeVisible();
    await expect(page.locator('a:text("+ Create Job Position")')).toBeVisible();
    await expect(page.locator('a:text("View Job Positions")')).toBeVisible();
  });
});

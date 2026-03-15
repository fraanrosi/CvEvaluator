import { test, expect } from '@playwright/test';

const uniqueId = Date.now();
const TEST_USER = {
  fullName: `Test User ${uniqueId}`,
  email: `testuser_${uniqueId}@test.com`,
  password: 'Test@123456',
};

test.describe('Auth flows', () => {
  test('register a new user', async ({ page }) => {
    await page.goto('/register');

    await expect(page.locator('h2')).toHaveText('Register');

    await page.locator('input[formcontrolname="fullName"]').fill(TEST_USER.fullName);
    await page.locator('input[formcontrolname="email"]').fill(TEST_USER.email);
    await page.locator('input[formcontrolname="password"]').fill(TEST_USER.password);
    await page.locator('input[formcontrolname="confirmPassword"]').fill(TEST_USER.password);

    await page.locator('button[type="submit"]').click();

    // Should redirect to login and show success toast
    await expect(page).toHaveURL(/\/login/, { timeout: 10_000 });
    await expect(page.locator('text=Account created')).toBeVisible({ timeout: 5_000 });
  });

  test('login with the registered user', async ({ page }) => {
    await page.goto('/login');

    await expect(page.locator('h2')).toHaveText('Login');

    await page.locator('input[formcontrolname="email"]').fill(TEST_USER.email);
    await page.locator('input[formcontrolname="password"]').fill(TEST_USER.password);

    await page.locator('button[type="submit"]').click();

    // Should redirect to dashboard and show welcome toast
    await expect(page).toHaveURL(/\/dashboard/, { timeout: 10_000 });
    await expect(page.locator('text=Welcome back')).toBeVisible({ timeout: 5_000 });
  });

  test('login with invalid credentials shows error', async ({ page }) => {
    await page.goto('/login');

    await page.locator('input[formcontrolname="email"]').fill('nonexistent@test.com');
    await page.locator('input[formcontrolname="password"]').fill('wrongpassword');

    await page.locator('button[type="submit"]').click();

    await expect(page.locator('text=Invalid credentials')).toBeVisible({ timeout: 5_000 });
  });

  test('unauthenticated user is redirected to login', async ({ page }) => {
    await page.goto('/dashboard');

    await expect(page).toHaveURL(/\/login/, { timeout: 5_000 });
  });
});

import { test, expect } from '@playwright/test';

const uniqueId = Date.now();
const TEST_USER = {
  fullName: `Test User ${uniqueId}`,
  email: `testuser_${uniqueId}@test.com`,
  password: 'Test@123456',
};

test.describe('Auth flows', () => {
  test('login page renders with dark theme and brand', async ({ page }) => {
    await page.goto('/login');

    await expect(page.locator('h2')).toHaveText('Welcome back');
    await expect(page.locator('text=CvEvaluator')).toBeVisible();
    await expect(page.locator('a[href="/forgot-password"]')).toBeVisible();
    await expect(page.locator('a[href="/register"]')).toBeVisible();
  });

  test('register page renders correctly', async ({ page }) => {
    await page.goto('/register');

    await expect(page.locator('h2')).toHaveText('Create account');
    await expect(page.locator('input[formcontrolname="fullName"]')).toBeVisible();
    await expect(page.locator('input[formcontrolname="email"]')).toBeVisible();
    await expect(page.locator('input[formcontrolname="password"]')).toBeVisible();
    await expect(page.locator('input[formcontrolname="confirmPassword"]')).toBeVisible();
    await expect(page.locator('a[href="/login"]')).toBeVisible();
  });

  test('register a new user', async ({ page }) => {
    await page.goto('/register');

    await page.locator('input[formcontrolname="fullName"]').fill(TEST_USER.fullName);
    await page.locator('input[formcontrolname="email"]').fill(TEST_USER.email);
    await page.locator('input[formcontrolname="password"]').fill(TEST_USER.password);
    await page.locator('input[formcontrolname="confirmPassword"]').fill(TEST_USER.password);

    await page.locator('button[type="submit"]').click();

    await expect(page).toHaveURL(/\/login/, { timeout: 10_000 });
    await expect(page.locator('text=Account created')).toBeVisible({ timeout: 5_000 });
  });

  test('login with the registered user', async ({ page }) => {
    await page.goto('/login');

    await page.locator('input[formcontrolname="email"]').fill(TEST_USER.email);
    await page.locator('input[formcontrolname="password"]').fill(TEST_USER.password);

    await page.locator('button[type="submit"]').click();

    await expect(page).toHaveURL(/\/dashboard/, { timeout: 10_000 });
    await expect(page.locator('h1:text("Dashboard")')).toBeVisible({ timeout: 5_000 });
  });

  test('login with invalid credentials shows error', async ({ page }) => {
    await page.goto('/login');

    await page.locator('input[formcontrolname="email"]').fill('nonexistent@test.com');
    await page.locator('input[formcontrolname="password"]').fill('wrongpassword');

    await page.locator('button[type="submit"]').click();

    await expect(page.locator('text=Invalid credentials')).toBeVisible({ timeout: 5_000 });
  });

  test('login form validates required fields', async ({ page }) => {
    await page.goto('/login');

    const submitButton = page.locator('button[type="submit"]');
    await expect(submitButton).toBeDisabled();

    await page.locator('input[formcontrolname="email"]').fill('valid@test.com');
    await expect(submitButton).toBeDisabled();

    await page.locator('input[formcontrolname="password"]').fill('123456');
    await expect(submitButton).toBeEnabled();
  });

  test('register form validates password match', async ({ page }) => {
    await page.goto('/register');

    await page.locator('input[formcontrolname="fullName"]').fill('Test');
    await page.locator('input[formcontrolname="email"]').fill('test@test.com');
    await page.locator('input[formcontrolname="password"]').fill('password1');
    await page.locator('input[formcontrolname="confirmPassword"]').fill('password2');

    // Trigger form validation by touching
    await page.locator('input[formcontrolname="confirmPassword"]').blur();

    const submitButton = page.locator('button[type="submit"]');
    await expect(submitButton).toBeDisabled();
  });

  test('unauthenticated user is redirected to login', async ({ page }) => {
    await page.goto('/dashboard');

    await expect(page).toHaveURL(/\/login/, { timeout: 5_000 });
  });

  test('navigate from login to register and back', async ({ page }) => {
    await page.goto('/login');

    await page.locator('a[href="/register"]').click();
    await expect(page).toHaveURL(/\/register/);
    await expect(page.locator('h2')).toHaveText('Create account');

    await page.locator('a[href="/login"]').click();
    await expect(page).toHaveURL(/\/login/);
    await expect(page.locator('h2')).toHaveText('Welcome back');
  });
});

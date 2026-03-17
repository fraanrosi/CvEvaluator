import { test, expect } from '@playwright/test';

test.describe('Forgot Password flow', () => {
  test('forgot password page renders correctly', async ({ page }) => {
    await page.goto('/forgot-password');

    await expect(page.locator('h2')).toHaveText('Forgot password');
    await expect(page.locator('text=Enter your email')).toBeVisible();
    await expect(page.locator('input[formcontrolname="email"]')).toBeVisible();
    await expect(page.locator('button[type="submit"]')).toBeVisible();
    await expect(page.locator('a[href="/login"]')).toBeVisible();
  });

  test('forgot password form validates email', async ({ page }) => {
    await page.goto('/forgot-password');

    const submitButton = page.locator('button[type="submit"]');
    await expect(submitButton).toBeDisabled();

    await page.locator('input[formcontrolname="email"]').fill('valid@test.com');
    await expect(submitButton).toBeEnabled();
  });

  test('forgot password submits and shows success message', async ({ page }) => {
    await page.goto('/forgot-password');

    await page.locator('input[formcontrolname="email"]').fill('test@example.com');
    await page.locator('button[type="submit"]').click();

    await expect(page.locator('text=reset link has been sent')).toBeVisible({ timeout: 10_000 });
    await expect(page.locator('text=Send again')).toBeVisible();
  });

  test('navigate from login to forgot password', async ({ page }) => {
    await page.goto('/login');

    await page.locator('a[href="/forgot-password"]').click();
    await expect(page).toHaveURL(/\/forgot-password/);
    await expect(page.locator('h2')).toHaveText('Forgot password');
  });
});

test.describe('Reset Password page', () => {
  test('shows invalid link message when no params', async ({ page }) => {
    await page.goto('/reset-password');

    await expect(page.locator('text=Invalid or missing reset link')).toBeVisible();
    await expect(page.locator('a[href="/forgot-password"]')).toBeVisible();
  });

  test('shows form when token and email params present', async ({ page }) => {
    await page.goto('/reset-password?token=fake-token&email=test@test.com');

    await expect(page.locator('h2')).toHaveText('Reset password');
    await expect(page.locator('input[formcontrolname="newPassword"]')).toBeVisible();
    await expect(page.locator('input[formcontrolname="confirmPassword"]')).toBeVisible();
    await expect(page.locator('button[type="submit"]')).toBeVisible();
  });

  test('validates password match', async ({ page }) => {
    await page.goto('/reset-password?token=fake-token&email=test@test.com');

    await page.locator('input[formcontrolname="newPassword"]').fill('password1');
    await page.locator('input[formcontrolname="confirmPassword"]').fill('password2');
    await page.locator('input[formcontrolname="confirmPassword"]').blur();

    const submitButton = page.locator('button[type="submit"]');
    await expect(submitButton).toBeDisabled();
  });

  test('validates minimum password length', async ({ page }) => {
    await page.goto('/reset-password?token=fake-token&email=test@test.com');

    await page.locator('input[formcontrolname="newPassword"]').fill('12345');
    await page.locator('input[formcontrolname="newPassword"]').blur();

    await expect(page.locator('text=At least 6 characters')).toBeVisible();
  });
});

test.describe('Confirm Email page', () => {
  test('shows error when no params', async ({ page }) => {
    await page.goto('/confirm-email');

    await expect(page.locator('text=Confirmation Failed')).toBeVisible({ timeout: 5_000 });
    await expect(page.locator('text=Invalid or missing confirmation link')).toBeVisible();
  });

  test('shows loading state then result with params', async ({ page }) => {
    await page.goto('/confirm-email?token=fake-token&email=test@test.com');

    // Should attempt confirmation and show result (will fail with fake token)
    await expect(page.locator('text=Confirmation Failed').or(page.locator('text=Email Confirmed'))).toBeVisible({ timeout: 10_000 });
  });

  test('resend confirmation button visible on failure', async ({ page }) => {
    await page.goto('/confirm-email?token=fake-token&email=test@test.com');

    await expect(page.locator('text=Confirmation Failed')).toBeVisible({ timeout: 10_000 });
    await expect(page.locator('text=Resend confirmation email')).toBeVisible();
    await expect(page.locator('a[href="/login"]')).toBeVisible();
  });
});

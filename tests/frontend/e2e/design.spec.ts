import { test, expect } from '@playwright/test';

test.describe('Dark theme and design consistency', () => {
  test('login page has dark background', async ({ page }) => {
    await page.goto('/login');

    const body = page.locator('body');
    await expect(body).toHaveClass(/bg-surface-950/);
    await expect(body).toHaveClass(/antialiased/);
  });

  test('login page shows brand with accent color', async ({ page }) => {
    await page.goto('/login');

    const accentSpan = page.locator('h1 span.text-accent');
    await expect(accentSpan).toHaveText('Evaluator');
  });

  test('404 page uses dark theme', async ({ page }) => {
    await page.goto('/nonexistent-page-xyz');

    await expect(page.locator('text=404')).toBeVisible();
    await expect(page.locator('text=Page not found')).toBeVisible();

    const container = page.locator('.bg-surface-950').first();
    await expect(container).toBeVisible();
  });

  test('register page has background decoration orbs', async ({ page }) => {
    await page.goto('/register');

    const blurOrbs = page.locator('.blur-3xl');
    await expect(blurOrbs.first()).toBeVisible();
  });

  test('forgot password page uses card component', async ({ page }) => {
    await page.goto('/forgot-password');

    const card = page.locator('.card');
    await expect(card.first()).toBeVisible();
  });

  test('auth pages use Sora font for headings', async ({ page }) => {
    await page.goto('/login');

    const heading = page.locator('h1');
    const fontFamily = await heading.evaluate(el => getComputedStyle(el).fontFamily);
    expect(fontFamily).toContain('Sora');
  });
});

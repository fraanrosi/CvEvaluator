import { test, expect } from '@playwright/test';

test.describe('Landing page', () => {
  test('renders hero section with brand and headline', async ({ page }) => {
    await page.goto('/');

    await expect(page.locator('nav')).toBeVisible();
    await expect(page.locator('nav')).toContainText('CvEvaluator');
    await expect(page.locator('h1')).toContainText('Evaluate resumes');
    await expect(page.locator('h1')).toContainText('smarter and faster');
  });

  test('has sign in and get started links in navbar', async ({ page }) => {
    await page.goto('/');

    await expect(page.locator('nav a[href="/login"]')).toBeVisible();
    await expect(page.locator('nav a[href="/register"]')).toBeVisible();
  });

  test('has CTA button linking to register', async ({ page }) => {
    await page.goto('/');

    const cta = page.locator('a[href="/register"]:text("Start evaluating for free")');
    await expect(cta).toBeVisible();
  });

  test('renders features section with four cards', async ({ page }) => {
    await page.goto('/');

    await expect(page.locator('#features')).toBeVisible();
    await expect(page.locator('text=Everything you need to hire better')).toBeVisible();

    const cards = page.locator('.card-hover');
    await expect(cards).toHaveCount(4);
  });

  test('renders how it works section with three steps', async ({ page }) => {
    await page.goto('/');

    await expect(page.locator('text=Three simple steps')).toBeVisible();
    await expect(page.locator('text=Define your position')).toBeVisible();
    await expect(page.locator('text=Upload CVs')).toBeVisible();
    await expect(page.locator('text=Get AI evaluations')).toBeVisible();
  });

  test('renders CTA section', async ({ page }) => {
    await page.goto('/');

    await expect(page.locator('text=Ready to streamline your hiring?')).toBeVisible();
    await expect(page.locator('a[href="/register"]:text("Create your free account")')).toBeVisible();
  });

  test('renders footer with copyright', async ({ page }) => {
    await page.goto('/');

    const footer = page.locator('footer');
    await expect(footer).toBeVisible();
    await expect(footer).toContainText('CvEvaluator');
    await expect(footer).toContainText(`${new Date().getFullYear()}`);
  });

  test('sign in link navigates to login', async ({ page }) => {
    await page.goto('/');

    await page.locator('nav a[href="/login"]').click();
    await expect(page).toHaveURL(/\/login/);
  });

  test('get started link navigates to register', async ({ page }) => {
    await page.goto('/');

    await page.locator('nav a[href="/register"]').click();
    await expect(page).toHaveURL(/\/register/);
  });

  test('uses dark theme background', async ({ page }) => {
    await page.goto('/');

    const hero = page.locator('.bg-surface-950').first();
    await expect(hero).toBeVisible();
  });

  test('has background decoration blurs', async ({ page }) => {
    await page.goto('/');

    const blurOrbs = page.locator('.blur-3xl');
    await expect(blurOrbs.first()).toBeVisible();
  });
});

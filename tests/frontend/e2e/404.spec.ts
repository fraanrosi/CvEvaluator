import { test, expect } from '@playwright/test';

test.describe('404 page', () => {
  test('shows 404 for invalid route', async ({ page }) => {
    await page.goto('/this-route-does-not-exist');

    await expect(page.locator('text=404')).toBeVisible({ timeout: 5_000 });
    await expect(page.locator('text=Page not found')).toBeVisible();
    await expect(page.locator('a[href="/dashboard"]')).toBeVisible();
  });
});

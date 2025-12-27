# Fix for Unity Files Not Loading

## The Problem

The catch-all redirect in `netlify.toml` was intercepting Unity file requests and redirecting them to `/index.html`, causing the browser to receive HTML instead of JavaScript files.

## The Solution

We're using a `_redirects` file in the `public/` folder instead of redirects in `netlify.toml`. This file:
1. Gets copied to `dist/_redirects` during build
2. Is processed by Netlify **after** static files are served
3. Only redirects paths that don't match existing files

## Files Changed

1. **Created**: `web/public/_redirects` - Contains redirect rule
2. **Updated**: `web/netlify.toml` - Removed redirect rule (using _redirects instead)

## Next Steps

1. **Rebuild the app**:
   ```bash
   cd web
   npm run build
   ```

2. **Verify `_redirects` exists in dist**:
   - Check that `web/dist/_redirects` file exists
   - It should contain: `/*    /index.html    200`

3. **Redeploy to Netlify**:
   - Make sure the `dist` folder includes the `_redirects` file
   - Deploy the updated build

4. **Test**:
   - After deployment, Unity files should load correctly
   - No more "Unexpected token '<'" errors

## Why This Works

The `_redirects` file in Netlify is processed **after** static file serving, meaning:
- If a file exists at `/unity/Build/CoICT3DMap.loader.js`, it's served directly
- Only if the file doesn't exist does the redirect to `/index.html` apply
- This is exactly what we need for Unity files to work


# Testing Unity Files Access

## Quick Test

Try accessing these URLs directly in your browser (replace `yoursite.netlify.app` with your actual Netlify URL):

1. `https://yoursite.netlify.app/unity/Build/CoICT3DMap.loader.js`
   - Should show JavaScript code (not HTML)
   - If it shows HTML or 404, the file isn't being served correctly

2. `https://yoursite.netlify.app/unity/index.html`
   - Should show the Unity HTML page

3. `https://yoursite.netlify.app/unity/Build/CoICT3DMap.data.unityweb`
   - Should download or show binary data (not HTML)

## If Files Return HTML Instead of Content

This means the redirect is catching them. The `_redirects` file might not be working as expected.

## Alternative Solution

If `_redirects` doesn't work, we might need to:
1. Remove the catch-all redirect
2. Only redirect specific routes
3. Or use a different approach




















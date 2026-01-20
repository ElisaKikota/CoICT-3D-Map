# Troubleshooting: Game Not Loading

## Issue: Loading screen shows 100% but game doesn't appear

### Check Browser Console

1. Open your browser's Developer Tools (F12)
2. Go to the **Console** tab
3. Look for errors (they'll be in red)

Common errors to look for:
- `Failed to load resource: /unity/Build/CoICT3DMap.loader.js`
- `CORS policy` errors
- `createUnityInstance is not defined`
- 404 errors for Unity files

### Verify Files Exist

Make sure these files exist in `web/dist/unity/Build/` after building:

- `CoICT3DMap.loader.js`
- `CoICT3DMap.framework.js.unityweb`
- `CoICT3DMap.data.unityweb`
- `CoICT3DMap.wasm.unityweb`

### Check File Paths

1. After building (`npm run build`), check `web/dist/unity/` folder
2. Make sure the `Build/` folder contains all 4 Unity files
3. Open `web/dist/unity/index.html` in a browser directly - does it work?

### Quick Fixes

**Fix 1: Rebuild and redeploy**
```bash
cd web
npm run build
# Then redeploy the dist folder to Netlify
```

**Fix 2: Check Netlify deployment**
1. Go to Netlify dashboard
2. Check the deployment logs
3. Look for any build errors or warnings

**Fix 3: Test locally first**
```bash
cd web
npm run build
npm run preview
# Open http://localhost:4173
# Check browser console for errors
```

**Fix 4: Check if Unity files are being copied**

The Unity files should be in `web/public/unity/Build/` before building.

If they're missing, copy them:
```powershell
Copy-Item -Path "Builds\CoICT3DMap\Build\*" -Destination "web\public\unity\Build\" -Recurse -Force
```

### Common Issues

**Issue: 404 errors for Unity files**
- **Solution**: Make sure files are in `web/public/unity/Build/` before building
- Files must be copied BEFORE running `npm run build`

**Issue: CORS errors**
- **Solution**: The `netlify.toml` has been updated to fix CORS headers
- Rebuild and redeploy

**Issue: createUnityInstance is not defined**
- **Solution**: The loader.js file isn't loading correctly
- Check that `CoICT3DMap.loader.js` exists and is accessible
- Check browser Network tab to see if it's loading

**Issue: Game loads but is blank/black**
- **Solution**: This is a Unity issue, not a web issue
- Check Unity console for errors
- Verify your Unity build works when opened directly

### Debugging Steps

1. **Check Network Tab**
   - Open DevTools → Network tab
   - Refresh the page
   - Look for failed requests (red status codes)
   - Check if Unity files are loading (200 status)

2. **Check if iframe is loading**
   - In Console, type: `document.querySelector('iframe')`
   - Check if the iframe exists
   - Check iframe src: Should be `/unity/index.html`

3. **Test Unity HTML directly**
   - Try opening `https://yoursite.netlify.app/unity/index.html` directly
   - Does the game load? If yes, the issue is with the iframe
   - If no, the issue is with the Unity files or paths

4. **Check file sizes**
   - Unity files (especially .data.unityweb) can be large
   - Make sure they're not 0 bytes
   - Large files might take time to download

### Still Not Working?

1. Share the browser console errors
2. Check the Network tab - which files are failing?
3. Verify the build output in `web/dist/` matches expected structure




















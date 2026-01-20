# Unity WebGL Build Guide for CoICT 3D Map

This guide will walk you through building your Unity project as WebGL for web deployment.

## Prerequisites

1. Unity Editor (with WebGL Build Support module installed)
2. Your CoICT 3D Map project open in Unity

## Step 1: Install WebGL Build Support (if not already installed)

1. Open Unity Hub
2. Go to **Installs** tab
3. Find your Unity version (e.g., Unity 2022.x)
4. Click the **Settings/gear icon** → **Add Modules**
5. Check **WebGL Build Support**
6. Click **Install**

## Step 2: Configure WebGL Build Settings

1. In Unity, go to **File** → **Build Settings**
2. Select **WebGL** platform (if not already selected)
3. Click **Switch Platform** (if needed - this may take a few minutes)

### Configure WebGL Template (Optional but Recommended)

1. Still in Build Settings, click **Player Settings**
2. In the Inspector, expand **Publishing Settings**
3. Under **Compression Format**, select:
   - **Gzip** (good balance) or
   - **Brotli** (best compression, but not supported by all browsers)
4. Make sure **Compression Fallback** is checked

### Set Build Output Settings

1. In **Player Settings** → **Resolution and Presentation**:
   - **Default Canvas Width**: 1080
   - **Default Canvas Height**: 1920
   - **Run In Background**: ✓ (checked)
   
2. **WebGL Template**: You can use:
   - **Default** (simple template)
   - **Minimal** (even simpler)
   - Or your custom template

### Memory Settings

1. In **Player Settings** → **Publishing Settings**:
   - Set **Memory Size** (e.g., 2048 MB) - adjust based on your project size
   - **Exception Support**: Full (if you want stack traces) or None (smaller build)

## Step 3: Build the WebGL Project

### Option A: Quick Build

1. In **Build Settings**, click **Build**
2. Choose/create a folder for the build (recommended: `Builds/WebGL/`)
3. Click **Select Folder**
4. Wait for the build to complete (this may take several minutes)

### Option B: Build and Run (for testing)

1. In **Build Settings**, click **Build And Run**
2. Choose the output folder
3. Unity will build and automatically open it in your browser

## Step 4: Locate Your Build Files

After building, you'll find these files in your build output folder:

```
Builds/WebGL/
├── index.html              ← Unity's default HTML
├── Build/
│   ├── YourProjectName.loader.js       ← Main loader script
│   ├── YourProjectName.framework.js.unityweb   ← Framework
│   ├── YourProjectName.data.unityweb   ← Game data (usually largest file)
│   └── YourProjectName.wasm.unityweb   ← WebAssembly code
└── TemplateData/           ← UI assets (logos, progress bars, etc.)
    ├── style.css
    ├── favicon.ico
    └── ... (other template assets)
```

**Note**: File names will match your **Product Name** from Player Settings.

## Step 5: Copy Files to Web Directory

### For CoICT 3D Map Project:

Based on your project, you need to copy files to `web/public/unity/`:

1. **From your Unity WebGL build folder**, copy:
   - `Build/` folder → `web/public/unity/Build/`
   - `TemplateData/` folder → `web/public/unity/TemplateData/`
   - `index.html` → `web/public/unity/index.html` (optional, Unity template)

2. **Make sure your build files are named:**
   - `CoICT3DMap.loader.js`
   - `CoICT3DMap.framework.js.unityweb`
   - `CoICT3DMap.data.unityweb`
   - `CoICT3DMap.wasm.unityweb`

### If File Names Don't Match:

If your Unity build creates files with different names, you have two options:

**Option 1: Rename in Unity (Recommended)**
1. Go to **Edit** → **Project Settings** → **Player**
2. Under **Product Name**, set it to: `CoICT3DMap`
3. Rebuild your WebGL project

**Option 2: Rename Files Manually**
1. After building, rename the files to match:
   - `YourBuildName.loader.js` → `CoICT3DMap.loader.js`
   - `YourBuildName.framework.js.unityweb` → `CoICT3DMap.framework.js.unityweb`
   - `YourBuildName.data.unityweb` → `CoICT3DMap.data.unityweb`
   - `YourBuildName.wasm.unityweb` → `CoICT3DMap.wasm.unityweb`

2. Then update `web/public/unity/index.html` to reference the correct file names

## Step 6: Verify File Structure

Your `web/public/unity/` folder should look like this:

```
web/public/unity/
├── Build/
│   ├── CoICT3DMap.loader.js
│   ├── CoICT3DMap.framework.js.unityweb
│   ├── CoICT3DMap.data.unityweb
│   └── CoICT3DMap.wasm.unityweb
├── TemplateData/
│   ├── style.css
│   ├── favicon.ico
│   └── ... (other template files)
└── index.html
```

## Step 7: Build Your React App

Now that Unity files are in place:

```bash
cd web
npm run build
```

This will copy everything from `public/unity/` to `dist/unity/` automatically.

## Troubleshooting

### Build Size Too Large

If your WebGL build is too large (>50MB):

1. **Enable Compression**:
   - Player Settings → Publishing Settings → Compression Format: **Brotli** or **Gzip**

2. **Reduce Texture Sizes**:
   - Reduce texture resolution in your assets
   - Use compressed texture formats

3. **Code Stripping**:
   - Player Settings → Other Settings → Managed Stripping Level: **Medium** or **High**

### Build Fails

- Check that WebGL module is installed
- Check Console for specific errors
- Try building to a different folder (with write permissions)
- Make sure no files in the build folder are open/locked

### Files Not Loading in Browser

- Check browser console for CORS errors
- Verify file names match what's in `index.html`
- Make sure `netlify.toml` has correct headers (already configured)

### Performance Issues

- Reduce texture quality
- Lower polygon count in models
- Disable unnecessary features
- Use LOD (Level of Detail) for objects
- Optimize shaders

## Quick Build Checklist

- [ ] WebGL module installed in Unity
- [ ] Switched platform to WebGL in Build Settings
- [ ] Set Product Name to "CoICT3DMap" (or rename files)
- [ ] Configured compression format (Gzip/Brotli)
- [ ] Built WebGL project to `Builds/WebGL/`
- [ ] Copied `Build/` folder to `web/public/unity/Build/`
- [ ] Copied `TemplateData/` to `web/public/unity/TemplateData/`
- [ ] Verified file names match expected names
- [ ] Ran `npm run build` in web directory
- [ ] Tested with `npm run preview`

## Next Steps

After building, follow the [README_BUILD.md](./README_BUILD.md) to build and deploy your React app to Netlify.




















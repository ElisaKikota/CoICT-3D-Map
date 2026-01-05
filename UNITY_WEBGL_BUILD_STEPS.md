# Unity WebGL Build Steps for CoICT 3D Map

## Quick Summary

Your project already has a WebGL build with correct file names in `Builds/CoICT3DMap/`. You can copy these files directly to `web/public/unity/`. However, if you need to rebuild, follow the steps below.

## Step-by-Step Build Process

### Step 1: Open Build Settings

1. Open your Unity project
2. Go to **File** → **Build Settings** (or press `Ctrl+Shift+B` / `Cmd+Shift+B`)

### Step 2: Select WebGL Platform

1. In the Build Settings window, find **WebGL** in the platform list
2. If WebGL is not selected:
   - Click on **WebGL**
   - Click **Switch Platform** button
   - Wait for Unity to switch platforms (this may take a few minutes)

### Step 3: Configure Product Name

**IMPORTANT:** The product name must be "CoICT3DMap" for the files to match what the web app expects.

1. Click **Player Settings** button (or go to **Edit** → **Project Settings** → **Player**)
2. In the Inspector, find **Product Name**
3. Set it to: `CoICT3DMap` (exactly this name, no spaces)
4. This ensures your build files will be named:
   - `CoICT3DMap.loader.js`
   - `CoICT3DMap.framework.js.unityweb`
   - `CoICT3DMap.data.unityweb`
   - `CoICT3DMap.wasm.unityweb`

### Step 4: Configure WebGL Settings

1. Still in **Player Settings**, scroll down to **Publishing Settings** section
2. Configure these settings:
   - **Compression Format**: Choose `Gzip` (good balance) or `Brotli` (best compression)
   - **Decompression Fallback**: ✓ Checked
   - **Memory Size**: Adjust based on your project (try 1024 MB or 2048 MB)
   - **Exception Support**: `Full` (for debugging) or `None` (smaller build)

3. Scroll to **Resolution and Presentation**:
   - **Default Canvas Width**: `1080`
   - **Default Canvas Height**: `1920`
   - **Run In Background**: ✓ Checked

### Step 5: Build the WebGL Project

1. Go back to **Build Settings** window
2. Make sure **WebGL** is selected
3. Click **Build** button
4. Choose/create the output folder:
   - Recommended: `Builds/CoICT3DMap/` (or create a new folder)
5. Click **Select Folder**
6. Wait for the build to complete (this can take 5-20 minutes depending on project size)

### Step 6: Verify Build Files

After building, check that these files exist in your build folder:

```
Builds/CoICT3DMap/
├── Build/
│   ├── CoICT3DMap.loader.js          ← Main loader
│   ├── CoICT3DMap.framework.js.unityweb  ← Framework
│   ├── CoICT3DMap.data.unityweb      ← Game data (largest file)
│   └── CoICT3DMap.wasm.unityweb      ← WebAssembly code
├── TemplateData/                      ← UI assets
│   ├── style.css
│   ├── favicon.ico
│   └── ... (other template files)
└── index.html                         ← Unity HTML template
```

### Step 7: Copy Files to Web Directory

Copy the build files to your web directory:

1. **Copy the Build folder:**
   - From: `Builds/CoICT3DMap/Build/`
   - To: `web/public/unity/Build/`
   - Make sure all 4 files are copied:
     - `CoICT3DMap.loader.js`
     - `CoICT3DMap.framework.js.unityweb`
     - `CoICT3DMap.data.unityweb`
     - `CoICT3DMap.wasm.unityweb`

2. **Copy the TemplateData folder:**
   - From: `Builds/CoICT3DMap/TemplateData/`
   - To: `web/public/unity/TemplateData/`

3. **Copy index.html (optional):**
   - From: `Builds/CoICT3DMap/index.html`
   - To: `web/public/unity/index.html`
   - (This is Unity's default template - you may already have a customized one)

### Step 8: Final File Structure

Your `web/public/unity/` should look like this:

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
│   └── ... (all template files)
└── index.html
```

## Quick Copy Command (If Using Existing Build)

If you already have a build in `Builds/CoICT3DMap/`, you can copy it directly:

**Windows (PowerShell):**
```powershell
# Copy Build folder
Copy-Item -Path "Builds\CoICT3DMap\Build\*" -Destination "web\public\unity\Build\" -Recurse -Force

# Copy TemplateData folder
Copy-Item -Path "Builds\CoICT3DMap\TemplateData\*" -Destination "web\public\unity\TemplateData\" -Recurse -Force
```

**Mac/Linux:**
```bash
# Copy Build folder
cp -r Builds/CoICT3DMap/Build/* web/public/unity/Build/

# Copy TemplateData folder
cp -r Builds/CoICT3DMap/TemplateData/* web/public/unity/TemplateData/
```

## Troubleshooting

### Files Have Wrong Names

If your build files are named something like `3DCoICT.*` or `WebGL.*` instead of `CoICT3DMap.*`:

1. Go to **Edit** → **Project Settings** → **Player**
2. Change **Product Name** to exactly: `CoICT3DMap`
3. Rebuild the WebGL project

### Build Size Too Large

If your build is very large (>50MB):

1. Enable compression: **Publishing Settings** → **Compression Format**: `Brotli`
2. Reduce texture sizes in your assets
3. Use code stripping: **Other Settings** → **Managed Stripping Level**: `Medium` or `High`

### Build Fails

- Make sure WebGL module is installed (Unity Hub → Installs → Add Modules)
- Check Console for specific errors
- Try building to a different folder
- Make sure no files in build folder are locked/open

### Files Not Loading in Browser

- Verify file names match exactly: `CoICT3DMap.*`
- Check browser console for errors
- Make sure files are in `web/public/unity/Build/` (not just `web/unity/`)
- Clear browser cache

## Next Steps

After copying the files:

1. Build your React app: `cd web && npm run build`
2. Test locally: `npm run preview`
3. Deploy to Netlify (see README_BUILD.md)










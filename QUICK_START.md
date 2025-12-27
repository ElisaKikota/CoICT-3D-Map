# Quick Start: Unity WebGL Build for Web Deployment

## ⚡ Fastest Way (If You Already Have a Build)

You already have a WebGL build in `Builds/CoICT3DMap/` with correct file names!

**Just copy the files:**

```powershell
# From project root directory
Copy-Item -Path "Builds\CoICT3DMap\Build\*" -Destination "web\public\unity\Build\" -Recurse -Force
Copy-Item -Path "Builds\CoICT3DMap\TemplateData\*" -Destination "web\public\unity\TemplateData\" -Recurse -Force
```

Then build your React app:
```bash
cd web
npm run build
```

---

## 🔨 If You Need to Rebuild

### 1. Set Product Name in Unity

**IMPORTANT:** The product name must be `CoICT3DMap` (not "3DCoICT")

1. Open Unity
2. **Edit** → **Project Settings** → **Player**
3. Set **Product Name** to: `CoICT3DMap`
4. This ensures build files are named correctly

### 2. Build WebGL

1. **File** → **Build Settings**
2. Select **WebGL** platform
3. Click **Switch Platform** (if needed)
4. Click **Build**
5. Choose output folder: `Builds/CoICT3DMap/`
6. Wait for build to complete

### 3. Copy Files to Web Directory

Copy from `Builds/CoICT3DMap/` to `web/public/unity/`:

- `Build/` folder → `web/public/unity/Build/`
- `TemplateData/` folder → `web/public/unity/TemplateData/`

### 4. Verify File Names

Make sure these files exist in `web/public/unity/Build/`:

- ✅ `CoICT3DMap.loader.js`
- ✅ `CoICT3DMap.framework.js.unityweb`
- ✅ `CoICT3DMap.data.unityweb`
- ✅ `CoICT3DMap.wasm.unityweb`

### 5. Build React App

```bash
cd web
npm install    # First time only
npm run build  # Creates dist/ folder
```

### 6. Deploy to Netlify

Deploy the `web/dist/` folder to Netlify (drag & drop or Git integration).

---

## 📚 Detailed Guides

- **Full Unity Build Guide**: See `UNITY_WEBGL_BUILD_STEPS.md`
- **React Build & Deploy**: See `web/README_BUILD.md`
- **Complete Instructions**: See `web/BUILD_INSTRUCTIONS.md`

---

## ✅ Checklist

- [ ] Product Name set to `CoICT3DMap` in Unity
- [ ] WebGL build completed
- [ ] Files copied to `web/public/unity/Build/`
- [ ] File names match `CoICT3DMap.*`
- [ ] `npm run build` completed successfully
- [ ] Tested with `npm run preview`
- [ ] Deployed to Netlify


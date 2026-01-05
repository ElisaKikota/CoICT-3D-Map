# Quick Build Guide

## How to Build

1. **Navigate to the web directory:**
   ```bash
   cd web
   ```

2. **Install dependencies (first time only):**
   ```bash
   npm install
   ```

3. **Build the app:**
   ```bash
   npm run build
   ```

## Where Files Go

After building, all production files will be in:
```
web/dist/
```

This `dist` folder contains:
- `index.html` - Your main HTML file
- `assets/` - Compiled JavaScript and CSS
- `unity/` - Unity game files (copied from `public/unity/`)

## Deploy to Netlify

### Option 1: Drag & Drop
1. Build the app: `npm run build`
2. Go to Netlify dashboard
3. Drag the `web/dist` folder to Netlify

### Option 2: Git Integration
1. Push your code to GitHub/GitLab
2. Connect repository to Netlify
3. Netlify will automatically:
   - Run `npm run build`
   - Deploy from `dist` folder
   - (Configured in `netlify.toml`)

### Option 3: Netlify CLI
```bash
npm install -g netlify-cli
netlify login
netlify deploy --prod --dir=dist
```

## Important: Unity Files Location

Make sure your Unity WebGL build files are in:
```
web/public/unity/Build/
```

Files needed:
- `CoICT3DMap.loader.js`
- `CoICT3DMap.data.unityweb`
- `CoICT3DMap.framework.js.unityweb`
- `CoICT3DMap.wasm.unityweb`

These will automatically be copied to `dist/unity/Build/` during build.

## Test Locally Before Deploying

```bash
npm run preview
```

Then open `http://localhost:4173` in your browser.










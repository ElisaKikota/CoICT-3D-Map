# Build Instructions for CoICT 3D Map Web App

## Prerequisites

Make sure you have Node.js and npm installed.

## Step 1: Install Dependencies

If you haven't already, install the required npm packages:

```bash
cd web
npm install
```

## Step 2: Build the React App

Run the build command:

```bash
npm run build
```

This will:
- Compile your React components
- Bundle all JavaScript and CSS
- Copy files from `public/` folder to `dist/` folder
- Create optimized production files in the `dist/` directory

## Step 3: Verify Build Output

After building, check the `dist/` folder structure:

```
web/dist/
├── index.html              # Main HTML file
├── assets/                 # Compiled JS and CSS
│   ├── index-*.js
│   └── index-*.css
├── unity/                  # Unity game files (copied from public/unity/)
│   ├── Build/
│   │   ├── CoICT3DMap.loader.js
│   │   ├── CoICT3DMap.data.unityweb
│   │   ├── CoICT3DMap.framework.js.unityweb
│   │   └── CoICT3DMap.wasm.unityweb
│   ├── index.html
│   └── TemplateData/
└── vite.svg                # Static assets
```

## Step 4: Deploy to Netlify

### Option A: Deploy via Netlify CLI

1. Install Netlify CLI (if not already installed):
   ```bash
   npm install -g netlify-cli
   ```

2. Login to Netlify:
   ```bash
   netlify login
   ```

3. Deploy:
   ```bash
   netlify deploy --prod --dir=dist
   ```

### Option B: Deploy via Netlify Dashboard

1. Go to your Netlify dashboard
2. Drag and drop the `web/dist` folder, OR
3. Connect your Git repository and set:
   - **Build command**: `npm run build`
   - **Publish directory**: `dist`
   - **Base directory**: `web` (if deploying from root)

### Option C: Use Netlify Build Settings

The `netlify.toml` file is already configured. If you connect your Git repo:

- Netlify will automatically run `npm run build` when you push
- It will publish from the `dist` directory
- All headers and redirects are configured

## Important Notes

1. **Unity Files Location**: Unity build files should be in `web/public/unity/` - they will automatically be copied to `dist/unity/` during build.

2. **If Unity Files Are Missing**: 
   - Copy your Unity WebGL build files to `web/public/unity/Build/`
   - Make sure these files exist:
     - `CoICT3DMap.loader.js`
     - `CoICT3DMap.data.unityweb`
     - `CoICT3DMap.framework.js.unityweb`
     - `CoICT3DMap.wasm.unityweb`

3. **Testing Locally**: Before deploying, test the build locally:
   ```bash
   npm run preview
   ```
   This will serve the `dist` folder at `http://localhost:4173`

4. **Development Mode**: To test changes during development:
   ```bash
   npm run dev
   ```
   This runs a development server at `http://localhost:5173`

## File Structure Summary

```
web/
├── dist/                    # ← BUILD OUTPUT (deploy this to Netlify)
│   ├── index.html
│   ├── assets/
│   └── unity/
├── public/                  # ← Static files (copied to dist during build)
│   └── unity/              # ← Place Unity files here
├── src/                     # ← React source code
│   ├── App.jsx
│   ├── App.css
│   └── main.jsx
├── index.html               # ← Template (used during build)
├── package.json
├── vite.config.js
└── netlify.toml            # ← Netlify configuration
```

## Troubleshooting

- **Build fails**: Make sure all dependencies are installed (`npm install`)
- **Unity game not loading**: Check that Unity files are in `public/unity/Build/`
- **404 errors**: Make sure `netlify.toml` redirects are configured correctly




















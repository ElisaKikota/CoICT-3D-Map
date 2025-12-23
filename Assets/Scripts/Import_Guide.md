# Blender Asset Import Guide

## Step 1: Exporting from Blender

1. **Open your Blender file** with the new map
2. **Select all objects** you want to export (or select the entire scene)
3. **File → Export → FBX (.fbx)**
4. **Export Settings:**
   - ✅ Selected Objects (if you selected specific objects)
   - ✅ Apply Transform
   - ✅ Forward: -Z Forward
   - ✅ Up: Y Up
   - ✅ Scale: 1.0
   - ✅ Apply Modifiers
   - ✅ Smoothing: Face (or Edge if needed)

## Step 2: Importing to Unity

1. **Drag your FBX file** into `Assets/Models/` folder
2. **Select the imported FBX** in the Project window
3. **In the Inspector, check these settings:**

### Model Tab:
- Scale Factor: 1 (or adjust if model is too large/small)
- ✅ Import Blender Materials (if you have materials)
- ✅ Generate Colliders (we'll add custom ones later)

### Materials Tab:
- Material Creation Mode: Standard (or Import Standard Materials if you have them)
- Location: Use External Materials (Legacy) or By Material Name

### Animation Tab:
- If no animations: Uncheck "Import Animation"

## Step 3: Procedural Texture Baking for Grass and Ground

Since meshes may not be named, we'll need to:

1. **Identify grass/ground meshes** in the imported model
2. **Create a script** to automatically detect and bake textures
3. **Apply procedural materials** to grass and ground

### Manual Process (Quick):
1. Select your imported model in the scene
2. Expand it in the hierarchy to see child meshes
3. Look for meshes that look like grass/ground
4. For each grass/ground mesh:
   - Select the mesh
   - In Inspector → Materials, create a new material
   - Use a procedural grass shader or standard shader with grass texture
   - Adjust tiling and color

### Automated Process (Better):
We'll create a script to auto-detect and apply materials.

## Step 4: Handling Unnamed Meshes

Since meshes aren't named, we'll:
1. Use a script to scan all meshes
2. Identify by material/shader properties or mesh characteristics
3. Apply appropriate materials automatically


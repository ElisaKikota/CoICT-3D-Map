# Search UI Setup Guide

## Phase 1: Basic Search Implementation

### 1. Create UI Canvas
- Create a new Canvas in your scene
- Set Canvas Scaler to "Scale With Screen Size"
- Set Reference Resolution to 1920x1080

### 2. Create Search Input Field
- Create UI > Input Field as child of Canvas
- Name it "SearchInputField"
- Position it at the top of the screen
- Set width to 80% of screen width
- Set height to 50px
- Add placeholder text: "Search buildings..."

### 3. Create Autocomplete Dropdown
- Create UI > Panel as child of Canvas
- Name it "AutocompleteDropdown"
- Set width to match SearchInputField
- Set height to 200px
- Position it below SearchInputField
- Add ScrollRect component
- Set Vertical Scrollbar to "New Scrollbar"

### 4. Create Dropdown Content
- Create empty GameObject as child of AutocompleteDropdown
- Name it "Content"
- Add VerticalLayoutGroup component
- Set Child Force Expand to true
- Set Child Control Size to true

### 5. Create Dropdown Item Prefab
- Create UI > Button as child of Content
- Name it "DropdownItemPrefab"
- Set height to 40px
- **For TextMeshPro (Recommended):**
  - Add TextMeshPro - Text (UI) component as child
  - Set text alignment to center
  - **IMPORTANT**: Make sure the TextMeshPro component is a direct child of the Button
  - **IMPORTANT**: Set the TextMeshPro component's text to "Button" initially (this will be overridden by code)
- **For Regular Unity Text:**
  - Add Text component as child
  - Set text alignment to center
  - **IMPORTANT**: Make sure the Text component is a direct child of the Button
  - **IMPORTANT**: Set the Text component's text to "Button" initially (this will be overridden by code)
- Add DropdownItem script to the Button
- Create prefab from this button

### 6. Create Clear Button
- Create UI > Button as child of Canvas
- Name it "ClearSearchButton"
- Position it next to SearchInputField
- Set text to "X"
- Set size to 40x40px

### 7. Setup SearchManager
- Create empty GameObject in scene
- Name it "SearchManager"
- Add SearchManager script
- Assign references:
  - Search Input Field: SearchInputField
  - Autocomplete Dropdown: AutocompleteDropdown
  - Dropdown Content: Content
  - Dropdown Item Prefab: DropdownItemPrefab
  - Clear Search Button: ClearSearchButton

### 8. Test the Search
- Play the scene
- Type in the search field
- Verify autocomplete dropdown appears
- Test selecting items from dropdown
- Verify clear button works

## Next Steps
- Phase 2: Bottom Sheet implementation
- Phase 3: Visual effects and highlighting
- Phase 4: Camera movement system

## Troubleshooting

### Common Issues:

1. **Dropdown items show "Button" text instead of building names:**
   - **For TextMeshPro**: Make sure the TextMeshPro - Text (UI) component is a direct child of the Button
   - **For Regular Text**: Make sure the Text component is a direct child of the Button
   - Check that the Text/TextMeshPro component is properly assigned
   - Verify the DropdownItem script is attached to the Button
   - Check the Console for debug messages about text setting
   - **Common Issue**: Using TextMeshPro but code is looking for regular Text component

2. **Dropdown doesn't appear:**
   - Make sure SearchInputField is assigned to SearchManager
   - Check that AutocompleteDropdown is assigned
   - Verify dropdown is initially hidden in the scene

3. **Search doesn't work:**
   - Check that searchInputField.onValueChanged is connected
   - Verify minSearchCharacters is set to 2
   - Check Console for search debug messages

4. **UI Layout Issues:**
   - Make sure Canvas Scaler is set to "Scale With Screen Size"
   - Check that all UI elements are children of the Canvas
   - Verify anchor points are set correctly

### Debug Steps:
1. Check Console for debug messages
2. Verify all UI references are assigned in SearchManager
3. Test with different search terms
4. Check that dropdown items are being created

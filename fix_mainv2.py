import re

# Read the file
with open(r'C:\Projects\MissionPlanner\MainV2.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# Fix 1: Add InitializeChatSidebar() call
# Remove the broken literal backticks first
content = content.replace('`r`n`r`n            // Initialize AI Chat Sidebar Panel`r`n            InitializeChatSidebar();`r`n', '\r\n')

# Now add it properly
pattern1 = r'(MyView\.AddScreen\(new MainSwitcher\.Screen\("ChatAssistant", typeof\(GCSViews\.ChatAssistant\), false\)\);)\r?\n(\r?\n\s+try)'
replacement1 = r'\1\r\n\r\n            // Initialize AI Chat Sidebar Panel\r\n            InitializeChatSidebar();\r\n\2'
content = re.sub(pattern1, replacement1, content)

# Fix 2: Replace Ctrl+L handler
pattern2 = r'if \(keyData == \(Keys\.Control \| Keys\.L\)\) // limits\s*\{\s*//new DigitalSkyUI\(\)\.ShowUserControl\(\);\s*new SpectrogramUI\(\)\.Show\(\);\s*return true;\s*\}'
replacement2 = '''if (keyData == (Keys.Control | Keys.L)) // AI Assistant
            {
                ToggleChatSidebar();
                return true;
            }'''
content = re.sub(pattern2, replacement2, content, flags=re.DOTALL)

# Write back
with open(r'C:\Projects\MissionPlanner\MainV2.cs', 'w', encoding='utf-8', newline='') as f:
    f.write(content)

print("Successfully updated MainV2.cs")

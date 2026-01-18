import re

# Read the file
with open(r'C:\Projects\ArduPilot-AI-Backend\ap_offline_chat_tool\api_server.py', 'r', encoding='utf-8') as f:
    content = f.read()

# Add the is_telemetry_valid function after the logger initialization
telemetry_check = '''

def is_telemetry_valid(telemetry):
    """Check if telemetry data indicates a real drone connection"""
    if not telemetry:
        return False
    
    # Check battery - if voltage is 0, no drone connected
    if "battery" in telemetry:
        voltage = telemetry["battery"].get("voltage", 0)
        if voltage > 0:
            return True
    
    # Check GPS - if satellites > 0, drone is connected
    if "gps" in telemetry:
        sats = telemetry["gps"].get("satellites", 0)
        if sats > 0:
            return True
    
    # Check status - if mode is not empty/default
    if "status" in telemetry:
        mode = telemetry["status"].get("mode", "")
        if mode and mode != "UNKNOWN" and mode != "":
            return True
    
    return False

'''

# Insert after logger.info line
content = content.replace(
    'logger.info(f"AI Backend API Server initialized")',
    'logger.info(f"AI Backend API Server initialized")' + telemetry_check
)

# Update AGENT_MODE_PROMPT - add connection status
content = content.replace(
    'CURRENT TELEMETRY:\n{telemetry}',
    'CONNECTION STATUS: {connection_status}\n\n{telemetry_section}'
)

# Update ASK_MODE_PROMPT - add connection status  
content = content.replace(
    '- Be concise but thorough\n\nCURRENT TELEMETRY:\n{telemetry}',
    '- Be concise but thorough\n- Be HONEST about connection status\n\nCONNECTION STATUS: {connection_status}\n\n{telemetry_section}'
)

# Update the chat endpoint to use connection detection
old_logic = '''        # Get telemetry
        telemetry = data.get('telemetry', {})
        telemetry_str = format_telemetry_for_prompt(telemetry) if telemetry else "No telemetry data available"
        
        logger.info(f"Processing message in {mode} mode: {user_message}")
        
        # Select prompt based on mode
        if mode == 'agent':
            system_prompt = AGENT_MODE_PROMPT.format(telemetry=telemetry_str)
        else:  # ask mode
            system_prompt = ASK_MODE_PROMPT.format(telemetry=telemetry_str)'''

new_logic = '''        # Get telemetry
        telemetry = data.get('telemetry', {})
        
        # Check if drone is actually connected
        is_connected = is_telemetry_valid(telemetry)
        
        if is_connected:
            connection_status = "CONNECTED to drone"
            telemetry_str = format_telemetry_for_prompt(telemetry)
            telemetry_section = f"CURRENT TELEMETRY:\\n{telemetry_str}"
        else:
            connection_status = "NOT CONNECTED to drone"
            telemetry_section = "TELEMETRY: No drone connected. All values are zero/default."
        
        logger.info(f"Processing message in {mode} mode: {user_message} (Connected: {is_connected})")
        
        # Select prompt based on mode
        if mode == 'agent':
            system_prompt = AGENT_MODE_PROMPT.format(
                connection_status=connection_status,
                telemetry_section=telemetry_section
            )
        else:  # ask mode
            system_prompt = ASK_MODE_PROMPT.format(
                connection_status=connection_status,
                telemetry_section=telemetry_section
            )'''

content = content.replace(old_logic, new_logic)

# Write back
with open(r'C:\Projects\ArduPilot-AI-Backend\ap_offline_chat_tool\api_server.py', 'w', encoding='utf-8') as f:
    f.write(content)

print('✅ API server updated successfully!')
print('The AI will now be honest about drone connection status.')
